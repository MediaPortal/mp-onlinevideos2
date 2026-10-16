namespace WebServiceCore.Models.Entities;
public class AccountToken {
 public string Id { get; set; } = "";
 public string UserId { get; set; } = "";
 public string Address { get; set; } = "";
 public string Kind { get; set; } = "";
 public DateTime Expires { get; set; }
}
public class MailJob {
 public int Id { get; set; }
 public string UserId { get; set; } = "";
 public string Address { get; set; } = "";
 public string Kind { get; set; } = "";
 public string EventKey { get; set; } = "";
 public string Subject { get; set; } = "";
 public string Body { get; set; } = "";
 public DateTime Due { get; set; }
 public int Attempts { get; set; }
 public string Status { get; set; } = "pending";
}

public class SiteChange {
 public int Id {get;set;}
 public string Name {get;set;}="";
 public string OwnerId {get;set;}="";
 public string Xml {get;set;}="";
 public string RequiredDll {get;set;}
 public byte[] Icon {get;set;}
 public byte[] Banner {get;set;}
 public string Status {get;set;}="pending";
 public DateTime Created {get;set;}
}
