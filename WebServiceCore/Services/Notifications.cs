using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;
namespace WebServiceCore.Services;
public sealed class Notifications(OnlineVideosDataContext db,IConfiguration config) {
 public string BaseUrl {
  get {var value=config["Mail:PublicUrl"]?.TrimEnd('/');if(!Uri.TryCreate(value,UriKind.Absolute,out var uri)||uri.Scheme!="https") throw new System.ComponentModel.DataAnnotations.ValidationException("The operator must configure an HTTPS Mail:PublicUrl before sending invitations.");return value;}
 }
 public async Task Direct(string user,string address,string kind,string key,string subject,string body) {
  if(!AccountManager.ValidAddress(address)) return;
  if(await db.MailJobs.CountAsync(x=>x.Status=="pending")>=10000) throw new System.ComponentModel.DataAnnotations.ValidationException("Mail queue is full.");
  if(db.MailJobs.Local.Any(x=>x.EventKey==key&&string.Equals(x.Address,address,StringComparison.OrdinalIgnoreCase)) || await db.MailJobs.AnyAsync(x=>x.EventKey==key&&x.Address==address)) return;
  db.MailJobs.Add(new MailJob{UserId=user,Address=address,Kind=kind,EventKey=key,Subject=subject,Body=body,Due=DateTime.UtcNow});
 }
 public async Task Event(string kind,string key,string label,string owner=null) {
  if(string.IsNullOrEmpty(config["Mail:PublicUrl"])) return;
  var users=await db.Users.Where(u=>!u.IsDisabled&&u.EmailConfirmed).ToListAsync();
  foreach(var u in users) {
   var eligible=u.IsAdmin && (kind=="changes"&&u.NotifyChanges || kind=="packages"&&u.NotifyPackages || kind=="reports"&&u.NotifyReports) || kind=="own"&&u.Email==owner&&u.NotifyOwn;
   if(!eligible) continue;
   var eventKey=kind=="reports"?"reports:"+DateTime.UtcNow.ToString("yyyyMMddHH"):kind+":"+key;
   var text=kind=="reports"?"New reports are available. This message summarizes the current hour; open the website to review them.":label;
   await Direct(u.Email,u.ContactEmail.ToLowerInvariant(),kind,eventKey,"OnlineVideos: "+(kind=="reports"?"report summary":label),text+"\n\n"+BaseUrl);
   if(kind=="reports") foreach(var job in db.MailJobs.Local.Where(j=>j.EventKey==eventKey)) job.Due=new DateTime(DateTime.UtcNow.Year,DateTime.UtcNow.Month,DateTime.UtcNow.Day,DateTime.UtcNow.Hour,0,0,DateTimeKind.Utc).AddHours(1);
  }
  await db.SaveChangesAsync();
 }
 public async Task Deliver(CancellationToken ct) {
  if(!config.GetValue<bool>("Mail:Enabled")) return;
  var from=config["Mail:From"];
  if(!AccountManager.ValidAddress(from)) return;
  var jobs=await db.MailJobs.Where(j=>j.Status=="pending"&&j.Due<=DateTime.UtcNow&&j.Attempts<5).OrderBy(j=>j.Id).Take(20).ToListAsync();
  foreach(var j in jobs) {
   var u=await db.Users.FindAsync(new object[]{j.UserId},ct);
   var allowed=u!=null && string.Equals(u.ContactEmail,j.Address,StringComparison.OrdinalIgnoreCase) && (j.Kind=="account" || !u.IsDisabled&&u.EmailConfirmed&&(j.Kind=="own"?u.NotifyOwn:u.IsAdmin&&(j.Kind=="changes"&&u.NotifyChanges||j.Kind=="packages"&&u.NotifyPackages||j.Kind=="reports"&&u.NotifyReports)));
   if(j.Kind=="account") allowed=allowed&&await db.AccountTokens.AnyAsync(t=>t.UserId==j.UserId&&t.Expires>DateTime.UtcNow,ct);
   if(allowed && j.Kind=="changes" && int.TryParse(j.EventKey.Split(':').Last(),out var changeId)) allowed=await db.SiteChanges.AnyAsync(x=>x.Id==changeId&&x.Status=="pending",ct);
   if(!allowed) {j.Status="cancelled";j.Body="";await db.SaveChangesAsync(ct);continue;}
   j.Attempts++;j.Due=DateTime.UtcNow.AddMinutes(5*j.Attempts);await db.SaveChangesAsync(ct);
   try {
    using var message=new MailMessage(from,j.Address,j.Subject,j.Body);
    using var smtp=new SmtpClient(config["Mail:Host"]??"127.0.0.1",config.GetValue("Mail:Port",25)){EnableSsl=config.GetValue<bool>("Mail:StartTls"),Timeout=15000};
    if(!string.IsNullOrEmpty(config["Mail:Username"])) smtp.Credentials=new System.Net.NetworkCredential(config["Mail:Username"],config["Mail:Password"]);
    await smtp.SendMailAsync(message,ct);j.Status="sent";j.Body="";
   }catch(SmtpException){if(j.Attempts>=5){j.Status="failed";j.Body="";}}
   await db.SaveChangesAsync(ct);
  }
  await db.MailJobs.Where(j=>j.Status!="pending"&&j.Due<DateTime.UtcNow.AddDays(-30)).ExecuteDeleteAsync(ct);
  await db.AccountTokens.Where(t=>t.Expires<DateTime.UtcNow).ExecuteDeleteAsync(ct);
 }
}
public sealed class MailWorker(IServiceScopeFactory scopes,ILogger<MailWorker> log):BackgroundService {
 protected override async Task ExecuteAsync(CancellationToken ct) {
  using var timer=new PeriodicTimer(TimeSpan.FromMinutes(1));
  while(await timer.WaitForNextTickAsync(ct)) {
   try{using var scope=scopes.CreateScope();await scope.ServiceProvider.GetRequiredService<Notifications>().Deliver(ct);}
   catch(OperationCanceledException) when(ct.IsCancellationRequested){break;}
   catch(Exception ex){log.LogError("Mail queue processing failed: {Type}",ex.GetType().Name);}
  }
 }
}
