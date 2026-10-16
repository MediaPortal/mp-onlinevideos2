using System.ComponentModel.DataAnnotations;
using WebServiceCore.Services;
using WebServiceCore.Models.Entities;
namespace WebServiceCore.Models.Dto;
public class SubmitDllDto
{
[Required, StringLength(254)] public string Email { get; set; }
 [Required, StringLength(256)] public string Password { get; set; }
 [Required, StringLength(120)] public string Name { get; set; }
 [Required, MinLength(1), MaxLength(InputRules.MaxDllBytes)] public byte[] Data { get; set; }
}
