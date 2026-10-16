using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;
using WebServiceCore.Services;
namespace WebServiceCore.Pages;
[Authorize]
public class AccountModel(OnlineVideosDataContext db,AccountManager accounts):PageModel {
 public User Account {get;set;}
 public async Task OnGetAsync(){Account=await db.Users.FindAsync(User.Identity.Name);}
 public async Task<IActionResult> OnPostPasswordAsync(string currentPassword,string newPassword,string repeatPassword) {
  try{if(newPassword!=repeatPassword)throw new ValidationException("The new passwords do not match.");await accounts.ChangePassword(User.Identity.Name,currentPassword,newPassword);await HttpContext.SignOutAsync();return RedirectToPage("/Login");}
  catch(ValidationException ex){ModelState.AddModelError("",ex.Message);await OnGetAsync();return Page();}
 }
 public async Task<IActionResult> OnPostPreferencesAsync(string address,bool changes,bool packages,bool reports,bool own) {
  try{await accounts.Preferences(User.Identity.Name,address,changes,packages,reports,own);TempData["Notice"]="Preferences saved. If needed, a confirmation link has been queued for your email address.";return RedirectToPage();}
  catch(ValidationException ex){ModelState.AddModelError("",ex.Message);await OnGetAsync();return Page();}
 }
}
