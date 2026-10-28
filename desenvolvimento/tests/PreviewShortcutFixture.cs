using System;using System.IO;using System.Drawing;using System.Windows.Forms;using System.Security.Cryptography;
static class PreviewShortcutFixture {
 static void Check(bool v,string name){if(!v)throw new Exception(name);}
 static string Hash(Bitmap b){using(var stream=new MemoryStream()){b.Save(stream,System.Drawing.Imaging.ImageFormat.Png);using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(stream.ToArray()));}}
 [STAThread]static void Main(string[] args){
  string dir=args[0];Directory.CreateDirectory(dir);SavedSettings.OverrideDirectory=Path.Combine(dir,"preferences-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(SavedSettings.OverrideDirectory);
  PanelPlacement.Set(PanelLayoutMode.Centered,true);var shortcut=new LayoutShortcut();Check(shortcut.Update(true,true)&&PanelPlacement.Mode==PanelLayoutMode.Expanded,"F8 first press");Check(!shortcut.Update(true,true)&&PanelPlacement.Mode==PanelLayoutMode.Expanded,"Held F8 repeated");shortcut.Update(false,true);Check(shortcut.Update(true,true)&&PanelPlacement.Mode==PanelLayoutMode.Classic,"F8 second press");shortcut.Update(false,false);Check(!shortcut.Update(true,false)&&PanelPlacement.Mode==PanelLayoutMode.Classic,"Inactive game changed position");Check(!shortcut.Update(true,true),"Held inactive press triggered on focus return");shortcut.Update(false,true);Check(shortcut.Update(true,true)&&PanelPlacement.Mode==PanelLayoutMode.ScreenEdges,"F8 fourth mode");shortcut.Update(false,true);Check(shortcut.Update(true,true)&&PanelPlacement.Mode==PanelLayoutMode.Centered,"F8 wraparound");PanelPlacement.Load();Check(PanelPlacement.Mode==PanelLayoutMode.Centered,"F8 did not persist");
  using(var host=new Panel {Size=new Size(270,128),BackColor=LauncherTheme.Background})using(var preview=new PlacementPreview()){
   host.Controls.Add(preview);string previous=null;
   foreach(PanelLayoutMode mode in Enum.GetValues(typeof(PanelLayoutMode))){preview.Mode=mode;string hidden=null,without=null;
    for(int show=0;show<2;show++)for(int frame=0;frame<2;frame++){preview.ShowAttacks=show==1;preview.OuterFrame=frame==1;using(var image=new Bitmap(270,128)){preview.DrawToBitmap(image,new Rectangle(0,0,270,128));string digest=Hash(image);image.Save(Path.Combine(dir,"preview-"+mode+"-"+show+"-"+frame+".png"));if(show==0&&frame==0)hidden=digest;if(show==0&&frame==1)Check(digest==hidden,"Hidden panel still shows frame");if(show==1&&frame==0){without=digest;Check(digest!=hidden,"Attack toggle did not change preview");}if(show==1&&frame==1){Check(digest!=without,"Frame toggle did not change preview");Check(digest!=previous,"Placement did not change preview");previous=digest;}}}
   }
  }
  Console.WriteLine("PASS: F8 rising edge, no held repeat, focus gating, wraparound and saved choice; 16 preview states respond to layout and both checkboxes.");
 }
}
