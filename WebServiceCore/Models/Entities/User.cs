using System.ComponentModel.DataAnnotations;

namespace WebServiceCore.Models.Entities
{
    public class User
    {
        [Key]
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsPublisher { get; set; }
        public bool IsDisabled { get; set; }
        public string ContactEmail { get; set; } = "";
        public bool EmailConfirmed { get; set; }
        public bool NotifyChanges { get; set; }
        public bool NotifyPackages { get; set; }
        public bool NotifyReports { get; set; }
        public bool NotifyOwn { get; set; }
        public List<Site> Sites { get; set; }
        public List<Dll> Dlls { get; set; }
    }
}
