// Standalone add-on. Build with the Windows .NET Framework C# compiler (x64).
// The game is never edited on disk. Only our own suspended child is patched.
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Windows.Forms;

public sealed class Patch { public int rva; public string expected, replacement; }
public sealed class Relocation { public int offset,sign,width; }
public sealed class Profile {
    public int format; public string game_sha256, payload_sha256,variant;
    public long image_base, allocation; public int allocation_size;
    public Patch[] patches; public Dictionary<string,int> symbols;
    public Relocation[] relocations;
    public byte[] resolved_payload;
    public int diagnostic_arena_rva,diagnostic_fps_rva,diagnostic_speed_rva;
}
static class Native {
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct STARTUPINFO { public int cb; public string reserved,desktop,title; public int x,y,xsize,ysize,xc,yc,fill,flags; public short show,cbReserved; public IntPtr reserved2,input,output,error; }
    [StructLayout(LayoutKind.Sequential)] public struct PROCESS_INFORMATION { public IntPtr process,thread; public int pid,tid; }
    [StructLayout(LayoutKind.Sequential)] public struct PBI { public IntPtr reserved,peb,reserved2,reserved3,pid,reserved4; }
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int left,top,right,bottom; }
    [StructLayout(LayoutKind.Sequential)] public struct POINT { public int x,y; }
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool CreateProcess(string app,StringBuilder cmd,IntPtr pa,IntPtr ta,bool inherit,uint flags,IntPtr env,string cwd,ref STARTUPINFO si,out PROCESS_INFORMATION pi);
    [DllImport("ntdll.dll")] public static extern int NtQueryInformationProcess(IntPtr process,int cls,ref PBI info,int length,out int returned);
    [DllImport("kernel32.dll",SetLastError=true)] public static extern bool ReadProcessMemory(IntPtr p,IntPtr address,byte[] b,IntPtr size,out IntPtr read);
    [DllImport("kernel32.dll",SetLastError=true)] public static extern bool WriteProcessMemory(IntPtr p,IntPtr address,byte[] b,IntPtr size,out IntPtr written);
    [DllImport("kernel32.dll",SetLastError=true)] public static extern IntPtr VirtualAllocEx(IntPtr p,IntPtr address,IntPtr size,uint type,uint protect);
    [DllImport("kernel32.dll",SetLastError=true)] public static extern bool VirtualProtectEx(IntPtr p,IntPtr address,IntPtr size,uint protect,out uint old);
    [DllImport("kernel32.dll")] public static extern bool FlushInstructionCache(IntPtr p,IntPtr address,IntPtr size);
    [DllImport("kernel32.dll")] public static extern uint ResumeThread(IntPtr t);
    [DllImport("kernel32.dll")] public static extern bool TerminateProcess(IntPtr p,uint code);
    [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr p);
    [DllImport("kernel32.dll")] public static extern IntPtr GetCurrentProcess();
    [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr w,out RECT r);
    [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr w,ref POINT p);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr w,out uint id);
    [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr w);
    [DllImport("user32.dll",SetLastError=true)] public static extern bool SetWindowPos(IntPtr w,IntPtr after,int x,int y,int width,int height,uint flags);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int key);
}

sealed class Memory {
    public IntPtr Handle;
    public Memory(IntPtr h) { Handle=h; }
    public byte[] Read(long a,int n) {
        byte[] b=new byte[n]; IntPtr read;
        if(!Native.ReadProcessMemory(Handle,new IntPtr(a),b,new IntPtr(n),out read) || read.ToInt64()!=n)
            throw new IOException(UiLanguage.Text("Leitura de memoria indisponivel em 0x")+a.ToString("X"));
        return b;
    }
    public int I32(long a) { return BitConverter.ToInt32(Read(a,4),0); }
    public uint U32(long a) { return BitConverter.ToUInt32(Read(a,4),0); }
    public long Pointer32(long a) { return U32(a); }
    public void Write(long a,byte[] b) {
        IntPtr written;
        if(!Native.WriteProcessMemory(Handle,new IntPtr(a),b,new IntPtr(b.Length),out written) || written.ToInt64()!=b.Length)
            throw new IOException(UiLanguage.Text("Nao foi possivel escrever o patch em memoria."));
    }
    public void Protect(long a,int n,uint protection) {
        uint old;
        if(!Native.VirtualProtectEx(Handle,new IntPtr(a),new IntPtr(n),protection,out old))
            throw new IOException(UiLanguage.Text("Nao foi possivel proteger a memoria do complemento."));
    }
}

public sealed class MoveRow {
    public string Button,Name,Cost; public bool Available; public bool Ultimate; public bool Selected;
    public MoveRow(string b,string n,string c,bool a,bool u) { Button=b;Name=n;Cost=c;Available=a;Ultimate=u; }
}

static class GameData {
    // The game's BtlText_FindEntry counts '*' records, returning +8 from '*'.
    // This is also the list used by its original on-screen technique names.
    public static List<string> ParseMoveNames(byte[] data) {
        List<string> names=new List<string>(); int p=0;
        if(data.Length>=2 && BitConverter.ToUInt16(data,0)==0xfeff) p=2;
        while(p+2<=data.Length && names.Count<5) {
            int start=p;
            while(p+2<=data.Length) {
                ushort ch=BitConverter.ToUInt16(data,p);
                if(ch==0 || ch==10 || ch==13) break;
                p+=2;
            }
            if(p+2>data.Length) break;
            if(BitConverter.ToUInt16(data,start)==0) break;
            int tag=start+4; // each line begins with two mask characters
            if(tag+2>p) { p+=2;continue; }
            if(BitConverter.ToUInt16(data,tag)==0x26) tag+=8; // &ddd
            if(tag+2>p) { p+=2;continue; }
            if(BitConverter.ToUInt16(data,tag)==0x40) break;
            if(BitConverter.ToUInt16(data,tag)==0x2a && p>=tag+8) {
                string name=Encoding.Unicode.GetString(data,tag+8,p-tag-8).Trim('\ufeff',' ','\t');
                if(name.Length>0 && name.Length<180 && !name.Any(c=>char.IsControl(c))) names.Add(name);
                else return new List<string>();
            }
            p+=2;
        }
        return names;
    }
    public static int SkillCost(int raw,bool discount) { return Math.Max(1,raw-(discount?1:0)); }
    public static int SuperCost(int raw,bool discount) { return discount?raw/2:raw; }
    public static string KiBars(int units) {
        // Display real bars, without a unit suffix. Half-bar item discounts
        // remain exact, written as a fraction instead of a misleading integer.
        int whole=units/20000,part=units%20000;
        if(part==0)return whole.ToString();
        if(part==10000)return (whole==0?"":whole.ToString())+"½";
        return (units/20000.0).ToString("0.##",System.Globalization.CultureInfo.InvariantCulture);
    }
    public static bool Ability(byte[] fighter,int slot,int bit) {
        int at=0x9a4+slot*0xa4+0x28+(bit/32)*4;
        return (BitConverter.ToUInt32(fighter,at)&(1u<<(bit%32)))!=0;
    }
}

// Positions come from the currently visible native HUD sprite, in the game's 512x448 space.
sealed class TechniquePrompt {
    public RectangleF NativeBounds;public int Texture;public Rectangle Uv;public string Button;public int Side,AspectMilli,Layout;
    public RectangleF Project(Size size) {
        float aspect=AspectMilli>=1000 && AspectMilli<=5000?AspectMilli/1000f:(float)size.Width/Math.Max(1,size.Height);
        float w=Math.Min(size.Width,size.Height*aspect),h=w/aspect,left=(size.Width-w)/2,top=(size.Height-h)/2;
        float narrow=aspect>1.34f?1.3333333f/aspect:1;
        float pivot=Layout==2?256:Side==0?0:512;
        float x=pivot+(NativeBounds.X-pivot)*narrow;
        return new RectangleF(left+x*w/512,top+NativeBounds.Y*h/448,NativeBounds.Width*narrow*w/512,NativeBounds.Height*h/448);
    }
    public void Draw(Graphics g,Size size,GameButtonIcons icons) {
        RectangleF box=Project(size);
        RectangleF ink=icons.TriangleInk(box,Texture,Uv);
        // Replace only the colored glyph; keep the original button shell, hand and animation.
        float margin=Math.Max(1,Math.Min(box.Width,box.Height)*.025f);
        RectangleF cover=RectangleF.Inflate(ink,Math.Max(margin,box.Width*.25f),Math.Max(margin,box.Height*.16f));
        cover.Intersect(new RectangleF(box.Left-box.Width*.1f,box.Top+box.Height*.25f,box.Width*1.2f,box.Height*.75f));
        using(var brush=new SolidBrush(Color.FromArgb(18,27,30)))g.FillRectangle(brush,cover);
        if(!icons.DrawFaceSymbol(g,Button,ink))using(var pen=new Pen(Color.FromArgb(255,145,205),Math.Max(2,ink.Width*.1f))) {
            var shape=RectangleF.Inflate(ink,-1,-1);
            if(Button=="○")g.DrawEllipse(pen,shape);else g.DrawRectangle(pen,shape.X,shape.Y,shape.Width,shape.Height);
        }
    }
}

sealed class Reader {
    readonly Memory m; readonly Profile p;
    readonly int[] switchReport={-1,-1,-1,-1};
    readonly int[] fusionReport={0,0,0,0};
    readonly uint[] oldSeq=new uint[4]; readonly long[] freshAt=new long[4];
    readonly Dictionary<long,List<string>> nameCache=new Dictionary<long,List<string>>();
    public Reader(Memory memory,Profile profile) { m=memory;p=profile; }
    public string NativePerformance {get{
        try{
        var result=new StringBuilder();
        if(p.diagnostic_arena_rva>0)result.Append(" port_arena=0x").Append(BitConverter.ToUInt64(m.Read(p.image_base+p.diagnostic_arena_rva,8),0).ToString("X"));
        if(p.diagnostic_fps_rva>0)result.Append(" game_fps=").Append(BitConverter.ToSingle(m.Read(p.image_base+p.diagnostic_fps_rva,4),0).ToString("F2",System.Globalization.CultureInfo.InvariantCulture));
        if(p.diagnostic_speed_rva>0)result.Append(" game_speed=").Append(BitConverter.ToSingle(m.Read(p.image_base+p.diagnostic_speed_rva,4),0).ToString("F2",System.Globalization.CultureInfo.InvariantCulture));
        return result.ToString();
        }catch(IOException){return " native_metrics=unavailable";}
    }}
    long Sym(string n) { return p.image_base+p.symbols[n]; }
    bool BattleActive {get{
        if(m.I32(Sym("gBattleWork")+0x19f8)!=0)return true;
        long manager=m.Pointer32(Sym("gBtlChars")),sequence=m.Pointer32(Sym("gBtlSeq"));
        return manager!=0&&sequence!=0&&m.I32(manager)==2&&m.Pointer32(manager+4)!=0&&m.I32(sequence)==3;
    }}
    public int AspectMilli { get { return m.I32(Sym("sAspectMilli")); } }
    // BattleSetup.rule.screenMode is at gBattleWork + 0x08 + 0x1c. Online presents one local view.
    public bool LocalSplit {get{return HudLayout.IsLocalSplit(m.I32(Sym("online_mode")),m.I32(Sym("gBattleWork")+0x24));}}
    public bool Quad {get{return p.variant=="tagteam"&&m.I32(Sym("online_mode"))==0&&m.I32(Sym("gBattleWork")+0x19f8)!=0&&m.I32(p.allocation+0xf900)==4;}}
    int PlayerCount {get{return Quad?4:2;}}
    long Mail(int side){return p.allocation+0xf000+(side/2)*0x1000;}
    bool Owns(long chr,int side){return p.variant=="tagteam"&&m.I32(Sym("online_mode"))==0?m.I32(chr+4)==side:m.I32(chr)==side;}
    public string MenuStatus {get;private set;}
    static int I(byte[] b,int off) { return BitConverter.ToInt32(b,off); }
    static string Text(byte[] data,int offset,int count){int end=offset;while(end<offset+count&&data[end]!=0)end++;return Encoding.UTF8.GetString(data,offset,end-offset);}
    public void RefreshCharacterArt(TransformationArt art,TransformationPanel[] panels){
        if(p.variant!="characters")return;
        bool online=m.I32(Sym("online_mode"))>0;
        foreach(int id in panels.SelectMany(panel=>panel.Options).Select(o=>o.Character).Distinct()){
            long address=0;
            if(!online&&id>=198&&id<255)address=Sym("sChara")+(id-198)*0x2464;
            if(!online&&id>=0&&id<161){int entry=BitConverter.ToInt16(m.Read(Sym("sReplOf")+id*2,2),0);if(entry>0&&entry<=161)address=Sym("sRepl")+(entry-1)*0x2464;}
            if(address==0){art.SetCustom(id,null);continue;}
            byte[] b=m.Read(address,0x2464);int baseId=I(b,4932);if(baseId<0||baseId>=161){art.SetCustom(id,null);continue;}
            art.SetCustom(id,new CustomCharacterArt {Base=baseId,Portrait=Text(b,5344,400),Name=Text(b,9036,64),NameSet=I(b,9100)!=0||id>=198,Label=Text(b,9104,96),LabelSet=I(b,9200)!=0});
        }
    }
    public TransformationPanel[] SampleTransformations() {
        var panels=new List<TransformationPanel>();
        if(!BattleActive || (m.U32(Sym("gBattleWork")+0x19f0)&0xff00)!=0)return panels.ToArray();
        bool online=m.I32(Sym("online_mode"))>0;int localSide=online?m.I32(p.allocation+0xf40c):-1;
        for(int side=0;side<PlayerCount;side++) {
            if(online&&side!=localSide)continue;
            if(Program.DiagnosticTelemetry){
            long fusion=Mail(side)+0x700+(side&1)*32;int phase=m.I32(fusion+8);
            if(phase!=fusionReport[side]) {
                fusionReport[side]=phase;long actor=m.Pointer32(Mail(side)+0x004+(side&1)*16);
                Program.SessionLog("Fusion phase="+phase+" side="+side+" reads="+m.I32(fusion+28)+" action="+(actor==0?-1:m.I32(actor+0x948)));
            }
            long diagnostic=Mail(side)+0x600+(side&1)*64;
            int status=m.I32(diagnostic+40);
            if(status!=switchReport[side]) {
                switchReport[side]=status;
                if(status>0)Program.SessionLog("Switch status="+status+" side="+side+" target="+m.I32(diagnostic)+" age="+m.I32(diagnostic+4)+" attempts="+m.I32(diagnostic+36));
            }
            }
            long address=Mail(side)+0x100+(side&1)*128;byte[] box=m.Read(address,128);uint seq=BitConverter.ToUInt32(box,0);
            if((seq&1)!=0 || m.U32(address)!=seq || (I(box,12)!=1 && I(box,12)!=2 && I(box,12)!=3))continue;
            long chr=BitConverter.ToUInt32(box,4);if(chr==0 || !Owns(chr,side) || m.I32(chr+0x1278)!=0 || m.I32(chr+0xc)!=I(box,16))continue;
            var options=new List<TransformationOption>();
            for(int slot=0;slot<4;slot++) {
                int at=32+slot*16,id=I(box,at),cost=I(box,at+4);
                if(I(box,at+12)==0 || id<0 || id>=255 || cost<0 || cost>25500000)continue;
                var option=new TransformationOption {Slot=slot,Character=id,Cost=cost/100000,Available=I(box,at+8)!=0};
                if(I(box,12)==2 && cost>=0 && cost<5){byte[] gauge=m.Read(chr+0x9a4+cost*0xa4+0x40,8);option.Health=Math.Max(0,I(gauge,0));option.HealthMax=Math.Max(1,I(gauge,4));}
                options.Add(option);
            }
            if(options.Count>0 || I(box,20)!=0)panels.Add(new TransformationPanel {Side=side,OnlineLocal=online,IsSwitch=I(box,12)==2,IsFusion=I(box,12)==3,HasFusion=I(box,20)!=0,HasForms=I(box,24)!=0,Options=options.ToArray()});
        }
        return panels.ToArray();
    }
    public TechniquePrompt[] SamplePrompts() {
        var result=new List<TechniquePrompt>();
        // The variant has only two shared native prompt-state rows. Do not draw
        // a guessed replacement over a different fighter in the four-view mode.
        if(Quad)return result.ToArray();
        if(!BattleActive || (m.U32(Sym("gBattleWork")+0x19f0)&0xf900)!=0)return result.ToArray();
        long hud=m.Pointer32(Sym("gHud"));if(hud==0 || (m.U32(hud+0x30)&0x10)==0)return result.ToArray();
        long prompt=m.Pointer32(Sym("gHudPrompt"));if(prompt==0)return result.ToArray();
        byte[] data=m.Read(prompt,0x2d0);bool online=m.I32(Sym("online_mode"))>0;
        int localSide=online?m.I32(p.allocation+0xf40c):-1;
        for(int side=0;side<Math.Min(2,PlayerCount);side++) {
            if(online&&side!=localSide)continue;
            long chr=m.Pointer32(Mail(side)+0x004+(side&1)*16);if(chr==0 || !Owns(chr,side) || m.I32(chr+0x1278)!=0)continue;
            int action=m.I32(chr+0x948),slot=(action-0x106)%3;
            if(action<0x106 || action>0x13b || slot==1)continue;
            long sprites=BitConverter.ToUInt32(data,4),nodes=BitConverter.ToUInt32(data,8);if(sprites==0 || nodes==0)continue;
            byte[] root=m.Read(nodes+23*0x38,0x38);if((I(root,0)&1)!=0)continue;
            int aspect=m.I32(Sym("sAspectMilli")),layout=m.I32(Sym("sHudLayout"));
            Action<int,int,bool> add=(nodeId,spriteId,logicalHold)=>{
                byte[] node=m.Read(nodes+nodeId*0x38,0x38),spr=m.Read(sprites+spriteId*0x1c,0x1c);
                // Nodes/sprites are shared by both HUD sides. The last rendered side
                // may have hidden them; the selected side's hold state is authoritative.
                if(!logicalHold&&((I(node,0)&1)!=0 || (I(spr,0)&1)!=0 || spr[0x1b]<16))return;
                float nx=I(root,0x10)+I(root,0x18)+I(node,0x10)+I(node,0x18);
                float ny=I(root,0x14)+I(root,0x1c)+I(node,0x14)+I(node,0x1c);
                float x0=nx+BitConverter.ToInt16(spr,8),x1=nx+BitConverter.ToInt16(spr,10);
                float y0=ny+BitConverter.ToInt16(spr,12),y1=ny+BitConverter.ToInt16(spr,14);
                bool mirror=online?false:((I(root,0)|I(node,0))&2)!=0;
                if(mirror){float z=511-x1;x1=511-x0;x0=z;}
                if(x1<=x0 || y1<=y0 || x1-x0>96 || y1-y0>96)return;
                result.Add(new TechniquePrompt {NativeBounds=new RectangleF(x0,y0,x1-x0,y1-y0),Texture=BitConverter.ToInt16(spr,4),Uv=Rectangle.FromLTRB(BitConverter.ToInt16(spr,16),BitConverter.ToInt16(spr,20),BitConverter.ToInt16(spr,18),BitConverter.ToInt16(spr,22)),Side=online?0:side,AspectMilli=aspect,Layout=layout,Button=slot==0?"□":"○"});
            };
            int state=I(data,0x18+side*4),kind=I(data,0xc0+side*4);
            // Column 2 is the native Triangle. Column 1 already shows Square.
            if(state>=1 && state<=9 && kind>=0 && kind<12 && (kind&3)==2)add(24,1,true);
            int rowState=I(data,0xe8+side*4),count=I(data,0x120+side*4);
            // BtlCharApi_GetButtonIcon uses Triangle=0, Square=1, Circle=2, X=3.
            // This is NOT the numbering used by HudPrompt_SetButton (Triangle=2).
            // Never cover Circle (2) or an already correct native Square (1).
            if(rowState>=0 && rowState<=9 && count>=1 && count<=4)
                for(int i=0;i<count;i++) {
                    int icon=I(data,0x128+side*0x60+i*0x18);
                    if(icon==0){add(26,6+i,false);add(26,14+i,false);}
                }
        }
        return result.ToArray();
    }
    public MoveRow[][] Sample() {
        MoveRow[][] result=new MoveRow[PlayerCount][];MenuStatus="waiting for fresh charge input";
        if(!BattleActive) { MenuStatus="battle inactive";nameCache.Clear(); return result; }
        uint flags=m.U32(Sym("gBattleWork")+0x19f0);
        if((flags&0xe700)!=0){MenuStatus="battle flags="+flags.ToString("X");return result;} // pause, demo, ready, loading, restart
        if(m.Pointer32(Sym("gBtlChars"))==0 || m.Pointer32(Sym("gBtlSeq"))==0){MenuStatus="battle pointers unavailable";return result;}
        long hud=m.Pointer32(Sym("gHud"));
        if(hud==0 || (m.U32(hud+0x30)&1)==0){MenuStatus="native HUD hidden";return result;}
        bool online=m.I32(Sym("online_mode"))>0;int localSide=online?m.I32(p.allocation+0xf40c):-1;
        for(int side=0;side<PlayerCount;side++) {
            if(online&&side!=localSide)continue;
            long mail=Mail(side)+0x000+(side&1)*16;
            byte[] box=m.Read(mail,16); uint seq=BitConverter.ToUInt32(box,0);
            if((seq&1)!=0 || m.U32(mail)!=seq) continue;
            if(seq!=oldSeq[side]) { oldSeq[side]=seq; freshAt[side]=Environment.TickCount; }
            if(unchecked((uint)(Environment.TickCount-(int)freshAt[side]))>200) continue;
            uint raw=BitConverter.ToUInt32(box,8);
            if((raw&0x200)==0) continue;MenuStatus="charge input; actor metadata pending";
            long chr=BitConverter.ToUInt32(box,4);
            if(chr==0) continue;
            byte[] fighter=m.Read(chr,0x1600);
            if(!Owns(chr,side) || I(fighter,0x1278)!=0) continue;
            int slot=I(fighter,0x994),count=I(fighter,0x998),objid=I(fighter,0xc);
            if(slot<0 || slot>=count || count>5 || objid<0 || objid>=4096) continue;
            long obj=m.Pointer32(Sym("gBtlObjTbl")+objid*4);
            if(obj==0) continue;
            byte[] pointers=m.Read(obj+0x92c,8);
            long super=BitConverter.ToUInt32(pointers,0), skill=BitConverter.ToUInt32(pointers,4);
            if(super==0 || skill==0) continue;
            byte[] supers=m.Read(super,0x1a8),skills=m.Read(skill,0x98);
            long script=m.Pointer32(obj+0xbc);
            if(script==0) continue;
            long nameKey=script|((long)m.U32(obj+0xc)<<32);
            List<string> names;
            if(!nameCache.TryGetValue(nameKey,out names)) {
                // Read line by line; do not cross an unmapped allocation boundary.
                List<byte> text=new List<byte>();
                bool ended=false;
                for(int off=0;off<65536 && !ended;) {
                    int chunk=(int)Math.Min(256,4096-((script+off)&4095));
                    byte[] part=m.Read(script+off,chunk);
                    for(int j=0;j+1<part.Length;j+=2) {
                        text.Add(part[j]);text.Add(part[j+1]);
                        if(part[j]==0 && part[j+1]==0) { ended=true; break; }
                    }
                    off+=chunk;
                }
                names=GameData.ParseMoveNames(text.ToArray());
                if(names.Count==5) nameCache[nameKey]=names;
            }
            if(names.Count!=5){MenuStatus="charge input; move names unavailable";continue;} // never label unknown data as a real move
            int gauge=0x9a4+slot*0xa4+0x40;
            int ki=I(fighter,gauge+12),stocks=I(fighter,gauge+20);
            bool half=GameData.Ability(fighter,slot,0x2b),less=GameData.Ability(fighter,slot,0x15);
            List<MoveRow> rows=new List<MoveRow>();
            for(int k=0;k<2;k++) {
                int cost=GameData.SkillCost(unchecked((sbyte)skills[0x94+k]),less);
                rows.Add(new MoveRow(k==0?"R2 + ↑ / ↓":"R2 + ← / →",names[k],cost.ToString(),stocks>=cost*100000,false));
            }
            for(int k=0;k<3;k++) {
                int cost=GameData.SuperCost(I(supers,0x194+(k+2)*4),half);
                if(cost<0 || cost>1000000) throw new IOException(UiLanguage.Text("Tabela de custo inesperada."));
                string unit=GameData.KiBars(cost);
                bool maxPower=((fighter[0x10ad]|fighter[0x1085])&0x40)!=0;
                rows.Add(new MoveRow(new[]{"R2 + □","R2 + △","R2 + ○"}[k],names[k+2],unit,ki>=cost && (k!=2 || maxPower),k==2));
            }
            uint face=raw&0xb000;int selected=-1;
            if(face==0x8000)selected=2;else if(face==0x1000)selected=3;else if(face==0x2000)selected=4;
            else if(face==0){if((raw&0x50)!=0 && (raw&0xa0)==0)selected=0;else if((raw&0xa0)!=0 && (raw&0x50)==0)selected=1;}
            for(int i=0;i<rows.Count;i++)rows[i].Selected=i==selected;
            result[online?0:side]=rows.ToArray();MenuStatus="moves ready";
        }
        return result;
    }
}

