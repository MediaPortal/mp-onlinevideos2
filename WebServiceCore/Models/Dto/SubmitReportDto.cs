using System.ComponentModel.DataAnnotations;
using WebServiceCore.Services;
using WebServiceCore.Models.Entities;
namespace WebServiceCore.Models.Dto;
public class SubmitReportDto
{
[Required, StringLength(120)] public string SiteName { get; set; }
 [Required, StringLength(2000, MinimumLength=1)] public string Message { get; set; }
 [EnumDataType(typeof(ReportType))] public ReportType Type { get; set; }
 [StringLength(254)] public string Email { get; set; }
 [StringLength(256)] public string Password { get; set; }
}
