using System;using System.IO;using System.Drawing;using System.Reflection;using System.Windows.Forms;
static class HudRecoveryFixture {
 sealed class LayeredProbe:Form {protected override CreateParams CreateParams{get{var p=base.CreateParams;p.ExStyle|=0x80000|0x80;return p;}}}
 [STAThread]static void Main(string[] args){
  SavedSettings.OverrideDirectory=Path.Combine(Path.GetDirectoryName(args[1]),"hud-preferences-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(SavedSettings.OverrideDirectory);string modeFile=PanelPlacement.FilePath,attackFile=HudPreferences.FilePath;byte[] modeData=File.Exists(modeFile)?File.ReadAllBytes(modeFile):null,attackData=File.Exists(attackFile)?File.ReadAllBytes(attackFile):null;
  var type=typeof(Overlay);var draw=type.GetMethod("DrawScene",BindingFlags.Instance|BindingFlags.NonPublic);var refresh=type.GetMethod("RefreshSavedHudPreferences",BindingFlags.Instance|BindingFlags.NonPublic);var next=type.GetField("nextPreferenceRead",BindingFlags.Instance|BindingFlags.NonPublic);var bounds=type.GetField("panelBounds",BindingFlags.Instance|BindingFlags.NonPublic);int count=0;
  try{HudPreferences.SetShowAttacks(true,true);using(var overlay=new Overlay(null,null,true,args[0]))using(var probe=new LayeredProbe()){
   probe.ShowInTaskbar=false;
   foreach(var size in new[]{new Size(640,480),new Size(1920,1080),new Size(3440,1440)}){
    float classicLeft=0;overlay.SetPreviewSize(size.Width,size.Height);overlay.SetPreviewAspect(size.Width==640?1333:0);
    foreach(var mode in new[]{PanelLayoutMode.Classic,PanelLayoutMode.Expanded,PanelLayoutMode.Centered}){
     PanelPlacement.Set(mode,true);PanelPlacement.Set(PanelLayoutMode.Classic,false);next.SetValue(overlay,DateTime.MinValue);refresh.Invoke(overlay,null);if(PanelPlacement.Mode!=mode)throw new Exception("Saved layout was not applied: "+mode);
     using(var bitmap=new Bitmap(size.Width,size.Height,System.Drawing.Imaging.PixelFormat.Format32bppPArgb))using(var g=Graphics.FromImage(bitmap)){
      g.Clear(Color.Transparent);draw.Invoke(overlay,new object[]{g});var box=((RectangleF[])bounds.GetValue(overlay))[0];int visible=0;for(int y=0;y<bitmap.Height;y+=4)for(int x=0;x<bitmap.Width;x+=4)if(bitmap.GetPixel(x,y).A!=0)visible++;
      if(visible<20||box.Width<=0||box.Left<0||box.Right>size.Width)throw new Exception("Empty/offscreen layout "+mode);
      if(mode==PanelLayoutMode.Classic)classicLeft=box.Left;else if(mode==PanelLayoutMode.Expanded&&size.Width>640&&box.Left>=classicLeft)throw new Exception("Expanded layout did not move outward");
      probe.Bounds=new Rectangle(10,10,size.Width,size.Height);AlphaHud.Present(probe,bitmap);AlphaHud.Present(probe,bitmap);count++;
     }
    }
   }
  }}finally{if(modeData==null){if(File.Exists(modeFile))File.Delete(modeFile);}else File.WriteAllBytes(modeFile,modeData);if(attackData==null){if(File.Exists(attackFile))File.Delete(attackFile);}else File.WriteAllBytes(attackFile,attackData);PanelPlacement.Load();HudPreferences.Load();}
  string report="PASS: "+count+" persisted-layout/render/Windows-layered-presentation cases; Classic->Expanded->Centered at 4:3, 16:9 and ultrawide, saved preferences restored. This is not a live game reproduction.";File.WriteAllText(args[1],report);Console.WriteLine(report);
 }
}
