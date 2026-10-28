from pathlib import Path
import shutil,json,hashlib,subprocess,sys,re
r=Path(__file__).resolve().parents[2];out=r/'desenvolvimento';src=out/'src'
bundle='''using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
static class OnlineSingleFile {
    public static string Prepare(){
        string root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BT3-SparkingControls","online-test-@VERSION@");Directory.CreateDirectory(root);
        var assembly=Assembly.GetExecutingAssembly();
        foreach(string name in assembly.GetManifestResourceNames()){
            if(!name.StartsWith("OnlineBundle/",StringComparison.Ordinal))continue;
            string path=Path.GetFullPath(Path.Combine(root,name.Substring(13).Replace('/',Path.DirectorySeparatorChar)));
            if(!path.StartsWith(Path.GetFullPath(root)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new IOException("Invalid resource path.");
            byte[] data;using(var input=assembly.GetManifestResourceStream(name))using(var buffer=new MemoryStream()){input.CopyTo(buffer);data=buffer.ToArray();}
            bool same=false;if(File.Exists(path))using(var hash=SHA256.Create())same=Convert.ToBase64String(hash.ComputeHash(File.ReadAllBytes(path)))==Convert.ToBase64String(hash.ComputeHash(data));
            if(!same){Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,data);}
        }
        return root;
    }
}
'''
version=hashlib.sha256((out/'SparkingControls.payload').read_bytes()).hexdigest()[:12]+'-'+re.search(r'AssemblyInformationalVersion\("([^"]+)"\)',(src/'AssemblyInfo.cs').read_text(encoding='utf-8-sig')).group(1);(src/'OnlineSingleFile.cs').write_text(bundle.replace('@VERSION@',version),encoding='utf-8-sig')

if not (out/'assets').is_dir():raise FileNotFoundError('Missing desenvolvimento/assets')
shutil.copy2(r/'LICENSE-AGPL-3.0.txt',out/'LICENSE-AGPL-3.0.txt')
files=['AssemblyInfo.cs','SparkingControls.cs','GameButtonIcons.cs','Transformations.cs','UiLanguage.cs','OnlineSingleFile.cs','GameSession.cs','AdaptiveCompatibility.cs','VariantSupport.cs']
resources={n:out/n for n in ['profile.json','SparkingControls.payload','LICENSE-AGPL-3.0.txt','compatibility.json','compatibility-legacy.json']}
for p in (out/'variants').rglob('*'):
 if p.is_file():resources[p.relative_to(out).as_posix()]=p
for p in (out/'assets').iterdir():
 if p.is_file():resources['assets/'+p.name]=p
for p in src.iterdir():
 if p.is_file():resources['src/'+p.name]=p
subprocess.run([sys.executable,str(out/'tools/build-icon-windows.py')],check=True,cwd=r)
csc=r'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe';dest=Path(sys.argv[1]) if len(sys.argv)>1 else r/'BT3-SparkingControls.exe'
args=[csc,'/nologo','/target:winexe','/platform:x64','/optimize+','/out:'+str(dest),'/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.Web.Extensions.dll','/win32icon:'+str(out/'assets/dragon-ball-three-stars.ico')]
args+=['/resource:'+str(p)+',OnlineBundle/'+n for n,p in resources.items()];args+=[str(src/n) for n in files]
subprocess.run(args,check=True,cwd=r)
(out/'build-info.json').write_text(json.dumps({'root':str(Path.home()/'AppData/Local/BT3-SparkingControls'/('online-test-'+version)),'exe':str(dest),'sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'game_sha256':json.loads((out/'profile.json').read_text())['game_sha256']},indent=2),encoding='utf-8')
print('Built separate Windows ONLINE TEST executable:',dest)
