using System;using System.IO;
static class SavedSettingsFixture {
 static void Require(bool v,string name){if(!v)throw new Exception(name);}
 static void Main(string[] args){
  string common=Path.Combine(args[0],"settings-fixture-"+Guid.NewGuid().ToString("N")),a=Path.Combine(common,"online-test-a"),b=Path.Combine(common,"online-test-b");Directory.CreateDirectory(a);Directory.CreateDirectory(b);
  foreach(string name in new[]{"language.txt","game-path.txt"}){
   string first=name=="language.txt"?"es-419":@"D:\Games\BT3\Tenkaichi3Decomp.exe";File.WriteAllText(Path.Combine(a,name),first);
   string shared=SavedSettings.Resolve(common,a,name);Require(File.ReadAllText(shared)==first,"Old value not migrated");
   File.WriteAllText(shared,"latest choice");File.WriteAllText(Path.Combine(b,name),"stale default");Require(SavedSettings.Resolve(common,b,name)==shared,"Update changed settings path");Require(File.ReadAllText(shared)=="latest choice","Update overwrote saved preference");
  }
  string missing=Path.Combine(common,"missing-legacy");Require(File.ReadAllText(SavedSettings.Resolve(common,missing,"language.txt"))=="latest choice","Reopening without old directory lost settings");
  File.WriteAllText(Path.Combine(b,"migration-only.txt"),"fallback choice");Require(File.ReadAllText(SavedSettings.Resolve(common,missing,"migration-only.txt"))=="fallback choice","Previous version discovery failed");
  Console.WriteLine("PASS: language/game path migrated, shared settings preserved across changed version folders, previous-version fallback and reopening verified in isolated fixture.");
 }
}
