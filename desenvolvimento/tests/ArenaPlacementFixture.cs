using System;using System.IO;using System.Text;using System.Diagnostics;using System.Runtime.InteropServices;using System.Collections.Generic;using System.Web.Script.Serialization;
static class ArenaPlacementFixture {
 [DllImport("kernel32.dll")]static extern uint WaitForSingleObject(IntPtr handle,uint milliseconds);
 static int checks;static void Check(bool value,string label){checks++;if(!value)throw new Exception(label);}
 static long Run(string probe,string destination,int kind,out long addon){
  var info=new Native.STARTUPINFO();info.cb=Marshal.SizeOf(info);Native.PROCESS_INFORMATION pi;addon=0;
  var environment=Marshal.StringToHGlobalUni(GameSession.ChildEnvironment());
  try{Check(Native.CreateProcess(probe,new StringBuilder("\""+probe+"\" \""+destination+"\""),IntPtr.Zero,IntPtr.Zero,false,0x08000404,environment,Path.GetDirectoryName(destination),ref info,out pi),"Create suspended native fixture");}finally{Marshal.FreeHGlobal(environment);}
  bool exited=false;
  try{
   var memory=new Memory(pi.process);
   if(kind==1){addon=Native.VirtualAllocEx(pi.process,new IntPtr(0x13000000),new IntPtr(0x10000),0x3000,4).ToInt64();Check(addon==0x13000000,"Reproduce previous allocator");}
   if(kind==3)Check(Native.VirtualAllocEx(pi.process,new IntPtr(0x60000000),new IntPtr(0x10000),0x3000,4).ToInt64()==0x60000000,"Occupy first safe slot");
   if(kind==2||kind==3){addon=Program.AllocateAddon(memory,new Profile{allocation_size=0x10000});Check(Program.AddonAddressAllowed(addon,0x10000),"Production allocator stays in safe range");}
   Check(Native.ResumeThread(pi.thread)!=0xffffffff,"Resume fixture");Check(WaitForSingleObject(pi.process,10000)==0,"Fixture completed");exited=true;
   var result=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(destination));return Convert.ToInt64(result["arena"]);
  }finally{if(!exited)Native.TerminateProcess(pi.process,8);Native.CloseHandle(pi.thread);Native.CloseHandle(pi.process);}
 }
 [STAThread]static void Main(string[] args){
  string probe=args[0],output=args[1];Directory.CreateDirectory(output);long addon;
  var rows=new List<string>();
  for(int iteration=0;iteration<3;iteration++)for(int kind=0;kind<4;kind++){
   long arena=Run(probe,Path.Combine(output,"arena-"+iteration+"-"+kind+".json"),kind,out addon);
   Check(kind==1?arena!=0x13000000:arena==0x13000000,"Only old add-on reservation displaces the game arena");
   if(kind==2)Check(addon==0x60000000,"First safe candidate");if(kind==3)Check(addon==0x60100000,"Occupied safe candidate skipped");
   rows.Add("iteration="+iteration+" case="+kind+" add-on=0x"+addon.ToString("X")+" game-arena=0x"+arena.ToString("X"));
  }
  foreach(long address in new[]{0x003b0000L,0x10000000L,0x12000000L,0x13000000L,0x14000000L,0x1eff0000L,0x20000000L,0x70000000L,0x7f000000L})Check(!Program.AddonAddressAllowed(address,0x10000),"Reject game/preferred/scratchpad/out-of-range address");
  Check(!Program.AddonAddressAllowed(0x6fff0000,0x20000),"Reject scratchpad-crossing region");Check(!Program.AddonAddressAllowed(0x60000000,0),"Reject zero size");Check(!Program.AddonAddressAllowed(long.MaxValue,0x10000),"Reject overflowing address");
  rows.Add("PASS: "+checks+" checks; 12 suspended native process cases verify startup arena preservation, reproduce the old displacement, test safe-slot fallback and protect reserved ranges. Not a GPU/FPS benchmark.");
  File.WriteAllLines(Path.Combine(output,"arena-placement-results.txt"),rows);Console.WriteLine(string.Join(Environment.NewLine,rows));
 }
}
