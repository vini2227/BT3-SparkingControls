using System;using System.IO;using System.Collections;using System.Reflection;using System.Diagnostics;using System.Drawing;using System.Drawing.Imaging;using System.Windows.Forms;using System.Runtime.InteropServices;
static class CroppedHudFixture {
 static BindingFlags f=BindingFlags.Instance|BindingFlags.NonPublic;static int checks;
 [DllImport("user32.dll")]static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")]static extern int GetGuiResources(IntPtr h,int flags);
 static void Check(bool value,string name){checks++;if(!value)throw new Exception(name);}
 sealed class Probe:Form {protected override CreateParams CreateParams{get{var p=base.CreateParams;p.ExStyle|=0x80000|0x80;return p;}}}
 [STAThread]static void Main(string[] args){
  string output=args[1];Directory.CreateDirectory(output);SavedSettings.OverrideDirectory=Path.Combine(output,"settings");Directory.CreateDirectory(SavedSettings.OverrideDirectory);
  // Real GDI presentation: exact premultiplied pixels, stable allocation at constant size, disposal.
  using(var warm=new Probe())using(var bitmap=new Bitmap(10,10,PixelFormat.Format32bppPArgb)){warm.FormBorderStyle=FormBorderStyle.None;warm.Location=new Point(-12000,-12000);AlphaHud.Present(warm,bitmap);}
  GC.Collect();GC.WaitForPendingFinalizers();long created=AlphaHud.SurfaceCreates;int gdi0=GetGuiResources(Process.GetCurrentProcess().Handle,0);
  using(var w=new Probe())using(var image=new Bitmap(5120,1440,PixelFormat.Format32bppPArgb)){
   w.FormBorderStyle=FormBorderStyle.None;w.StartPosition=FormStartPosition.Manual;w.Location=new Point(-12000,-12000);w.CreateControl();
   using(var g=Graphics.FromImage(image)){g.Clear(Color.Transparent);using(var brush=new SolidBrush(Color.FromArgb(127,240,120,60)))g.FillRectangle(brush,100,80,400,200);}
   var region=new Rectangle(95,75,410,210);var position=new Point(-11000,-10000);long bytes=AlphaHud.CopiedBytes;
   for(int i=0;i<5000;i++)AlphaHud.Present(w,image,region,position);
   Check(AlphaHud.SurfaceCreates==created+1,"Same-size presentations must reuse a single DIB");
   Check(w.Size==region.Size&&w.Location==position,"Native window must match cropped size and screen position: "+w.Bounds+" expected "+new Rectangle(position,region.Size));
   Check(AlphaHud.CopiedBytes-bytes==(long)5000*410*210*4,"Copy only visible region, not desktop");
   var pool=(IDictionary)typeof(AlphaHud).GetField("surfaces",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
   var surface=pool[w];var pointer=(IntPtr)surface.GetType().GetField("Bits").GetValue(surface);
   var data=image.LockBits(region,ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);
   try{var actual=new byte[region.Width*4];var expected=new byte[actual.Length];for(int y=0;y<region.Height;y++){
    Marshal.Copy(IntPtr.Add(pointer,y*actual.Length),actual,0,actual.Length);Marshal.Copy(IntPtr.Add(data.Scan0,y*data.Stride),expected,0,expected.Length);
    for(int x=0;x<actual.Length;x++)if(actual[x]!=expected[x])throw new Exception("Premultiplied pixels changed in crop");
   }}finally{image.UnlockBits(data);}Check(true,"Exact pixels");
   AlphaHud.Present(w,image,new Rectangle(100,100,200,100),position);Check(AlphaHud.SurfaceCreates==created+2,"Resize recreates one surface");
  }
  GC.Collect();GC.WaitForPendingFinalizers();Check(GetGuiResources(Process.GetCurrentProcess().Handle,0)<=gdi0+3,"Disposal releases GDI surface: before="+gdi0+" after="+GetGuiResources(Process.GetCurrentProcess().Handle,0));
  var t=typeof(Overlay);var present=t.GetMethod("PresentAlphaHud",f);var clear=t.GetMethod("Clear",f);
  foreach(var size in new[]{new Size(640,480),new Size(1920,1080),new Size(3440,1440),new Size(5120,1440)})
  foreach(PanelLayoutMode mode in Enum.GetValues(typeof(PanelLayoutMode))){
   PanelPlacement.Set(mode,false);
   using(var overlay=new Overlay(null,null,true,args[0])){
    ((Timer)t.GetField("timer",f).GetValue(overlay)).Stop();overlay.SetPreviewSize(size.Width,size.Height);overlay.Location=new Point(-12000,-12000);overlay.Show();
    t.GetField("gameBounds",f).SetValue(overlay,new Rectangle(-12000,-12000,size.Width,size.Height));
    t.GetField("preview",f).SetValue(overlay,false);typeof(Control).GetMethod("RecreateHandle",f).Invoke(overlay,null);
    clear.Invoke(overlay,null);Check(!IsWindowVisible(overlay.Handle),"Idle window hidden");
    for(int i=0;i<1000;i++)present.Invoke(overlay,null);
    Check(t.GetField("alphaCanvas",f).GetValue(overlay)==null,"Idle polling allocates no canvas");
    overlay.SetTransformationPreview();present.Invoke(overlay,null);System.Threading.Thread.Sleep(110);present.Invoke(overlay,null);
    Check(IsWindowVisible(overlay.Handle),"Cards show after fade starts");
    var virtualSize=(Size)t.GetProperty("ViewportSize",f).GetValue(overlay,null);Check(virtualSize==size,"Cropping never changes layout viewport: "+virtualSize+" expected "+size);
    var visible=(Rectangle)t.GetMethod("VisibleHudBounds",f).Invoke(overlay,null);Check(overlay.Size==visible.Size,"Only visible area presented");
    var cropped=(Bitmap)t.GetField("alphaCanvas",f).GetValue(overlay);
    Check(cropped.Size==visible.Size,"CPU composition is cropped too");
    Check(((Bitmap)t.GetField("menuScratch",f).GetValue(overlay)).Size==new Size(1,1),"Measurement surface is one pixel");
    using(var reference=new Bitmap(size.Width,size.Height,PixelFormat.Format32bppPArgb)){
     using(var g=Graphics.FromImage(reference)){g.Clear(Color.Transparent);t.GetMethod("DrawScene",f).Invoke(overlay,new object[]{g});}
     var expected=reference.LockBits(visible,ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);
     var actual=cropped.LockBits(new Rectangle(Point.Empty,cropped.Size),ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);
     try{var a=new byte[cropped.Width*4];var b=new byte[a.Length];for(int y=0;y<cropped.Height;y++){
      Marshal.Copy(IntPtr.Add(expected.Scan0,y*expected.Stride),a,0,a.Length);Marshal.Copy(IntPtr.Add(actual.Scan0,y*actual.Stride),b,0,b.Length);
      for(int x=0;x<a.Length;x++)if(a[x]!=b[x])throw new Exception("Cropped composition pixel mismatch "+size+" "+mode+" at "+x+","+y);
     }}finally{reference.UnlockBits(expected);cropped.UnlockBits(actual);}
     Check(true,"Cropped composition equals full viewport pixel for pixel");
    }

    Check((long)overlay.Width*overlay.Height<(long)size.Width*size.Height/2,"Single card palette uses less than half a frame");
    var original=overlay.Bounds;long before=AlphaHud.SurfaceCreates;
    for(int i=0;i<500;i++)present.Invoke(overlay,null);
    Check(AlphaHud.SurfaceCreates==before&&overlay.Bounds==original,"Unchanged menu neither reallocates nor drifts");
    if(size.Width==5120&&mode==PanelLayoutMode.ScreenEdges){
     var gameBounds=(Rectangle)t.GetField("gameBounds",f).GetValue(overlay);Check(overlay.Left-gameBounds.Left<200,"Ultrawide edge palette anchored near picture left");
     using(var copy=new Bitmap(size.Width,size.Height,PixelFormat.Format32bppPArgb))using(var g=Graphics.FromImage(copy)){
      g.Clear(Color.FromArgb(32,52,64));t.GetMethod("DrawScene",f).Invoke(overlay,new object[]{g});copy.Save(Path.Combine(output,"ultrawide-screen-edges.png"));
     }
    }
    clear.Invoke(overlay,null);Check(!IsWindowVisible(overlay.Handle),"Menu close hides actual window");
   }
  }
  // File notifications trigger live reload without re-reading files on every poll.
  using(var overlay=new Overlay(null,null,true,args[0])){
   ((Timer)t.GetField("timer",f).GetValue(overlay)).Stop();t.GetMethod("WatchPreferences",f).Invoke(overlay,null);
   PanelPlacement.Set(PanelLayoutMode.ScreenEdges,true);System.Threading.Thread.Sleep(150);
   PanelPlacement.Set(PanelLayoutMode.Classic,false);t.GetField("nextPreferenceRead",f).SetValue(overlay,DateTime.MinValue);t.GetMethod("RefreshSavedHudPreferences",f).Invoke(overlay,null);
   Check(PanelPlacement.Mode==PanelLayoutMode.ScreenEdges,"File change notification reloads saved layout");
   PanelPlacement.Set(PanelLayoutMode.Classic,false);t.GetField("nextPreferenceRead",f).SetValue(overlay,DateTime.MinValue);t.GetMethod("RefreshSavedHudPreferences",f).Invoke(overlay,null);
   Check(PanelPlacement.Mode==PanelLayoutMode.Classic,"Unchanged files are not re-read on every half-second poll");
  }
  // Persist the new mode and all existing toggle types, then reload as on app reopening.
  PanelPlacement.Set(PanelLayoutMode.ScreenEdges,true);PanelPlacement.Set(PanelLayoutMode.Classic,false);PanelPlacement.Load();Check(PanelPlacement.Mode==PanelLayoutMode.ScreenEdges,"Edge mode survives reload");
  var shortcut=new LayoutShortcut();Check(shortcut.Update(true,true)&&PanelPlacement.Mode==PanelLayoutMode.Centered,"Shortcut cycles from fourth mode to first");
  HudPreferences.SetShowAttacks(false,true);HudPreferences.SetShowAttacks(true,false);HudPreferences.Load();Check(!HudPreferences.ShowAttacks,"Attack preference saved");
  AttackAppearance.SetOuterFrame(true,true);AttackAppearance.SetOuterFrame(false,false);AttackAppearance.Load();Check(AttackAppearance.OuterFrame,"Frame preference saved");
  foreach(int aspect in new[]{1333,1778,2333,3556}){
   var picture=HudLayout.Picture(new Size(5120,1440),aspect);Check(HudLayout.Controls(new Size(5120,1440),aspect,PanelLayoutMode.ScreenEdges)==picture,"Edge mode follows active picture, excludes decorative bars");
   Check(HudLayout.PlayerArea(new Size(5120,1440),aspect,PanelLayoutMode.ScreenEdges,0,true).Right<=picture.Left+picture.Width/2+.1,"Split player stays in own viewport");
  }
  string report="PASS: "+checks+" checks; persistent cropped DIB pixels, reuse, resize, GDI disposal, idle hiding, four resolutions x four layouts, stable virtual positioning, edge-mode and toggle persistence. No live-game FPS claim.";
  File.WriteAllText(Path.Combine(output,"cropped-hud-results.txt"),report);Console.WriteLine(report);
 }
}
