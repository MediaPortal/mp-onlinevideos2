using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebServiceCore.Models;
namespace WebServiceCore.Services;
public static class AccountSchema {
 public static async Task Upgrade(OnlineVideosDataContext db) {
  await db.Database.OpenConnectionAsync();
  using var tx = await db.Database.BeginTransactionAsync();
  var columns = new HashSet<string>();
  using (var cmd=db.Database.GetDbConnection().CreateCommand()) {
   cmd.Transaction=tx.GetDbTransaction(); cmd.CommandText="PRAGMA table_info(Users)";
   using var reader=await cmd.ExecuteReaderAsync(); while(await reader.ReadAsync()) columns.Add(reader.GetString(1));
  }
  foreach(var name in new[]{"IsPublisher","IsDisabled","EmailConfirmed","NotifyChanges","NotifyPackages","NotifyReports","NotifyOwn"})
   if(!columns.Contains(name)) await db.Database.ExecuteSqlRawAsync($"ALTER TABLE Users ADD COLUMN {name} INTEGER NOT NULL DEFAULT 0");
  if(!columns.Contains("ContactEmail")) await db.Database.ExecuteSqlRawAsync("ALTER TABLE Users ADD COLUMN ContactEmail TEXT NOT NULL DEFAULT ''");
  await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS AccountTokens (Id TEXT NOT NULL PRIMARY KEY, UserId TEXT NOT NULL, Address TEXT NOT NULL, Kind TEXT NOT NULL, Expires TEXT NOT NULL)");
  await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS MailJobs (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, UserId TEXT NOT NULL, Address TEXT NOT NULL, Kind TEXT NOT NULL, EventKey TEXT NOT NULL, Subject TEXT NOT NULL, Body TEXT NOT NULL, Due TEXT NOT NULL, Attempts INTEGER NOT NULL, Status TEXT NOT NULL)");
  await db.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS IX_MailJobs_Event ON MailJobs(EventKey, Address)");
  await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS SiteChanges (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, OwnerId TEXT NOT NULL, Xml TEXT NOT NULL, RequiredDll TEXT NULL, Icon BLOB NULL, Banner BLOB NULL, Status TEXT NOT NULL, Created TEXT NOT NULL)");
  await tx.CommitAsync();
 }
}
