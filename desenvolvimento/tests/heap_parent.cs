using System;using System.IO;using System.Diagnostics;
static class HeapParent {
 static int Main(string[] args){
  if(args.Length!=2)return 2;
  File.WriteAllText(args[1]+".parent.txt",Environment.GetEnvironmentVariable("_NO_DEBUG_HEAP")+"|"+Environment.GetEnvironmentVariable("BT3_HEAP_TEST_SENTINEL"));
  using(var p=Process.Start(new ProcessStartInfo(args[0],"\""+args[1]+"\""){UseShellExecute=false,CreateNoWindow=true})){p.WaitForExit();return p.ExitCode;}
 }
}
