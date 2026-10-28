using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
static class OnlineSingleFile {
    public static string Prepare(){
        string root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BT3-SparkingControls","online-test-c77c984a6dad-1.1-beta");Directory.CreateDirectory(root);
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
