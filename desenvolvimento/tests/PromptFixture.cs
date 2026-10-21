using System;using System.IO;using System.Drawing;using System.Runtime.InteropServices;using System.Web.Script.Serialization;
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
  // Reproduce the actual command-row layout captured from the running game.
  I(0x2f08,1);I(0x1f40c,0);I(0x8948,0x106);I(0x4018,0);I(0x40c0,13);
  I(0x40e8,2);I(0x4120,1);I(0x4128,0); // Triangle is row icon ZERO.
  long rowNode=0x6000+26*0x38;I(rowNode,0);I(rowNode+0x10,60);I(rowNode+0x14,400);
  long rowSprite=0x5000+6*0x1c;I(rowSprite,0);S(rowSprite+4,22);
  S(rowSprite+8,-12);S(rowSprite+10,12);S(rowSprite+12,-16);S(rowSprite+14,16);S(rowSprite+16,32);S(rowSprite+18,56);S(rowSprite+20,0);S(rowSprite+22,32);
  memory.Write(start+rowSprite+0x1b,new byte[]{128});
  long ghost=0x5000+14*0x1c;I(ghost,1);
  prompts=reader.SamplePrompts();Assert(prompts.Length==1&&prompts[0].Button=="□","Real held Triangle row icon zero was not covered.");
  Assert(prompts[0].NativeBounds==new RectangleF(48,384,24,32),"Command row must use the actual native sprite rectangle.");
  if(args.Length>1)using(var image=new Bitmap(args[1]))using(var g=Graphics.FromImage(image))using(var icons=new GameButtonIcons(args[3])) {
    prompts[0].Draw(g,image.Size,icons);image.Save(args[2]);
  }
  using(var icons=new GameButtonIcons(args[3])) {
    var box=prompts[0].Project(new Size(1920,1080));var ink=icons.TriangleInk(box,prompts[0].Texture,prompts[0].Uv);
    Assert(ink.Width<box.Width&&ink.Height<box.Height,"Cover must be smaller than the entire animated button.");
    Assert(ink.Top>box.Top,"Cover must not extend upward into the hand.");
    File.WriteAllText(Path.Combine(Path.GetDirectoryName(args[0]),"prompt-symbol-hd.txt"),string.Join("\r\n",icons.HdLoaded));
  }
  I(0x4128,1);Assert(reader.SamplePrompts().Length==0,"Existing Square row must not be covered.");
  I(0x4128,2);Assert(reader.SamplePrompts().Length==0,"Circle row must not be mistaken for Triangle.");
  I(0x4128,0);I(0x8948,0x107);Assert(reader.SamplePrompts().Length==0,"Blast 2 must retain its native Triangle row.");
  I(0x8948,0x108);Assert(reader.SamplePrompts()[0].Button=="○","Ultimate Triangle row must follow Circle.");
  I(0x8948,0x106);I(0x40e8,7);Assert(reader.SamplePrompts().Length==1,"Command row fade-out must retain coverage.");
  I(0x40e8,-1);Assert(reader.SamplePrompts().Length==0,"Inactive command row must not leave a cover behind.");
  File.WriteAllText(args[0],"PASS: "+assertions+" isolated-button and real command-row prompt checks; local side independent of last HUD draw; native Triangle covered by selected Square/Circle; existing Square and super-2 Triangle preserved; online P2 left, offline P2 side retained.\r\n");
  return 0;
 }
}
