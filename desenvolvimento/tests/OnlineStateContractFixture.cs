using System;using System.IO;using System.Linq;using System.Diagnostics;using System.Collections.Generic;
static class OnlineStateContractFixture {
 static int checks;static void Check(bool v,string message){checks++;if(!v)throw new Exception(message);}
 static int Find(byte[] code,params byte[] pattern){for(int i=0;i+pattern.Length<=code.Length;i++)if(code.Skip(i).Take(pattern.Length).SequenceEqual(pattern))return i;throw new Exception("Pattern absent");}
 static void Main(string[] args){
  string folder=args[1];Directory.CreateDirectory(folder);var bytes=File.ReadAllBytes(args[0]);var image=new PeImage(bytes);var metadata=OnlineCompatibility.Metadata();
  OnlineCompatibility.CheckNetActiveContract(image,metadata);Check(true,"Updated native state ABI accepted");
  int rva=image.Symbol("Port_NetActive"),raw=image.RawOffset(rva,128);byte[] head=image.Read(rva,128);
  int load=Find(head,0x44,0x8b),test=Find(head,0x45,0x85),result=Find(head,0x0f,0x9f,0xc0),finish=Find(head,0xc3),branch=test+3;
  foreach(var change in new[]{new[]{load+3,4},new[]{test+2,1},new[]{branch,1},new[]{result+1,2},new[]{result+2,1},new[]{finish,1},new[]{finish-1,1}}){
   byte[] bad=(byte[])bytes.Clone();bad[raw+change[0]]^=(byte)change[1];bool refused=false;
   try{OnlineCompatibility.CheckNetActiveContract(new PeImage(bad),metadata);}catch(IOException){refused=true;}
   Check(refused,"Changed target/register/signed condition/boolean result/return/restore rejected at "+change[0]);
  }
  byte[] renamed=(byte[])bytes.Clone();int oldReg=(head[load+2]>>3)&7,newReg=oldReg==2?1:2;
  renamed[raw+load+2]=(byte)(5|(newReg<<3));renamed[raw+test+2]=(byte)(0xc0|(newReg<<3)|newReg);
  OnlineCompatibility.CheckNetActiveContract(new PeImage(renamed),metadata);Check(true,"Consistent volatile-register allocation accepted without new signature");
  // Initializer is called and owned by the game, not copied or patched by us.
  // Altering its bytes must not invalidate an unchanged cached-state interface.
  byte[] initializationChanged=(byte[])bytes.Clone();initializationChanged[raw+image.FunctionLength("Port_NetActive")-1]^=0x5a;
  var resolved=Program.LoadProfile();OnlineCompatibility.ResolveImage(new PeImage(initializationChanged),resolved,metadata);
  Check(resolved.resolved_payload!=null,"Initializer/alignment change accepted through the boundary contract");
  Check(resolved.patches.Length==1&&resolved.patches[0].rva==image.Symbol("Port_PadRead"),"Only original packet hook retained");
  if(args.Length>2){var historical=new PeImage(File.ReadAllBytes(args[2]));var previous=Program.LoadProfile();OnlineCompatibility.ResolveImage(historical,previous,metadata);OnlineCompatibility.CheckNetActiveContract(historical,metadata);Check(true,"Previous actual game accepted too");}
  var times=new List<double>();for(int i=0;i<5;i++){var timer=Stopwatch.StartNew();Program.Validate(args[0],Program.LoadProfile());timer.Stop();times.Add(timer.Elapsed.TotalMilliseconds);}
  string report="PASS: "+checks+" native state-contract checks. Full Validate timings ms: "+string.Join(", ",times.Select(t=>t.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)))+". Startup-only file/ABI validation, not game-loading or FPS measurement. No game process launched.";
  File.WriteAllText(Path.Combine(folder,"online-state-contract-results.txt"),report);Console.WriteLine(report);
 }
}