// Overlay panels follow the rendered picture, not decorative bars around it.
// Choose a centered 3:2, expanded 16:9 or classic 4:3 control-safe region.
enum PanelLayoutMode { Centered,Expanded,Classic,ScreenEdges }
static class SavedSettings {
    internal static string OverrideDirectory {get;set;}
    public static string CommonRoot {get{return OverrideDirectory??Path.GetDirectoryName(Program.Root);}}
    public static string LegacyRoot {get{return OverrideDirectory??Program.Root;}}
    public static string FilePath(string name){return Resolve(CommonRoot,LegacyRoot,name);}
    public static string Resolve(string commonRoot,string legacyRoot,string name){
        Directory.CreateDirectory(commonRoot);string target=Path.Combine(commonRoot,name);if(File.Exists(target))return target;
        string legacy=Path.Combine(legacyRoot,name);
        if(!File.Exists(legacy)){
            legacy=Directory.GetDirectories(commonRoot,"online-test-*").Select(dir=>new FileInfo(Path.Combine(dir,name))).Where(f=>f.Exists).OrderByDescending(f=>f.LastWriteTimeUtc).Select(f=>f.FullName).FirstOrDefault();
        }
        if(legacy!=null&&File.Exists(legacy)){
            try{File.Copy(legacy,target,false);}catch(IOException){if(!File.Exists(target))throw;}
        }
        return target;
    }
}

static class PanelPlacement {
    public static PanelLayoutMode Mode {get;private set;}
    public static bool Expanded {get {return Mode==PanelLayoutMode.Expanded;}}
    public static string FilePath {get {return Path.Combine(SavedSettings.CommonRoot,"panel-layout.txt");}}
    public static void Load() {
        Mode=PanelLayoutMode.Centered;
        try {string file=File.Exists(FilePath)?FilePath:Path.Combine(SavedSettings.LegacyRoot,"panel-layout.txt");if(File.Exists(file)){string value=File.ReadAllText(file).Trim();Mode=value=="edges"?PanelLayoutMode.ScreenEdges:value=="classic"?PanelLayoutMode.Classic:value=="expanded"?PanelLayoutMode.Expanded:PanelLayoutMode.Centered;}}
        catch(IOException){}catch(UnauthorizedAccessException){}
    }
    public static void Set(bool expanded,bool persist){Set(expanded?PanelLayoutMode.Expanded:PanelLayoutMode.Centered,persist);}
    public static void Set(PanelLayoutMode mode,bool persist){
        if(mode<PanelLayoutMode.Centered||mode>PanelLayoutMode.ScreenEdges)mode=PanelLayoutMode.Centered;
        if(persist){Directory.CreateDirectory(SavedSettings.CommonRoot);File.WriteAllText(FilePath,mode==PanelLayoutMode.ScreenEdges?"edges":mode==PanelLayoutMode.Classic?"classic":mode==PanelLayoutMode.Expanded?"expanded":"centered");}Mode=mode;
    }
}

public sealed class ShortcutConfig {public int layout,frame;public int? attacks;}
static class ShortcutBindings {
    public static Keys Layout {get;private set;}public static Keys Frame {get;private set;}public static Keys Attacks {get;private set;}
    public static bool Locked {get;private set;}
    public static void SetLocked(bool value,bool persist){if(persist){Directory.CreateDirectory(SavedSettings.CommonRoot);File.WriteAllText(Path.Combine(SavedSettings.CommonRoot,"shortcut-lock.txt"),value?"locked":"unlocked");}Locked=value;}
    public const Keys DefaultLayout=Keys.F8,DefaultFrame=Keys.F7,DefaultAttacks=Keys.F6;
    static ShortcutBindings(){Layout=DefaultLayout;Frame=DefaultFrame;Attacks=DefaultAttacks;}
    public static string FilePath {get{return Path.Combine(SavedSettings.CommonRoot,"keyboard-shortcuts.json");}}
    public static bool Valid(Keys value){int code=(int)(value&Keys.KeyCode);return value==Keys.None||(code>=8&&code<=254&&code!=16&&code!=17&&code!=18&&code!=91&&code!=92&&((int)value&~0x7ffff)==0);}
    public static void Load(){
        try{string lockFile=Path.Combine(SavedSettings.CommonRoot,"shortcut-lock.txt");Locked=File.Exists(lockFile)&&File.ReadAllText(lockFile).Trim()=="locked";}catch(IOException){}catch(UnauthorizedAccessException){}
        try{if(!File.Exists(FilePath)){Layout=DefaultLayout;Frame=DefaultFrame;Attacks=DefaultAttacks;return;}var c=new JavaScriptSerializer().Deserialize<ShortcutConfig>(File.ReadAllText(FilePath));if(c==null)return;Keys layout=(Keys)c.layout,frame=(Keys)c.frame,attacks=c.attacks.HasValue?(Keys)c.attacks.Value:DefaultAttacks;if(!c.attacks.HasValue&&(attacks==layout||attacks==frame))attacks=Keys.None;if(Valid(layout)&&Valid(frame)&&Valid(attacks)&&Unique(layout,frame,attacks)){Layout=layout;Frame=frame;Attacks=attacks;}}

        catch(IOException){}catch(UnauthorizedAccessException){}catch(ArgumentException){}catch(InvalidOperationException){}
    }
    static bool Unique(Keys layout,Keys frame,Keys attacks){return (layout==Keys.None||layout!=frame)&&(attacks==Keys.None||(attacks!=layout&&attacks!=frame));}
    public static void Set(Keys layout,Keys frame,bool persist){Set(layout,frame,Attacks,persist);}
    public static void Set(Keys layout,Keys frame,Keys attacks,bool persist){
        if(!Valid(layout)||!Valid(frame)||!Valid(attacks))throw new IOException(UiLanguage.Text("Atalho inválido."));
        if(!Unique(layout,frame,attacks))throw new IOException(UiLanguage.Text("Essa combinação já está atribuída ao outro atalho."));
        if(persist){Directory.CreateDirectory(SavedSettings.CommonRoot);File.WriteAllText(FilePath,new JavaScriptSerializer().Serialize(new ShortcutConfig {layout=(int)layout,frame=(int)frame,attacks=(int)attacks}));}Layout=layout;Frame=frame;Attacks=attacks;
    }
    public static string Label(Keys key){if(key==Keys.None)return UiLanguage.Text("Definir...");return ((key&Keys.Control)!=0?"Ctrl + ":"")+((key&Keys.Alt)!=0?"Alt + ":"")+((key&Keys.Shift)!=0?"Shift + ":"")+(key&Keys.KeyCode).ToString();}
    public static bool IsDown(Keys binding){
        if(binding==Keys.None)return false;bool control=(Native.GetAsyncKeyState(0x11)&0x8000)!=0,alt=(Native.GetAsyncKeyState(0x12)&0x8000)!=0,shift=(Native.GetAsyncKeyState(0x10)&0x8000)!=0;
        return control==((binding&Keys.Control)!=0)&&alt==((binding&Keys.Alt)!=0)&&shift==((binding&Keys.Shift)!=0)&&(Native.GetAsyncKeyState((int)(binding&Keys.KeyCode))&0x8000)!=0;
    }
}
sealed class AttackShortcut {
    bool held;public bool Update(bool down,bool gameFocused){bool pressed=down&&!held;held=down;if(!pressed||!gameFocused||ShortcutBindings.Locked)return false;HudPreferences.SetShowAttacks(!HudPreferences.ShowAttacks,true);return true;}
}
sealed class FrameShortcut {
    bool held;public bool Update(bool down,bool gameFocused){bool pressed=down&&!held;held=down;if(!pressed||!gameFocused||ShortcutBindings.Locked)return false;AttackAppearance.SetOuterFrame(!AttackAppearance.OuterFrame,true);return true;}
}
sealed class KeyCaptureDialog : Form {
    public Keys Selected {get;private set;}
    public KeyCaptureDialog(Keys current,Keys defaultKey){
        Selected=current;Text=UiLanguage.Text("Atalho de teclado");Icon=AppBrand.WindowIcon;StartPosition=FormStartPosition.CenterParent;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;ShowInTaskbar=false;ClientSize=new Size(600,192);BackColor=LauncherTheme.Background;ForeColor=LauncherTheme.Text;Font=new Font("Segoe UI",10f);Padding=new Padding(20);RightToLeft=UiLanguage.IsRightToLeft?RightToLeft.Yes:RightToLeft.No;RightToLeftLayout=UiLanguage.IsRightToLeft;
        var grid=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};grid.RowStyles.Add(new RowStyle(SizeType.Absolute,48));grid.RowStyles.Add(new RowStyle(SizeType.Percent,100));grid.RowStyles.Add(new RowStyle(SizeType.Absolute,54));
        grid.Controls.Add(new Label {Text=UiLanguage.Text("Pressione a nova tecla ou combinaÃ§Ã£o."),Dock=DockStyle.Fill,ForeColor=LauncherTheme.Text,Font=new Font("Segoe UI Semibold",11f)},0,0);
        grid.Controls.Add(new Label {Text=UiLanguage.Text("Prefira uma tecla que vocÃª nÃ£o usa no jogo."),Dock=DockStyle.Fill,ForeColor=LauncherTheme.Muted},0,1);
        var buttons=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft,WrapContents=false};var cancel=new RoundedButton {Name="cancelShortcut",Text=UiLanguage.Text("Cancelar"),AutoSize=true,DialogResult=DialogResult.Cancel};var clear=new RoundedButton {Name="clearShortcut",Text=UiLanguage.Text("Remover atalho"),AutoSize=true};clear.Click+=(o,e)=>{Selected=Keys.None;DialogResult=DialogResult.OK;};var reset=new RoundedButton {Name="resetShortcut",Text=UiLanguage.Text("Restaurar padrão"),AutoSize=true};reset.Click+=(o,e)=>{Selected=defaultKey;DialogResult=DialogResult.OK;};buttons.Controls.Add(cancel);buttons.Controls.Add(clear);buttons.Controls.Add(reset);grid.Controls.Add(buttons,0,2);Controls.Add(grid);CancelButton=cancel;
    }
    protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);}
    protected override bool ProcessCmdKey(ref Message msg,Keys keyData){if((keyData&Keys.KeyCode)==Keys.Escape){DialogResult=DialogResult.Cancel;return true;}if(ShortcutBindings.Valid(keyData)&&keyData!=Keys.None){Selected=keyData;DialogResult=DialogResult.OK;}return true;}
}

