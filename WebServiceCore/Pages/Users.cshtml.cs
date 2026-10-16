using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;
using WebServiceCore.Services;
namespace WebServiceCore.Pages;
[Authorize(Roles="admin")]
public class UsersModel(OnlineVideosDataContext db,AccountManager accounts):PageModel {
 public List<User> Accounts {get;set;}=[];
 [BindProperty(SupportsGet=true)] public string Search {get;set;}="";
 [BindProperty(SupportsGet=true)] public int PageNumber {get;set;}=1;
 public bool More {get;set;}
 public async Task OnGetAsync(){await Load();}
 async Task Load(){PageNumber=Math.Clamp(PageNumber,1,10000);var q=db.Users.AsNoTracking().Where(u=>!u.Email.StartsWith("__deleted__"));if(!string.IsNullOrWhiteSpace(Search)){Search=Search[..Math.Min(100,Search.Length)];q=q.Where(u=>u.Email.Contains(Search)||u.ContactEmail.Contains(Search));}Accounts=await q.OrderBy(u=>u.Email).Skip((PageNumber-1)*50).Take(51).Select(u=>new User{Email=u.Email,ContactEmail=u.ContactEmail,EmailConfirmed=u.EmailConfirmed,IsAdmin=u.IsAdmin,IsPublisher=u.IsPublisher,IsDisabled=u.IsDisabled}).ToListAsync();More=Accounts.Count>50;Accounts=Accounts.Take(50).ToList();}
 public Task<IActionResult> OnPostEmailAsync(string target,string address)=>Run(()=>accounts.SetEmail(User.Identity.Name,target,address));
 public Task<IActionResult> OnPostInviteAsync(string login,string address)=>Run(()=>accounts.Invite(User.Identity.Name,login,address));
 public Task<IActionResult> OnPostEditAsync(string target,string role,bool disabled)=>Run(()=>accounts.Edit(User.Identity.Name,target,role,disabled));
 public Task<IActionResult> OnPostDeleteAsync(string target,string confirmation)=>Run(()=>accounts.Delete(User.Identity.Name,target,confirmation));
 async Task<IActionResult> Run(Func<Task> action){try{await action();TempData["Notice"]="Account updated.";return RedirectToPage();}catch(ValidationException ex){ModelState.AddModelError("",ex.Message);await Load();return Page();}catch(UnauthorizedAccessException){return Forbid();}}
}
