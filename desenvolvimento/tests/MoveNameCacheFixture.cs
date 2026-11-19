using System;using System.IO;using System.Linq;using System.Text;using System.Diagnostics;
static class MoveNameCacheFixture {
 static int checks,reads,tick;static byte[] data;
 static void Check(bool value,string why){checks++;if(!value)throw new Exception(why);}
 static byte[] Read(long address,int count){reads++;Check((address&4095)+count<=4096,"page boundary");return data.Skip((int)(address-4096)).Take(count).ToArray();}
 static void Put(string text){data=Enumerable.Repeat((byte)65,65536).ToArray();byte[] b=Encoding.Unicode.GetBytes(text);Buffer.BlockCopy(b,0,data,0,b.Length);reads=0;}
 static string Rows(int count){return string.Join("\r\n",Enumerable.Range(0,count).Select(i=>"00*000Golpe "+i))+"\r\n";}
 static void Main(){try{Run();}catch(Exception ex){Console.WriteLine(ex);Environment.ExitCode=1;}}
 static void Run(){
  tick=0;var cache=new MoveNameCache(Read,()=>tick);Put(Rows(5)+new string('x',20000)+"\0");
  Check(cache.Get(1,4096).SequenceEqual(GameData.ParseMoveNames(data)),"five names preserved");Check(reads==1,"stop after fifth move, without reading trailing script");
  for(int i=0;i<10000;i++)cache.Get(1,4096);Check(reads==1,"successful names reused");
  cache.Clear();Put("00@000end\r\n"+new string('x',20000));Check(cache.Get(1,4096).Count==0&&reads==1,"native stop marker respected");
  cache.Clear();Put("not a moves list\0");Check(cache.Get(1,4096).Count==0,"unknown names remain unknown");int initial=reads;
  for(tick=1;tick<1000;tick++)cache.Get(1,4096);Check(reads==initial,"malformed text is not read every HUD tick");
  Put(Rows(5)+"\0");tick=1000;Check(cache.Get(1,4096).Count==5&&reads==1,"retry notices text becoming available");
  int attempts=0;var failing=new MoveNameCache((at,n)=>{attempts++;throw new IOException("unmapped");},()=>tick);
  tick=int.MaxValue-500;failing.Get(1,4096);tick=unchecked(tick+999);failing.Get(1,4096);Check(attempts==1,"retry works across TickCount wrap");tick=unchecked(tick+1);failing.Get(1,4096);Check(attempts==2,"retry after one second across wrap");
  for(int i=0;i<1000;i++)cache.Get(i+10,4096);Check(cache.Count==64,"cache memory bounded across changing custom models");cache.Clear();Check(cache.Count==0,"fight exit clears cache");
  var random=new Random(8173);
  for(int i=0;i<600;i++){
   string text=(i%2==0?"\ufeff":"")+string.Join(i%3==0?"\n":"\r\n",Enumerable.Range(0,random.Next(1,12)).Select(k=>(k%3==0?"00&123*000":"00*000")+(k==2&&i%7==0?"bad\tcontrol":"Ataque 日本語 "+k)))+"\0";
   Put(text);cache.Clear();Check(cache.Get(i,4096).SequenceEqual(GameData.ParseMoveNames(data)),"streaming parser regression "+i);
  }
  Put("invalid\0");cache.Clear();tick=0;var watch=Stopwatch.StartNew();for(int i=0;i<100000;i++)cache.Get(1,4096);watch.Stop();Check(reads==1,"100000 repeated failures cause only one remote read");
  Console.WriteLine("PASS "+checks+" move-name checks; 100000 repeated misses: "+watch.Elapsed.TotalMilliseconds.ToString("F2")+" ms, "+reads+" remote read. Synthetic benchmark; not a gameplay FPS measurement.");
 }
}
