using System;using System.IO;using System.Threading;using System.Diagnostics;
static class RendererCaptureFixture {
 static int checks;static void Check(bool value,string label){checks++;if(!value)throw new Exception(label);}
 [STAThread]static void Main(string[] args){
  string output=args[2];Directory.CreateDirectory(output);
  string previous=Environment.GetEnvironmentVariable("BT3_GS_VERBOSE");Environment.SetEnvironmentVariable("BT3_GS_VERBOSE",null);
  try{
   Check(!GameSession.ChildEnvironment().Contains("BT3_GS_VERBOSE="),"Normal launch does not enable verbose logging");
   Check(GameSession.ChildEnvironment(true).Contains("BT3_GS_VERBOSE=1\0"),"Diagnostic child enables native timers");
   Check(Environment.GetEnvironmentVariable("BT3_GS_VERBOSE")==null,"Parent environment unaffected");
   using(var session=new GameSession(args[0],args[1],true)){
    session.Start();var wait=Stopwatch.StartNew();while(!session.Done&&wait.ElapsedMilliseconds<30000)Thread.Sleep(10);
    Check(session.Done&&session.Failure==null,"Diagnostic tracked process chain finishes");
    Check(session.Latest!=null&&Program.AddonAddressAllowed(session.Latest.Profile.allocation,0x10000),"Pre-frame hook uses safe reservation");
    string text=File.ReadAllText(session.RendererLogPath);
    Check(text.Contains("CAPTURE_STDOUT_OK")&&text.Contains("CAPTURE_STDERR_OK"),"Both native streams reach the renderer file");
    Check(text.Contains("renderer-capture-test 0:")&&text.Contains("renderer-capture-test 4999:"),"All 5000 records captured without loss");
    Check(text.Contains("capture_write_ms="),"Native file I/O cost measured");
    File.Copy(session.RendererLogPath,Path.Combine(output,"renderer-capture-sample.txt"),true);
    string io=Array.Find(text.Split('\n'),line=>line.StartsWith("capture_write_ms="));
    string report="PASS: "+checks+" checks; opt-in only, child-only environment, direct native stdout/stderr capture, 5000 records, safe reservation and descendant pre-first-instruction hook. "+io+" on this machine; not a game FPS measurement.";
    File.WriteAllText(Path.Combine(output,"renderer-capture-results.txt"),report);Console.WriteLine(report);
   }
  }finally{Environment.SetEnvironmentVariable("BT3_GS_VERBOSE",previous);}
 }
}