sealed class LayoutShortcut {
    bool held;
    public bool Update(bool down,bool gameFocused){bool pressed=down&&!held;held=down;if(!pressed||!gameFocused||ShortcutBindings.Locked)return false;PanelPlacement.Set((PanelLayoutMode)(((int)PanelPlacement.Mode+1)%4),true);return true;}
}

static class HudPreferences {
    static HudPreferences(){ShowAttacks=true;}
    public static bool ShowAttacks {get;private set;}
    public static string FilePath {get{return Path.Combine(SavedSettings.CommonRoot,"attack-panel.txt");}}
    public static void Load(){ShowAttacks=true;try{if(File.Exists(FilePath))ShowAttacks=File.ReadAllText(FilePath).Trim()!="hidden";}catch(IOException){}catch(UnauthorizedAccessException){}}
    public static void SetShowAttacks(bool show,bool persist){if(persist){Directory.CreateDirectory(SavedSettings.CommonRoot);File.WriteAllText(FilePath,show?"shown":"hidden");}ShowAttacks=show;}
}

static class AttackAppearance {
    public static bool OuterFrame {get;private set;}
    public static string FilePath {get{return Path.Combine(SavedSettings.CommonRoot,"attack-frame.txt");}}
    public static void Load(){OuterFrame=false;try{if(File.Exists(FilePath))OuterFrame=File.ReadAllText(FilePath).Trim()=="framed";}catch(IOException){}catch(UnauthorizedAccessException){}}
    public static void SetOuterFrame(bool show,bool persist){if(persist){Directory.CreateDirectory(SavedSettings.CommonRoot);File.WriteAllText(FilePath,show?"framed":"cards");}OuterFrame=show;}
}

static class HudLayout {
    public static RectangleF Picture(Size size,int aspectMilli) {
        float cw=Math.Max(1,size.Width),ch=Math.Max(1,size.Height);
        float aspect=aspectMilli>=1333&&aspectMilli<=4000?aspectMilli/1000f:cw/ch;
        float w=Math.Min(cw,ch*aspect),h=w/aspect;
        return new RectangleF((cw-w)/2,(ch-h)/2,w,h);
    }
    public static RectangleF Controls(Size size,int aspectMilli,bool expanded=false){return Controls(size,aspectMilli,expanded?PanelLayoutMode.Expanded:PanelLayoutMode.Centered);}
    public static RectangleF Controls(Size size,int aspectMilli,PanelLayoutMode mode) {
        RectangleF picture=Picture(size,aspectMilli);if(mode==PanelLayoutMode.ScreenEdges)return picture;float limit=mode==PanelLayoutMode.Classic?4f/3f:mode==PanelLayoutMode.Expanded?16f/9f:3f/2f;float width=Math.Min(picture.Width,picture.Height*limit);
        return new RectangleF(picture.X+(picture.Width-width)/2,picture.Y,width,picture.Height);
    }
    public static bool IsLocalSplit(int onlineMode,int screenMode){return onlineMode==0&&screenMode==1;}
    public static RectangleF PlayerArea(Size size,int aspectMilli,PanelLayoutMode mode,int side,bool localSplit){return PlayerArea(size,aspectMilli,mode,side,localSplit,false);}
    public static RectangleF PlayerArea(Size size,int aspectMilli,PanelLayoutMode mode,int side,bool localSplit,bool quad){
        if(quad){
            RectangleF quadPicture=Picture(size,aspectMilli);float vw=quadPicture.Width/2,vh=quadPicture.Height/2;
            var quadView=new RectangleF(quadPicture.Left+(side%2)*vw,quadPicture.Top+(side/2)*vh,vw,vh);
            if(mode==PanelLayoutMode.ScreenEdges)return quadView;
            float quadLimit=mode==PanelLayoutMode.Classic?4f/3f:mode==PanelLayoutMode.Expanded?16f/9f:3f/2f,quadWidth=Math.Min(quadView.Width,quadView.Height*quadLimit);
            return new RectangleF(quadView.Left+(quadView.Width-quadWidth)/2,quadView.Top,quadWidth,quadView.Height);
        }
        if(!localSplit)return Controls(size,aspectMilli,mode);
        RectangleF picture=Picture(size,aspectMilli);
        var view=new RectangleF(picture.Left+(side==0?0:picture.Width/2),picture.Top,picture.Width/2,picture.Height);
        if(mode==PanelLayoutMode.ScreenEdges)return view;
        float limit=mode==PanelLayoutMode.Classic?4f/3f:mode==PanelLayoutMode.Expanded?16f/9f:3f/2f;
        float width=Math.Min(view.Width,view.Height*limit);
        return new RectangleF(view.Left+(view.Width-width)/2,view.Top,width,view.Height);
    }
    public static RectangleF Dock(RectangleF area,SizeF panel,int side,bool top) {
        float margin=area.Width*.025f,x=side==0?area.Left+margin:area.Right-margin-panel.Width;
        float y=top?area.Top+area.Height*.30f:area.Bottom-area.Height*.065f-panel.Height;
        x=Math.Max(area.Left,Math.Min(x,area.Right-panel.Width));y=Math.Max(area.Top,Math.Min(y,area.Bottom-panel.Height));
        return new RectangleF(x,y,panel.Width,panel.Height);
    }
}

// Purely visual, monotonic-time transition; no input reads, waits or game writes.
sealed class HudMenuFade : IDisposable {
    public const double InSeconds=.100,OutSeconds=.080;
    public float Opacity {get;private set;}
    public bool Target {get;private set;}
    public Bitmap Image {get;private set;}
    public Rectangle Bounds {get;private set;}
    double last=-1;
    public bool Update(bool visible,double now){
        double dt=last<0?0:Math.Max(0,now-last);last=now;Target=visible;
        float before=Opacity;
        Opacity=(float)(visible?Math.Min(1,Opacity+dt/InSeconds):Math.Max(0,Opacity-dt/OutSeconds));
        if(Opacity<.0001f)Opacity=0;else if(Opacity>.9999f)Opacity=1;
        if(!visible&&Opacity==0)DiscardImage();
        return before!=Opacity;
    }
    public void Cache(Bitmap image,Rectangle bounds){DiscardImage();Image=image;Bounds=bounds;}
    void DiscardImage(){if(Image!=null){Image.Dispose();Image=null;}Bounds=Rectangle.Empty;}
    public void Draw(Graphics g){
        if(Image==null||Opacity<=0)return;
        if(Opacity>=1){g.DrawImageUnscaled(Image,Bounds.Location);return;}
        using(var attributes=new System.Drawing.Imaging.ImageAttributes()){
            var matrix=new System.Drawing.Imaging.ColorMatrix();matrix.Matrix33=Opacity;attributes.SetColorMatrix(matrix);
            g.DrawImage(Image,Bounds,0,0,Image.Width,Image.Height,GraphicsUnit.Pixel,attributes);
        }
    }
    public void Reset(){Opacity=0;Target=false;last=-1;DiscardImage();}
    public void Dispose(){Reset();}
}

