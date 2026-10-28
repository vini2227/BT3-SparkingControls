using System;using System.IO;using System.Threading;using System.Diagnostics;using System.Reflection;using System.Runtime.Serialization;
static class QuietHandoffFixture {
 static int checks;static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
 [STAThread]static void Main(string[] args){
  string folder=args[1];Directory.CreateDirectory(folder);string previous=Environment.GetEnvironmentVariable("BT3_GS_VERBOSE");bool enabled=Program.DiagnosticTelemetry;
  try{
   Environment.SetEnvironmentVariable("BT3_GS_VERBOSE","1");
   Check(!GameSession.ChildEnvironment(false,true).Contains("BT3_GS_VERBOSE="),"Quiet child removes inherited verbose flag");
   Check(GameSession.ChildEnvironment(true).Contains("BT3_GS_VERBOSE=1\0"),"Explicit recording remains available");
   Check(Environment.GetEnvironmentVariable("BT3_GS_VERBOSE")=="1","Parent environment remains unchanged");
   Program.DiagnosticTelemetry=false;string log=Path.Combine(Program.Root,"launcher.log");byte[] before=File.Exists(log)?File.ReadAllBytes(log):null;
   for(int i=0;i<1000;i++)Program.SessionLog("Quiet test record");
   Check(before==null?!File.Exists(log):Convert.ToBase64String(before)==Convert.ToBase64String(File.ReadAllBytes(log)),"Quiet calls perform no launcher writes");
   var empty=(Overlay)FormatterServices.GetUninitializedObject(typeof(Overlay));var record=typeof(Overlay).GetMethod("RecordHudCost",BindingFlags.NonPublic|BindingFlags.Instance);
   record.Invoke(empty,new object[]{0L});Check((long)typeof(Overlay).GetField("hudTicks",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(empty)==0,"Quiet HUD does not collect periodic timing/process metrics");
   for(int i=0;i<3;i++)using(var session=new GameSession(args[0],null,false,true,true,true)){
    session.Start();var timer=Stopwatch.StartNew();while(!session.Done&&timer.ElapsedMilliseconds<10000)Thread.Sleep(10);
    Check(session.Failure==null,"Offline handoff failure: "+session.Failure);Check(session.Done&&session.Detached,"Detached child termination observed");
    Check(session.DetachedThreadHoldVerified,"Primary thread remains suspended across detachment; resumed API balances expected count");
    Check(session.Latest!=null&&Program.AddonAddressAllowed(session.Latest.Profile.allocation,0x10000),"Same verified packet hook and safe arena");
    Check(session.RendererLogPath==null,"Quiet handoff creates no renderer capture file");
   }
   string report="PASS: "+checks+" checks; quiet logging writes suppressed, periodic HUD statistics skipped, inherited native verbose disabled only in child, explicit recording retained, three real game-process pre-first-instruction patch/readback and debugger-detach cases. Primary thread held through detach and child terminated before any game code executes. No gameplay/GPU/FPS claim; offline detach is an opt-in isolation test, not the normal online launch.";
   File.WriteAllText(Path.Combine(folder,"quiet-handoff-results.txt"),report);Console.WriteLine(report);
  }finally{Program.DiagnosticTelemetry=enabled;Environment.SetEnvironmentVariable("BT3_GS_VERBOSE",previous);}
 }
}
