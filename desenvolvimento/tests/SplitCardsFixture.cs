using System;using System.IO;using System.Drawing;using System.Reflection;using System.Windows.Forms;
static class SplitCardsFixture {
 static int checks;static BindingFlags f=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Check(bool v,string s){checks++;if(!v)throw new Exception(s);}
 static bool Inside(RectangleF a,RectangleF b){return b.Left>=a.Left-.5&&b.Top>=a.Top-.5&&b.Right<=a.Right+.5&&b.Bottom<=a.Bottom+.5;}
 [STAThread]static void Main(string[] args){
  string dest=args[1];Directory.CreateDirectory(dest);SavedSettings.OverrideDirectory=Path.Combine(dest,"settings");Directory.CreateDirectory(SavedSettings.OverrideDirectory);
  var split=typeof(Overlay).GetField("localSplit",f);var panels=typeof(Overlay).GetField("transformations",f);var bounds=typeof(Overlay).GetField("panelBounds",f);var draw=typeof(Overlay).GetMethod("DrawScene",f);
  int[,] cases={{640,480,0},{1440,1080,0},{1920,1080,0},{2560,1080,0},{3440,1440,0},{5120,1440,0},{1920,1200,0},{1920,1080,1333}};
  for(int n=0;n<cases.GetLength(0);n++)foreach(PanelLayoutMode mode in Enum.GetValues(typeof(PanelLayoutMode))){
   PanelPlacement.Set(mode,false);Size size=new Size(cases[n,0],cases[n,1]);int aspect=cases[n,2];RectangleF[] fixedPosition=null;
   using(var o=new Overlay(null,null,true,args[0])){
    ((Timer)typeof(Overlay).GetField("timer",f).GetValue(o)).Stop();o.SetPreviewSize(size.Width,size.Height);o.SetPreviewAspect(aspect);split.SetValue(o,true);
    for(int kind=0;kind<3;kind++){
     if(kind==0)o.SetTransformationPreview();else if(kind==1)o.SetFusionPreview();else o.SetSwitchPreview();
     var p=((TransformationPanel[])panels.GetValue(o))[0];
     var p2=new TransformationPanel {Side=1,Options=p.Options,IsSwitch=p.IsSwitch,IsFusion=p.IsFusion,HasFusion=p.HasFusion,HasForms=p.HasForms};
     using(var b=new Bitmap(size.Width,size.Height))using(var g=Graphics.FromImage(b)){
      g.Clear(Color.FromArgb(43,70,88));using(var bg=new SolidBrush(Color.FromArgb(48,80,66)))g.FillRectangle(bg,0,0,size.Width/2,size.Height);using(var pen=new Pen(Color.Gray,2))g.DrawLine(pen,size.Width/2,0,size.Width/2,size.Height);
      panels.SetValue(o,new[]{p,p2});draw.Invoke(o,new object[]{g});var both=(RectangleF[])((RectangleF[])bounds.GetValue(o)).Clone();
      Check(both[0].Right<=(size.Width/2)&&both[1].Left>=size.Width/2,"Card menus stay in their halves");Check(both[0].Size==both[1].Size,"Equal card scale for both players");
      if(fixedPosition==null)fixedPosition=both;else Check(both[0]==fixedPosition[0]&&both[1]==fixedPosition[1],"Forms/fusion/switch keep identical card positions");
      for(int side=0;side<2;side++){
       var area=HudLayout.PlayerArea(size,aspect,mode,side,true);Check(Inside(area,both[side]),"Cards inside player safe area");Check(Math.Abs(both[side].Left-area.Left-area.Width*.025f)<1,"Left anchor in each viewport");
       panels.SetValue(o,new[]{side==0?p:p2});draw.Invoke(o,new object[]{g});Check(((RectangleF[])bounds.GetValue(o))[side]==both[side],"Opening other card menu cannot move/resize this one");
       var candidate=side==0?p:p2;var options=candidate.Options;candidate.Options=new TransformationOption[0];draw.Invoke(o,new object[]{g});Check(((RectangleF[])bounds.GetValue(o))[side]==both[side],"Empty slots preserve menu size");candidate.Options=options;
      }
      if(size.Width==1920&&size.Height==1080&&aspect==0&&mode==PanelLayoutMode.Expanded){
       g.Clear(Color.FromArgb(43,70,88));using(var bg=new SolidBrush(Color.FromArgb(48,80,66)))g.FillRectangle(bg,0,0,size.Width/2,size.Height);using(var pen=new Pen(Color.Gray,2))g.DrawLine(pen,size.Width/2,0,size.Width/2,size.Height);panels.SetValue(o,new[]{p,p2});draw.Invoke(o,new object[]{g});b.Save(Path.Combine(dest,new[]{"forms","fusion","switch"}[kind]+"-1920x1080.png"));
      }
      // Online-local menus keep the previous left full-screen anchor even if the battle split rule is set.
      p2.OnlineLocal=true;panels.SetValue(o,new[]{p2});draw.Invoke(o,new object[]{g});var online=((RectangleF[])bounds.GetValue(o))[0];var full=HudLayout.Controls(size,aspect,mode);Check(Inside(full,online),"Online menu inside full-screen safe region");Check(Math.Abs(online.Left-full.Left-full.Width*.025f)<1,"Online local left anchor preserved");
      split.SetValue(o,false);draw.Invoke(o,new object[]{g});Check(((RectangleF[])bounds.GetValue(o))[0]==online,"Online appearance independent of local split flag");
      p2.OnlineLocal=false;panels.SetValue(o,new[]{p2});draw.Invoke(o,new object[]{g});var oldRight=((RectangleF[])bounds.GetValue(o))[1];Check(Math.Abs(oldRight.Right-(full.Right-full.Width*.025f))<1,"Single-screen P2 retains original right anchor");split.SetValue(o,true);
     }
    }
   }
  }
  string text="PASS: "+checks+" card-menu checks: transformations/fusion/switch, eight resolution/aspect cases, three placements, both/solo/empty cards, fixed positions and unchanged online/single-screen anchors. Offscreen .NET rendering; live split and Linux native runtime unverified.";File.WriteAllText(Path.Combine(dest,"split-cards-results.txt"),text);Console.WriteLine(text);
 }
}
