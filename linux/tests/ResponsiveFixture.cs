using System;using System.IO;using System.Drawing;using System.Reflection;using System.Collections.Generic;
static class ResponsiveFixture {
 static int checks;
 static void Check(bool okay,string message){checks++;if(!okay)throw new Exception(message);}
 static bool Inside(RectangleF a,RectangleF b){return b.Left>=a.Left-.5f&&b.Top>=a.Top-.5f&&b.Right<=a.Right+.5f&&b.Bottom<=a.Bottom+.5f;}
 [STAThread] static void Main(string[] args){
  string game=args[0],output=args[1];Directory.CreateDirectory(output);var report=new List<string>();
  foreach(var size in new[]{new Size(320,240),new Size(640,480),new Size(800,600),new Size(1280,720),new Size(1920,1080),new Size(1920,1200),new Size(2560,1080),new Size(3440,1440),new Size(5120,1440),new Size(1080,1920)})
  foreach(int aspect in new[]{0,1333,1600,1778,2333,2390,2667,3200,3556,4000}){
   RectangleF picture=HudLayout.Picture(size,aspect),safe=HudLayout.Controls(size,aspect);
   Check(Inside(new RectangleF(PointF.Empty,size),picture),"Picture escaped client");Check(Inside(picture,safe),"Safe area escaped picture");
   Check(Math.Abs(safe.Left+safe.Width/2-size.Width/2)<.1f,"Safe area is off center");Check(safe.Width/safe.Height<=3f/2f+.001f,"Safe area too wide");
   foreach(int side in new[]{0,1}){
    var dock=HudLayout.Dock(safe,new SizeF(safe.Width*.4f,safe.Height*.27f),side,false);Check(Inside(picture,dock),"Dock escaped picture");
   }
  }
  var letterbox=HudLayout.Controls(new Size(1920,1080),1333);Check(letterbox.Left>239&&letterbox.Left<241,"4:3 side art not excluded");
  var ultra=HudLayout.Controls(new Size(5120,1440),3556);Check(ultra.Left>1270,"32:9 controls still at edge");
  foreach(var size in new[]{new Size(1920,1080),new Size(3440,1440),new Size(5120,1440)}){
   var safe=HudLayout.Controls(size,0);Check(Math.Abs(safe.Width/safe.Height-1.5f)<.001f,"Widescreen must use centered 3:2");
   var dock=HudLayout.Dock(safe,new SizeF(300,200),0,false);var four=HudLayout.Picture(size,1333);float fourAnchor=four.Left+four.Width*.025f;
   Check(dock.Left<fourAnchor && fourAnchor-dock.Left<size.Height*.1f,"Anchor must stay slightly left of centered 4:3");
  }
  Check(Math.Abs(HudLayout.Controls(new Size(1440,1080),1333).Width-1439.64f)<1,"4:3 was changed");
  var metrics=new NativeMenuMetrics(game);float regular=metrics.Measure("Instantaneous Transmission",2),tight=metrics.Measure("Instantaneous Transmission",2,.85f);
  Check(tight<regular&&tight>regular*.65f,"Tracking not tightened coherently");report.Add("Name advance: "+regular+" -> "+tight+" px; original glyph sizing retained.");
  var draw=typeof(Overlay).GetMethod("DrawScene",BindingFlags.Instance|BindingFlags.NonPublic);var field=typeof(Overlay).GetField("panelBounds",BindingFlags.Instance|BindingFlags.NonPublic);var rows=typeof(Overlay).GetField("rows",BindingFlags.Instance|BindingFlags.NonPublic);
  int[,] cases={{1920,1080,1333},{1440,1080,1333},{1920,1080,1778},{1920,1200,1600},{3440,1440,2389},{5120,1440,3556},{640,480,1333},{1280,720,2333}};
  using(var sheet=new Bitmap(1000,240*cases.GetLength(0)))using(var sg=Graphics.FromImage(sheet))using(var label=new Font("Arial",11)){
   sg.Clear(Color.FromArgb(21,24,30));
   using(var overlay=new Overlay(null,null,true,game)){
    for(int i=0;i<cases.GetLength(0);i++){
     var size=new Size(cases[i,0],cases[i,1]);int aspect=cases[i,2];overlay.SetPreviewSize(size.Width,size.Height);overlay.SetPreviewAspect(aspect);size=overlay.ClientSize;
     RectangleF picture=HudLayout.Picture(size,aspect),safe=HudLayout.Controls(size,aspect);
     using(var bitmap=new Bitmap(size.Width,size.Height))using(var g=Graphics.FromImage(bitmap)){
      g.Clear(Color.FromArgb(66,40,26));using(var bg=new SolidBrush(Color.FromArgb(34,68,94)))g.FillRectangle(bg,picture);
      using(var line=new Pen(Color.FromArgb(170,175,200),2))g.DrawRectangle(line,safe.X,safe.Y,safe.Width-1,safe.Height-1);
      draw.Invoke(overlay,new object[]{g});var bounds=((RectangleF[])field.GetValue(overlay))[0];Check(Inside(picture,bounds),"Move panel escaped picture "+i);Check(Inside(safe,bounds),"Move panel escaped safe area "+i);
      bitmap.Save(Path.Combine(output,"moves-"+i+".png"));
      float factor=Math.Min(980f/size.Width,200f/size.Height);sg.DrawImage(bitmap,10,i*240+27,size.Width*factor,size.Height*factor);
      sg.DrawString(size.Width+"x"+size.Height+" / game aspect "+aspect+" / panel "+Rectangle.Round(bounds),label,Brushes.White,10,i*240+3);
      report.Add(size+" aspect="+aspect+" picture="+picture+" safe="+safe+" moves="+bounds);
      // Split local battle panels must fit without overlapping.
      var original=(MoveRow[][])rows.GetValue(overlay);rows.SetValue(overlay,new[]{original[0],original[0]});draw.Invoke(overlay,new object[]{g});var both=(RectangleF[])field.GetValue(overlay);
      Check(Inside(safe,both[0])&&Inside(safe,both[1]),"Split panels escaped safe area");Check(both[0].Right<=both[1].Left+.1f,"Split panels overlap");rows.SetValue(overlay,original);
     }
    }
   }
   sheet.Save(Path.Combine(output,"responsive-contact-sheet.png"));
  }
  using(var overlay=new Overlay(null,null,true,game)){
   foreach(int kind in new[]{0,1,2})foreach(int aspect in new[]{1333,1778,2389,3556})foreach(int side in new[]{0,1}){
    overlay.SetPreviewSize(1920,1080);overlay.SetPreviewAspect(aspect);if(kind==0)overlay.SetTransformationPreview();else if(kind==1)overlay.SetFusionPreview();else overlay.SetSwitchPreview();if(side==1)overlay.SetOnlineSidePreview(false);
    using(var bitmap=new Bitmap(1920,1080))using(var g=Graphics.FromImage(bitmap)){
     draw.Invoke(overlay,new object[]{g});var bounds=((RectangleF[])field.GetValue(overlay))[side];Check(Inside(HudLayout.Controls(overlay.ClientSize,aspect),bounds),"Card panel escaped safe area");
     if(kind==0&&side==0&&aspect==1333)bitmap.Save(Path.Combine(output,"forms-4x3.png"));
    }
   }
  }
  foreach(var mode in new[]{PanelLayoutMode.Centered,PanelLayoutMode.Expanded,PanelLayoutMode.Classic}){
   PanelPlacement.Set(mode,false);
   using(var overlay=new Overlay(null,null,true,game)){
    foreach(var size in new[]{new Size(1920,1080),new Size(3440,1440),new Size(5120,1440)}){
     overlay.SetPreviewSize(size.Width,size.Height);overlay.SetPreviewAspect(0);
     using(var bitmap=new Bitmap(size.Width,size.Height))using(var g=Graphics.FromImage(bitmap)){
      draw.Invoke(overlay,new object[]{g});var bounds=((RectangleF[])field.GetValue(overlay))[0];var area=HudLayout.Controls(size,0,mode);
      Check(Inside(area,bounds),"Move panel escaped selected placement "+mode);Check(Math.Abs(bounds.Left-(area.Left+area.Width*.025f))<1,"Selected mode did not drive overlay anchor");
     }
    }
   }
   using(var overlay=new Overlay(null,null,true,game)){
    overlay.SetPreviewSize(3440,1440);overlay.SetPreviewAspect(0);
    foreach(int kind in new[]{0,1,2}){
     if(kind==0)overlay.SetTransformationPreview();else if(kind==1)overlay.SetFusionPreview();else overlay.SetSwitchPreview();
     using(var bitmap=new Bitmap(3440,1440))using(var g=Graphics.FromImage(bitmap)){draw.Invoke(overlay,new object[]{g});Check(Inside(HudLayout.Controls(overlay.ClientSize,0,mode),((RectangleF[])field.GetValue(overlay))[0]),"Card panel escaped selected placement "+mode);}
    }
   }
  }
  PanelPlacement.Set(false,false);
  report.Add("PASS: "+checks+" responsive layout/typography checks; real rendering at eight viewport/aspect combinations; transformations/fusion/switch and split panels fit.");File.WriteAllLines(Path.Combine(output,"responsive-test-results.txt"),report);Console.WriteLine(report[report.Count-1]);
 }
}
