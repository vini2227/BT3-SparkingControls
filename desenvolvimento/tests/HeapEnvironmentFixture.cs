using System;using System.IO;using System.Text;using System.Diagnostics;using System.Collections.Generic;using System.Runtime.InteropServices;using System.Web.Script.Serialization;
static class HeapEnvironmentFixture {
 [StructLayout(LayoutKind.Explicit,Size=176)]struct Event{[FieldOffset(0)]public uint Code;[FieldOffset(4)]public uint Pid;[FieldOffset(8)]public uint Tid;[FieldOffset(16)]public IntPtr File;[FieldOffset(16)]public uint Exception;}
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool WaitForDebugEvent(out Event e,uint ms);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool ContinueDebugEvent(uint pid,uint tid,uint status);
 [DllImport("kernel32.dll")]static extern bool DebugSetProcessKillOnExit(bool kill);
 static int checks;static void Check(bool x,string m){checks++;if(!x)throw new Exception(m);}
 static void Run(string exe,string arguments,string cwd){
  var si=new Native.STARTUPINFO();si.cb=Marshal.SizeOf(si);Native.PROCESS_INFORMATION pi;
  IntPtr environment=Marshal.StringToHGlobalUni(GameSession.ChildEnvironment());
  try{Check(Native.CreateProcess(exe,new StringBuilder("\""+exe+"\" "+arguments),IntPtr.Zero,IntPtr.Zero,false,0x08000401,environment,cwd,ref si,out pi),"Create test process");}finally{Marshal.FreeHGlobal(environment);}
  try{
   DebugSetProcessKillOnExit(false);var live=new HashSet<uint>();var clock=Stopwatch.StartNew();bool started=false;
   while(true){
    Event e;if(!WaitForDebugEvent(out e,1000)){if(clock.ElapsedMilliseconds>30000){Native.TerminateProcess(pi.process,7);throw new Exception("Debugger fixture timeout");}continue;}
    uint status=0x10002;
    if(e.Code==3){live.Add(e.Pid);started=true;if(e.File!=IntPtr.Zero)Native.CloseHandle(e.File);}
    else if(e.Code==6){if(e.File!=IntPtr.Zero)Native.CloseHandle(e.File);}
    else if(e.Code==5)live.Remove(e.Pid);
    else if(e.Code==1&&e.Exception!=0x80000003&&e.Exception!=0x80000004)status=0x80010001;
    Check(ContinueDebugEvent(e.Pid,e.Tid,status),"Continue child event");if(started&&live.Count==0)break;
   }
  }finally{Native.CloseHandle(pi.thread);Native.CloseHandle(pi.process);}
 }
 [STAThread]static void Main(string[] args){
  string native=args[0],host=args[1],dest=args[2];Directory.CreateDirectory(dest);
  string old=Environment.GetEnvironmentVariable("_NO_DEBUG_HEAP"),oldSentinel=Environment.GetEnvironmentVariable("BT3_HEAP_TEST_SENTINEL");
  try{
   Environment.SetEnvironmentVariable("_NO_DEBUG_HEAP","0");Environment.SetEnvironmentVariable("BT3_HEAP_TEST_SENTINEL","São Paulo 東京 العربية = retained");
   string block=GameSession.ChildEnvironment();Check(block.EndsWith("\0\0"),"Double NUL terminator");
   var dictionary=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);string previous=null;
   foreach(string line in block.Split('\0'))if(line.Length>0){int separator=line.IndexOf('=',1);Check(separator>0,"Environment entry syntax");string key=line.Substring(0,separator);if(previous!=null)Check(StringComparer.OrdinalIgnoreCase.Compare(previous,key)<0,"Sorted unique environment");previous=key;dictionary.Add(key,line.Substring(separator+1));}
   Check(dictionary["_NO_DEBUG_HEAP"]=="1","Override child only");Check(Environment.GetEnvironmentVariable("_NO_DEBUG_HEAP")=="0","Parent environment unchanged");
   Check(dictionary["BT3_HEAP_TEST_SENTINEL"]=="São Paulo 東京 العربية = retained","Unicode and equals in values");
   foreach(System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())if(!string.Equals((string)entry.Key,"_NO_DEBUG_HEAP",StringComparison.OrdinalIgnoreCase))Check(dictionary[(string)entry.Key]==(string)entry.Value,"Inherited environment preserved");
   foreach(bool descendant in new[]{false,true}){
    string path=Path.Combine(dest,descendant?"descendant.json":"direct.json");
    Run(descendant?host:native,descendant?"\""+native+"\" \""+path+"\"":"\""+path+"\"",dest);
    var result=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(path));
    Check(Convert.ToString(result["no_debug_heap"])=="1","Child inherited normal-heap policy");
    Check(Convert.ToInt32(result["lfh_enabled"])==1&&Convert.ToInt32(result["heap_after"])==2,"Native LFH enabled while process remains debugged");
    if(descendant)Check(File.ReadAllText(path+".parent.txt")=="1|São Paulo 東京 العربية = retained","Host inherited full Unicode environment");
   }
   string text="PASS: "+checks+" environment, normal native heap/LFH, Unicode, parent isolation and descendant inheritance checks. Debug-process tracking remains enabled; this is not a live game FPS test.";
   File.WriteAllText(Path.Combine(dest,"heap-environment-results.txt"),text);Console.WriteLine(text);
  }finally{Environment.SetEnvironmentVariable("_NO_DEBUG_HEAP",old);Environment.SetEnvironmentVariable("BT3_HEAP_TEST_SENTINEL",oldSentinel);}
 }
}