sealed class Overlay : Form {
    Process game; Reader reader; readonly Timer timer;readonly GameSession session;
    readonly PrivateFontCollection battleFonts=new PrivateFontCollection();
    readonly FontFamily battleFamily; readonly FontFamily menuFamily;
    readonly PrivateFontCollection menuFonts=new PrivateFontCollection();
    readonly GameMenuSkin menuSkin; readonly NativeMenuMetrics menuMetrics; Bitmap alphaCanvas;string lastVisualKey;
    Bitmap menuScratch;bool updateMenuSnapshots,measureMenuOnly;Rectangle alphaBounds;
    readonly HudMenuFade[] attackFades={new HudMenuFade(),new HudMenuFade(),new HudMenuFade(),new HudMenuFade()},cardFades={new HudMenuFade(),new HudMenuFade(),new HudMenuFade(),new HudMenuFade()};
    bool quad;Rectangle gameBounds;
    Size ViewportSize {get{return preview||gameBounds.Width<1?ClientSize:gameBounds.Size;}}
    FileSystemWatcher preferenceWatcher;volatile bool preferencesDirty=true;DateTime nextPreferenceSafetyRead=DateTime.MinValue;
    DateTime nextWindowRefresh=DateTime.MinValue;IntPtr cachedGameWindow;
    DateTime nextPresentation=DateTime.MinValue,nextPreferenceRead=DateTime.MinValue,nextHudError=DateTime.MinValue,nextHudNotice=DateTime.MinValue;string lastHudNotice;
    readonly GameButtonIcons buttonIcons; readonly TransformationArt transformationArt;
    TransformationPanel[] transformations=new TransformationPanel[0];
    MoveRow[][] rows=new MoveRow[4][]; TechniquePrompt[] prompts=new TechniquePrompt[0]; readonly bool preview;
    readonly RectangleF[] panelBounds=new RectangleF[4];
    bool localSplit;
    int aspectMilli;readonly LayoutShortcut layoutShortcut=new LayoutShortcut();readonly FrameShortcut frameShortcut=new FrameShortcut();bool frameShortcutNotice,attackShortcutNotice;readonly AttackShortcut attackShortcut=new AttackShortcut();DateTime layoutNoticeUntil=DateTime.MinValue;
    RectangleF ControlArea { get { return HudLayout.Controls(ViewportSize,aspectMilli,PanelPlacement.Mode); } }
    public void SetPreviewAspect(int value) { aspectMilli=value; }
    public Overlay(Process process,Reader r,bool demo,string gameExe=null,GameSession chain=null) {
        game=process;reader=r;preview=demo;session=chain;
        if(!demo){PanelPlacement.Load();HudPreferences.Load();AttackAppearance.Load();ShortcutBindings.Load();WatchPreferences();Program.SessionLog("HUD preferences: "+PanelPlacement.Mode+"; attacks="+HudPreferences.ShowAttacks+"; settings="+PanelPlacement.FilePath);}
        if(gameExe==null) { string saved=SavedSettings.FilePath("game-path.txt");if(File.Exists(saved))gameExe=File.ReadAllText(saved).Trim(); }
        buttonIcons=new GameButtonIcons(gameExe);transformationArt=new TransformationArt(gameExe);menuSkin=new GameMenuSkin(gameExe);menuMetrics=new NativeMenuMetrics(gameExe);
        string fontPath=Path.Combine(Program.Root,"assets","Anton-Regular.ttf");
        if(File.Exists(fontPath))battleFonts.AddFontFile(fontPath);
        battleFamily=battleFonts.Families.Length>0?battleFonts.Families[0]:FontFamily.GenericSansSerif;
        string uiFont=GameMenuSkin.UiFontPath(gameExe);
        if(uiFont!=null)try{menuFonts.AddFontFile(uiFont);}catch(ArgumentException){}catch(IOException){}
        menuFamily=menuFonts.Families.Length>0?menuFonts.Families[0]:FontFamily.GenericSansSerif;
        FormBorderStyle=FormBorderStyle.None; ShowInTaskbar=false; TopMost=true;
        BackColor=Color.Black;TransparencyKey=Color.Empty;DoubleBuffered=true;
        Text="BT3 Sparking Controls - ONLINE TEST HUD"; StartPosition=FormStartPosition.Manual;
        timer=new Timer();timer.Interval=16;timer.Tick+=Tick;timer.Start();
        if(preview) {
            Bounds=new Rectangle(80,80,1280,720);BackColor=Color.FromArgb(25,32,48);TransparencyKey=Color.Empty;
            rows[0]=new[]{new MoveRow("R2 + ↑ / ↓","Instantaneous Transmission","2",true,false),new MoveRow("R2 + ← / →","I'll Beat You In 5 Seconds!","2",true,false),new MoveRow("R2 + □","10x Kamehameha","3",true,false),new MoveRow("R2 + △","Meteor Crash","3",false,false),new MoveRow("R2 + ○","Dragon Fist","5",false,true)};rows[0][0].Selected=true;
        }
    }
    protected override bool ShowWithoutActivation { get { return !preview; } }
    protected override CreateParams CreateParams {
        get { var cp=base.CreateParams;if(!preview) cp.ExStyle|=0x08000020|0x00080000|0x80;return cp; }
    }
    DateTime nextPerformanceLog=DateTime.UtcNow.AddSeconds(30);long hudTicks;double hudMilliseconds,hudMaxMilliseconds;
    [DllImport("user32.dll")] static extern int GetGuiResources(IntPtr process,int flags);
    void RecordHudCost(long started){
        if(!Program.DiagnosticTelemetry)return;
        double ms=(Stopwatch.GetTimestamp()-started)*1000.0/Stopwatch.Frequency;hudTicks++;hudMilliseconds+=ms;hudMaxMilliseconds=Math.Max(hudMaxMilliseconds,ms);
        if(DateTime.UtcNow<nextPerformanceLog)return;nextPerformanceLog=DateTime.UtcNow.AddSeconds(30);
        try{using(var own=Process.GetCurrentProcess())Program.SessionLog("HUD performance: ticks="+hudTicks+" avg_ms="+(hudMilliseconds/Math.Max(1,hudTicks)).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+" max_ms="+hudMaxMilliseconds.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+" private_mb="+(own.PrivateMemorySize64/1048576)+" handles="+own.HandleCount+" gdi="+GetGuiResources(own.Handle,0)+" cpu_s="+own.TotalProcessorTime.TotalSeconds.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+" ink_scans="+buttonIcons.TriangleInkScans+" presents="+AlphaHud.Presents+" dib_creates="+AlphaHud.SurfaceCreates+" copied_mb="+(AlphaHud.CopiedBytes/1048576)+" overlay="+Size+" viewport="+ViewportSize+" game_private_mb="+(game.PrivateMemorySize64/1048576)+" game_working_mb="+(game.WorkingSet64/1048576)+" game_handles="+game.HandleCount+" game_cpu_s="+game.TotalProcessorTime.TotalSeconds.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+(session==null?"":session.Performance)+GameSession.SystemMemoryPerformance+reader.NativePerformance);}catch(IOException){}catch(UnauthorizedAccessException){}catch(System.ComponentModel.Win32Exception){}
        hudTicks=0;hudMilliseconds=0;hudMaxMilliseconds=0;
    }
    void Tick(object sender,EventArgs e) {
        if(preview) { Invalidate();return; }
        if(session!=null) {
            var snapshot=session.Latest;
            if(snapshot!=null && snapshot.Pid!=game.Id) {
                game=Process.GetProcessById(snapshot.Pid);nextWindowRefresh=DateTime.MinValue;cachedGameWindow=IntPtr.Zero;reader=new Reader(new Memory(snapshot.Handle),snapshot.Profile);Clear();
                Program.SessionLog("HUD follows game generation "+snapshot.Generation+"; PID "+snapshot.Pid);
            }
            if(session.Done) {Close();return;}
            if(game.HasExited) {Clear();return;}
        }else if(game.HasExited) { Close();return; }
        long hudStarted=Program.DiagnosticTelemetry?Stopwatch.GetTimestamp():0;
        try {
            if(DateTime.UtcNow>=nextWindowRefresh){game.Refresh();cachedGameWindow=game.MainWindowHandle;nextWindowRefresh=DateTime.UtcNow.AddMilliseconds(250);}
            IntPtr hwnd=cachedGameWindow; uint foregroundPid;
            Native.GetWindowThreadProcessId(Native.GetForegroundWindow(),out foregroundPid);
            Native.RECT rect;
            bool shortcutDown=ShortcutBindings.IsDown(ShortcutBindings.Layout),frameDown=ShortcutBindings.IsDown(ShortcutBindings.Frame),attackDown=ShortcutBindings.IsDown(ShortcutBindings.Attacks);
            if(hwnd==IntPtr.Zero || foregroundPid!=game.Id || Native.IsIconic(hwnd) || !Native.GetClientRect(hwnd,out rect)) { layoutShortcut.Update(shortcutDown,false);frameShortcut.Update(frameDown,false);attackShortcut.Update(attackDown,false);HudNotice("window inactive; game="+game.Id+"; foreground="+foregroundPid);Clear();return; }
            Native.POINT pos=new Native.POINT();Native.ClientToScreen(hwnd,ref pos);
            Rectangle bounds=new Rectangle(pos.x,pos.y,rect.right,rect.bottom);
            if(gameBounds!=bounds){gameBounds=bounds;lastVisualKey=null;}
            RefreshSavedHudPreferences();
            try{if(layoutShortcut.Update(shortcutDown,true)){frameShortcutNotice=false;attackShortcutNotice=false;lastVisualKey=null;layoutNoticeUntil=DateTime.UtcNow.AddSeconds(1.8);Program.SessionLog("HUD layout shortcut: "+PanelPlacement.Mode);}}catch(IOException ex){HudReadError("shortcut setting",ex);}catch(UnauthorizedAccessException ex){HudReadError("shortcut setting",ex);}
            try{if(frameShortcut.Update(frameDown,true)){frameShortcutNotice=true;attackShortcutNotice=false;lastVisualKey=null;layoutNoticeUntil=DateTime.UtcNow.AddSeconds(1.8);}}catch(IOException ex){HudReadError("frame shortcut setting",ex);}catch(UnauthorizedAccessException ex){HudReadError("frame shortcut setting",ex);}
            try{if(attackShortcut.Update(attackDown,true)){attackShortcutNotice=true;frameShortcutNotice=false;lastVisualKey=null;layoutNoticeUntil=DateTime.UtcNow.AddSeconds(1.8);}}catch(IOException ex){HudReadError("attack shortcut setting",ex);}catch(UnauthorizedAccessException ex){HudReadError("attack shortcut setting",ex);}

            // A transient prompt/card read must not blank the other independent components.
            try{aspectMilli=reader.AspectMilli;quad=reader.Quad;localSplit=reader.LocalSplit||quad;}catch(IOException ex){HudReadError("aspect",ex);}
            try{rows=reader.Sample();}catch(IOException ex){rows=new MoveRow[4][];HudReadError("attacks",ex);}
            try{prompts=reader.SamplePrompts();}catch(IOException ex){prompts=new TechniquePrompt[0];HudReadError("prompts",ex);}
            try{transformations=reader.SampleTransformations();reader.RefreshCharacterArt(transformationArt,transformations);}catch(IOException ex){transformations=new TransformationPanel[0];HudReadError("cards",ex);}
            PresentAlphaHud();HudNotice("mode="+PanelPlacement.Mode+"; attacks="+HudPreferences.ShowAttacks+"; aspect="+aspectMilli+"; client="+ViewportSize+"; moves="+(rows[0]!=null||rows[1]!=null)+"; cards="+transformations.Length+"; "+reader.MenuStatus);
        } catch(IOException ex) { HudReadError("window",ex);Clear(); }
        catch(InvalidOperationException ex) { HudReadError("window",ex);Clear(); }
        catch(System.ComponentModel.Win32Exception ex){lastVisualKey=null;HudReadError("presentation",ex);}
        finally{RecordHudCost(hudStarted);}
    }
    void HudNotice(string message){if(!Program.DiagnosticTelemetry)return;if(message==lastHudNotice||DateTime.UtcNow<nextHudNotice)return;nextHudNotice=DateTime.UtcNow.AddSeconds(2);lastHudNotice=message;try{Program.SessionLog("HUD state: "+message);}catch(IOException){}catch(UnauthorizedAccessException){}}
    void HudReadError(string component,Exception error){if(!Program.DiagnosticTelemetry)return;if(DateTime.UtcNow<nextHudError)return;nextHudError=DateTime.UtcNow.AddSeconds(5);try{Program.SessionLog("HUD "+component+"; mode="+PanelPlacement.Mode+"; aspect="+aspectMilli+"; bounds="+Bounds+"; "+error.Message);}catch(IOException){}catch(UnauthorizedAccessException){}}
    void WatchPreferences(){
        try{
            Directory.CreateDirectory(SavedSettings.CommonRoot);
            preferenceWatcher=new FileSystemWatcher(SavedSettings.CommonRoot){NotifyFilter=NotifyFilters.LastWrite|NotifyFilters.FileName,IncludeSubdirectories=false};
            FileSystemEventHandler changed=(o,e)=>{if(e.Name=="panel-layout.txt"||e.Name=="attack-panel.txt"||e.Name=="attack-frame.txt"||e.Name=="keyboard-shortcuts.json"||e.Name=="shortcut-lock.txt")preferencesDirty=true;};
            preferenceWatcher.Changed+=changed;preferenceWatcher.Created+=changed;preferenceWatcher.Deleted+=changed;preferenceWatcher.Renamed+=(o,e)=>{preferencesDirty=true;};preferenceWatcher.Error+=(o,e)=>{preferencesDirty=true;};preferenceWatcher.EnableRaisingEvents=true;
        }catch(IOException){if(preferenceWatcher!=null)preferenceWatcher.Dispose();preferenceWatcher=null;}catch(UnauthorizedAccessException){if(preferenceWatcher!=null)preferenceWatcher.Dispose();preferenceWatcher=null;}
    }
    void RefreshSavedHudPreferences(){
        if(DateTime.UtcNow<nextPreferenceRead)return;nextPreferenceRead=DateTime.UtcNow.AddMilliseconds(500);
        if(preferenceWatcher!=null&&!preferencesDirty&&DateTime.UtcNow<nextPreferenceSafetyRead)return;preferencesDirty=false;nextPreferenceSafetyRead=DateTime.UtcNow.AddSeconds(30);
        ShortcutBindings.Load();
        var mode=PanelPlacement.Mode;bool attacks=HudPreferences.ShowAttacks,frame=AttackAppearance.OuterFrame;
        try{
            if(File.Exists(PanelPlacement.FilePath)){string value=File.ReadAllText(PanelPlacement.FilePath).Trim();if(value=="classic"||value=="centered"||value=="expanded"||value=="edges")PanelPlacement.Set(value=="edges"?PanelLayoutMode.ScreenEdges:value=="classic"?PanelLayoutMode.Classic:value=="expanded"?PanelLayoutMode.Expanded:PanelLayoutMode.Centered,false);}
            if(File.Exists(HudPreferences.FilePath)){string value=File.ReadAllText(HudPreferences.FilePath).Trim();if(value=="shown"||value=="hidden")HudPreferences.SetShowAttacks(value=="shown",false);}
            if(File.Exists(AttackAppearance.FilePath)){string value=File.ReadAllText(AttackAppearance.FilePath).Trim();if(value=="framed"||value=="cards")AttackAppearance.SetOuterFrame(value=="framed",false);}
        }catch(IOException){}catch(UnauthorizedAccessException){}
        if(mode!=PanelPlacement.Mode||attacks!=HudPreferences.ShowAttacks||frame!=AttackAppearance.OuterFrame){lastVisualKey=null;Program.SessionLog("HUD preferences reapplied: "+PanelPlacement.Mode+"; attacks="+HudPreferences.ShowAttacks);}
    }
    void PresentCachedHud(){
        // Reassert the no-activation overlay after full-screen/monitor transitions.
        if(DateTime.UtcNow>=nextPresentation)Native.SetWindowPos(Handle,new IntPtr(-1),0,0,0,0,0x13);
        Rectangle visible=VisibleHudBounds();
        if(visible.Width<1||visible.Height<1){HideIdleHud();return;}
        AlphaHud.Present(this,alphaCanvas,new Rectangle(Point.Empty,alphaCanvas.Size),new Point(gameBounds.Left+alphaBounds.Left,gameBounds.Top+alphaBounds.Top));if(hudWindowHidden){ShowWindow(Handle,4);hudWindowHidden=false;}nextPresentation=DateTime.UtcNow.AddSeconds(1);
    }
    void Clear() { if(!preview)HideIdleHud();if(rows.Any(r=>r!=null) || prompts.Length>0 || transformations.Length>0 || attackFades.Any(f=>f.Opacity>0)||cardFades.Any(f=>f.Opacity>0)) { rows=new MoveRow[4][];prompts=new TechniquePrompt[0];transformations=new TransformationPanel[0];ResetMenuFades();lastVisualKey=null;if(preview)Invalidate();else PresentAlphaHud(); } }
    protected override void OnPaint(PaintEventArgs e) {
        if(!preview)return;
        base.OnPaint(e); Graphics g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        if(preview) {
            using(var f=new Font("Segoe UI",23,FontStyle.Bold)) g.DrawString("BT3  /  SPARKING CONTROLS",f,Brushes.White,44,40);
            using(var f=new Font("Segoe UI",12)) g.DrawString(UiLanguage.Text("PREVIEW VISUAL — valores ilustrativos; no jogo, os dados vêm do personagem ativo."),f,Brushes.LightSteelBlue,46,84);
        }
        DrawScene(g);
    }
    protected override void OnPaintBackground(PaintEventArgs e) {if(preview)base.OnPaintBackground(e);}
    void DrawScene(Graphics g) {
        g.SmoothingMode=SmoothingMode.AntiAlias;g.InterpolationMode=InterpolationMode.HighQualityBicubic;
        foreach(var prompt in prompts)prompt.Draw(g,ViewportSize,buttonIcons);
        // The established move panel is drawn LAST and always stays above prompt covers.
        if(preview){
            if(HudPreferences.ShowAttacks)for(int side=0;side<rows.Length;side++)if(rows[side]!=null)DrawPanel(g,side,rows[side]);
            foreach(var panel in transformations)DrawTransformations(g,panel);
        }else DrawAnimatedMenus(g);
        if(!preview&&DateTime.UtcNow<layoutNoticeUntil)DrawLayoutNotice(g);
    }
    bool UpdateMenuFades(double now){
        bool changed=false;
        for(int side=0;side<rows.Length;side++){
            changed|=attackFades[side].Update(HudPreferences.ShowAttacks&&rows[side]!=null,now);
            changed|=cardFades[side].Update(transformations.Any(p=>p.DisplaySide==side),now);
        }
        return changed;
    }
    void ResetMenuFades(){foreach(var fade in attackFades)fade.Reset();foreach(var fade in cardFades)fade.Reset();}
    void DrawAnimatedMenus(Graphics g){
        for(int side=0;side<rows.Length;side++){
            int index=side;
            DrawFadedMenu(g,attackFades[side],rows[side]!=null?new Action<Graphics>(canvas=>DrawPanel(canvas,index,rows[index])):null,side);
        }
        for(int side=0;side<rows.Length;side++){
            var panel=transformations.FirstOrDefault(p=>p.DisplaySide==side);
            DrawFadedMenu(g,cardFades[side],panel!=null?new Action<Graphics>(canvas=>DrawTransformations(canvas,panel)):null,side);
        }
    }
    void DrawFadedMenu(Graphics g,HudMenuFade fade,Action<Graphics> render,int side){
        if(fade.Opacity<=0)return;
        if(fade.Target&&render!=null&&(updateMenuSnapshots||fade.Image==null)){
            // Measure geometry without allocating or rasterizing a viewport-sized bitmap.
            if(menuScratch==null)menuScratch=new Bitmap(1,1,System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using(var measure=Graphics.FromImage(menuScratch)){
                measureMenuOnly=true;try{render(measure);}finally{measureMenuOnly=false;}
            }
            Rectangle box=Rectangle.Intersect(new Rectangle(Point.Empty,ViewportSize),Rectangle.Inflate(Rectangle.Ceiling(panelBounds[side]),3,3));
            if(box.Width>0&&box.Height>0){
                Bitmap image=new Bitmap(box.Width,box.Height,System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                try{using(var canvas=Graphics.FromImage(image)){
                    canvas.Clear(Color.Transparent);canvas.SmoothingMode=SmoothingMode.AntiAlias;canvas.InterpolationMode=InterpolationMode.HighQualityBicubic;
                    canvas.TranslateTransform(-box.Left,-box.Top);render(canvas);
                }fade.Cache(image,box);image=null;}finally{if(image!=null)image.Dispose();}
            }
        }
        fade.Draw(g);
    }
    void DrawLayoutNotice(Graphics g){
        string mode=UiLanguage.Text(PanelPlacement.Mode==PanelLayoutMode.ScreenEdges?"Bordas da tela":PanelPlacement.Mode==PanelLayoutMode.Classic?"Clássica (4:3)":PanelPlacement.Mode==PanelLayoutMode.Expanded?"Expandida":"Centralizada");
        using(var font=new Font("Segoe UI",10f,FontStyle.Bold)){string text=attackShortcutNotice?ShortcutBindings.Label(ShortcutBindings.Attacks)+"  ·  "+UiLanguage.Text("Mostrar painel de ataques")+"  "+(HudPreferences.ShowAttacks?"✓":"—"):frameShortcutNotice?ShortcutBindings.Label(ShortcutBindings.Frame)+"  ·  "+UiLanguage.Text("Moldura externa dos ataques")+"  "+(AttackAppearance.OuterFrame?"✓":"—"):ShortcutBindings.Label(ShortcutBindings.Layout)+"  ·  "+mode;float width=Math.Min(ViewportSize.Width-24,g.MeasureString(text,font).Width+32);var box=new RectangleF((ViewportSize.Width-width)/2,Math.Max(20,ViewportSize.Height*.19f),width,32);using(var bg=new SolidBrush(Color.FromArgb(225,20,26,34)))g.FillRectangle(bg,box);using(var line=new Pen(Color.FromArgb(170,220,175,88)))g.DrawRectangle(line,box.X,box.Y,box.Width,box.Height);using(var fg=new SolidBrush(Color.FromArgb(255,221,151)))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})g.DrawString(text,font,fg,box,format);}
    }
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr window,int command);
    bool hudWindowHidden;
    void HideIdleHud(){if(!hudWindowHidden){ShowWindow(Handle,0);hudWindowHidden=true;}lastVisualKey=null;}
    Rectangle VisibleHudBounds(){
        Rectangle visible=Rectangle.Empty;
        Action<Rectangle> add=box=>{if(box.Width>0&&box.Height>0)visible=visible.IsEmpty?box:Rectangle.Union(visible,box);};
        foreach(var fade in attackFades.Concat(cardFades))if(fade.Opacity>0&&fade.Image!=null)add(fade.Bounds);
        foreach(var prompt in prompts)add(Rectangle.Inflate(Rectangle.Ceiling(prompt.Project(ViewportSize)),4,4));
        if(DateTime.UtcNow<layoutNoticeUntil)add(new Rectangle(0,(int)Math.Max(20,ViewportSize.Height*.19f)-2,ViewportSize.Width,36));
        return Rectangle.Intersect(Rectangle.Inflate(visible,3,3),new Rectangle(Point.Empty,ViewportSize));
    }
    void PresentAlphaHud() {
        if(preview || ViewportSize.Width<1 || ViewportSize.Height<1)return;
        if(gameBounds.Width<1)gameBounds=Bounds;
        bool fading=UpdateMenuFades((double)Stopwatch.GetTimestamp()/Stopwatch.Frequency);
        bool content=prompts.Length>0||attackFades.Any(f=>f.Opacity>0)||cardFades.Any(f=>f.Opacity>0)||DateTime.UtcNow<layoutNoticeUntil;
        if(!content){HideIdleHud();return;}
        var key=new StringBuilder();key.Append(gameBounds.ToString()).Append(aspectMilli).Append(localSplit).Append(quad).Append(PanelPlacement.Mode).Append(HudPreferences.ShowAttacks).Append(AttackAppearance.OuterFrame).Append(DateTime.UtcNow<layoutNoticeUntil);
        foreach(var set in rows){key.Append('|');if(set!=null)foreach(var row in set)key.Append(row.Button).Append(row.Name).Append(row.Cost).Append(row.Available).Append(row.Selected);}
        foreach(var set in transformations){key.Append(set.Side).Append(set.OnlineLocal).Append(set.IsSwitch).Append(set.IsFusion).Append(set.HasFusion).Append(set.HasForms);foreach(var item in set.Options)key.Append(item.Character).Append(':').Append(item.Health).Append(':').Append(item.HealthMax).Append(':').Append(item.Cost).Append(':').Append(item.Available).Append(':').Append(transformationArt.Get(item.Character)!=null);}
        foreach(var prompt in prompts)key.Append(prompt.Button).Append(prompt.Texture).Append(prompt.Uv).Append(prompt.Project(ViewportSize).ToString());
        string state=key.ToString();updateMenuSnapshots=state!=lastVisualKey;
        if(!updateMenuSnapshots&&!fading){if(alphaCanvas!=null&&DateTime.UtcNow>=nextPresentation&&(prompts.Length>0||attackFades.Any(f=>f.Opacity>0)||cardFades.Any(f=>f.Opacity>0)||DateTime.UtcNow<layoutNoticeUntil)){Native.SetWindowPos(Handle,new IntPtr(-1),0,0,0,0,0x13);nextPresentation=DateTime.UtcNow.AddSeconds(1);}return;}
        // Build menu snapshots first; their bounds define the actual composition surface.
        if(menuScratch==null)menuScratch=new Bitmap(1,1,System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        using(var measure=Graphics.FromImage(menuScratch))DrawAnimatedMenus(measure);
        updateMenuSnapshots=false;
        alphaBounds=VisibleHudBounds();
        if(alphaBounds.Width<1||alphaBounds.Height<1){HideIdleHud();return;}
        if(alphaCanvas==null || alphaCanvas.Size!=alphaBounds.Size){if(alphaCanvas!=null)alphaCanvas.Dispose();alphaCanvas=new Bitmap(alphaBounds.Width,alphaBounds.Height,System.Drawing.Imaging.PixelFormat.Format32bppPArgb);}
        using(var g=Graphics.FromImage(alphaCanvas)){g.Clear(Color.Transparent);g.TranslateTransform(-alphaBounds.Left,-alphaBounds.Top);DrawScene(g);}
        PresentCachedHud();lastVisualKey=state;
    }
    // Fill the portrait window with a centered crop, using identical X/Y scale.
    static RectangleF PortraitFillCrop(RectangleF source,RectangleF target) {
        float factor=Math.Max(target.Width/source.Width,target.Height/source.Height);
        float width=target.Width/factor,height=target.Height/factor;
        return new RectangleF(source.X+(source.Width-width)/2,source.Y+(source.Height-height)/2,width,height);
    }
    void DrawTransformations(Graphics g,TransformationPanel panel) {
        bool split=localSplit&&!panel.OnlineLocal;
        RectangleF area=HudLayout.PlayerArea(ViewportSize,aspectMilli,PanelPlacement.Mode,panel.DisplaySide,split,quad);
        float scale=Math.Max(.65f,Math.Min(1.8f,area.Height/900f));
        if(split)scale*=.70f;
        scale=Math.Min(scale,Math.Min(area.Width*.94f/350f,area.Height*.64f/274f));
        float cardW=104*scale,cardH=108*scale,gap=7*scale,pad=12*scale;
        int[] gridX={0,1,1,2};float[] gridY={.5f,0,1,.5f};
        // Reserve the same header space for every panel, including team switch.
        float tabs=27*scale;
        float width=3*cardW+2*gap+2*pad;
        float height=2*cardH+gap+2*pad+tabs;
        RectangleF dock=HudLayout.Dock(area,new SizeF(width,height),split?0:panel.DisplaySide,true);float x=dock.X,y=dock.Y;
        panelBounds[panel.DisplaySide]=dock;if(measureMenuOnly)return;
        // Cards are drawn directly over the game; no outer panel background or border.
        if(panel.HasForms&&panel.HasFusion&&!panel.IsSwitch) {
            using(var font=new Font(menuFamily,14*scale,FontStyle.Regular,GraphicsUnit.Pixel)) {
                var text=new RectangleF(x+pad+42*scale,y+3*scale,width-2*pad-84*scale,24*scale);
                var left=new RectangleF(text.Left-40*scale,y+pad+tabs,40*scale,29*scale);var right=new RectangleF(text.Right+8*scale,y+pad+tabs,40*scale,29*scale);
                if(!buttonIcons.DrawSingle(g,"L1",left))DrawLettering(g,"L1",font,Color.White,left,StringAlignment.Center,scale);
                if(!buttonIcons.DrawSingle(g,"R1",right))DrawLettering(g,"R1",font,Color.White,right,StringAlignment.Center,scale);
            }
        }
        for(int slot=0;slot<4;slot++) {
            var option=panel.Options.FirstOrDefault(item=>item.Slot==slot);
            var card=new RectangleF(x+pad+gridX[slot]*(cardW+gap),y+pad+tabs+gridY[slot]*(cardH+gap),cardW,cardH);
            float cut=5*scale;
            using(var frame=new GraphicsPath()) {
                frame.AddPolygon(new[]{new PointF(card.Left+cut,card.Top),new PointF(card.Right,card.Top),new PointF(card.Right,card.Bottom),new PointF(card.Left+cut,card.Bottom),new PointF(card.Left,card.Bottom-cut),new PointF(card.Left,card.Top+cut)});
                var saved=g.Save();g.SetClip(frame);
                using(var bg=new SolidBrush(Color.FromArgb(155,17,42,61)))g.FillRectangle(bg,card);
                if(option==null) {
                    g.Restore(saved);
                    using(var outline=new Pen(Color.FromArgb(95,111,133,151),Math.Max(1,scale)))g.DrawPath(outline,frame);
                    continue;
                }
                menuSkin.Strip(g,card,false);
                RectangleF face=new RectangleF(card.X+4*scale,card.Y+4*scale,card.Width-8*scale,60*scale);
                var portrait=transformationArt.Get(option.Character);
                if(portrait!=null) {
                    g.DrawImage(portrait,face,PortraitFillCrop(transformationArt.Crop(option.Character),face),GraphicsUnit.Pixel);
                    if(!option.Available)using(var dim=new SolidBrush(Color.FromArgb(125,12,18,28)))g.FillRectangle(dim,face);
                }
                if(panel.IsSwitch && option.HealthMax>0) {
                    float fraction=Math.Max(0,Math.Min(1,(float)option.Health/option.HealthMax));
                    var bar=new RectangleF(face.Left,face.Bottom-7*scale,face.Width,6*scale);
                    using(var background=new SolidBrush(Color.FromArgb(230,8,20,14)))g.FillRectangle(background,bar);
                    var fill=new RectangleF(bar.Left+scale,bar.Top+scale,Math.Max(0,(bar.Width-2*scale)*fraction),bar.Height-2*scale);
                    if(fill.Width>0)using(var green=new LinearGradientBrush(fill,Color.FromArgb(127,255,100),Color.FromArgb(13,176,28),90f))g.FillRectangle(green,fill);
                    using(var border=new Pen(Color.FromArgb(230,4,40,13),Math.Max(1,scale)))g.DrawRectangle(border,bar.X,bar.Y,bar.Width,bar.Height);
                }
                RectangleF button=new RectangleF(card.X+5*scale,card.Y+5*scale,22*scale,22*scale);
                if(!buttonIcons.DrawSingle(g,option.Button,button))using(var font=new Font("Segoe UI Symbol",17*scale))g.DrawString(option.Button,font,Brushes.White,button);
                var costBox=new RectangleF(card.Right-27*scale,card.Y+3*scale,22*scale,25*scale);
                if(!panel.IsSwitch && !menuSkin.Cost(g,option.Cost.ToString(),true,costBox,option.Available))using(var font=new Font(battleFamily,21*scale,FontStyle.Regular,GraphicsUnit.Pixel))DrawLettering(g,option.Cost.ToString(),font,Color.LightCyan,costBox,StringAlignment.Far,scale);
                menuMetrics.Wrapped(g,transformationArt.Name(option.Character),menuFamily,option.Available?Color.White:Color.FromArgb(165,183,200),new RectangleF(card.X+4*scale,card.Y+65*scale,card.Width-8*scale,40*scale),.76f*scale);
                g.Restore(saved);
                using(var line=new Pen(option.Available?Color.FromArgb(180,133,192,219):Color.FromArgb(95,111,133,151),Math.Max(1,scale)))g.DrawPath(line,frame);
            }
        }
    }
    public void SetFusionPreview() { SetTransformationPreview();foreach(var panel in transformations){panel.HasFusion=true;panel.HasForms=true;panel.IsFusion=true;int[] ids={51,52,53};for(int i=0;i<panel.Options.Length;i++)panel.Options[i].Character=ids[i];} }
    public void SetOnlineSidePreview(bool online) { foreach(var panel in transformations){panel.Side=1;panel.OnlineLocal=online;} }
    public void SetSwitchPreview() { SetTransformationPreview();foreach(var panel in transformations){panel.IsSwitch=true;for(int i=0;i<panel.Options.Length;i++){panel.Options[i].HealthMax=30000;panel.Options[i].Health=25000-i*8500;}} }
    public void SetTransformationPreview() {
        rows=new MoveRow[4][];transformations=new[]{new TransformationPanel {Side=0,Options=new[]{new TransformationOption {Slot=0,Character=4,Cost=1,Available=true},new TransformationOption {Slot=1,Character=5,Cost=2,Available=true},new TransformationOption {Slot=2,Character=6,Cost=3,Available=false}}}};
    }
    void DrawPanel(Graphics g,int side,MoveRow[] moves) {
        RectangleF area=HudLayout.PlayerArea(ViewportSize,aspectMilli,PanelPlacement.Mode,side,localSplit,quad);
        float scale=.75f*Math.Max(.65f,Math.Min(1.8f,area.Height/900f));
        scale=Math.Min(scale,area.Height*.65f/(48*moves.Length+24));
        if(localSplit)scale=Math.Min(scale*.70f,area.Width*.90f/824f);
        float commandWidth,costWidth;
        float margin=area.Width*.025f,slotWidth=area.Width-2*margin;
        // Each local half has its own left anchor. Neither size nor position depends on the other menu.
        float w=MeasurePanelWidth(g,menuFamily,moves,scale,localSplit?int.MaxValue:(int)area.Width,false,out commandWidth,out costWidth,menuMetrics);
        if(localSplit){
            float available=area.Width*.90f;
            w=Math.Min(w,available-24*scale);
        }else{
            // Keep the established single-view layout.
            scale=Math.Min(scale,slotWidth/400f);
            w=MeasurePanelWidth(g,menuFamily,moves,scale,(int)area.Width,false,out commandWidth,out costWidth,menuMetrics);
        }
        float rh=48*scale,pad=12*scale;w=Math.Min(w,slotWidth-2*pad);float frameW=w+2*pad;
        panelBounds[side]=HudLayout.Dock(area,new SizeF(frameW,rh*moves.Length+2*pad),localSplit?0:side,false);
        if(measureMenuOnly)return;float frameX=panelBounds[side].X,frameY=panelBounds[side].Y;
        if(AttackAppearance.OuterFrame)menuSkin.Frame(g,panelBounds[side],scale);float x=frameX+pad,y=frameY+pad;
        for(int i=0;i<moves.Length;i++) {
            var row=moves[i];float yy=y+i*rh;var box=new RectangleF(x,yy,w,40*scale);
            DrawAttackCard(g,box,row.Selected,scale);
            var commandBox=new RectangleF(x+10*scale,yy,commandWidth-15*scale,box.Height);
            var nameBox=new RectangleF(x+commandWidth,yy,w-commandWidth-costWidth-8*scale,box.Height);
            var costBox=new RectangleF(x+w-71*scale,yy+3*scale,34*scale,box.Height-6*scale);
            Color fg=row.Selected?Color.FromArgb(255,247,15):row.Available?Color.White:Color.FromArgb(176,194,212);
            if(!buttonIcons.Draw(g,row.Button,commandBox,scale))using(var font=new Font("Segoe UI Symbol",20*scale))DrawLettering(g,row.Button,font,fg,commandBox,StringAlignment.Near,scale);
            menuMetrics.Draw(g,row.Name,menuFamily,fg,nameBox,2*scale,false,true,.85f);
            if(!menuSkin.Cost(g,row.Cost,i<2,costBox,row.Available))using(var font=new Font(battleFamily,29*scale,FontStyle.Regular,GraphicsUnit.Pixel))DrawLettering(g,row.Cost,font,i<2?Color.LightCyan:Color.LightYellow,costBox,StringAlignment.Far,scale);
            if(row.Ultimate){var boltBox=new RectangleF(x+w-31*scale,yy+6*scale,25*scale,box.Height-12*scale);if(!menuSkin.Bolt(g,boltBox,row.Available))DrawSparkingBolt(g,boltBox,row.Available,scale);}
        }
    }
    void DrawAttackCard(Graphics g,RectangleF box,bool selected,float scale){
        float cut=5*scale;using(var frame=new GraphicsPath()){
            frame.AddPolygon(new[]{new PointF(box.Left+cut,box.Top),new PointF(box.Right,box.Top),new PointF(box.Right,box.Bottom),new PointF(box.Left+cut,box.Bottom),new PointF(box.Left,box.Bottom-cut),new PointF(box.Left,box.Top+cut)});
            var saved=g.Save();g.SetClip(frame);using(var bg=new SolidBrush(Color.FromArgb(155,17,42,61)))g.FillRectangle(bg,box);g.Restore(saved);
            using(var line=new Pen(selected?Color.FromArgb(190,225,196,121):Color.FromArgb(150,111,133,151),Math.Max(1,scale)))g.DrawPath(line,frame);
        }
    }
    static void DrawMenuText(Graphics g,string text,Font font,Color color,RectangleF box,float scale) {
        using(var format=new StringFormat{Alignment=StringAlignment.Near,LineAlignment=StringAlignment.Center,FormatFlags=StringFormatFlags.NoWrap,Trimming=StringTrimming.EllipsisCharacter})
        using(var path=new GraphicsPath())using(var fill=new SolidBrush(color))using(var edge=new Pen(Color.FromArgb(32,43,94),Math.Max(1,1.2f*scale))){path.AddString(text,font.FontFamily,(int)font.Style,font.Size,box,format);g.DrawPath(edge,path);g.FillPath(fill,path);}
    }
    internal static float MeasurePanelWidth(Graphics g,FontFamily family,MoveRow[] moves,float scale,int viewportWidth,bool split,out float commandWidth,out float costWidth,NativeMenuMetrics metrics=null) {
        float maxName=0,maxButton=0,maxCost=0;
        using(var name=new Font(family,25*scale,FontStyle.Regular,GraphicsUnit.Pixel))
        using(var button=new Font("Segoe UI Symbol",20*scale,FontStyle.Regular,GraphicsUnit.Pixel))
        using(var number=new Font(family,29*scale,FontStyle.Regular,GraphicsUnit.Pixel))
        foreach(var row in moves) {
            maxName=Math.Max(maxName,metrics!=null?metrics.Measure(row.Name,2*scale,.85f):g.MeasureString(row.Name,name).Width);
            maxButton=Math.Max(maxButton,Math.Max(g.MeasureString(row.Button,button).Width,GameButtonIcons.Measure(row.Button,scale)));
            maxCost=Math.Max(maxCost,g.MeasureString(row.Cost,number).Width);
        }
        commandWidth=maxButton+26*scale;
        costWidth=Math.Max(maxCost+40*scale,74*scale);
        float desired=commandWidth+maxName+14*scale+costWidth+12*scale;
        return Math.Min(desired,Math.Min(viewportWidth*(split?.47f:.78f),800*scale));
    }
    static void DrawSparkingBolt(Graphics g,RectangleF box,bool available,float scale) {
        PointF[] shape={new PointF(.52f,0),new PointF(1,0),new PointF(.62f,.38f),new PointF(.96f,.38f),new PointF(.12f,1),new PointF(.37f,.55f),new PointF(0,.55f)};
        for(int i=0;i<shape.Length;i++)shape[i]=new PointF(box.X+shape[i].X*box.Width,box.Y+shape[i].Y*box.Height);
        using(var path=new GraphicsPath())
        using(var fill=new SolidBrush(available?Color.FromArgb(255,224,115):Color.FromArgb(195,201,214)))
        using(var outline=new Pen(Color.FromArgb(15,19,28),Math.Max(1.2f,1.5f*scale)){LineJoin=LineJoin.Round}) {
            path.AddPolygon(shape);g.DrawPath(outline,path);g.FillPath(fill,path);
        }
    }
    public void SaveSkinReport(string path){File.WriteAllLines(path,menuSkin.HdLoaded.Concat(buttonIcons.HdLoaded));}
    public void SetPreviewSize(int width,int height) { ClientSize=new Size(width,height); }
    static void DrawLettering(Graphics g,string text,Font font,Color color,RectangleF bounds,StringAlignment alignment,float scale) {
        using(var format=new StringFormat { Alignment=alignment,LineAlignment=StringAlignment.Center,FormatFlags=StringFormatFlags.NoWrap,Trimming=StringTrimming.EllipsisCharacter })
        using(var path=new GraphicsPath())
        using(var fill=new SolidBrush(color))
        using(var stroke=new Pen(Color.FromArgb(15,19,28),Math.Max(1.2f,2*scale)) { LineJoin=LineJoin.Round }) {
            path.AddString(text,font.FontFamily,(int)font.Style,font.Size,bounds,format);
            g.DrawPath(stroke,path);g.FillPath(fill,path);
        }
    }
    public void SavePreview(string path) { using(var bmp=new Bitmap(Width,Height)) { DrawToBitmap(bmp,new Rectangle(0,0,Width,Height));bmp.Save(path);
        Rectangle crop=Rectangle.Intersect(new Rectangle(0,0,Width,Height),Rectangle.Inflate(Rectangle.Ceiling(panelBounds[0]),6,6));
        if(crop.Width>0 && crop.Height>0)using(var detail=bmp.Clone(crop,bmp.PixelFormat))detail.Save(Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+"-detail.png")); } }
    public void SaveTransparentPreview(string path) {
        using(var bitmap=new Bitmap(Width,Height,System.Drawing.Imaging.PixelFormat.Format32bppPArgb)) {
            using(var graphics=Graphics.FromImage(bitmap)){graphics.Clear(Color.Transparent);DrawScene(graphics);}
            Rectangle crop=Rectangle.Intersect(new Rectangle(0,0,Width,Height),Rectangle.Inflate(Rectangle.Ceiling(panelBounds[0]),6,6));
            if(crop.Width>0 && crop.Height>0)using(var detail=bitmap.Clone(crop,bitmap.PixelFormat))detail.Save(path);
        }
    }
    protected override void Dispose(bool disposing) { if(disposing) { timer.Dispose();if(preferenceWatcher!=null)preferenceWatcher.Dispose();AlphaHud.Release(this);buttonIcons.Dispose();menuSkin.Dispose();transformationArt.Dispose();battleFonts.Dispose();menuFonts.Dispose();if(alphaCanvas!=null)alphaCanvas.Dispose();if(menuScratch!=null)menuScratch.Dispose();ResetMenuFades(); }base.Dispose(disposing); }
}

