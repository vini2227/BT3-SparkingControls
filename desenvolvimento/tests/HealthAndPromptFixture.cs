using System;using System.IO;using System.Runtime.InteropServices;using System.Web.Script.Serialization;
static class PromptFixture {
 static Memory memory;static long start;static int assertions;
 static void I(long offset,int value){memory.Write(start+offset,BitConverter.GetBytes(value));}
 static void S(long offset,short value){memory.Write(start+offset,BitConverter.GetBytes(value));}
 static void Assert(bool okay,string what){assertions++;if(!okay)throw new Exception(what);}
 static int Main(string[] args){
  IntPtr handle=Native.GetCurrentProcess();IntPtr allocation=IntPtr.Zero;
  for(long candidate=0x10000000;candidate<0x70000000 && allocation==IntPtr.Zero;candidate+=0x1000000)allocation=Native.VirtualAllocEx(handle,new IntPtr(candidate),new IntPtr(0x30000),0x3000,4);
  if(allocation==IntPtr.Zero)throw new Exception("Fixture allocation failed.");start=allocation.ToInt64();memory=new Memory(handle);
  var p=new Profile {image_base=start,allocation=start+0x10000,symbols=new System.Collections.Generic.Dictionary<string,int> { {"gBattleWork",0},{"gHud",0x2f00},{"gHudPrompt",0x2f04},{"online_mode",0x2f08},{"sAspectMilli",0x2f0c},{"sHudLayout",0x2f10} }};
  I(0x19f8,1);I(0x2f00,(int)(start+0x3000));I(0x3030,0x10);I(0x2f04,(int)(start+0x4000));I(0x2f08,1);I(0x2f0c,1778);I(0x2f10,0);
  I(0x4004,(int)(start+0x5000));I(0x4008,(int)(start+0x6000));I(0x4014,1);
  I(0x4018,6);I(0x401c,0);I(0x40c0,6);I(0x40c4,13);I(0x40e8,-1);I(0x40ec,-1);
  I(0x8000,0);I(0x8948,0x106);I(0xa000,1);I(0xa948,0x106);I(0x1f004,(int)(start+0x8000));I(0x1f014,(int)(start+0xa000));I(0x1f40c,0);
  // Simulate the other HUD side having hidden the shared button node and sprite.
  long node=0x6000+24*0x38;I(node,1);I(node+0x10,64);I(node+0x14,384);
  long sprite=0x5000+0x1c;I(sprite,1);S(sprite+8,-12);S(sprite+10,12);S(sprite+12,-12);S(sprite+14,12);
  var reader=new Reader(memory,p);var prompts=reader.SamplePrompts();
  Assert(prompts.Length==1,"Holding prompt was lost when the renderer last selected the opponent.");
  Assert(prompts[0].Button=="□","Super 1 holding prompt must display Square.");
  Assert(prompts[0].NativeBounds.X==52,"Online own prompt must remain on the left.");
  I(0x40c0,5);Assert(reader.SamplePrompts().Length==0,"An existing Square prompt must not be covered.");
  I(0x40c0,6);I(0x8948,0x107);Assert(reader.SamplePrompts().Length==0,"Super 2 already uses Triangle.");
  I(0x8948,0x108);Assert(reader.SamplePrompts()[0].Button=="○","Ultimate holding prompt must follow Circle.");
  I(0x4018,0);I(0x401c,6);I(0x40c4,6);I(0x1f40c,1);I(0xa948,0x106);
  prompts=reader.SamplePrompts();Assert(prompts.Length==1&&prompts[0].Button=="□"&&prompts[0].Side==0,"Online P2 holding prompt must use local left HUD.");
  I(0x2f08,0);prompts=reader.SamplePrompts();Assert(prompts.Length==1&&prompts[0].Side==1,"Offline P2 must retain its own side.");
  I(0x401c,0);Assert(reader.SamplePrompts().Length==0,"Inactive hold prompt must not draw.");
  I(0x1f104,(int)(start+0x8000));I(0x1f10c,2);I(0x1f120,4);I(0x1f124,1);I(0x1f128,1);I(0x1f12c,1);
  I(0x8000+0x9a4+0xa4+0x40,12345);I(0x8000+0x9a4+0xa4+0x44,30000);
  var panels=reader.SampleTransformations();Assert(panels.Length==1&&panels[0].IsSwitch,"Reserve panel fixture missing.");
  Assert(panels[0].Options[0].Health==12345&&panels[0].Options[0].HealthMax==30000,"Reserve life must come from the member selected by the card.");
  File.WriteAllText(args[0],"PASS: "+assertions+" holding-prompt checks; local side independent of last HUD draw; native Triangle covered by selected Square/Circle; existing Square and super-2 Triangle preserved; online P2 left, offline P2 side retained.\r\n");
  return 0;
 }
}
