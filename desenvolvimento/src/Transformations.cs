// Character metadata / BPE and GS layout derived from port/src/gs/fight_panels_ui.cpp, AGPL-3.0-or-later.
// Game artwork is read from the selected installation at runtime; it is never distributed with this add-on.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;

sealed class TransformationOption {
    public int Slot,Character,Cost,Health,HealthMax;public bool Available;
    public string Button { get { return new[]{"□","△","×","○"}[Slot]; } }
}
sealed class TransformationPanel {
    public int Side;public bool IsSwitch,IsFusion,HasFusion,HasForms,OnlineLocal;
    public int DisplaySide {get{return OnlineLocal?0:Side;}}public TransformationOption[] Options;
}
sealed class TransformationNames {public string[] names=new string[0];public int[][] faces=new int[0][];}
sealed class CustomCharacterArt {public string Name,Label,Portrait;public int Base;public bool NameSet,LabelSet;}
sealed class TransformationArt : IDisposable {
    readonly Dictionary<int,CustomCharacterArt> custom=new Dictionary<int,CustomCharacterArt>();int generation;
    public void SetCustom(int id,CustomCharacterArt value){lock(cache){CustomCharacterArt old;custom.TryGetValue(id,out old);if(old==null&&value==null)return;if(old!=null&&value!=null&&old.Name==value.Name&&old.Label==value.Label&&old.Portrait==value.Portrait&&old.Base==value.Base&&old.NameSet==value.NameSet&&old.LabelSet==value.LabelSet)return;if(value==null)custom.Remove(id);else custom[id]=value;generation++;Bitmap image;if(cache.TryGetValue(id,out image)&&image!=null)image.Dispose();cache.Remove(id);}}
    readonly string folder;readonly TransformationNames metadata;
    readonly Dictionary<int,Bitmap> cache=new Dictionary<int,Bitmap>();bool disposed;
    public TransformationArt(string exe) {
        folder=Path.GetDirectoryName(exe??"");
        try {metadata=new JavaScriptSerializer().Deserialize<TransformationNames>(File.ReadAllText(Path.Combine(Program.Root,"assets","transform-characters.json")));}catch(IOException){}catch(ArgumentException){}
    }
    string OriginalName(int id) {return metadata!=null && metadata.names!=null && id>=0 && id<metadata.names.Length?metadata.names[id]:"#"+id;}
    public string Name(int id){lock(cache){CustomCharacterArt c;if(!custom.TryGetValue(id,out c))return OriginalName(id);string fallback=OriginalName(c.Base);string[] lines=fallback.Split('\n');string name=c.NameSet?c.Name:lines[0];string label=c.LabelSet?c.Label:(lines.Length>1?lines[1]:"");return name+(string.IsNullOrEmpty(label)?"":"\n"+label);}}
    public RectangleF Crop(int id) {
        CustomCharacterArt c;lock(cache){if(custom.TryGetValue(id,out c)&&string.IsNullOrEmpty(c.Portrait))id=c.Base;}
        int x=128,y=155;if(metadata!=null && metadata.faces!=null && id>=0 && id<metadata.faces.Length && metadata.faces[id].Length==2){x=metadata.faces[id][0];y=metadata.faces[id][1];}
        return new RectangleF(Math.Max(0,Math.Min(128,x-64)),Math.Max(0,Math.Min(384,y-56)),128,128);
    }
    public Bitmap Get(int id) {
        int epoch;CustomCharacterArt customArt;
        lock(cache) {
            Bitmap ready;if(cache.TryGetValue(id,out ready))return ready;
            if(disposed || id<0 || id>=255)return null;
            cache[id]=null;epoch=generation;custom.TryGetValue(id,out customArt);
        }
        ThreadPool.QueueUserWorkItem(_=>{
            Bitmap art=null;
            try {
                int pictureId=customArt==null?id:customArt.Base;
                string relative=Path.Combine("pzs3us1",(0x2f8+pictureId).ToString("D5")+".bin");
                string path=Path.Combine(folder,"mods",relative);if(!File.Exists(path))path=Path.Combine(folder,"gamedata",relative);
                if(customArt!=null&&!string.IsNullOrEmpty(customArt.Portrait))path=Path.IsPathRooted(customArt.Portrait)?customArt.Portrait:Path.Combine(folder,customArt.Portrait);
                var info=new FileInfo(path);if(info.Exists && info.Length>=8 && info.Length<8*1024*1024)art=Decode(File.ReadAllBytes(path));
            }catch(IOException){}catch(UnauthorizedAccessException){}catch(ArgumentException){}catch(OverflowException){}
            lock(cache){if(disposed||epoch!=generation){if(art!=null)art.Dispose();if(epoch!=generation)cache.Remove(id);}else cache[id]=art;}
        });
        return null;
    }
    static int Int(byte[] b,int off) {if(off<0 || off>b.Length-4)throw new IOException("Truncated portrait");return BitConverter.ToInt32(b,off);}
    public static byte[] Unpack(byte[] packed) {
        int total=Int(packed,0),length=Int(packed,4);
        if(total<=0 || total>8*1024*1024 || length<0 || (long)length+8>packed.Length)throw new IOException("Invalid portrait size");
        byte[] output=new byte[total];int at=8,end=8+length,n=0;long steps=0,budget=Math.Min((long)total*512+1048576,67108864);
        Func<int> take=()=>{if(at>=end)throw new IOException("Truncated BPE data");return packed[at++];};
        while(at<end && n<total) {
            byte[] left=new byte[256],right=new byte[256];int[] stack=new int[512];for(int i=0;i<256;i++)left[i]=(byte)i;
            int c=0,count=take();
            while(true) {
                if(count>127){c+=count-127;count=0;}
                if(c>=256)break;
                for(int i=0;i<=count && c<256;i++,c++){left[c]=(byte)take();if(c!=left[c])right[c]=(byte)take();}
                if(c>=256)break;count=take();
            }
            int size=take()*256+take(),sp=0;
            while(true) {
                if(++steps>budget)throw new IOException("BPE expansion budget");
                int ch;if(sp>0)ch=stack[--sp];else {if(size--==0)break;ch=take();}
                if(ch==left[ch]){if(n>=total)throw new IOException("BPE output overflow");output[n++]=(byte)ch;}
                else {if(sp>=510)throw new IOException("BPE stack overflow");stack[sp++]=right[ch];stack[sp++]=left[ch];}
            }
        }
        if(n!=total)throw new IOException("Incomplete portrait");return output;
    }
    static readonly int[,] Block32={{0, 1, 4, 5, 16, 17, 20, 21}, {2, 3, 6, 7, 18, 19, 22, 23}, {8, 9, 12, 13, 24, 25, 28, 29}, {10, 11, 14, 15, 26, 27, 30, 31}};
    static readonly int[,] Col32={{0, 1, 4, 5, 8, 9, 12, 13}, {2, 3, 6, 7, 10, 11, 14, 15}, {16, 17, 20, 21, 24, 25, 28, 29}, {18, 19, 22, 23, 26, 27, 30, 31},
                                     {32, 33, 36, 37, 40, 41, 44, 45}, {34, 35, 38, 39, 42, 43, 46, 47}, {48, 49, 52, 53, 56, 57, 60, 61}, {50, 51, 54, 55, 58, 59, 62, 63}};
    static readonly int[,] Col8={{0, 4, 16, 20, 32, 36, 48, 52, 2, 6, 18, 22, 34, 38, 50, 54}, {8, 12, 24, 28, 40, 44, 56, 60, 10, 14, 26, 30, 42, 46, 58, 62},
                                          {33, 37, 49, 53, 1, 5, 17, 21, 35, 39, 51, 55, 3, 7, 19, 23}, {41, 45, 57, 61, 9, 13, 25, 29, 43, 47, 59, 63, 11, 15, 27, 31}};
    static int A32(int bw,int x,int y){return (((y&~31)*bw+((x>>1)&~31)+Block32[(y>>3)&3,(x>>3)&7])<<6)+Col32[y&7,x&7];}
    static int A8(int bw,int x,int y){int col=(y&15)>>2,sx=(col&1)!=0?(x&15)^4:x&15;return ((((y>>1)&~31)*(bw>>1)+((x>>2)&~31)+Block32[(y>>4)&3,(x>>4)&7])<<8)+Col8[y&3,sx]+col*64;}
    public static Bitmap Decode(byte[] packed) {
        byte[] raw=Unpack(packed);int pixels=checked(Int(raw,32)*4+96),clut=checked(Int(raw,36)*4+96);
        if(pixels<0 || pixels>raw.Length-131072 || clut<0 || clut>raw.Length-1024)throw new IOException("Invalid portrait texture");
        byte[] mem=new byte[1048576],rgba=new byte[256*512*4];
        for(int y=0;y<256;y++)for(int x=0;x<128;x++)Buffer.BlockCopy(raw,pixels+(y*128+x)*4,mem,(A32(2,x,y)*4)&0xffffc,4);
        for(int y=0;y<512;y++)for(int x=0;x<256;x++) {
            int i=mem[A8(4,x,y)&0xfffff];i=(i&24)==8?i+8:(i&24)==16?i-8:i;
            int src=clut+i*4,dst=(y*256+x)*4;rgba[dst]=raw[src+2];rgba[dst+1]=raw[src+1];rgba[dst+2]=raw[src];rgba[dst+3]=(byte)Math.Min(255,raw[src+3]*2);
        }
        var bmp=new Bitmap(256,512,PixelFormat.Format32bppArgb);var data=bmp.LockBits(new Rectangle(0,0,256,512),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
        try{Marshal.Copy(rgba,0,data.Scan0,rgba.Length);}finally{bmp.UnlockBits(data);}return bmp;
    }
    public void Dispose(){lock(cache){disposed=true;foreach(var image in cache.Values)if(image!=null)image.Dispose();cache.Clear();}}
}