static class LauncherTheme {
    public static readonly Color Background=Color.FromArgb(18,18,21),Surface=Color.FromArgb(44,38,32),Field=Color.FromArgb(25,24,25),Text=Color.FromArgb(242,242,240),Muted=Color.FromArgb(191,180,165),Accent=Color.FromArgb(255,180,56);
    public static void Combo(ComboBox combo) { combo.BackColor=Field;combo.ForeColor=Text;combo.FlatStyle=FlatStyle.Flat; }
}
sealed class RoundedButton : Button {
    bool over,down;public bool Primary;
    public RoundedButton(){FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;UseVisualStyleBackColor=false;ForeColor=LauncherTheme.Text;BackColor=LauncherTheme.Surface;Padding=new Padding(14,6,14,6);MinimumSize=new Size(100,40);Cursor=Cursors.Hand;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);}
    protected override void OnMouseEnter(EventArgs e){over=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){over=false;down=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnMouseDown(MouseEventArgs e){down=true;Invalidate();base.OnMouseDown(e);}
    protected override void OnMouseUp(MouseEventArgs e){down=false;Invalidate();base.OnMouseUp(e);}
    protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}
    protected override void OnLostFocus(EventArgs e){Invalidate();base.OnLostFocus(e);}
    protected override void OnPaint(PaintEventArgs e){
        Graphics g=e.Graphics;g.Clear(Parent==null?LauncherTheme.Background:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;
        Color fill=Primary?LauncherTheme.Accent:LauncherTheme.Surface;if(over)fill=Primary?Color.FromArgb(255,202,103):Color.FromArgb(63,47,32);if(down)fill=Primary?Color.FromArgb(223,141,42):Color.FromArgb(37,31,27);if(!Enabled)fill=LauncherTheme.Field;
        RectangleF rect=new RectangleF(1,1,Math.Max(1,Width-3),Math.Max(1,Height-3));float diameter=Math.Min(16,Math.Min(rect.Width,rect.Height));
        using(var path=new GraphicsPath()){
            path.AddArc(rect.Left,rect.Top,diameter,diameter,180,90);path.AddArc(rect.Right-diameter,rect.Top,diameter,diameter,270,90);path.AddArc(rect.Right-diameter,rect.Bottom-diameter,diameter,diameter,0,90);path.AddArc(rect.Left,rect.Bottom-diameter,diameter,diameter,90,90);path.CloseFigure();
            if(Primary&&Enabled){using(var brush=new LinearGradientBrush(rect,fill,Color.FromArgb(Math.Max(0,fill.R-15),Math.Max(0,fill.G-36),Math.Max(0,fill.B-19)),90f))g.FillPath(brush,path);}else using(var brush=new SolidBrush(fill))g.FillPath(brush,path);using(var pen=new Pen(Focused?Color.FromArgb(255,216,136):Primary?Color.FromArgb(255,206,116):Color.FromArgb(106,78,45),Focused?2:1))g.DrawPath(pen,path);
        }
        TextRenderer.DrawText(g,Text,Font,ClientRectangle,Enabled?(Primary?Color.FromArgb(30,20,9):ForeColor):LauncherTheme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|(RightToLeft==RightToLeft.Yes?TextFormatFlags.RightToLeft:0));
    }
}

