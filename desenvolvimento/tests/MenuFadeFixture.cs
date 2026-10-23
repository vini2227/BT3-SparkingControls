using System;using System.IO;using System.Reflection;using System.Drawing;using System.Windows.Forms;
static class MenuFadeFixture {
 static int checks;static void Check(bool b,string s){checks++;if(!b)throw new Exception(s);}
 static BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
 static FieldInfo Field(string s){return typeof(Overlay).GetField(s,flags);}
 static MethodInfo Method(string s){return typeof(Overlay).GetMethod(s,flags);}
 static long Alpha(Bitmap b){long n=0;for(int y=0;y<b.Height;y+=3)for(int x=0;x<b.Width;x+=3)n+=b.GetPixel(x,y).A;return n;}
 [STAThread]static void Main(string[] args){
  string dest=args[1];Directory.CreateDirectory(dest);SavedSettings.OverrideDirectory=Path.Combine(dest,"isolated-settings");Directory.CreateDirectory(SavedSettings.OverrideDirectory);HudPreferences.SetShowAttacks(true,false);AttackAppearance.SetOuterFrame(false,false);
  using(var fade=new HudMenuFade()){
   Check(!fade.Update(true,0)&&fade.Opacity==0,"Starts transparent");fade.Update(true,.025);Check(Math.Abs(fade.Opacity-.25)<.001,"25 ms in");fade.Update(true,.100);Check(fade.Opacity==1,"100 ms in");Check(!fade.Update(true,.110),"Stable end");fade.Update(false,.150);Check(Math.Abs(fade.Opacity-.5)<.001,"40 ms out");fade.Update(true,.175);Check(Math.Abs(fade.Opacity-.75)<.001,"Reopen reverses without jump");fade.Update(false,.255);Check(fade.Opacity==0,"80 ms out");
   fade.Update(true,.255);fade.Update(true,.355);using(var b=new Bitmap(10,10))using(var g=Graphics.FromImage(b)){g.Clear(Color.Transparent);fade.Cache(new Bitmap(4,4),new Rectangle(1,1,4,4));fade.Draw(g);}fade.Reset();Check(fade.Opacity==0&&fade.Image==null,"Reset clears cached bitmap");
   for(int i=0;i<120;i++){fade.Update(i%2==0,1+i/120.0);Check(fade.Opacity>=0&&fade.Opacity<=1,"Rapid toggles bounded");}fade.Update(false,3);Check(fade.Opacity==0&&fade.Image==null,"Rapid toggles settle");
  }
  foreach(var size in new[]{new Size(640,480),new Size(1280,720),new Size(2560,1080)})using(var o=new Overlay(null,null,true,args[0])){
   ((Timer)Field("timer").GetValue(o)).Stop();o.SetPreviewSize(size.Width,size.Height);o.SetPreviewAspect(size.Width==640?1333:0);Field("preview").SetValue(o,false);
   var fades=(HudMenuFade[])Field("attackFades").GetValue(o);var cards=(HudMenuFade[])Field("cardFades").GetValue(o);var initial=Field("rows").GetValue(o);long[] sums=new long[6];Bitmap cached=null;
   double[] times={0,.025,.050,.100,.140,.180};
   for(int i=0;i<times.Length;i++){
    if(i==4)Field("rows").SetValue(o,new MoveRow[2][]);
    Field("updateMenuSnapshots").SetValue(o,i==0||i==4);Method("UpdateMenuFades").Invoke(o,new object[]{times[i]});
    using(var b=new Bitmap(size.Width,size.Height,System.Drawing.Imaging.PixelFormat.Format32bppPArgb))using(var g=Graphics.FromImage(b)){
     g.Clear(Color.Transparent);Method("DrawScene").Invoke(o,new object[]{g});sums[i]=Alpha(b);b.Save(Path.Combine(dest,"attacks-"+size.Width+"-"+i+".png"));
    }
    if(i==1){cached=fades[0].Image;Check(cached!=null&&cached.Width<size.Width,"Cropped snapshot");}
    if(i==2||i==3||i==4)Check(Object.ReferenceEquals(cached,fades[0].Image),"Opacity animation reuses snapshot");
   }
   Check(sums[0]==0&&sums[1]>0&&sums[1]<sums[2]&&sums[2]<sums[3],"Rendered fade-in alpha");Check(sums[4]>0&&sums[4]<sums[3]&&sums[5]==0,"Retained last content fades fully out");Check(fades[0].Image==null,"Hidden cache disposed");
   double time=1;
   foreach(int kind in new[]{0,1,2}){
    Method("ResetMenuFades").Invoke(o,null);if(kind==0)o.SetTransformationPreview();else if(kind==1)o.SetFusionPreview();else o.SetSwitchPreview();
    Field("updateMenuSnapshots").SetValue(o,true);Method("UpdateMenuFades").Invoke(o,new object[]{time});Method("UpdateMenuFades").Invoke(o,new object[]{time+.1});
    using(var b=new Bitmap(size.Width,size.Height))using(var g=Graphics.FromImage(b)){g.Clear(Color.Transparent);Method("DrawScene").Invoke(o,new object[]{g});Check(Alpha(b)>0&&cards[0].Image!=null,"Card kind renders");}
    Field("transformations").SetValue(o,new TransformationPanel[0]);Method("UpdateMenuFades").Invoke(o,new object[]{time+.14});using(var b=new Bitmap(size.Width,size.Height))using(var g=Graphics.FromImage(b)){g.Clear(Color.Transparent);Method("DrawScene").Invoke(o,new object[]{g});Check(Alpha(b)>0,"Cards remain during fade-out");}
    Method("UpdateMenuFades").Invoke(o,new object[]{time+.18});Check(cards[0].Opacity==0&&cards[0].Image==null,"Card kind fully closes");time+=1;
   }
   Field("rows").SetValue(o,initial);Method("UpdateMenuFades").Invoke(o,new object[]{10.0});Method("UpdateMenuFades").Invoke(o,new object[]{10.1});Field("preview").SetValue(o,true);Method("Clear").Invoke(o,null);Check(fades[0].Opacity==0&&cards[0].Opacity==0,"Focus/session clear is immediate");
  }
  string report="PASS: "+checks+" fade checks: 100 ms in / 80 ms out, continuous reversal, rapid taps, cropped snapshot reuse, true alpha rendering, full fade-out after source disappears, all four menu types at 4:3/16:9/ultrawide, immediate focus/session reset. Pure managed offscreen tests; not live gameplay.";Console.WriteLine(report);File.WriteAllText(Path.Combine(dest,"menu-fade-results.txt"),report);
 }
}
