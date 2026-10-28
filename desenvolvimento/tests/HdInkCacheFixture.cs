using System;using System.Drawing;using System.Reflection;using System.Collections.Generic;using System.Diagnostics;
static class HdInkCacheFixture {
 static void Check(bool b,string s){if(!b)throw new Exception(s);}
 static void Main(){
  var flags=BindingFlags.Instance|BindingFlags.NonPublic;
  using(var icons=new GameButtonIcons(null)){
   var images=(Dictionary<int,Bitmap>)typeof(GameButtonIcons).GetField("promptTriangles",flags).GetValue(icons);
   var sizes=(Dictionary<int,Size>)typeof(GameButtonIcons).GetField("promptTriangleSizes",flags).GetValue(icons);
   var image=new Bitmap(1024,1024);using(var g=Graphics.FromImage(image)){g.Clear(Color.Transparent);g.FillRectangle(Brushes.Lime,256,384,512,384);}images[12]=image;sizes[12]=new Size(64,64);
   var uv=new Rectangle(0,0,64,64);var box=new RectangleF(10,20,64,64);var sw=Stopwatch.StartNew();var first=icons.TriangleInk(box,12,uv);sw.Stop();double cold=sw.Elapsed.TotalMilliseconds;
   Check(first==new RectangleF(26,44,32,24),"HD ink bounds preserved");Check(icons.TriangleInkScans==1,"first scan");
   sw.Restart();for(int i=0;i<100000;i++){var target=new RectangleF(0,0,128,128);Check(icons.TriangleInk(target,12,uv)==new RectangleF(32,48,64,48),"cache independent of target position/size");}sw.Stop();
   Check(icons.TriangleInkScans==1,"100000 redraws must not rescan HD pixels");
   for(int i=0;i<600;i++)icons.TriangleInk(box,12,new Rectangle(i,0,1,1));
   var cache=(Dictionary<string,RectangleF>)typeof(GameButtonIcons).GetField("triangleInkCache",flags).GetValue(icons);Check(cache.Count<=256,"bounded cache");
   Console.WriteLine("PASS: 100000 cached HD projections; first scan "+cold.ToString("F2")+" ms; cached total "+sw.Elapsed.TotalMilliseconds.ToString("F2")+" ms; exact crop and scaling; bounded cache.");
  }
 }
}
