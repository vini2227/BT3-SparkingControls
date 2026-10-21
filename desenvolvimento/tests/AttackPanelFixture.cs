using System;using System.IO;using System.Drawing;using System.Reflection;
static class AttackPanelFixture {
 [STAThread]static void Main(string[] args){
  var method=typeof(Overlay).GetMethod("DrawScene",BindingFlags.NonPublic|BindingFlags.Instance);bool original=HudPreferences.ShowAttacks;string output=args[1];Directory.CreateDirectory(output);
  try{using(var overlay=new Overlay(null,null,true,args[0])){
   overlay.SetPreviewSize(1280,720);
   Func<string,int> render=name=>{using(var bmp=new Bitmap(1280,720))using(var g=Graphics.FromImage(bmp)){g.Clear(Color.Transparent);method.Invoke(overlay,new object[]{g});bmp.Save(Path.Combine(output,name+".png"));int pixels=0;for(int y=0;y<bmp.Height;y++)for(int x=0;x<bmp.Width;x++)if(bmp.GetPixel(x,y).A!=0)pixels++;return pixels;}};
   HudPreferences.SetShowAttacks(true,false);if(render("attacks-visible")<100)throw new Exception("Move panel did not render");
   HudPreferences.SetShowAttacks(false,false);if(render("attacks-hidden")!=0)throw new Exception("Hidden attack list still rendered");
   overlay.SetFusionPreview();System.Threading.Thread.Sleep(500);if(render("forms-attacks-hidden")<100)throw new Exception("Hiding attacks also hid the cards");
   HudPreferences.SetShowAttacks(true,false);overlay.SetFusionPreview();render("forms-attacks-visible");
  }}finally{HudPreferences.SetShowAttacks(original,false);}
  Console.WriteLine("PASS: hiding attacks removes their pixels while preserving transformation/fusion cards; no persisted setting changed by render fixture.");
 }
}
