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
public class ReviewsModel(OnlineVideosDataContext db,SiteReviews reviews,PackageReview packages):PageModel {
 public List<SiteChange> Changes {get;set;}=[];
 public List<PackageReview.Pending> Packages {get;set;}=[];
 public async Task OnGetAsync(){Changes=await db.SiteChanges.AsNoTracking().Where(x=>x.Status=="pending").OrderBy(x=>x.Created).Take(100).Select(x=>new SiteChange{Id=x.Id,Name=x.Name,OwnerId=x.OwnerId,Created=x.Created,Xml=x.Xml}).ToListAsync();if(Directory.Exists(packages.Root))foreach(var f in Directory.EnumerateFiles(packages.Root,"*.json").Take(100)){try{var p=System.Text.Json.JsonSerializer.Deserialize<PackageReview.Pending>(await System.IO.File.ReadAllTextAsync(f));if(p!=null)Packages.Add(p);}catch(System.Text.Json.JsonException){}}}
 public async Task<IActionResult> OnPostDecideAsync(int id,string decision){if(decision is not ("approve" or "reject"))return BadRequest();try{await reviews.Decide(User.Identity.Name,id,decision=="approve");return RedirectToPage();}catch(ValidationException ex){ModelState.AddModelError("",ex.Message);await OnGetAsync();return Page();}catch(UnauthorizedAccessException){return Forbid();}}
}
