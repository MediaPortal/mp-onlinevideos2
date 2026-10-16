using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebServiceCore.Services;
namespace WebServiceCore.Pages;
public class ActivateModel(AccountManager accounts):PageModel {
 [BindProperty(SupportsGet=true)] public string Token {get;set;}="";
 public bool Done {get;set;}
 public void OnGet(){Response.Headers["Referrer-Policy"]="no-referrer";Response.Headers.CacheControl="no-store";}
 public async Task<IActionResult> OnPostAsync(string password,string repeatPassword) {
  OnGet();try{if(password!=repeatPassword)throw new ValidationException("The passwords do not match.");await accounts.Activate(Token,password);Done=true;Token="";return Page();}
  catch(ValidationException ex){ModelState.AddModelError("",ex.Message);return Page();}
 }
}
