using System;using System.IO;using System.Drawing;using System.Reflection;using System.Windows.Forms;
static class SplitScreenFixture {
 static int checks;static void Check(bool ok,string s){checks++;if(!ok)throw new Exception(s);}
 static bool Inside(RectangleF a,RectangleF b){return b.Left>=a.Left-.5&&b.Top>=a.Top-.5&&b.Right<=a.Right+.5&&b.Bottom<=a.Bottom+.5;}
 static BindingFlags f=BindingFlags.Instance|BindingFlags.NonPublic;
 [STAThread]static void Main(string[] args){
  string dest=args[1];Directory.CreateDirectory(dest);SavedSettings.OverrideDirectory=Path.Combine(dest,"settings");Directory.CreateDirectory(SavedSettings.OverrideDirectory);
  Check(HudLayout.IsLocalSplit(0,1),"Local split detected");Check(!HudLayout.IsLocalSplit(1,1),"Online split rule must use full screen");Check(!HudLayout.IsLocalSplit(2,1),"Online guest must use full screen");Check(!HudLayout.IsLocalSplit(0,0),"Single-view mode unchanged");
  var rs=typeof(Overlay).GetField("rows",f);var split=typeof(Overlay).GetField("localSplit",f);var bounds=typeof(Overlay).GetField("panelBounds",f);var draw=typeof(Overlay).GetMethod("DrawScene",f);
  Size[] sizes={new Size(640,480),new Size(1440,1080),new Size(1920,1080),new Size(2560,1080),new Size(3440,1440),new Size(5120,1440),new Size(1920,1200)};
  using(var sheet=new Bitmap(1060,sizes.Length*310))using(var sg=Graphics.FromImage(sheet))using(var font=new Font("Segoe UI",11)){
   sg.Clear(Color.FromArgb(19,23,29));
   for(int n=0;n<sizes.Length;n++)foreach(PanelLayoutMode mode in Enum.GetValues(typeof(PanelLayoutMode))){
    PanelPlacement.Set(mode,false);Size size=sizes[n];
    using(var overlay=new Overlay(null,null,true,args[0])){
     ((Timer)typeof(Overlay).GetField("timer",f).GetValue(overlay)).Stop();overlay.SetPreviewSize(size.Width,size.Height);overlay.SetPreviewAspect(0);split.SetValue(overlay,true);
     var first=((MoveRow[][])rs.GetValue(overlay))[0];var second=(MoveRow[])first.Clone();
     second[0]=new MoveRow("R2 + ↑ / ↓","Solar Flare","2",true,false);second[1]=new MoveRow("R2 + ← / →","Wild Sense","2",true,false);
     RectangleF[] baseline=new RectangleF[2];
     using(var b=new Bitmap(size.Width,size.Height))using(var g=Graphics.FromImage(b)){
      foreach(bool frame in new[]{false,true}){
       AttackAppearance.SetOuterFrame(frame,false);rs.SetValue(overlay,new[]{first,second});draw.Invoke(overlay,new object[]{g});var both=(RectangleF[])((RectangleF[])bounds.GetValue(overlay)).Clone();
       var picture=HudLayout.Picture(size,0);float divider=picture.Left+picture.Width/2;
       Check(both[0].Right<divider&&both[1].Left>divider,"Panels stay in own halves");Check(Math.Abs(both[0].Height-both[1].Height)<.1f,"Equal row count uses equal typography scale on both sides");
       for(int side=0;side<2;side++){
        var area=HudLayout.PlayerArea(size,0,mode,side,true);Check(Inside(area,both[side]),"Panel stays in safe viewport");Check(Math.Abs(both[side].Left-(area.Left+area.Width*.025f))<1,"Both panels left anchored");
        rs.SetValue(overlay,side==0?new[]{first,null}:new[]{null,second});draw.Invoke(overlay,new object[]{g});var alone=((RectangleF[])bounds.GetValue(overlay))[side];Check(alone==both[side],"Other player's menu cannot resize or move this panel");
        Check(HudLayout.PlayerArea(size,0,mode,side,false)==HudLayout.Controls(size,0,mode),"Online/single view area unchanged");
       }
       baseline=both;
      }
      if(mode==PanelLayoutMode.Expanded){
       AttackAppearance.SetOuterFrame(false,false);rs.SetValue(overlay,new[]{first,second});g.Clear(Color.FromArgb(43,70,88));using(var brush=new SolidBrush(Color.FromArgb(48,80,66)))g.FillRectangle(brush,0,0,size.Width/2,size.Height);using(var pen=new Pen(Color.FromArgb(190,213,221),Math.Max(1,size.Width/1000f)))g.DrawLine(pen,size.Width/2,0,size.Width/2,size.Height);draw.Invoke(overlay,new object[]{g});
       b.Save(Path.Combine(dest,"split-"+size.Width+"x"+size.Height+".png"));float factor=Math.Min(1040f/size.Width,270f/size.Height);sg.DrawImage(b,10,n*310+28,size.Width*factor,size.Height*factor);sg.DrawString(size.Width+" x "+size.Height+" | P1 and P2: bottom-left in each viewport",font,Brushes.White,10,n*310+5);
      }
     }
    }
   }
   sheet.Save(Path.Combine(dest,"splitscreen-contact-sheet.png"));
  }
  // Decorative bars are excluded before splitting the active picture.
  var letter=HudLayout.PlayerArea(new Size(1920,1080),1333,PanelLayoutMode.Expanded,0,true);Check(letter.Left>239&&letter.Right<961,"4:3 letterbox safe");
  File.WriteAllText(Path.Combine(dest,"splitscreen-results.txt"),"PASS: "+checks+" split detection, per-viewport anchoring, stable solo/both menus, frame on/off, seven resolutions and three placement modes. Managed rendering only; live split battle not verified.\n");Console.WriteLine("PASS: "+checks+" splitscreen checks.");
 }
}
