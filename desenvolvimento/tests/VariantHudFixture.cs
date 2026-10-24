using System;using System.IO;using System.Linq;using System.Drawing;using System.Reflection;using System.Windows.Forms;
static class VariantHudFixture {
 static int checks;static void Check(bool v,string m){checks++;if(!v)throw new Exception(m);}static BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static bool Inside(RectangleF a,RectangleF b){return b.Left>=a.Left-.5&&b.Top>=a.Top-.5&&b.Right<=a.Right+.5&&b.Bottom<=a.Bottom+.5;}
 [STAThread]static void Main(string[] args){Directory.CreateDirectory(args[1]);SavedSettings.OverrideDirectory=Path.Combine(args[1],"settings");Directory.CreateDirectory(SavedSettings.OverrideDirectory);
 Size[] sizes={new Size(640,480),new Size(1920,1080),new Size(1920,1200),new Size(2560,1080),new Size(3440,1440),new Size(5120,1440)};
 foreach(var size in sizes)foreach(PanelLayoutMode mode in Enum.GetValues(typeof(PanelLayoutMode)))foreach(int aspect in new[]{0,1333}){
  PanelPlacement.Set(mode,false);using(var o=new Overlay(null,null,true,args[0])){
   ((Timer)typeof(Overlay).GetField("timer",F).GetValue(o)).Stop();o.SetPreviewSize(size.Width,size.Height);o.SetPreviewAspect(aspect);typeof(Overlay).GetField("quad",F).SetValue(o,true);typeof(Overlay).GetField("localSplit",F).SetValue(o,true);
   var rows=typeof(Overlay).GetField("rows",F);var panels=typeof(Overlay).GetField("transformations",F);var bounds=typeof(Overlay).GetField("panelBounds",F);var draw=typeof(Overlay).GetMethod("DrawScene",F);
   var moves=((MoveRow[][])rows.GetValue(o))[0];rows.SetValue(o,new[]{moves,moves,moves,moves});using(var b=new Bitmap(size.Width,size.Height))using(var g=Graphics.FromImage(b)){
    foreach(bool frame in new[]{false,true}){AttackAppearance.SetOuterFrame(frame,false);draw.Invoke(o,new object[]{g});var boxes=(RectangleF[])bounds.GetValue(o);for(int side=0;side<4;side++){Check(Inside(HudLayout.PlayerArea(size,aspect,mode,side,true,true),boxes[side]),"Four attack panels stay in their quarters");Check(boxes[side].Width>0,"Panel rendered");}}
    RectangleF[] expected=null;
    for(int kind=0;kind<3;kind++){
     if(kind==0)o.SetTransformationPreview();else if(kind==1)o.SetFusionPreview();else o.SetSwitchPreview();var p=((TransformationPanel[])panels.GetValue(o))[0];
     panels.SetValue(o,Enumerable.Range(0,4).Select(i=>new TransformationPanel{Side=i,Options=p.Options,HasForms=p.HasForms,HasFusion=p.HasFusion,IsFusion=p.IsFusion,IsSwitch=p.IsSwitch}).ToArray());
     draw.Invoke(o,new object[]{g});var boxes=(RectangleF[])bounds.GetValue(o);
     for(int side=0;side<4;side++){Check(Inside(HudLayout.PlayerArea(size,aspect,mode,side,true,true),boxes[side]),"Four card panels stay in their quarters");if(expected!=null)Check(expected[side]==boxes[side],"Forms/fusion/switch share geometry");}
     if(expected==null)expected=(RectangleF[])boxes.Clone();
     if(size.Width==1920&&size.Height==1080&&mode==PanelLayoutMode.Expanded&&aspect==0)b.Save(Path.Combine(args[1],"quad-"+kind+".png"));
    }
   }
  }
 }
 using(var art=new TransformationArt(args[0])){
  string original=art.Name(4);art.SetCustom(198,new CustomCharacterArt{Base=4,Name="Custom Fighter",NameSet=true,Label="Custom Form",LabelSet=true});Check(art.Name(198)=="Custom Fighter\nCustom Form","Custom names and forms");
  art.SetCustom(4,new CustomCharacterArt{Base=4,Name="Replacement",NameSet=true,Label="",LabelSet=true});Check(art.Name(4)=="Replacement","Replacement clears form label");art.SetCustom(4,null);Check(art.Name(4)==original,"Online/original metadata restored");
 }
 Console.WriteLine("PASS: "+checks+" four-view geometry and custom metadata checks.");
 }
}
