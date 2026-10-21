using System;using System.IO;using System.Drawing;using System.Reflection;
static class AttackStyleFixture {
 [STAThread]static void Main(string[] args){Directory.CreateDirectory(args[1]);bool before=AttackAppearance.OuterFrame,shown=HudPreferences.ShowAttacks;var draw=typeof(Overlay).GetMethod("DrawScene",BindingFlags.Instance|BindingFlags.NonPublic);var field=typeof(Overlay).GetField("panelBounds",BindingFlags.Instance|BindingFlags.NonPublic);int[] edge=new int[2];
  try{HudPreferences.SetShowAttacks(true,false);using(var overlay=new Overlay(null,null,true,args[0])){overlay.SetPreviewSize(1920,1080);overlay.SetPreviewAspect(1778);
   for(int i=0;i<2;i++){AttackAppearance.SetOuterFrame(i==1,false);using(var bmp=new Bitmap(1920,1080))using(var g=Graphics.FromImage(bmp)){g.Clear(Color.Transparent);draw.Invoke(overlay,new object[]{g});var b=((RectangleF[])field.GetValue(overlay))[0];edge[i]=bmp.GetPixel((int)b.Left+3,(int)(b.Top+b.Height/2)).A;using(var crop=bmp.Clone(Rectangle.Ceiling(b),bmp.PixelFormat))crop.Save(Path.Combine(args[1],i==0?"attacks-card-style.png":"attacks-with-frame.png"));}}
  }}finally{AttackAppearance.SetOuterFrame(before,false);HudPreferences.SetShowAttacks(shown,false);}
  if(edge[0]!=0||edge[1]==0)throw new Exception("Optional external frame did not change outer pixels");Console.WriteLine("PASS: card-style attacks render without outer background; enabling the frame restores it without changing layout.");
 }
}
