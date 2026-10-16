using System.ComponentModel.DataAnnotations;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using WebServiceCore.Models;
using WebServiceCore.Models.Entities;
using SkiaSharp;
namespace WebServiceCore.Services;
public sealed class SiteReviews(OnlineVideosDataContext db,AccountManager accounts,PackageReview packages,Notifications mail,IWebHostEnvironment env,IImageService images) {
 static void Image(byte[] data,bool banner){if(data==null||data.Length==0)return;if(data.Length>InputRules.MaxImageBytes)throw new ValidationException("Image too large.");using var sk=SKData.CreateCopy(data);using var codec=SKCodec.Create(sk);if(codec==null||codec.EncodedFormat!=SKEncodedImageFormat.Png||codec.Info.Width>4096||codec.Info.Height>4096||codec.Info.Width<=0||codec.Info.Height<=0||codec.Info.Width!=(banner?3:1)*codec.Info.Height)throw new ValidationException("Invalid PNG dimensions.");}
 public async Task<int> Stage(User u,string name,string xml,string dll,byte[] icon,byte[] banner){
  Image(icon,false);Image(banner,true);await using var tx=await db.Database.BeginTransactionAsync();
  await db.Entry(u).ReloadAsync();
  if(db.Entry(u).State==EntityState.Detached || u.IsDisabled) throw new ValidationException("Account disabled.");
  var site=await db.Sites.FindAsync(name);
  if(!packages.IsPublisher(u) && (site==null||site.OwnerId!=u.Email)) throw new ValidationException("Publisher approval required.");
  if(site!=null && site.OwnerId!=u.Email&&!u.IsAdmin)throw new ValidationException("Only the owner or administrator can update a site.");
  if(await db.SiteChanges.CountAsync(x=>x.Status=="pending")>=100||await db.SiteChanges.CountAsync(x=>x.Status=="pending"&&x.OwnerId==u.Email)>=10)throw new ValidationException("Review queue quota reached.");
  var change=new SiteChange{Name=name,OwnerId=u.Email,Xml=xml,RequiredDll=dll,Icon=icon,Banner=banner,Created=DateTime.UtcNow};db.SiteChanges.Add(change);await db.SaveChangesAsync();
  await mail.Event("changes",change.Id.ToString(),"Site change awaiting review: "+name);await tx.CommitAsync();return change.Id;
 }
 public async Task Decide(string actor,int id,bool approve){
  await using var tx=await db.Database.BeginTransactionAsync();await accounts.Admin(actor);
  var change=await db.SiteChanges.FindAsync(id);if(change==null||change.Status!="pending")throw new ValidationException("Change is no longer pending.");
  var originals=new Dictionary<string,byte[]>();
  async Task Remember(string directory,byte[] replacement){
   if(replacement?.Length>0){var path=InputRules.AssetPath(env.WebRootPath,directory,change.Name,".png");
    if(File.Exists(path)&&new FileInfo(path).Length>InputRules.MaxDllBytes)throw new ValidationException("Existing image exceeds rollback limit.");
    originals[path]=File.Exists(path)?await File.ReadAllBytesAsync(path):null;
   }
  }
  try {
  if(approve){
   var owner=await db.Users.FindAsync(change.OwnerId);if(owner==null||owner.IsDisabled)throw new ValidationException("The submitting account is unavailable.");
   var site=await db.Sites.FindAsync(change.Name);
   if(site!=null&&site.OwnerId!=owner.Email&&!owner.IsAdmin)throw new ValidationException("Site ownership has changed.");
   if(site==null&&!packages.IsPublisher(owner))throw new ValidationException("Publisher access has been revoked.");
   if(!string.IsNullOrEmpty(change.RequiredDll)&&!await db.Dlls.AnyAsync(x=>x.Name==change.RequiredDll))throw new ValidationException("Required package is unavailable.");
   var doc=new XmlDocument{XmlResolver=null};using var reader=XmlReader.Create(new StringReader(change.Xml),new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=262144});doc.Load(reader);
   var node=(XmlElement)doc.SelectSingleNode("//Site");if(node==null||node.GetAttribute("name")!=change.Name||!InputRules.SafeName(change.Name))throw new ValidationException("Invalid site XML.");
   if(site==null){if(await db.Sites.CountAsync()>=10000)throw new ValidationException("Site quota reached.");site=new Site{Name=change.Name,OwnerId=owner.Email};db.Sites.Add(site);}
   site.XML=change.Xml;site.Language=node.GetAttribute("lang");site.Description=node.SelectSingleNode("Description")?.InnerText??"";site.IsAdult=bool.TryParse(node.GetAttribute("agecheck"),out var adult)&&adult;site.DllId=change.RequiredDll;site.LastUpdated=DateTime.UtcNow;node.SetAttribute("lastUpdated",site.LastUpdated.ToString("o"));site.XML=doc.OuterXml;site.State=SiteState.Working;
   await Remember("Icons",change.Icon);await Remember("Banners",change.Banner);
   if(change.Icon?.Length>0&&!((await images.TrySaveIcon(change.Icon,InputRules.AssetPath(env.WebRootPath,"Icons",change.Name,".png"))).StartsWith("Icon saved!",StringComparison.Ordinal)))throw new ValidationException("Icon could not be saved.");
   if(change.Banner?.Length>0&&!((await images.TrySaveBanner(change.Banner,InputRules.AssetPath(env.WebRootPath,"Banners",change.Name,".png"))).StartsWith("Banner saved!",StringComparison.Ordinal)))throw new ValidationException("Banner could not be saved.");
  }
  change.Status=approve?"approved":"rejected";change.Icon=null;change.Banner=null;
  await db.SaveChangesAsync();await mail.Event("own","site:"+id,"Site change "+change.Status+": "+change.Name,change.OwnerId);await tx.CommitAsync();
  }catch {
   foreach(var original in originals){
    if(original.Value==null){if(File.Exists(original.Key))File.Delete(original.Key);}
    else{var temporary=original.Key+"."+Guid.NewGuid().ToString("N")+".tmp";await File.WriteAllBytesAsync(temporary,original.Value);File.Move(temporary,original.Key,true);}
   }
   throw;
  }
 }
}
