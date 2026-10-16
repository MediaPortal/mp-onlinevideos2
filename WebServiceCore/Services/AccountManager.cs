using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;
namespace WebServiceCore.Services;
public sealed class AccountManager(OnlineVideosDataContext db, Notifications mail, ILogger<AccountManager> log) {
 public static bool ValidAddress(string value) => value?.Length <= 254 && MailAddress.TryCreate(value, out var parsed) && parsed.Address == value && !value.Contains('\r') && !value.Contains('\n');
 public static void Password(string value) { if(value == null || value.Length < 14 || value.Length > 256) throw new ValidationException("Use a password of 14 to 256 characters."); }
 async Task<User> Current(string id) {
  var u=await db.Users.FindAsync(id);
  if(u==null) return null;
  await db.Entry(u).ReloadAsync();
  return db.Entry(u).State==EntityState.Detached?null:u;
 }
 public async Task<User> Admin(string actor) {
  var a=await Current(actor);
  if(a==null || !a.IsAdmin || a.IsDisabled) throw new UnauthorizedAccessException();return a;
 }
 public async Task Invite(string actor,string name,string address) {
  await using var tx=await db.Database.BeginTransactionAsync(); await Admin(actor);
  if(string.IsNullOrWhiteSpace(name) || name.Length>100 || name.StartsWith("__deleted__",StringComparison.OrdinalIgnoreCase) || name.Any(char.IsControl) || !ValidAddress(address)) throw new ValidationException("Enter a valid login name and email address.");
  if(await db.Users.AnyAsync(u=>u.Email.ToUpper()==name.ToUpper())) throw new ValidationException("This login name is already in use.");
  var u=new User { Email=name, ContactEmail=address, IsDisabled=true };
  u.Password=Credentials.Hash(u,Convert.ToHexString(RandomNumberGenerator.GetBytes(32))); db.Users.Add(u);
  await Token(u,address,"invite"); await db.SaveChangesAsync(); await tx.CommitAsync(); log.LogInformation("Administrator {Actor} invited account {Target}",actor,name);
 }
 public async Task Edit(string actor,string target,string role,bool disabled) {
  if(role is not ("user" or "publisher" or "admin")) throw new ValidationException("Invalid role.");
  await using var tx=await db.Database.BeginTransactionAsync(); await Admin(actor);
  var u=await Current(target) ?? throw new ValidationException("Account not found.");
  if(string.Equals(actor,target,StringComparison.OrdinalIgnoreCase) && (disabled || role!="admin")) throw new ValidationException("You cannot disable or demote your own account.");
  if(u.IsAdmin && !u.IsDisabled && (role!="admin" || disabled) && await db.Users.CountAsync(x=>x.IsAdmin && !x.IsDisabled)<=1) throw new ValidationException("The last active administrator must remain.");
  if(disabled) await db.AccountTokens.Where(t=>t.UserId==target).ExecuteDeleteAsync();
  if(u.IsAdmin!=(role=="admin") || u.IsPublisher!=(role=="publisher") || u.IsDisabled!=disabled) u.SecurityVersion=Guid.NewGuid().ToString("N");
  u.IsAdmin=role=="admin"; u.IsPublisher=role=="publisher"; u.IsDisabled=disabled;
  await db.SaveChangesAsync(); await tx.CommitAsync(); log.LogInformation("Administrator {Actor} changed account {Target}",actor,target);
 }
 public async Task Delete(string actor,string target,string confirmation) {
  await using var tx=await db.Database.BeginTransactionAsync(); await Admin(actor);
  var u=await Current(target) ?? throw new ValidationException("Account not found.");
  if(string.Equals(actor,target,StringComparison.OrdinalIgnoreCase) || confirmation!=target) throw new ValidationException("Enter the exact login name; you cannot delete yourself.");
  if(u.IsAdmin && !u.IsDisabled && await db.Users.CountAsync(x=>x.IsAdmin&&!x.IsDisabled)<=1) throw new ValidationException("The last active administrator must remain.");
  var sites=await db.Sites.Where(x=>x.OwnerId==target).ToListAsync();var dlls=await db.Dlls.Where(x=>x.OwnerId==target).ToListAsync();
  if(sites.Count+dlls.Count>0) {
   var tomb=new User{Email="__deleted__"+Guid.NewGuid().ToString("N"),IsDisabled=true};tomb.Password=Credentials.Hash(tomb,Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));db.Users.Add(tomb);
   foreach(var s in sites) s.Owner=tomb; foreach(var d in dlls) d.Owner=tomb;
  }
  await db.SiteChanges.Where(x=>x.OwnerId==target&&x.Status=="pending").ExecuteDeleteAsync();
  await db.AccountTokens.Where(x=>x.UserId==target).ExecuteDeleteAsync();
  await db.MailJobs.Where(x=>x.UserId==target).ExecuteDeleteAsync();
  db.Users.Remove(u);await db.SaveChangesAsync();await tx.CommitAsync();log.LogInformation("Administrator {Actor} deleted account {Target}; contributions retained",actor,target);
 }
 public async Task SetEmail(string actor,string target,string address) {
  if(!ValidAddress(address)) throw new ValidationException("Enter a valid email address.");
  await using var tx=await db.Database.BeginTransactionAsync();await Admin(actor);
  var u=await Current(target) ?? throw new ValidationException("Account not found.");
  if(u.Email.StartsWith("__deleted__",StringComparison.Ordinal)) throw new ValidationException("Deleted account.");
  if(string.Equals(u.ContactEmail,address,StringComparison.OrdinalIgnoreCase)) return;
  u.ContactEmail=address;u.EmailConfirmed=false;
  await db.AccountTokens.Where(t=>t.UserId==target).ExecuteDeleteAsync();
  await db.MailJobs.Where(j=>j.UserId==target&&j.Status=="pending").ExecuteDeleteAsync();
  if(!u.IsDisabled) await Token(u,address,"verify");
  await db.SaveChangesAsync();await tx.CommitAsync();log.LogInformation("Administrator {Actor} changed email for account {Target}",actor,target);
 }
 public async Task ChangePassword(string actor,string oldPassword,string newPassword) {
  Password(newPassword);await using var tx=await db.Database.BeginTransactionAsync();var u=await Current(actor);
  if(u==null || u.IsDisabled || !Credentials.Verify(u,oldPassword)) throw new ValidationException("The current password is incorrect.");
  u.Password=Credentials.Hash(u,newPassword);await db.SaveChangesAsync();await tx.CommitAsync();
 }
 public async Task Preferences(string actor,string address,bool changes,bool packages,bool reports,bool own) {
  if(!ValidAddress(address)) throw new ValidationException("Enter a valid email address.");
  await using var tx=await db.Database.BeginTransactionAsync();var u=await Current(actor);
  if(u==null||u.IsDisabled) throw new UnauthorizedAccessException();
  if(!u.IsAdmin&&(changes||packages||reports)) throw new ValidationException("Administrator notifications require an administrator account.");
  u.NotifyChanges=changes;u.NotifyPackages=packages;u.NotifyReports=reports;u.NotifyOwn=own;
  if(!string.Equals(address,u.ContactEmail,StringComparison.OrdinalIgnoreCase) || !u.EmailConfirmed) {
   u.ContactEmail=address;u.EmailConfirmed=false;await Token(u,address,"verify");
  }
  await db.SaveChangesAsync();await tx.CommitAsync();
 }
 internal static string Digest(string token)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
 async Task Token(User u,string address,string kind) {
  await db.AccountTokens.Where(t=>t.UserId==u.Email).ExecuteDeleteAsync();
  await db.MailJobs.Where(j=>j.UserId==u.Email&&j.Kind=="account"&&j.Status=="pending").ExecuteDeleteAsync();
  var raw=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
  db.AccountTokens.Add(new AccountToken{Id=Digest(raw),UserId=u.Email,Address=address,Kind=kind,Expires=DateTime.UtcNow.AddHours(24)});
  await mail.Direct(u.Email,address,"account",Guid.NewGuid().ToString("N"),kind=="invite"?"Your OnlineVideos invitation":"Confirm your OnlineVideos email", "Open this link within 24 hours:\n"+mail.BaseUrl+"/Activate?token="+raw+"\nIf you did not request this, ignore this email.");
 }
 public async Task Activate(string token,string password) {
  if(token==null || token.Length!=64 || !token.All(Uri.IsHexDigit)) throw new ValidationException("Invalid or expired link.");
  await using var tx=await db.Database.BeginTransactionAsync();var t=await db.AccountTokens.FindAsync(Digest(token));
  if(t==null||t.Expires<DateTime.UtcNow) throw new ValidationException("Invalid or expired link.");
  var u=await Current(t.UserId) ?? throw new ValidationException("Invalid or expired link.");
  if(u.ContactEmail!=t.Address) throw new ValidationException("Invalid or expired link.");
  if(t.Kind=="invite") {Password(password);u.Password=Credentials.Hash(u,password);u.IsDisabled=false;}
  else if(u.IsDisabled) throw new ValidationException("Account disabled.");
  if(t.Kind=="invite") u.SecurityVersion=Guid.NewGuid().ToString("N");
  u.EmailConfirmed=true;db.AccountTokens.Remove(t);await db.SaveChangesAsync();await tx.CommitAsync();
 }
}
