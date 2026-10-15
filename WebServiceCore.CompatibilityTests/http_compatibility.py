"""Run on the isolated Linux test host; never point mutation tests at production."""
import atexit, base64, hashlib, json, sqlite3, struct, urllib.request, urllib.error, urllib.parse, zlib
from pathlib import Path

OLD='http://127.0.0.1:5000'
NEW='http://127.0.0.1:5086'
API='/api/OnlineVideos/'
checks=[]
def cleanup():
    root=Path('/srv/onlinevideos-net10-test')
    with sqlite3.connect(root/'test.db3') as db:
        db.execute('DELETE FROM Sites WHERE Name=?',('Net10CompatibilityTest',))
        db.execute('DELETE FROM Dlls WHERE Name=?',('Net10CompatibilityTest',))
        db.execute('DELETE FROM Users WHERE Email=?',('net10-compatibility@example.invalid',))
    for sub,extension in [('Icons','.png'),('Banners','.png'),('Dlls','.dll')]:
        (root/'wwwroot'/sub/('Net10CompatibilityTest'+extension)).unlink(missing_ok=True)
cleanup()
atexit.register(cleanup)
def request(base,path,body=None):
    headers={} if body is None else {'Content-Type':'application/json'}
    data=None if body is None else json.dumps(body).encode()
    try:
        with urllib.request.urlopen(urllib.request.Request(base+path,data=data,headers=headers),timeout=30) as r:return r.status,r.read()
    except urllib.error.HTTPError as e:return e.code,e.read()
def check(ok,label):
    assert ok,label
    checks.append(label)
def canonical(data):
    value=json.loads(data)
    if isinstance(value,dict) and 'status' in value and 'title' in value:
        # Framework-generated trace IDs and RFC links differ across runtimes.
        value={k:v for k,v in value.items() if k not in ('traceId','type')}
    if isinstance(value,list):value=sorted(value,key=lambda x:json.dumps(x,sort_keys=True))
    return value
def compare(path,label):
    a=request(OLD,path);b=request(NEW,path)
    check(a[0]==b[0],label+' status')
    try:equal=canonical(a[1])==canonical(b[1])
    except (ValueError,UnicodeDecodeError):equal=a[1]==b[1]
    check(equal,label+' body')
    return b
compare(API+'GetSitesOverview','Site overview')
compare(API+'GetDllsOverview','DLL overview')
for path in ['GetSiteXml?siteName=__compatibility_missing__','GetDll?name=__compatibility_missing__','GetReports?siteName=__compatibility_missing__','GetDllOwner?dllName=__compatibility_missing__']:
    compare(API+path,path.split('?')[0]+' missing')
with sqlite3.connect('/srv/onlinevideos-net10-test/test.db3') as db:
    site=db.execute('SELECT Name FROM Sites ORDER BY Name LIMIT 1').fetchone()[0]
for method in ['GetSiteXml','GetSiteIcon','GetSiteBanner','GetReports']:
    compare(API+method+'?'+urllib.parse.urlencode({'siteName':site}),method+' existing')
check(request(NEW,'/')[0]==200,'Razor home')
check(request(NEW,'/api/OnlineVideos/SubmitDll',{'Email':'missing@example.invalid','Password':'invalid','Name':'test','Data':'AQID'})[0]==400,'Unknown upload account rejected')
name='Net10CompatibilityTest'
email='net10-compatibility@example.invalid'
password='isolated-test-only'
with sqlite3.connect('/srv/onlinevideos-net10-test/test.db3') as db:
    db.execute('INSERT OR REPLACE INTO Users (Email,Password,IsAdmin) VALUES (?,?,0)',(email,password))
data=b'MZ isolated compatibility payload - never executed'
payload={'Email':email,'Password':password,'Name':name,'Data':base64.b64encode(data).decode()}
check(request(NEW,API+'SubmitDll',dict(payload,Password='wrong'))[0]==401,'Wrong upload password rejected')
check(request(NEW,API+'SubmitDll',payload)[0]==200,'DLL upload to copy')
status,body=request(NEW,API+'GetDll?name='+name)
check(status==200 and base64.b64decode(json.loads(body))==data,'DLL download bytes')
def png(w,h):
    def chunk(kind,data):return struct.pack('>I',len(data))+kind+data+struct.pack('>I',zlib.crc32(kind+data)&0xffffffff)
    return b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',w,h,8,2,0,0,0))+chunk(b'IDAT',zlib.compress((b'\x00'+b'\xff\x80\x00'*w)*h))+chunk(b'IEND',b'')
icon,banner=png(16,16),png(48,16)
site_payload={'Email':email,'Password':password,'SiteXml':f'<Site name="{name}" lang="en"><Description>Isolated compatibility test</Description></Site>','RequiredDll':name,'Icon':base64.b64encode(icon).decode(),'Banner':base64.b64encode(banner).decode()}
check(request(NEW,API+'SubmitSite',site_payload)[0]==200,'Site upload with PNGs to copy')
check(request(NEW,API+'GetSiteXml?siteName='+name)[0]==200,'Uploaded XML readable')
for method,expected in [('GetSiteIcon',icon),('GetSiteBanner',banner)]:
    status,body=request(NEW,API+method+'?siteName='+name)
    check(status==200 and base64.b64decode(json.loads(body))==expected,method+' bytes')
    check(request(NEW,API+method+'IfChanged?'+urllib.parse.urlencode({'siteName':name,'md5':hashlib.md5(expected).hexdigest()}))[0]==204,method+' unchanged 204')
check(request(NEW,API+'SubmitSite',site_payload)[0]==200,'Existing site update')
root=Path('/var/backups/onlinevideos-net10-test-20261015')
check(root.joinpath('production-dll.sha256').read_text()==hashlib.sha256(Path('/var/www/publish/WebServiceCore.dll').read_bytes()).hexdigest(),'Production binary unchanged')
print(json.dumps({'passed':len(checks),'checks':checks},indent=2))