sealed class LauncherComboBox : ComboBox {
    public LauncherComboBox(){DrawMode=DrawMode.OwnerDrawFixed;ItemHeight=22;FlatStyle=FlatStyle.Flat;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);}
    protected override void OnMouseEnter(EventArgs e){base.OnMouseEnter(e);Invalidate();}
    protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);Invalidate();}
    protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}
    protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
    protected override void OnSelectedIndexChanged(EventArgs e){base.OnSelectedIndexChanged(e);Invalidate();}
    protected override void OnDrawItem(DrawItemEventArgs e){
        if(e.Bounds.Width<=0||e.Bounds.Height<=0)return;
        bool selected=(e.State&DrawItemState.Selected)!=0;using(var brush=new SolidBrush(selected?Color.FromArgb(69,55,35):LauncherTheme.Field))e.Graphics.FillRectangle(brush,e.Bounds);
        string text=e.Index>=0&&e.Index<Items.Count?Items[e.Index].ToString():Text;var bounds=Rectangle.Inflate(e.Bounds,-8,0);
        var flags=TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis;if(RightToLeft==RightToLeft.Yes)flags|=TextFormatFlags.RightToLeft|TextFormatFlags.Right;
        TextRenderer.DrawText(e.Graphics,text,Font,bounds,Enabled?LauncherTheme.Text:LauncherTheme.Muted,flags);
    }
    protected override void WndProc(ref Message message){
        if(message.Msg==0x14){message.Result=new IntPtr(1);return;}
        base.WndProc(ref message);
    }
    protected override void OnPaint(PaintEventArgs e){
        if(Width<24||Height<8)return;
        Graphics g=e.Graphics;
        g.Clear(Parent==null?LauncherTheme.Background:Parent.BackColor);
            bool rtl=RightToLeft==RightToLeft.Yes;int x=rtl?1:Width-23;var button=new Rectangle(x,1,22,Height-2);
            using(var field=new SolidBrush(LauncherTheme.Field))g.FillRectangle(field,ClientRectangle);
            var textArea=new Rectangle(rtl?27:8,1,Math.Max(1,Width-35),Height-2);string selected=SelectedItem==null?Text:SelectedItem.ToString();
            var textFlags=TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPadding;if(rtl)textFlags|=TextFormatFlags.RightToLeft|TextFormatFlags.Right;
            TextRenderer.DrawText(g,selected,Font,textArea,Enabled?LauncherTheme.Text:LauncherTheme.Muted,textFlags);
            using(var brush=new SolidBrush(LauncherTheme.Surface))g.FillRectangle(brush,button);
            int cx=x+11,cy=Height/2;using(var brush=new SolidBrush(LauncherTheme.Accent))g.FillPolygon(brush,new[]{new Point(cx-3,cy-1),new Point(cx+3,cy-1),new Point(cx,cy+2)});
            g.SmoothingMode=SmoothingMode.AntiAlias;
            using(var outline=new GraphicsPath()){
                float w=Width-1,h=Height-1,radius=6;outline.AddArc(0,0,radius,radius,180,90);outline.AddArc(w-radius,0,radius,radius,270,90);outline.AddArc(w-radius,h-radius,radius,radius,0,90);outline.AddArc(0,h-radius,radius,radius,90,90);outline.CloseFigure();
                using(var line=new Pen(Focused?Color.FromArgb(235,169,75):Color.FromArgb(87,72,54)))g.DrawPath(line,outline);
            }
    }

}

sealed class LauncherCard : Panel {
    public LauncherCard(){BackColor=Color.FromArgb(31,29,29);Padding=new Padding(16);SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);}
    protected override void OnPaintBackground(PaintEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;e.Graphics.Clear(Parent==null?LauncherTheme.Background:Parent.BackColor);using(var path=Shape())using(var brush=new SolidBrush(BackColor))e.Graphics.FillPath(brush,path);}
    GraphicsPath Shape(){var p=new GraphicsPath();float w=Math.Max(10,Width-2),h=Math.Max(10,Height-2),a=18;p.AddArc(1,1,a,a,180,90);p.AddArc(w-a,1,a,a,270,90);p.AddArc(w-a,h-a,a,a,0,90);p.AddArc(1,h-a,a,a,90,90);p.CloseFigure();return p;}
    protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var path=Shape())using(var pen=new Pen(Color.FromArgb(99,73,44)))e.Graphics.DrawPath(pen,path);float x=RightToLeft==RightToLeft.Yes?Width-2.5f:2.5f;using(var glow=new LinearGradientBrush(new RectangleF(x,15,2,Math.Max(2,Height-30)),Color.FromArgb(215,255,187,65),Color.FromArgb(100,223,106,40),90f))using(var edge=new Pen(glow,2)){edge.StartCap=LineCap.Round;edge.EndCap=LineCap.Round;e.Graphics.DrawLine(edge,x,16,x,Height-16);}}
}
sealed class PlacementPreview : Control {
    public PanelLayoutMode Mode;public bool ShowAttacks=true,OuterFrame;
    public PlacementPreview(){Name="placementPreview";Size=new Size(270,128);AccessibleRole=AccessibleRole.Graphic;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);}
    protected override void OnPaint(PaintEventArgs e){
        var g=e.Graphics;g.Clear(Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;
        float h=Height-8,w=Math.Min(Width-8,h*16/9);h=w*9/16;var screen=new RectangleF((Width-w)/2,(Height-h)/2,w,h);
        using(var bg=new LinearGradientBrush(screen,Color.FromArgb(28,39,47),Color.FromArgb(19,25,33),90f))g.FillRectangle(bg,screen);
        var area=HudLayout.Controls(new Size(1920,1080),1778,Mode);float scale=w/1920;var safe=new RectangleF(screen.X+area.X*scale,screen.Y+area.Y*scale,area.Width*scale,area.Height*scale);
        using(var wash=new SolidBrush(Color.FromArgb(14,238,184,80)))g.FillRectangle(wash,safe);using(var line=new Pen(Color.FromArgb(106,148,133,102))){line.DashStyle=DashStyle.Dot;g.DrawRectangle(line,safe.X,safe.Y,safe.Width,safe.Height);}
        for(int side=0;side<2;side++){float x=side==0?screen.Left+8:screen.Right-w*.33f-8;using(var life=new SolidBrush(side==0?Color.FromArgb(156,177,142):Color.FromArgb(126,146,163)))g.FillRectangle(life,x,screen.Top+7,w*.33f,3);}
        // Small character cards keep their position when the attack list is hidden.
        float anchor=safe.Left+safe.Width*.025f,cw=w*.034f,ch=h*.095f,gap=2,top=screen.Top+h*.31f;
        int[] gx={0,1,1,2};float[] gy={.5f,0,1,.5f};
        for(int i=0;i<4;i++){var card=new RectangleF(anchor+gx[i]*(cw+gap),top+gy[i]*(ch+gap),cw,ch);using(var b=new SolidBrush(Color.FromArgb(190,39,65,79)))g.FillRectangle(b,card);using(var p=new Pen(Color.FromArgb(141,164,180),.7f))g.DrawRectangle(p,card.X,card.Y,card.Width,card.Height);}
        using(var actor=new SolidBrush(Color.FromArgb(149,130,90))){float x=screen.X+w*.59f,y=screen.Y+h*.60f;g.FillEllipse(actor,x,y,4,4);g.FillRectangle(actor,x-1,y+5,6,10);}
        if(ShowAttacks){
            float rowH=h*.038f,rowGap=2,listW=w*.39f,listH=rowH*5+rowGap*4,x=anchor,y=screen.Bottom-h*.065f-listH;
            if(OuterFrame){var frame=new RectangleF(x-3,y-3,listW+6,listH+6);using(var b=new SolidBrush(Color.FromArgb(210,12,39,67)))g.FillRectangle(b,frame);using(var p=new Pen(Color.FromArgb(192,216,232),.9f))g.DrawRectangle(p,frame.X,frame.Y,frame.Width,frame.Height);}
            for(int i=0;i<5;i++){var row=new RectangleF(x,y+i*(rowH+rowGap),listW,rowH);using(var b=new SolidBrush(Color.FromArgb(200,24,49,65)))g.FillRectangle(b,row);using(var p=new Pen(Color.FromArgb(123,148,166),.6f))g.DrawRectangle(p,row.X,row.Y,row.Width,row.Height);using(var icon=new SolidBrush(Color.FromArgb(230,184,83)))g.FillRectangle(icon,row.X+3,row.Y+1,3,Math.Max(1,row.Height-2));using(var name=new SolidBrush(Color.FromArgb(192,205,214)))g.FillRectangle(name,row.X+10,row.Y+rowH/2,listW*.55f,1);}
        }
        using(var border=new Pen(Color.FromArgb(121,91,52)))g.DrawRectangle(border,screen.X,screen.Y,screen.Width,screen.Height);
    }
}

static class AppBrand {
    public static readonly Bitmap Image=LoadImage();
    public static readonly Icon WindowIcon=new Icon(Path.Combine(Program.Root,"assets","dragon-ball-three-stars.ico"),new Size(48,48));
    static Bitmap LoadImage(){using(var input=System.Drawing.Image.FromFile(Path.Combine(Program.Root,"assets","dragon-ball-three-stars.png")))return new Bitmap(input);}
}
sealed class DragonBallMark : Control {
    public DragonBallMark(){Name="dragonBallMark";Size=new Size(60,60);TabStop=false;AccessibleRole=AccessibleRole.Graphic;AccessibleName="Three-star Dragon Ball";SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);}
    protected override void OnRightToLeftChanged(EventArgs e){base.OnRightToLeftChanged(e);Anchor=AnchorStyles.Top|(RightToLeft==RightToLeft.Yes?AnchorStyles.Right:AnchorStyles.Left);}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent.BackColor);g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=PixelOffsetMode.HighQuality;int size=Math.Min(Width,Height);g.DrawImage(AppBrand.Image,new Rectangle((Width-size)/2,(Height-size)/2,size,size));}
}
sealed class BrandTitle : Control {
    public BrandTitle(){Text="BT3 Sparking Controls";Font=new Font("Bahnschrift SemiBold",23f,FontStyle.Regular);Height=38;AccessibleRole=AccessibleRole.StaticText;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);}
    protected override void OnPaint(PaintEventArgs e){
        var g=e.Graphics;g.Clear(Parent.BackColor);var flags=TextFormatFlags.NoPadding|TextFormatFlags.SingleLine|TextFormatFlags.VerticalCenter;
        int brandWidth=TextRenderer.MeasureText(g,"BT3",Font,Size.Empty,flags).Width;int restWidth=TextRenderer.MeasureText(g,"Sparking Controls",Font,Size.Empty,flags).Width;int gap=8;
        int x=RightToLeft==RightToLeft.Yes?Math.Max(0,Width-brandWidth-gap-restWidth):0;
        TextRenderer.DrawText(g,"BT3",Font,new Rectangle(x,0,brandWidth,Height),Color.FromArgb(246,186,85),flags);
        TextRenderer.DrawText(g,"Sparking Controls",Font,new Rectangle(x+brandWidth+gap,0,Math.Max(1,Width-x-brandWidth-gap),Height),LauncherTheme.Text,flags);
    }
}

sealed class LauncherCheckBox : CheckBox {
    public LauncherCheckBox(){ForeColor=LauncherTheme.Text;Cursor=Cursors.Hand;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);}
    public override Size GetPreferredSize(Size proposed){Size text=TextRenderer.MeasureText(Text,Font);return new Size(text.Width+30,Math.Max(28,text.Height+8));}
    protected override void OnPaint(PaintEventArgs e){
        var g=e.Graphics;g.Clear(Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;bool rtl=RightToLeft==RightToLeft.Yes;float side=16,x=rtl?Width-side-2:1,y=(Height-side)/2;
        using(var p=new GraphicsPath()){p.AddArc(x,y,5,5,180,90);p.AddArc(x+side-5,y,5,5,270,90);p.AddArc(x+side-5,y+side-5,5,5,0,90);p.AddArc(x,y+side-5,5,5,90,90);p.CloseFigure();using(var fill=new SolidBrush(Checked?LauncherTheme.Accent:LauncherTheme.Field))g.FillPath(fill,p);using(var edge=new Pen(Focused?LauncherTheme.Accent:Color.FromArgb(101,106,115)))g.DrawPath(edge,p);}
        if(Checked)using(var pen=new Pen(Color.FromArgb(33,25,13),2)){pen.StartCap=LineCap.Round;pen.EndCap=LineCap.Round;g.DrawLines(pen,new[]{new PointF(x+4,y+8),new PointF(x+7,y+11),new PointF(x+12,y+5)});}
        var bounds=new Rectangle(rtl?0:25,0,Math.Max(1,Width-25),Height);var flags=TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.NoPadding;if(rtl)flags|=TextFormatFlags.RightToLeft|TextFormatFlags.Right;TextRenderer.DrawText(g,Text,Font,bounds,ForeColor,flags);
    }
}

sealed class LauncherPathField : Panel {
    readonly TextBox input;
    public LauncherPathField(TextBox box){input=box;Height=38;TabStop=false;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);input.BorderStyle=BorderStyle.None;input.Margin=new Padding(0);Controls.Add(input);input.GotFocus+=(o,e)=>Invalidate();input.LostFocus+=(o,e)=>Invalidate();ResizeInput();}
    void ResizeInput(){if(input!=null)input.SetBounds(11,Math.Max(2,(Height-input.PreferredHeight)/2),Math.Max(1,Width-22),input.PreferredHeight);}
    protected override void OnResize(EventArgs e){base.OnResize(e);ResizeInput();}
    protected override void OnPaintBackground(PaintEventArgs e){
        var g=e.Graphics;g.Clear(Parent==null?LauncherTheme.Background:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;
        using(var shape=new GraphicsPath()){
            float w=Width-2,h=Height-2,a=12;shape.AddArc(1,1,a,a,180,90);shape.AddArc(w-a,1,a,a,270,90);shape.AddArc(w-a,h-a,a,a,0,90);shape.AddArc(1,h-a,a,a,90,90);shape.CloseFigure();
            using(var brush=new SolidBrush(LauncherTheme.Field))g.FillPath(brush,shape);using(var pen=new Pen(ContainsFocus?Color.FromArgb(217,164,79):Color.FromArgb(60,65,73)))g.DrawPath(pen,shape);
        }
    }
}

sealed class LauncherHeader : TableLayoutPanel {
    protected override void OnPaint(PaintEventArgs e){
        base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        var rule=new RectangleF(0,Height-3,Math.Max(1,Width),2);using(var brush=new LinearGradientBrush(rule,Color.FromArgb(180,250,164,55),Color.FromArgb(0,250,164,55),0f))g.FillRectangle(brush,rule);
        // Three small stars echo the emblem without competing with the title.
        for(int star=0;star<3;star++){float cx=RightToLeft==RightToLeft.Yes?14+star*19:Width-14-star*19,cy=53;var points=new PointF[10];for(int i=0;i<10;i++){double angle=-Math.PI/2+i*Math.PI/5;float radius=i%2==0?6:2.6f;points[i]=new PointF(cx+(float)Math.Cos(angle)*radius,cy+(float)Math.Sin(angle)*radius);}using(var fill=new SolidBrush(Color.FromArgb(star==0?160:115,229,132,54)))g.FillPolygon(fill,points);}
    }
}

