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
        private readonly Notifications _notifications;
        private readonly PackageReview _packages;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageService _imageService;
        private readonly IConfiguration _config;
        private readonly ILogger<OnlineVideosController> _logger;

        public OnlineVideosController(OnlineVideosDataContext context, IWebHostEnvironment environment, IImageService imageService, IConfiguration config, ILogger<OnlineVideosController> logger, Credentials credentials, PackageReview packages, Notifications notifications = null)
        {
            _context = context;
            _notifications = notifications;
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
            // An unpublished dependency may be submitted for joint administrator review.

            var reviews=HttpContext.RequestServices.GetRequiredService<SiteReviews>();
            var reviewId=await reviews.Stage(user,siteName,xml.OuterXml,dto.RequiredDll,dto.Icon,dto.Banner);
            return Accepted(new { ReviewId=reviewId, Message="Site change queued for administrator review." });
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
            if(_notifications != null) await _notifications.Event("packages", id, "Package awaiting review: " + dto.Name);
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
            if(_notifications != null) {
                if(!moderation) await _notifications.Event("reports", report.Id.ToString(), "New reports");
                else await _notifications.Event("own", report.Id.ToString(), "Report decision for " + site.Name + ": " + dto.Type, site.OwnerId);
            }
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
