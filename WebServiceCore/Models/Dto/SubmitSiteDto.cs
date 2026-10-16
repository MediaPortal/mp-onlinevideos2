using System.ComponentModel.DataAnnotations;
using WebServiceCore.Services;
using WebServiceCore.Models.Entities;
namespace WebServiceCore.Models.Dto;
public class SubmitSiteDto
{
[Required, StringLength(254)] public string Email { get; set; }
 [Required, StringLength(256)] public string Password { get; set; }
 [Required, StringLength(262144)] public string SiteXml { get; set; }
 [MaxLength(InputRules.MaxImageBytes)] public byte[] Icon { get; set; }
 [MaxLength(InputRules.MaxImageBytes)] public byte[] Banner { get; set; }
 [StringLength(120)] public string RequiredDll { get; set; }
}