sealed class GameLocationWindow : Form {
    readonly TextBox pathBox=new TextBox();
    readonly Label status=new Label();
    readonly List<Action> refreshText=new List<Action>();
    readonly string pathFile;
    string statusKey;
    public string GamePath { get; private set; }
    public Profile GameProfile { get; private set; }
    T Bind<T>(T control,string key) where T:Control { refreshText.Add(()=>control.Text=UiLanguage.Text(key));control.Text=UiLanguage.Text(key);return control; }
    void SetStatus(string key,Color color) { statusKey=key;status.ForeColor=color;status.Text=UiLanguage.Text(key); }
    public GameLocationWindow(string initial,string savedPathFile) {
        pathFile=savedPathFile;PanelPlacement.Load();HudPreferences.Load();AttackAppearance.Load();ShortcutBindings.Load();Icon=AppBrand.WindowIcon;ShowIcon=true;Text="BT3 Sparking Controls";StartPosition=FormStartPosition.CenterScreen;
        FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;AutoScaleMode=AutoScaleMode.Font;Font=new Font("Segoe UI",10f);
        ClientSize=new Size(920,644);Padding=new Padding(24);BackColor=LauncherTheme.Background;ForeColor=LauncherTheme.Text;
        var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,BackColor=LauncherTheme.Background};layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,84));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,146));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,288));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var header=new LauncherHeader {Dock=DockStyle.Fill,ColumnCount=2,RowCount=2,Margin=new Padding(0,0,0,12)};header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,78));header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute,42));header.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var mark=new DragonBallMark {Margin=new Padding(0),Anchor=AnchorStyles.Left|AnchorStyles.Top};header.Controls.Add(mark,0,0);header.SetRowSpan(mark,2);
        header.Controls.Add(new BrandTitle {Dock=DockStyle.Fill,Margin=new Padding(0)},1,0);
        header.Controls.Add(Bind(new Label {AutoSize=true,ForeColor=LauncherTheme.Muted,Font=new Font(Font.FontFamily,9.5f),Margin=new Padding(1,0,0,0)},"Controles de batalha"),1,1);layout.Controls.Add(header,0,0);
        var gameCard=new LauncherCard {Dock=DockStyle.Fill,Margin=new Padding(0,0,0,14)};
        var gameGrid=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=3,BackColor=gameCard.BackColor};gameGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));gameGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        gameGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,25));gameGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,40));gameGrid.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var gameLabel=Bind(new Label {AutoSize=true,ForeColor=Color.FromArgb(240,192,113),Font=new Font("Segoe UI Semibold",9.5f,FontStyle.Regular),Margin=new Padding(0)},"Local do BT3 Decomp");gameGrid.Controls.Add(gameLabel,0,0);gameGrid.SetColumnSpan(gameLabel,2);
        pathBox.BackColor=LauncherTheme.Field;pathBox.ForeColor=LauncherTheme.Text;pathBox.BorderStyle=BorderStyle.FixedSingle;pathBox.Text=initial??"";pathBox.Anchor=AnchorStyles.Left|AnchorStyles.Right;pathBox.Margin=new Padding(0,0,12,0);pathBox.Font=new Font("Segoe UI",10f);
        var browse=Bind(new RoundedButton {AutoSize=true,Margin=new Padding(0)},"Procurar...");browse.Click+=(o,e)=>Browse();var pathField=new LauncherPathField(pathBox){Dock=DockStyle.Fill,Margin=new Padding(0,0,12,0)};gameGrid.Controls.Add(pathField,0,1);gameGrid.Controls.Add(browse,1,1);
        status.AutoSize=true;status.MaximumSize=new Size(800,0);status.Font=new Font(Font.FontFamily,9f);status.Margin=new Padding(0,5,0,0);SetStatus(File.Exists(initial)?"Caminho salvo carregado.":"Escolha o executável na pasta atual do jogo.",LauncherTheme.Muted);gameGrid.Controls.Add(status,0,2);gameGrid.SetColumnSpan(status,2);gameCard.Controls.Add(gameGrid);layout.Controls.Add(gameCard,0,1);
        var prefs=new LauncherCard {Dock=DockStyle.Fill,Margin=new Padding(0,0,0,14)};
        var grid=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=3,BackColor=prefs.BackColor};grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,48));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,52));grid.RowStyles.Add(new RowStyle(SizeType.Absolute,24));grid.RowStyles.Add(new RowStyle(SizeType.Absolute,42));grid.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        grid.Controls.Add(Bind(new Label {AutoSize=true,ForeColor=Color.FromArgb(240,192,113),Font=new Font("Segoe UI Semibold",9.5f,FontStyle.Regular),Margin=new Padding(0)},"Idioma"),0,0);
        grid.Controls.Add(Bind(new Label {AutoSize=true,ForeColor=Color.FromArgb(240,192,113),Font=new Font("Segoe UI Semibold",9.5f,FontStyle.Regular),Margin=new Padding(16,0,0,0)},"Posição dos painéis"),1,0);
        var language=new LauncherComboBox {Name="language",DropDownStyle=ComboBoxStyle.DropDownList,MaxDropDownItems=16,Dock=DockStyle.Fill,Margin=new Padding(0,0,0,0)};LauncherTheme.Combo(language);language.RightToLeft=RightToLeft.No;language.Items.AddRange(UiLanguage.NativeNames);language.SelectedIndex=UiLanguage.CurrentIndex;grid.Controls.Add(language,0,1);
        var placement=new LauncherComboBox {Name="panelPlacement",DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill,Margin=new Padding(16,0,0,0)};LauncherTheme.Combo(placement);var layoutKey=new RoundedButton {Name="layoutShortcutKey",AutoSize=false,MinimumSize=new Size(76,28),Size=new Size(76,28),Padding=new Padding(8,3,8,3),Margin=new Padding(8,0,0,0),Anchor=AnchorStyles.Top};
        var positionRow=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=1,Margin=new Padding(16,0,0,0)};positionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));positionRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));positionRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));placement.Margin=new Padding(0);positionRow.Controls.Add(placement,0,0);var keyCaption=Bind(new Label {Name="layoutKeyLabel",AutoSize=true,Anchor=AnchorStyles.Top,ForeColor=LauncherTheme.Muted,Margin=new Padding(8,6,0,0)},"Atalho");positionRow.Controls.Add(keyCaption,1,0);positionRow.Controls.Add(layoutKey,2,0);grid.Controls.Add(positionRow,1,1);
        var details=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0,8,0,0),BackColor=prefs.BackColor};details.RowStyles.Add(new RowStyle(SizeType.Percent,100));details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,286));details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        var preview=new PlacementPreview {Margin=new Padding(0,0,12,0),Mode=PanelPlacement.Mode,ShowAttacks=HudPreferences.ShowAttacks,OuterFrame=AttackAppearance.OuterFrame};details.Controls.Add(preview,0,0);
        var textGrid=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=6,Margin=new Padding(0)};textGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,22));textGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));textGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,36));textGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,36));textGrid.RowStyles.Add(new RowStyle(SizeType.Absolute,30));textGrid.RowStyles.Add(new RowStyle(SizeType.Percent,100));textGrid.Controls.Add(Bind(new Label {AutoSize=true,ForeColor=LauncherTheme.Text,Font=new Font(Font.FontFamily,9f,FontStyle.Bold),Margin=new Padding(0)},"16:9 e ultrawide"),0,0);
        var description=new Label {AutoSize=true,MaximumSize=new Size(600,0),ForeColor=LauncherTheme.Muted,Font=new Font(Font.FontFamily,9f),Margin=new Padding(0)};textGrid.Controls.Add(description,0,1);textGrid.Controls.Add(Bind(new Label {AutoSize=true,MaximumSize=new Size(500,0),ForeColor=Color.FromArgb(236,178,94),Font=new Font(Font.FontFamily,9f),Margin=new Padding(0,4,0,0)},"Atalhos funcionam durante a luta."),0,5);details.Controls.Add(textGrid,1,0);grid.Controls.Add(details,0,2);grid.SetColumnSpan(details,2);
        var attacks=Bind(new LauncherCheckBox {Name="showAttackPanel",AutoSize=true,Checked=HudPreferences.ShowAttacks,Margin=new Padding(0,2,0,0)},"Mostrar painel de ataques");
        bool updatingAttacks=false;attacks.CheckedChanged+=(o,e)=>{if(updatingAttacks)return;try{HudPreferences.SetShowAttacks(attacks.Checked,true);}catch(Exception ex){updatingAttacks=true;attacks.Checked=HudPreferences.ShowAttacks;updatingAttacks=false;MessageBox.Show(this,UiLanguage.Text("Não foi possível salvar a exibição dos ataques.")+Environment.NewLine+ex.Message,Text,MessageBoxButtons.OK,MessageBoxIcon.Warning);}};

        var outerFrame=Bind(new LauncherCheckBox {Name="attackOuterFrame",AutoSize=true,Checked=AttackAppearance.OuterFrame,Margin=new Padding(0,2,0,0)},"Moldura externa dos ataques");
        bool updatingFrame=false;outerFrame.CheckedChanged+=(o,e)=>{if(updatingFrame)return;try{AttackAppearance.SetOuterFrame(outerFrame.Checked,true);}catch(Exception ex){updatingFrame=true;outerFrame.Checked=AttackAppearance.OuterFrame;updatingFrame=false;MessageBox.Show(this,UiLanguage.Text("Não foi possível salvar a exibição dos ataques.")+Environment.NewLine+ex.Message,Text,MessageBoxButtons.OK,MessageBoxIcon.Warning);}};
        attacks.CheckedChanged+=(o,e)=>{preview.ShowAttacks=attacks.Checked;preview.Invalidate();};outerFrame.CheckedChanged+=(o,e)=>{preview.OuterFrame=outerFrame.Checked;preview.Invalidate();};
        var frameKey=new RoundedButton {Name="frameShortcutKey",AutoSize=false,MinimumSize=new Size(76,28),Size=new Size(76,28),Padding=new Padding(8,3,8,3),Margin=new Padding(8,0,0,0),Anchor=AnchorStyles.Top};
        var attackKey=new RoundedButton {Name="attackShortcutKey",AutoSize=false,MinimumSize=new Size(76,28),Size=new Size(76,28),Padding=new Padding(8,3,8,3),Margin=new Padding(8,0,0,0),Anchor=AnchorStyles.Left};
        Action<Control,RoundedButton,string,int> addShortcutRow=(option,key,name,row)=>{var line=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=1,Margin=new Padding(0)};line.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));line.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));line.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));option.Anchor=AnchorStyles.Left;key.Anchor=AnchorStyles.Left;line.Controls.Add(option,0,0);line.Controls.Add(Bind(new Label {Name=name,AutoSize=true,Anchor=AnchorStyles.Left,ForeColor=LauncherTheme.Muted,Margin=new Padding(8,0,0,0)},"Atalho"),1,0);line.Controls.Add(key,2,0);textGrid.Controls.Add(line,0,row);};
        addShortcutRow(attacks,attackKey,"attackKeyLabel",2);addShortcutRow(outerFrame,frameKey,"frameKeyLabel",3);
        var lockKeys=Bind(new LauncherCheckBox {Name="lockHudShortcuts",AutoSize=true,Checked=ShortcutBindings.Locked,Margin=new Padding(0,2,0,0)},"Bloquear atalhos durante a luta");
        lockKeys.CheckedChanged+=(o,e)=>{try{ShortcutBindings.SetLocked(lockKeys.Checked,true);}catch(Exception ex){lockKeys.Checked=ShortcutBindings.Locked;MessageBox.Show(this,ex.Message,Text,MessageBoxButtons.OK,MessageBoxIcon.Warning);}};textGrid.Controls.Add(lockKeys,0,4);
        Action refreshKeys=()=>{frameKey.Anchor=AnchorStyles.Left;layoutKey.Text=ShortcutBindings.Label(ShortcutBindings.Layout);frameKey.Text=ShortcutBindings.Label(ShortcutBindings.Frame);attackKey.Text=ShortcutBindings.Label(ShortcutBindings.Attacks);attackKey.Width=Math.Max(76,TextRenderer.MeasureText(attackKey.Text,attackKey.Font).Width+24);attackKey.AccessibleName=UiLanguage.Text("Atalho de teclado")+": "+UiLanguage.Text("Mostrar painel de ataques");layoutKey.Width=Math.Max(76,TextRenderer.MeasureText(layoutKey.Text,layoutKey.Font).Width+24);frameKey.Width=Math.Max(76,TextRenderer.MeasureText(frameKey.Text,frameKey.Font).Width+24);layoutKey.AccessibleName=UiLanguage.Text("Atalho de teclado")+": "+UiLanguage.Text("Posição dos painéis");frameKey.AccessibleName=UiLanguage.Text("Atalho de teclado")+": "+UiLanguage.Text("Moldura externa dos ataques");};refreshText.Add(refreshKeys);refreshKeys();
        Action<int> changeKey=target=>{using(var capture=new KeyCaptureDialog(target==0?ShortcutBindings.Layout:target==1?ShortcutBindings.Frame:ShortcutBindings.Attacks,target==0?ShortcutBindings.DefaultLayout:target==1?ShortcutBindings.DefaultFrame:ShortcutBindings.DefaultAttacks)){if(capture.ShowDialog(this)==DialogResult.OK){try{ShortcutBindings.Set(target==0?capture.Selected:ShortcutBindings.Layout,target==1?capture.Selected:ShortcutBindings.Frame,target==2?capture.Selected:ShortcutBindings.Attacks,true);refreshKeys();}catch(Exception ex){MessageBox.Show(this,ex.Message,Text,MessageBoxButtons.OK,MessageBoxIcon.Warning);}}}};
        layoutKey.Click+=(o,e)=>changeKey(0);frameKey.Click+=(o,e)=>changeKey(1);attackKey.Click+=(o,e)=>changeKey(2);prefs.Controls.Add(grid);layout.Controls.Add(prefs,0,2);
        bool refreshingPlacement=false;
        Action refreshPlacement=()=>{refreshingPlacement=true;placement.BeginUpdate();placement.Items.Clear();foreach(string key in new[]{"Centralizada","Expandida","Clássica (4:3)","Bordas da tela"})placement.Items.Add(UiLanguage.Text(key));placement.SelectedIndex=(int)PanelPlacement.Mode;placement.EndUpdate();preview.Mode=PanelPlacement.Mode;preview.Invalidate();description.Text=UiLanguage.Text(PanelPlacement.Mode==PanelLayoutMode.ScreenEdges?"Painéis nas bordas da imagem, inclusive em ultrawide.":PanelPlacement.Mode==PanelLayoutMode.Classic?"Posição original em uma área 4:3.":PanelPlacement.Expanded?"Mais espaço entre os painéis.":"Equilíbrio entre centro e bordas.");refreshingPlacement=false;};refreshText.Add(refreshPlacement);refreshPlacement();
        placement.SelectedIndexChanged+=(o,e)=>{if(refreshingPlacement)return;try{PanelPlacement.Set((PanelLayoutMode)placement.SelectedIndex,true);refreshPlacement();}catch(Exception ex){MessageBox.Show(this,UiLanguage.Text("Não foi possível salvar a posição dos painéis.")+Environment.NewLine+ex.Message,Text,MessageBoxButtons.OK,MessageBoxIcon.Warning);refreshPlacement();}};
        var footer=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0,4,0,0)};footer.RowStyles.Add(new RowStyle(SizeType.Percent,100));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footer.Controls.Add(Bind(new Label {AutoSize=true,MaximumSize=new Size(330,0),ForeColor=LauncherTheme.Muted,Font=new Font(Font.FontFamily,8.5f),Anchor=AnchorStyles.Left,Margin=new Padding(0)},"Idioma e posição salvos automaticamente."),0,0);
        var buttons=new FlowLayoutPanel {FlowDirection=FlowDirection.RightToLeft,Dock=DockStyle.Fill,AutoSize=true,WrapContents=false,Margin=new Padding(0)};
        var start=Bind(new RoundedButton {Primary=true,AutoSize=true,Margin=new Padding(10,0,0,0)},"Salvar e iniciar");var save=Bind(new RoundedButton {AutoSize=true,Margin=new Padding(10,0,0,0)},"Salvar caminho");var close=Bind(new RoundedButton {AutoSize=true,DialogResult=DialogResult.Cancel,Margin=new Padding(10,0,0,0)},"Fechar");
        start.Click+=(o,e)=>Save(true);save.Click+=(o,e)=>Save(false);buttons.Controls.Add(start);buttons.Controls.Add(save);buttons.Controls.Add(close);footer.Controls.Add(buttons,1,0);layout.Controls.Add(footer,0,3);Controls.Add(layout);AcceptButton=start;CancelButton=close;ApplyLanguageDirection();
        pathBox.TextChanged+=(o,e)=>SetStatus("Clique em Salvar caminho ou Salvar e iniciar para confirmar.",LauncherTheme.Muted);
        language.SelectedIndexChanged+=(o,e)=>{try{UiLanguage.Set(UiLanguage.Codes[language.SelectedIndex],true);SuspendLayout();foreach(var update in refreshText)update();status.Text=UiLanguage.Text(statusKey);ApplyLanguageDirection();ResumeLayout(true);}catch(Exception ex){MessageBox.Show(this,UiLanguage.Text("Não foi possível salvar o idioma.")+Environment.NewLine+ex.Message,Text,MessageBoxButtons.OK,MessageBoxIcon.Warning);}};
    }
    protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);}
    void ApplyLanguageDirection(){RightToLeft=UiLanguage.IsRightToLeft?RightToLeft.Yes:RightToLeft.No;RightToLeftLayout=UiLanguage.IsRightToLeft;pathBox.RightToLeft=RightToLeft.No;}

    void Browse() {
        using(var select=new OpenFileDialog { Title=UiLanguage.Text("Escolha o executável do BT3 Decomp"),Filter="Tenkaichi3Decomp.exe|Tenkaichi3Decomp.exe|"+UiLanguage.Text("Executáveis (*.exe)")+"|*.exe",CheckFileExists=true }) {
            try { string current=Path.GetFullPath(pathBox.Text.Trim().Trim('"'));string directory=Path.GetDirectoryName(current);if(Directory.Exists(directory))select.InitialDirectory=directory; }catch(ArgumentException){}catch(NotSupportedException){}catch(PathTooLongException){}
            if(select.ShowDialog(this)==DialogResult.OK)pathBox.Text=select.FileName;
        }
    }
    void Save(bool start) {
        try {
            string exe=Path.GetFullPath(pathBox.Text.Trim().Trim('"'));
            if(!string.Equals(Path.GetExtension(exe),".exe",StringComparison.OrdinalIgnoreCase))throw new IOException(UiLanguage.Text("Selecione o arquivo executável (.exe) do jogo."));
            Profile profile=Program.LoadProfile();Program.Validate(exe,profile);
            File.WriteAllText(pathFile,exe);pathBox.Text=exe;
            GamePath=exe;GameProfile=profile;SetStatus("Caminho verificado e salvo.",Color.FromArgb(125,219,168));
            if(start) { DialogResult=DialogResult.OK;Close(); }
        }catch(Exception ex) { Program.SessionLog("Launcher validation: "+ex);SetStatus(ex.Message.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()??UiLanguage.Text("Não foi possível salvar. Confira o caminho e tente novamente."),Color.FromArgb(255,142,151)); }
    }
}

