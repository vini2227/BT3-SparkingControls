using System;using System.IO;using System.Drawing;using System.Drawing.Imaging;using System.Reflection;using System.Runtime.InteropServices;using System.Windows.Forms;
static class CroppedCompositionFixture {
 static BindingFlags f=BindingFlags.NonPublic|BindingFlags.Instance;
 static object Get(Overlay o,string n){return typeof(Overlay).GetField(n,f).GetValue(o);}
 static void Set(Overlay o,string n,object v){typeof(Overlay).GetField(n,f).SetValue(o,v);}
 static void Call(Overlay o,string n,params object[] args){typeof(Overlay).GetMethod(n,f).Invoke(o,args);}
 [STAThread]static void Main(string[] args){int checks=0;SavedSettings.OverrideDirectory=Path.Combine(args[1],"settings");Directory.CreateDirectory(SavedSettings.OverrideDirectory);
 foreach(var size in new[]{new Size(640,480),new Size(1920,1080),new Size(3440,1440),new Size(5120,1440)})foreach(PanelLayoutMode mode in Enum.GetValues(typeof(PanelLayoutMode)))foreach(int kind in new[]{0,1,2,3}){
 PanelPlacement.Set(mode,true);PanelPlacement.Load();if(PanelPlacement.Mode!=mode)throw new Exception("Setting lost");checks++;
 using(var o=new Overlay(null,null,true,args[0])){((Timer)Get(o,"timer")).Stop();o.SetPreviewSize(size.Width,size.Height);if(kind==1)o.SetTransformationPreview();if(kind==2)o.SetFusionPreview();if(kind==3)o.SetSwitchPreview();Set(o,"preview",false);Set(o,"updateMenuSnapshots",true);Call(o,"UpdateMenuFades",0d);Call(o,"UpdateMenuFades",.11d);Call(o,"ComposeHud");
 var cropped=(Bitmap)Get(o,"alphaCanvas");var box=(Rectangle)Get(o,"alphaBounds");if(cropped.Size!=box.Size||((Bitmap)Get(o,"menuScratch")).Size!=new Size(1,1))throw new Exception("Full viewport scratch/composition");checks++;
 using(var expected=new Bitmap(size.Width,size.Height,PixelFormat.Format32bppPArgb)){using(var g=Graphics.FromImage(expected)){g.Clear(Color.Transparent);Call(o,"DrawScene",g);}var a=expected.LockBits(box,ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);var b=cropped.LockBits(new Rectangle(Point.Empty,cropped.Size),ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);try{var x=new byte[cropped.Width*4];var y=new byte[x.Length];for(int row=0;row<cropped.Height;row++){Marshal.Copy(IntPtr.Add(a.Scan0,row*a.Stride),x,0,x.Length);Marshal.Copy(IntPtr.Add(b.Scan0,row*b.Stride),y,0,y.Length);for(int i=0;i<x.Length;i++)if(x[i]!=y[i])throw new Exception("Pixel mismatch "+mode+" "+kind);}}finally{expected.UnlockBits(a);cropped.UnlockBits(b);}checks++;}
 Set(o,"preview",true);
 }}Console.WriteLine("PASS: "+checks+" Linux cropped composition/pixel/persistence checks; four layouts, four sizes, four menu types. No X11 calls.");}
}
