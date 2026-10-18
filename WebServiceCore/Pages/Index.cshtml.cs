using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;

namespace WebServiceCore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly OnlineVideosDataContext _context;

        public IndexModel(OnlineVideosDataContext context)
        {
            _context = context;
        }

        public string CurrentSortingProperty { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SortingProperty { get; set; } = nameof(Site.Name);
        [BindProperty(SupportsGet = true)]
        public bool Descending { get; set; }

        public IList<Site> Sites { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        public bool HasMore { get; set; }

        public async Task OnGet()
        {
            // Populate the model with the list of sites using the default sorting
            await SetSites(SortingProperty);
        }

        /// <summary>
        /// Called using ajax to change the sorting of the site grid.
        /// </summary>
        /// <param name="sortingProperty">The property to use when sorting the sites.</param>
        /// <returns>_SiteGridPartial that contains just the table of sites.</returns>
        public async Task<IActionResult> OnGetSiteGrid(string sortingProperty)
        {
            // This model is newly created on every request so the sites always need be retrieved on every request.
            // ToDo: Make use of Razor TempData to persist the retrieved sites across requests
            await SetSites(sortingProperty);
            // Return the partial generated using this model.
            if (string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
                return Partial("_SiteGridPartial", this);
            return Page();
        }

        /// <summary>
        /// Gets the sites from the backend, sorts them, and sets <see cref="CurrentSortingProperty"/> and <see cref="Sites"/> with the results.
        /// </summary>
        /// <param name="sortingProperty">The property to use when sorting the sites.</param>
        /// <returns></returns>
        async Task SetSites(string sortingProperty)
        {
            IQueryable<Site> query = from s in _context.Sites select s;

            switch (sortingProperty)
            {
                case nameof(Site.OwnerId): query = Descending ? query.OrderByDescending(s => s.OwnerId).ThenBy(s => s.Name) : query.OrderBy(s => s.OwnerId).ThenBy(s => s.Name); break;
                case nameof(Site.Language): query = Descending ? query.OrderByDescending(s => s.Language).ThenBy(s => s.Name) : query.OrderBy(s => s.Language).ThenBy(s => s.Name); break;
                case nameof(Site.LastUpdated): query = Descending ? query.OrderByDescending(s => s.LastUpdated).ThenBy(s => s.Name) : query.OrderBy(s => s.LastUpdated).ThenBy(s => s.Name); break;
                default:
                    query = Descending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name);
                    sortingProperty = nameof(Site.Name);
                    break;
            }
            SortingProperty = sortingProperty;
            CurrentSortingProperty = sortingProperty;

            PageNumber = Math.Clamp(PageNumber, 1, 50);
            Sites = await query
                .AsNoTrackingWithIdentityResolution()
                .Skip((PageNumber - 1) * 200).Take(201)
                .Select(s => new Site
                {
                    Name = s.Name,
                    OwnerId = s.OwnerId,
                    Language = s.Language,
                    LastUpdated = s.LastUpdated,
                    Description = s.Description,
                    IsAdult = s.IsAdult,
                    State = s.State,
                    ReportCount = s.Reports.Count
                })
                .ToListAsync();
            HasMore = Sites.Count > 200;
            if (HasMore) Sites.RemoveAt(200);
        }
    }
}
