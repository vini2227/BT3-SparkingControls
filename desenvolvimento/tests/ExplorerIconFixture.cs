using System;using System.IO;using System.Drawing;using System.Runtime.InteropServices;
static class BT3RefreshIcon {
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct SHFILEINFO {public IntPtr hIcon;public int iIcon;public uint attributes;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]public string display;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=80)]public string type;}
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)]static extern IntPtr SHGetFileInfo(string path,uint attributes,out SHFILEINFO info,uint size,uint flags);
 [DllImport("shell32.dll",CharSet=CharSet.Unicode,EntryPoint="SHUpdateImageW")]static extern void SHUpdateImage(string path,int iconIndex,uint flags,int imageIndex);
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)]static extern void SHChangeNotify(uint change,uint flags,string first,IntPtr second);
 [DllImport("user32.dll")]static extern bool DestroyIcon(IntPtr icon);
 static void Save(string exe,string output,bool small){SHFILEINFO info;SHGetFileInfo(exe,0,out info,(uint)Marshal.SizeOf(typeof(SHFILEINFO)),0x100u|(small?1u:0));if(info.hIcon==IntPtr.Zero)throw new IOException("Could not retrieve Shell icon.");try{using(var copy=(Icon)Icon.FromHandle(info.hIcon).Clone())using(var bitmap=copy.ToBitmap())bitmap.Save(output);}finally{DestroyIcon(info.hIcon);}}
 [STAThread]static int Main(string[] args){
  string exe=Path.GetFullPath(args[0]),folder=Path.GetFullPath(args[1]);Directory.CreateDirectory(folder);
  Save(exe,Path.Combine(folder,"shell-icon-before.png"),true);Save(exe,Path.Combine(folder,"shell-icon-large-before.png"),false);
  SHFILEINFO system,location;if(SHGetFileInfo(exe,0,out system,(uint)Marshal.SizeOf(typeof(SHFILEINFO)),0x4000)==IntPtr.Zero)throw new IOException("No system image index.");
  SHGetFileInfo(exe,0,out location,(uint)Marshal.SizeOf(typeof(SHFILEINFO)),0x1000);
  string iconFile=string.IsNullOrEmpty(location.display)?exe:location.display;
  SHUpdateImage(iconFile,location.iIcon,0,system.iIcon);SHChangeNotify(0x2000,0x1005,exe,IntPtr.Zero);SHChangeNotify(0x1000,0x1005,Path.GetDirectoryName(exe),IntPtr.Zero);
  Save(exe,Path.Combine(folder,"shell-icon-after.png"),true);Save(exe,Path.Combine(folder,"shell-icon-large-after.png"),false);
  using(var icon=new Icon(args[2],new Size(16,16)))using(var bmp=icon.ToBitmap())bmp.Save(Path.Combine(folder,"embedded-icon-expected.png"));
  File.WriteAllText(Path.Combine(folder,"icon-refresh-report.txt"),"System image index: "+system.iIcon+"\r\nSource: "+iconFile+"; resource index: "+location.iIcon+"\r\nSHUpdateImage and synchronous item/directory notifications sent. Before/after Shell icons saved.\r\n");return 0;
 }
}
