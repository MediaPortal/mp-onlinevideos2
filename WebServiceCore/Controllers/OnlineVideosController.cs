using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Xml;
using System.ComponentModel.DataAnnotations;
using WebServiceCore.Models;
using WebServiceCore.Models.Dto;
using WebServiceCore.Models.Entities;
using WebServiceCore.Services;

namespace WebServiceCore.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OnlineVideosController : ControllerBase
    {
        private readonly OnlineVideosDataContext _context;
        private readonly Credentials _credentials;
        private readonly PackageReview _packages;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageService _imageService;
        private readonly IConfiguration _config;
        private readonly ILogger<OnlineVideosController> _logger;

        public OnlineVideosController(OnlineVideosDataContext context, IWebHostEnvironment environment, IImageService imageService, IConfiguration config, ILogger<OnlineVideosController> logger, Credentials credentials, PackageReview packages)
        {
            _context = context;
            _credentials = credentials;
            _packages = packages;
            _environment = environment;
            _imageService = imageService;
            _config = config;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<ActionResult<string>> RegisterEmail(string email)
        {
            await Task.CompletedTask;
            return StatusCode(410, "Legacy email registration and password recovery are disabled. Contact the operator.");
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<ActionResult<string>> SubmitSite(SubmitSiteDto dto)
        {
            var user = await _credentials.Authenticate(dto.Email, dto.Password);
            if (user == null) return Unauthorized("Invalid credentials.");
            DateTime updateTime = DateTime.Now.ToUniversalTime(); // use universal Time when setting the current TimeStamp to the Database

            // is the given site xml at least valid xml?
            XmlDocument xml = new XmlDocument();
            string siteName = "";
            string lang = "";
            string desc = "";
            bool isAdult = false;
            try
            {
                using var reader = XmlReader.Create(new StringReader(dto.SiteXml), new XmlReaderSettings {
                    DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 262144
                });
                xml.XmlResolver = null;
                xml.Load(reader);
                if (xml.SelectNodes("//Site").Count != 1)
                    return BadRequest("Exactly one <Site> node expected!");

                siteName = xml.SelectSingleNode("//Site/@name").Value;
                lang = xml.SelectSingleNode("//Site/@lang").Value;
                desc = xml.SelectSingleNode("//Site/Description").InnerText;
                XmlNode n = xml.SelectSingleNode("//Site/@agecheck");
                if (n != null) isAdult = bool.Parse(n.Value);
                // write the current server time into the xml as lastUpdated, so comparing it later with the DB field will always work on any client
                // (the submitting client might have wrong time settings)
                XmlNode lastUpdatedNode = xml.SelectSingleNode("//Site/@lastUpdated");
                if (lastUpdatedNode == null)
                {
                    lastUpdatedNode = (xml.SelectSingleNode("//Site") as XmlElement).Attributes.Append(xml.CreateAttribute("lastUpdated"));
                }
                (lastUpdatedNode as XmlAttribute).Value = updateTime.ToString("o");
            }
            catch
            {
                return BadRequest("Invalid xml for site!");
            }

            if (!InputRules.SafeName(siteName) || lang.Length > 16 || desc.Length > 4000)
                return BadRequest("Invalid site metadata.");
            if (dto.RequiredDll != null && !InputRules.SafeName(dto.RequiredDll)) return BadRequest("Invalid DLL name.");
            // is the requiredDll available on the server?
            if (!string.IsNullOrEmpty(dto.RequiredDll) && !await _context.Dlls.AnyAsync(d => d.Name == dto.RequiredDll))
                return BadRequest("Required Dll not found on Server!");

            // does the site already exist?
            Site site = await _context.Sites.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Name == siteName);
            if (site != null)
            {
                // need to update
                if (site.Owner.Email == user.Email || user.IsAdmin)
                {
                    site.Language = lang;
                    site.Description = desc;
                    site.IsAdult = isAdult;
                    site.LastUpdated = updateTime;
                    if (site.XML != dto.SiteXml) site.XML = xml.OuterXml;
                    if (site.DllId != dto.RequiredDll) site.DllId = dto.RequiredDll;
                    site.State = SiteState.Working;
                    await _context.SaveChangesAsync();
                    return Ok("Site successfully updated!" + await SaveImages(siteName, dto.Icon, dto.Banner));
                }
                else
                    return Unauthorized("Only the owner or an admin can update existing sites!");
            }
            else
            {
                if (!_packages.IsPublisher(user)) return StatusCode(403, "An approved publisher must create new sites.");
                if (await _context.Sites.CountAsync() >= 10000) return StatusCode(429, "Site quota exceeded.");
                // insert new site
                site = new Site()
                {
                    Name = siteName,
                    Language = lang,
                    Description = desc,
                    IsAdult = isAdult,
                    LastUpdated = updateTime,
                    XML = xml.OuterXml,
                    DllId = dto.RequiredDll,
                    Owner = user,
                    State = SiteState.Working
                };
                _context.Sites.Add(site);
                await _context.SaveChangesAsync();
                return Ok("Site successfully added!" + await SaveImages(siteName, dto.Icon, dto.Banner));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<ActionResult<string>> SubmitDll(SubmitDllDto dto)
        {
            var user = await _credentials.Authenticate(dto.Email, dto.Password);
            if (user == null) return Unauthorized("Invalid credentials.");
            if (!_packages.IsPublisher(user)) return StatusCode(403, "Publisher approval required.");
            if (!InputRules.SafeName(dto.Name)) return BadRequest("Invalid DLL name.");
            var id = await _packages.Stage(user, dto.Name, dto.Data);
            return Accepted(new { ReviewId = id, Message = "Package staged privately. Local operator approval is required before publication." });
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<ActionResult<string>> SubmitReport(SubmitReportDto dto)
        {
            Site site = await _context.Sites.FirstOrDefaultAsync(s => s.Name == dto.SiteName);
            if (site == null)
                return BadRequest("Site not found!");

            var moderation = dto.Type is ReportType.ConfirmedBroken or ReportType.RejectedBroken or ReportType.Fixed;
            User user = null;
            if (moderation || dto.Email != null || dto.Password != null)
            {
                user = await _credentials.Authenticate(dto.Email, dto.Password);
                if (user == null) return Unauthorized("Invalid credentials.");
                if (user.Email != site.OwnerId && !user.IsAdmin) return StatusCode(403, "Only the owner or an administrator may moderate reports.");
            }
            var recent = DateTime.UtcNow.AddDays(-1);
            if (await _context.Reports.CountAsync() >= 100000 || await _context.Reports.CountAsync(r => r.SiteName == site.Name && r.Date >= recent) >= 50
                || await _context.Reports.CountAsync(r => r.SiteName == site.Name) >= 1000)
                return StatusCode(429, "Report quota exceeded.");
            var report = new Report { Date = DateTime.UtcNow, Message = dto.Message, Type = dto.Type, Site = site };
            // Anonymous suggestions/broken reports are unverified and never change authoritative state.
            if (moderation) site.State = dto.Type == ReportType.ConfirmedBroken ? SiteState.Broken : SiteState.Working;
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            // Report mail is deliberately disabled: do not turn unauthenticated reports into an email relay.
            return Ok("Report successfully submitted!");
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetReportDto>), 200)]
        public async Task<ActionResult<IEnumerable<GetReportDto>>> GetReports(string siteName)
        {
            return await _context.Reports
                .AsNoTrackingWithIdentityResolution()
                .Where(r => r.Site.Name == siteName)
                .OrderByDescending(r => r.Date).Take(1000)
                .Select(r => r.ToGetReportDto())
                .ToListAsync();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetSiteDto>), 200)]
        public async Task<ActionResult<IEnumerable<GetSiteDto>>> GetSitesOverview()
        {
            return await _context.Sites
                .AsNoTrackingWithIdentityResolution()
                .OrderBy(s => s.Name).Take(10000).Select(s => s.ToGetSiteDto())
                .ToListAsync();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetDllDto>), 200)]
        public async Task<ActionResult<IEnumerable<GetDllDto>>> GetDllsOverview()
        {
            return await _context.Dlls
                .AsNoTrackingWithIdentityResolution()
                .OrderBy(d => d.Name).Take(10000).Select(d => d.ToGetDllDto())
                .ToListAsync();
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetDllOwnerDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GetDllOwnerDto>> GetDllOwner(string dllName)
        {
            Dll dll = await _context.Dlls.FindAsync(dllName);
            if (dll == null)
                return NotFound();

            return dll.ToGetDllOwnerDto();
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<string>> GetSiteXml(string siteName)
        {
            Site site = await _context.Sites.FindAsync(siteName);
            if (site == null)
                return NotFound();
            return site.XML;
        }

        [HttpGet]
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<byte[]>> GetSiteIcon(string siteName)
        {
            string path = GetIconPath(siteName);
            return await GetFile(path);
        }

        [HttpGet]
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<byte[]>> GetSiteIconIfChanged(string siteName, string md5)
        {
            string path = GetIconPath(siteName);
            return await GetFile(path, md5);
        }

        [HttpGet]
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<byte[]>> GetSiteBanner(string siteName)
        {
            string path = GetBannerPath(siteName);
            return await GetFile(path);
        }

        [HttpGet]
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<byte[]>> GetSiteBannerIfChanged(string siteName, string md5)
        {
            string path = GetBannerPath(siteName);
            return await GetFile(path, md5);
        }

        [HttpGet]
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<byte[]>> GetDll(string name)
        {
            string path = GetDllPath(name);
            if (!await _context.Dlls.AnyAsync(d => d.Name == name)) return NotFound();
            return await GetFile(path);
        }

        async Task<ActionResult<byte[]>> GetFile(string path, string md5 = null)
        {
            if (!System.IO.File.Exists(path))
                return NotFound();
            if (new FileInfo(path).Length > InputRules.MaxDllBytes) return StatusCode(413);
            byte[] bytes = await System.IO.File.ReadAllBytesAsync(path);
            if (!string.IsNullOrEmpty(md5) && GetMD5Hash(bytes) == md5)
                return NoContent();
            return bytes;
        }

        #region Helper

        string GetIconPath(string siteName)
        {
            return GetSafeWebRootPath("Icons", siteName, ".png");
        }

        string GetBannerPath(string siteName)
        {
            return GetSafeWebRootPath("Banners", siteName, ".png");
        }

        string GetDllPath(string name)
        {
            return GetSafeWebRootPath("Dlls", name, ".dll");
        }

        string GetSafeWebRootPath(string directory, string fileName, string extension)
        {
            return InputRules.AssetPath(_environment.WebRootPath, directory, fileName, extension);
        }

        async Task<string> SaveImages(string siteName, byte[] icon, byte[] banner)
        {
            string message = "";
            if (icon != null && icon.Length > 0)
            {
                message += await _imageService.TrySaveIcon(icon, GetIconPath(siteName));
                message += " ";
            }
            if (banner != null && banner.Length > 0)
            {
                message += await _imageService.TrySaveBanner(banner, GetBannerPath(siteName));
            }
            return message;
        }

        string GetMD5Hash(byte[] data)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] md5Hash = md5.ComputeHash(data);
            string md5String = BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
            return md5String;
        }

        #endregion
    }
}
