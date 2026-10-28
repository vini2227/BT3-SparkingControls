using System;using System.IO;using System.Drawing;using System.Linq;using System.Windows.Forms;using System.Collections.Generic;
static class LauncherFixture {
 static int checks;static void Check(bool okay,string message){checks++;if(!okay)throw new Exception(message);}
 static IEnumerable<Control> All(Control root){foreach(Control c in root.Controls){yield return c;foreach(Control nested in All(c))yield return nested;}}
 [STAThread]static void Main(string[] args){
  SavedSettings.OverrideDirectory=Path.Combine(args[0],"isolated-preferences-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(SavedSettings.OverrideDirectory);string folder=args[0];Directory.CreateDirectory(folder);string file=PanelPlacement.FilePath;byte[] original=File.Exists(file)?File.ReadAllBytes(file):null;string language=UiLanguage.Code;string languageFile=SavedSettings.FilePath("language.txt");byte[] originalLanguage=File.Exists(languageFile)?File.ReadAllBytes(languageFile):null;
  string frameFile=AttackAppearance.FilePath;byte[] originalFrame=File.Exists(frameFile)?File.ReadAllBytes(frameFile):null;
  string attackFile=HudPreferences.FilePath;byte[] originalAttacks=File.Exists(attackFile)?File.ReadAllBytes(attackFile):null;
  string legacyLayout=Path.Combine(SavedSettings.LegacyRoot,"panel-layout.txt");byte[] legacyData=File.Exists(legacyLayout)?File.ReadAllBytes(legacyLayout):null;
  try{
   if(File.Exists(frameFile))File.Delete(frameFile);AttackAppearance.Load();Check(!AttackAppearance.OuterFrame,"External frame should be optional by default");
   if(File.Exists(legacyLayout))File.Delete(legacyLayout);
   if(File.Exists(attackFile))File.Delete(attackFile);HudPreferences.Load();Check(HudPreferences.ShowAttacks,"Attack list must be shown by default");
   HudPreferences.SetShowAttacks(false,true);HudPreferences.SetShowAttacks(true,false);HudPreferences.Load();Check(!HudPreferences.ShowAttacks,"Hidden attack preference did not persist");HudPreferences.SetShowAttacks(true,true);
   if(File.Exists(file))File.Delete(file);PanelPlacement.Load();Check(!PanelPlacement.Expanded,"Centered should be default");
   PanelPlacement.Set(true,true);PanelPlacement.Set(false,false);PanelPlacement.Load();Check(PanelPlacement.Expanded,"Expanded preference not restored");
   PanelPlacement.Set(false,true);PanelPlacement.Set(true,false);PanelPlacement.Load();Check(!PanelPlacement.Expanded,"Centered preference not restored");
   PanelPlacement.Set(PanelLayoutMode.Classic,true);PanelPlacement.Set(false,false);PanelPlacement.Load();Check(PanelPlacement.Mode==PanelLayoutMode.Classic&&File.ReadAllText(file)=="classic","Classic preference not restored");PanelPlacement.Set(false,true);
   foreach(var size in new[]{new Size(1920,1080),new Size(3440,1440),new Size(5120,1440)}){
    var centered=HudLayout.Controls(size,0,false);var expanded=HudLayout.Controls(size,0,true);
    Check(Math.Abs(centered.Width/centered.Height-1.5f)<.001,"Centered region changed");Check(Math.Abs(expanded.Width/expanded.Height-16f/9f)<.001,"Expanded region must restore 16:9");
    Check(expanded.Left<centered.Left,"Expanded is not closer to edge");
    var classic=HudLayout.Controls(size,0,PanelLayoutMode.Classic);Check(Math.Abs(classic.Width/classic.Height-4f/3f)<.001f,"Classic region is not exactly 4:3");Check(classic.Left>centered.Left,"Classic must be more central than Centered");
   }
   Check(HudLayout.Controls(new Size(1920,1080),1333,false)==HudLayout.Controls(new Size(1920,1080),1333,true),"4:3 must be identical");
   Check(HudLayout.Controls(new Size(1920,1080),1333,PanelLayoutMode.Classic)==HudLayout.Controls(new Size(1920,1080),1333,false),"Classic must preserve native 4:3");
   var catalog=(Dictionary<string,object>)new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(File.ReadAllText(Path.Combine(Program.Root,"assets","ui-translations.json")));
   var translations=(Dictionary<string,object>)catalog["translations"];Check(UiLanguage.Codes.Length==16&&UiLanguage.NativeNames.Length==16,"Official language count mismatch");
   foreach(string lang in UiLanguage.Codes){UiLanguage.Set(lang,true);UiLanguage.Set("en",false);UiLanguage.Load();Check(UiLanguage.Code==lang,"Saved locale not restored");foreach(var entry in (Dictionary<string,object>)translations[lang])Check(UiLanguage.Text(entry.Key)==(string)entry.Value,"Translation fallback for "+lang+" / "+entry.Key);}
   UiLanguage.Set("unsupported",false);Check(UiLanguage.Code=="en","Unknown locale must fall back to English");
   foreach(string lang in UiLanguage.Codes){
    UiLanguage.Set(lang,false);using(var window=new GameLocationWindow("",Path.Combine(folder,"unused-game-path.txt"))){
     window.Show();Application.DoEvents();var combo=window.Controls.Find("panelPlacement",true).OfType<ComboBox>().Single();
     Check((string)combo.Items[0]==UiLanguage.Text("Centralizada"),"Centered label not translated");Check((string)combo.Items[1]==UiLanguage.Text("Expandida"),"Expanded label not translated");
     Check(combo.Items.Count==4&&(string)combo.Items[2]==UiLanguage.Text("Clássica (4:3)"),"Classic option not translated");
     combo.SelectedIndex=2;Check(PanelPlacement.Mode==PanelLayoutMode.Classic&&File.ReadAllText(file)=="classic","Classic UI selection must persist");
     Check(window.RightToLeftLayout==(lang=="ar"),"Arabic RTL mismatch");
     combo.SelectedIndex=1;Check(PanelPlacement.Expanded&&File.ReadAllText(file)=="expanded","UI selection must persist");combo.SelectedIndex=0;Check(!PanelPlacement.Expanded&&File.ReadAllText(file)=="centered","UI centered selection must persist");
     var layoutKey=window.Controls.Find("layoutShortcutKey",true).Single();var frameKey=window.Controls.Find("frameShortcutKey",true).Single();var frameBox=window.Controls.Find("attackOuterFrame",true).Single();var comboScreen=combo.RectangleToScreen(combo.ClientRectangle);var keyScreen=layoutKey.RectangleToScreen(layoutKey.ClientRectangle);Check(Math.Abs(comboScreen.Top+comboScreen.Height/2-keyScreen.Top-keyScreen.Height/2)<=2,"Layout shortcut vertical alignment "+lang);Check(Math.Abs(frameBox.Top+frameBox.Height/2-frameKey.Top-frameKey.Height/2)<=3,"Frame shortcut vertical alignment "+lang);Check((lang=="ar"?Math.Abs(window.Controls.Find("frameKeyLabel",true).Single().Left-frameKey.Right):Math.Abs(window.Controls.Find("frameKeyLabel",true).Single().Right-frameKey.Left))<=12,"Frame shortcut must be adjacent "+lang+" checkbox="+frameBox.Bounds+" key="+frameKey.Bounds);combo.DroppedDown=true;Application.DoEvents();Check(combo.DroppedDown,"Dropdown cannot open "+lang);combo.DroppedDown=false;Application.DoEvents();Check(!combo.DroppedDown,"Dropdown cannot close "+lang);
     var lockToggle=window.Controls.Find("lockHudShortcuts",true).OfType<CheckBox>().Single();Check(!lockToggle.Checked,"Lock default disabled");lockToggle.Checked=true;ShortcutBindings.Load();Check(ShortcutBindings.Locked,"Lock persisted");lockToggle.Checked=false;ShortcutBindings.Load();Check(!ShortcutBindings.Locked,"Unlock persisted");
     var all=All(window).ToArray();var mini=all.OfType<PlacementPreview>().Single();Check(mini.Mode==PanelPlacement.Mode,"Preview placement mismatch");var frameToggle=all.OfType<CheckBox>().Single(c=>c.Name=="attackOuterFrame");Check(frameToggle.Text==UiLanguage.Text("Moldura externa dos ataques"),"Frame label translation");frameToggle.Checked=true;Check(mini.OuterFrame,"Preview did not show outer frame");AttackAppearance.Load();Check(AttackAppearance.OuterFrame,"Outer frame choice not saved");frameToggle.Checked=false;Check(!mini.OuterFrame,"Preview did not hide outer frame");AttackAppearance.Load();Check(!AttackAppearance.OuterFrame,"Card style choice not saved");var attackToggle=all.OfType<CheckBox>().Single(c=>c.Name=="showAttackPanel");Check(attackToggle.Text==UiLanguage.Text("Mostrar painel de ataques"),"Attack setting translation missing");attackToggle.Checked=false;Check(!mini.ShowAttacks,"Preview did not hide attacks");HudPreferences.Load();Check(!HudPreferences.ShowAttacks,"Unchecking must persist hidden attacks");attackToggle.Checked=true;Check(mini.ShowAttacks,"Preview did not restore attacks");HudPreferences.Load();Check(HudPreferences.ShowAttacks,"Rechecking must restore attacks");Check(all.OfType<RoundedButton>().Count()==7,"All launcher buttons must be rounded");Check(window.BackColor==LauncherTheme.Background,"Dark theme missing");
     foreach(var c in all.Where(c=>c.Visible&&c.Width>0&&c.Height>0)){
      if(c.Parent is FlowLayoutPanel||c.Parent is TableLayoutPanel)Check(c.Left>=-1&&c.Top>=-1&&c.Right<=c.Parent.ClientSize.Width+1&&c.Bottom<=c.Parent.ClientSize.Height+1,"Clipped control: "+lang+" / "+c.GetType().Name+" "+c.Text+" "+c.Bounds+" parent="+c.Parent.ClientSize);
     }
     using(var bmp=new Bitmap(window.Width,window.Height)){window.DrawToBitmap(bmp,new Rectangle(0,0,bmp.Width,bmp.Height));bmp.Save(Path.Combine(folder,"launcher-dark-"+lang+".png"));}
     // Changing language must keep the selection and its saved value.
     combo.SelectedIndex=1;var languageCombo=all.OfType<ComboBox>().First(c=>c!=combo);languageCombo.SelectedIndex=(languageCombo.SelectedIndex+1)%UiLanguage.Codes.Length;Check(combo.SelectedIndex==1&&PanelPlacement.Expanded,"Language switch reset preference");combo.SelectedIndex=0;
     combo.SelectedIndex=2;languageCombo.SelectedIndex=(languageCombo.SelectedIndex+1)%UiLanguage.Codes.Length;Check(combo.SelectedIndex==2&&PanelPlacement.Mode==PanelLayoutMode.Classic,"Language switch reset Classic preference");
     if(lang=="pt"){using(var bmp=new Bitmap(window.Width,window.Height)){window.DrawToBitmap(bmp,new Rectangle(0,0,bmp.Width,bmp.Height));bmp.Save(Path.Combine(folder,"launcher-classic-proof.png"));}}
     combo.SelectedIndex=0;
     ((TextBox)typeof(GameLocationWindow).GetField("pathBox",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(window)).Text="not-a-game.txt";typeof(GameLocationWindow).GetMethod("Save",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(window,new object[]{false});Check(window.Visible&&window.DialogResult!=DialogResult.OK,"Invalid location must remain in launcher "+lang);Check(All(window).OfType<Label>().Any(label=>label.Text==UiLanguage.Text("Selecione o executável nativo do Linux.")),"Inline validation message missing "+lang);
     window.Close();
    }
   }
   string result="PASS: "+checks+" launcher checks: all three persisted modes, default, 4:3 invariance, 16:9/ultrawide placement, 16 translated layouts, complete catalog strings, RTL, no clipped controls, rounded buttons, dark theme and language-switch preservation.";Console.WriteLine(result);File.WriteAllText(Path.Combine(folder,"launcher-test-results.txt"),result);
  }finally{
   if(originalFrame==null){if(File.Exists(frameFile))File.Delete(frameFile);}else File.WriteAllBytes(frameFile,originalFrame);AttackAppearance.Load();
   if(legacyData==null){if(File.Exists(legacyLayout))File.Delete(legacyLayout);}else File.WriteAllBytes(legacyLayout,legacyData);
   if(originalAttacks==null){if(File.Exists(attackFile))File.Delete(attackFile);}else File.WriteAllBytes(attackFile,originalAttacks);HudPreferences.Load();
   if(original==null){if(File.Exists(file))File.Delete(file);}else File.WriteAllBytes(file,original);
   if(originalLanguage==null){if(File.Exists(languageFile))File.Delete(languageFile);}else File.WriteAllBytes(languageFile,originalLanguage);
   UiLanguage.Set(language,false);PanelPlacement.Load();
  }
 }
}
