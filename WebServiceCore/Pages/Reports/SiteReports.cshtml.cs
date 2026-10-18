using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;

namespace WebServiceCore.Pages.Reports
{
    public class NewReport
    {
        [Required]
        public DateTime Date { get; set; }
        [Required, EnumDataType(typeof(ReportType))]
        public ReportType Type { get; set; }
        [Required, StringLength(2000, MinimumLength = 1)]
        public string Message { get; set; }
    }

    public class SiteReportsModel : PageModel
    {
        private readonly OnlineVideosDataContext _context;

        public SiteReportsModel(OnlineVideosDataContext context)
        {
            _context = context;
        }

        public IList<Report> Reports { get; set; }

        /// <summary>
        /// The name of the site. Populated automatically by the value contained in the url.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Site { get; set; }

        public string SiteOwner { get; set; }

        public bool CanEdit { get; set; }

        /// <summary>
        /// Populated by the submit report form on the page.
        /// </summary>
        [BindProperty]
        public NewReport NewReport { get; set; }

        /// <summary>
        /// Called when the page is requested with a GET request.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGet()
        {
            await SetReports();
            return Reports == null ? NotFound() : Page();
        }

        /// <summary>
        /// Called using ajax when a new report is POSTed.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostSubmitReport()
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();
            var saved = await TrySubmitReport();
            await SetReports();
            if (Reports == null) return NotFound();
            if (!saved && ModelState.IsValid) return Forbid();
            if (string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)) return Partial("_ReportGridPartial", this);
            return saved ? RedirectToPage(new { site = Site }) : Page();
        }

        async Task<bool> TrySubmitReport()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var site = await _context.Sites.FindAsync(Site);
            if (site == null)
                return false;

            // Recheck current access inside the write transaction; cookie claims can become stale.
            var actor = await _context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == User.Identity.Name);
            if (actor == null || actor.IsDisabled || (actor.Email != site.OwnerId && !actor.IsAdmin))
                return false;

            // Are all required fields present?
            if (!ModelState.IsValid)
                return false;

            Report report = new Report()
            {
                SiteName = Site,
                Date = DateTime.UtcNow,
                Type = NewReport.Type,
                Message = NewReport.Message
            };

            if (await _context.Reports.CountAsync() >= 100000 || await _context.Reports.CountAsync(r => r.SiteName == Site) >= 1000) return false;
            if (report.Type == ReportType.ConfirmedBroken) site.State = SiteState.Broken;
            else if (report.Type is ReportType.Fixed or ReportType.RejectedBroken) site.State = SiteState.Working;
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            var mail = HttpContext.RequestServices.GetRequiredService<WebServiceCore.Services.Notifications>();
            if (report.Type is ReportType.Suggestion or ReportType.Broken) await mail.Event("reports", report.Id.ToString(), "New reports");
            else await mail.Event("own", report.Id.ToString(), "Report decision for " + site.Name + ": " + report.Type, site.OwnerId);
            await transaction.CommitAsync();
            return true;
        }

        /// <summary>
        /// Should be called on every page request, including after form submission, to ensure existing reports are always displayed.
        /// </summary>
        /// <returns></returns>
        async Task SetReports()
        {
            var site = await _context.Sites
                .AsNoTrackingWithIdentityResolution()
                .Include(s => s.Reports.OrderByDescending(r => r.Date).Take(1000))
                .FirstOrDefaultAsync(s => s.Name == Site);

            if (site == null)
                return;

            SiteOwner = site.OwnerId;
            CanEdit = User.Identity.Name == site.OwnerId || User.IsInRole("admin");
            Reports = site.Reports.OrderByDescending(r => r.Date).Take(1000).ToList();
        }
    }
}