static class Program {
    public static readonly string Root=OnlineSingleFile.Prepare();
    public static string Hex(byte[] b) { return BitConverter.ToString(b).Replace("-","").ToLowerInvariant(); }
    public static byte[] Unhex(string s) { return Enumerable.Range(0,s.Length/2).Select(i=>Convert.ToByte(s.Substring(i*2,2),16)).ToArray(); }
    static string Hash(string path) { using(var s=File.OpenRead(path))using(var sha=SHA256.Create())return Hex(sha.ComputeHash(s)); }
    public static Profile LoadProfile() { return new JavaScriptSerializer().Deserialize<Profile>(File.ReadAllText(Path.Combine(Root,"profile.json"))); }
    public static long AllocateAddon(Memory m,Profile p) {
        // Allocation happens before the game's constructors reserve their memory.
        // MEM_FREE here does NOT mean unused by the game: 13000000..1F000000
        // is its preferred 192 MB Port_LowAlloc arena, reserved later at startup.
        // Keep the add-on high and below 2 GB for the existing 32-bit pointers
        // and rel32 trampoline, without displacing that arena or the image.
        List<long> candidates=new List<long>();
        for(long a=0x60000000;a<0x7f000000;a+=0x100000)if(AddonAddressAllowed(a,p.allocation_size))candidates.Add(a);
        foreach(long a in candidates) {
            IntPtr region=Native.VirtualAllocEx(m.Handle,new IntPtr(a),new IntPtr(p.allocation_size),0x3000,4);
            if(region.ToInt64()==a)return a;
        }
        throw new IOException(UiLanguage.Text("Nao foi encontrada uma regiao livre para o complemento. Nenhum arquivo foi alterado."));
    }
    internal static bool AddonAddressAllowed(long address,long size){
        return size>0&&size<=0x100000&&address>=0x60000000&&address<=0x7f000000-size&&
            (address+size<=0x70000000||address>=0x70004000);
    }
    public static byte[] RelocatePayload(Profile p,long actual) {
        byte[] data=p.resolved_payload==null?File.ReadAllBytes(Path.Combine(Root,"SparkingControls.payload")):(byte[])p.resolved_payload.Clone();long delta=actual-p.allocation;
        foreach(var r in p.relocations){
            if(r.width==8)Buffer.BlockCopy(BitConverter.GetBytes(checked(BitConverter.ToInt64(data,r.offset)+delta*r.sign)),0,data,r.offset,8);
            else Buffer.BlockCopy(BitConverter.GetBytes(checked((int)(BitConverter.ToInt32(data,r.offset)+delta*r.sign))),0,data,r.offset,4);
        }
        return data;
    }
    public static byte[] RelocatePatch(Patch p,long delta) {
        byte[] data=Unhex(p.replacement);
        Buffer.BlockCopy(BitConverter.GetBytes(checked((int)(BitConverter.ToInt32(data,1)+delta))),0,data,1,4);
        return data;
    }
    public static void Validate(string exe,Profile p) {
        if(p.format!=2||p.image_base!=0x20000000||p.allocation!=0x14000000||p.allocation_size!=0x10000||p.patches.Length!=1)throw new IOException(UiLanguage.Text("Perfil do complemento invalido."));
        if(!File.Exists(exe))throw new IOException(UiLanguage.Text("Selecione Tenkaichi3Decomp.exe."));
        if(Hash(Path.Combine(Root,"SparkingControls.payload"))!=p.payload_sha256)throw new IOException(UiLanguage.Text("O payload nao corresponde ao perfil. Extraia o pacote novamente."));
        byte[] gameBytes=File.ReadAllBytes(exe);PeImage image=null;
        if(PeImage.Digest(gameBytes)!=p.game_sha256){image=new PeImage(gameBytes);OnlineCompatibility.Resolve(image,p);}
        // Optional read-only diagnostics; never add an interface requirement or
        // a patch based on these symbols. Missing counters do not reject a build.
        try{
            if(image==null)image=new PeImage(gameBytes);
            if(image.HasSymbol("sLowArena")&&image.MatchesSymbol("sLowArena",image.Symbol("sLowArena"),"Port_LowAlloc"))p.diagnostic_arena_rva=image.Symbol("sLowArena");
            if(image.HasSymbol("_ZL4sFps"))p.diagnostic_fps_rva=image.Symbol("_ZL4sFps");
            if(image.HasSymbol("_ZL6sSpeed"))p.diagnostic_speed_rva=image.Symbol("_ZL6sSpeed");
        }catch(IOException){}catch(UnauthorizedAccessException){}
    }
    internal static bool DiagnosticTelemetry,OfflineDetachTest;
    static void Log(string s) { if(DiagnosticTelemetry)File.AppendAllText(Path.Combine(Root,"launcher.log"),DateTime.Now.ToString("s")+" "+s+Environment.NewLine); }
    [STAThread] public static int Main(string[] args) {
        DiagnosticTelemetry=args.Contains("--diagnostics");OfflineDetachTest=args.Contains("--offline-detach-test");
        Native.SetProcessDPIAware(); Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
        try {
            if(args.Length==2&&args[0]=="--variant-test")return VariantTests.Run(args[1]);
            UiLanguage.Load();PanelPlacement.Load();HudPreferences.Load();AttackAppearance.Load();ShortcutBindings.Load();string languageOverride=args.FirstOrDefault(a=>a.StartsWith("--lang="));if(languageOverride!=null)UiLanguage.Set(languageOverride.Substring(7),false);
            if(args.Length>1 && args[0]=="--compat-test"){return OnlineCompatibilityTests.Run(args[1]);}
            if(args.Length>0 && args[0]=="--selftest")throw new IOException("Use the separate packet-filter test report. The offline battle harness does not apply to this one-hook online prototype.");
            if(args.Length>1 && args[0]=="--probe") {
                var profile=LoadProfile();Validate(args[1],profile);Launch(Path.GetFullPath(args[1]),profile,false,true);return 0;
            }
            if(args.Length>2 && args[0]=="--restart-probe") {
                Validate(args[1],LoadProfile());using(var check=new GameSession(args[1],args[2])) {
                    check.Start();while(!check.Done)System.Threading.Thread.Sleep(20);
                    if(check.Failure!=null)throw check.Failure;
                    if(check.Latest==null)throw new IOException("The probe did not detect the descendant game.");
                }return 0;
            }
            if(args.Contains("--alpha-check")){AlphaHud.Check(Path.Combine(Root,"alpha-hud-check.txt"));return 0;}
            if(args.Length>0 && args[0]=="--preview") {
                using(var demo=new Overlay(null,null,true,args.FirstOrDefault(a=>!a.StartsWith("--")))) { string resolution=args.FirstOrDefault(a=>a.StartsWith("--resolution="));
                    if(resolution!=null) { var dimensions=resolution.Substring(13).Split('x');demo.SetPreviewSize(int.Parse(dimensions[0]),int.Parse(dimensions[1])); }
                    if(args.Contains("--transformations"))demo.SetTransformationPreview();if(args.Contains("--switch-preview"))demo.SetSwitchPreview();if(args.Contains("--fusion-preview"))demo.SetFusionPreview();if(args.Contains("--player2-preview"))demo.SetOnlineSidePreview(args.Contains("--online-preview"));
                    demo.SaveSkinReport(Path.Combine(Root,"menu-hd-loaded.txt"));demo.Show();Application.DoEvents();if(args.Contains("--transformations")){for(int wait=0;wait<15;wait++){System.Threading.Thread.Sleep(30);Application.DoEvents();}}demo.SavePreview(Path.Combine(Root,args.Contains("--transformations")?"Transformation-preview.png":"HUD-preview.png")); if(args.Contains("--alpha-preview"))demo.SaveTransparentPreview(Path.Combine(Root,args.Contains("--transformations")?"Transformation-alpha.png":"HUD-alpha.png")); if(args.Contains("--save-only"))return 0;Application.Run(demo); } return 0;
            }
            string chosen=args.FirstOrDefault(a=>!a.StartsWith("--"));
            string pathFile=SavedSettings.FilePath("game-path.txt");
            string initial=chosen??(File.Exists(pathFile)?File.ReadAllText(pathFile).Trim():"");
            string exe;Profile p;
            using(var location=new GameLocationWindow(initial,pathFile)) {
                if(args.Contains("--launcher-preview")) {
                    location.Show();Application.DoEvents();
                    using(var bitmap=new Bitmap(location.Width,location.Height)) { location.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(Root,"launcher-preview-"+UiLanguage.Code+".png")); }
                    return 0;
                }
                if(location.ShowDialog()!=DialogResult.OK)return 0;
                exe=location.GamePath;p=location.GameProfile;
            }
            if(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exe)).Length!=0) throw new IOException(UiLanguage.Text("Feche o jogo antes de iniciar pelo complemento. Ele aplica o patch antes do primeiro frame."));
            Launch(exe,p,!args.Contains("--no-hud"),false,DiagnosticTelemetry);return 0;
        } catch(Exception ex) { try { Log(ex.ToString()); }catch {} MessageBox.Show(ex.Message,"BT3 Sparking Controls - ONLINE TEST",MessageBoxButtons.OK,MessageBoxIcon.Error);return 1; }
    }
    public static void SessionLog(string message) { Log(message); }
    public static Profile PatchBeforeFirstFrame(IntPtr handle,long actual,string exe) {
        Profile p=LoadProfile();Validate(exe,p);Memory m=new Memory(handle);
            if(actual!=p.image_base) throw new IOException(UiLanguage.Text("Endereco base inesperado. O jogo foi encerrado antes de aplicar o patch."));
            foreach(var patch in p.patches) if(Hex(m.Read(actual+patch.rva,patch.expected.Length/2))!=patch.expected)throw new IOException(UiLanguage.Text("As instrucoes em memoria diferem da versao verificada."));
            long allocated=AllocateAddon(m,p),delta=allocated-p.allocation;
            m.Write(allocated,RelocatePayload(p,allocated));
            p.allocation=allocated;
            m.Protect(p.allocation,0xf000,0x20); // executable code RX, mailbox stays RW
            foreach(var patch in p.patches) {
                long addr=actual+patch.rva;byte[] data=RelocatePatch(patch,delta);uint old;
                if(!Native.VirtualProtectEx(handle,new IntPtr(addr),new IntPtr(data.Length),0x40,out old))throw new IOException(UiLanguage.Text("Falha ao preparar o patch em memoria."));
                m.Write(addr,data);uint ignored;
                if(!Native.VirtualProtectEx(handle,new IntPtr(addr),new IntPtr(data.Length),old,out ignored))throw new IOException(UiLanguage.Text("Falha ao restaurar protecao da memoria."));
            }
            if(!Native.FlushInstructionCache(handle,IntPtr.Zero,IntPtr.Zero))throw new IOException(UiLanguage.Text("Falha ao sincronizar as instrucoes."));
        foreach(var item in p.patches)
            if(Hex(m.Read(actual+item.rva,item.expected.Length/2))!=Hex(RelocatePatch(item,p.allocation-LoadProfile().allocation)))throw new IOException("Hook readback failed in restarted game.");
        return p;
    }
    static void Launch(string exe,Profile p,bool hud,bool probe=false,bool diagnostics=false) {
        if(!probe) {
            SessionLog("Diagnostic 1.1 Beta: HUD="+hud+"; offline_detach="+OfflineDetachTest+"; port arena excluded from add-on allocation; normal Windows heap; cropped persistent DIB");
            using(var session=new GameSession(exe,null,diagnostics,!diagnostics,OfflineDetachTest)) {
                session.Start();var state=session.Latest;
                if(state==null)throw new IOException("The game ended before the local controls could be prepared.");
                if(hud)using(var process=Process.GetProcessById(state.Pid))using(var overlay=new Overlay(process,new Reader(new Memory(state.Handle),state.Profile),false,exe,session))Application.Run(overlay);
                else while(!session.Done)System.Threading.Thread.Sleep(50);
                if(session.Failure!=null)throw new IOException(UiLanguage.Text("Não foi possível acompanhar a reinicialização do jogo."),session.Failure);
            }
            return;
        }

        Native.STARTUPINFO si=new Native.STARTUPINFO();si.cb=Marshal.SizeOf(si);
        Native.PROCESS_INFORMATION pi;
        if(!Native.CreateProcess(exe,new StringBuilder("\""+exe+"\""),IntPtr.Zero,IntPtr.Zero,false,4,IntPtr.Zero,Path.GetDirectoryName(exe),ref si,out pi))
            throw new IOException(UiLanguage.Text("Nao foi possivel iniciar o jogo. Erro Windows ")+Marshal.GetLastWin32Error());
        bool resumed=false;
        try {
            Memory m=new Memory(pi.process);Native.PBI info=new Native.PBI();int returned;
            if(Native.NtQueryInformationProcess(pi.process,0,ref info,Marshal.SizeOf(info),out returned)!=0) throw new IOException(UiLanguage.Text("Nao foi possivel verificar a base do jogo."));
            long actual=BitConverter.ToInt64(m.Read(info.peb.ToInt64()+16,8),0);
            if(actual!=p.image_base) throw new IOException(UiLanguage.Text("Endereco base inesperado. O jogo foi encerrado antes de aplicar o patch."));
            foreach(var patch in p.patches) if(Hex(m.Read(actual+patch.rva,patch.expected.Length/2))!=patch.expected)throw new IOException(UiLanguage.Text("As instrucoes em memoria diferem da versao verificada."));
            long allocated=AllocateAddon(m,p),delta=allocated-p.allocation;
            m.Write(allocated,RelocatePayload(p,allocated));
            p.allocation=allocated;
            m.Protect(p.allocation,0xf000,0x20); // executable code RX, mailbox stays RW
            foreach(var patch in p.patches) {
                long addr=actual+patch.rva;byte[] data=RelocatePatch(patch,delta);uint old;
                if(!Native.VirtualProtectEx(pi.process,new IntPtr(addr),new IntPtr(data.Length),0x40,out old))throw new IOException(UiLanguage.Text("Falha ao preparar o patch em memoria."));
                m.Write(addr,data);uint ignored;
                if(!Native.VirtualProtectEx(pi.process,new IntPtr(addr),new IntPtr(data.Length),old,out ignored))throw new IOException(UiLanguage.Text("Falha ao restaurar protecao da memoria."));
            }
            if(!Native.FlushInstructionCache(pi.process,IntPtr.Zero,IntPtr.Zero))throw new IOException(UiLanguage.Text("Falha ao sincronizar as instrucoes."));
            if(probe) {
                foreach(var patch in p.patches)if(Hex(m.Read(actual+patch.rva,patch.expected.Length/2))!=Hex(RelocatePatch(patch,delta)))throw new IOException(UiLanguage.Text("Falha na releitura do patch."));
                File.WriteAllText(Path.Combine(Root,"probe-results.txt"),
                    "PASS: resolved image/payload validation; suspended child creation; PEB/image base; original instruction signatures; free low-memory allocation; payload write; RX protection; one local packet hook; original battle command/decision code untouched; instruction-cache flush; readback.\r\n"+
                    "Game image base: 0x"+actual.ToString("X")+"; add-on base: 0x"+allocated.ToString("X")+".\r\n"+
                    "The child was terminated BEFORE ResumeThread: no game code was executed. This is not a gameplay test.\r\n"+
                    "Game SHA256 after probe: "+Hash(exe)+"\r\n");
                return;
            }
            if(Native.ResumeThread(pi.thread)==0xffffffff)throw new IOException(UiLanguage.Text("Falha ao liberar o inicio do jogo."));
            resumed=true;Log("Jogo iniciado com patch em memoria; PID "+pi.pid+"; HUD "+hud);
            using(Process game=Process.GetProcessById(pi.pid)) {
                if(hud)using(var overlay=new Overlay(game,new Reader(m,p),false,exe)) Application.Run(overlay);
                else game.WaitForExit();
            }
        } finally {
            if(!resumed) Native.TerminateProcess(pi.process,1);
            Native.CloseHandle(pi.thread);Native.CloseHandle(pi.process);
        }
    }
}

