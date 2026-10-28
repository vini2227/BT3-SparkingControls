using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

// GS address tables follow port/src/gs/gs_core.c (AGPL-3.0-or-later).
// Artwork is read from the user's game installation at runtime, never bundled.
sealed class GameButtonIcons : IDisposable {
    public readonly List<string> HdLoaded=new List<string>();
    readonly Dictionary<string,RectangleF> triangleInkCache=new Dictionary<string,RectangleF>();
    internal int TriangleInkScans;
    readonly Dictionary<int,Size> promptTriangleSizes=new Dictionary<int,Size>();
    readonly Dictionary<int,Bitmap> promptTriangles=new Dictionary<int,Bitmap>();
    readonly Dictionary<string,Rectangle> symbolBounds=new Dictionary<string,Rectangle>();
    readonly Dictionary<string,Bitmap> icons=new Dictionary<string,Bitmap>();
    readonly Dictionary<string,Rectangle> singleBounds=new Dictionary<string,Rectangle>();
    static readonly int[,] Block32={{0, 1, 4, 5, 16, 17, 20, 21}, {2, 3, 6, 7, 18, 19, 22, 23}, {8, 9, 12, 13, 24, 25, 28, 29}, {10, 11, 14, 15, 26, 27, 30, 31}};
    static readonly int[,] Block16={{0, 2, 8, 10}, {1, 3, 9, 11}, {4, 6, 12, 14}, {5, 7, 13, 15}, {16, 18, 24, 26}, {17, 19, 25, 27}, {20, 22, 28, 30}, {21, 23, 29, 31}};
    static readonly int[,] Col32={{0, 1, 4, 5, 8, 9, 12, 13}, {2, 3, 6, 7, 10, 11, 14, 15}, {16, 17, 20, 21, 24, 25, 28, 29}, {18, 19, 22, 23, 26, 27, 30, 31},
                                     {32, 33, 36, 37, 40, 41, 44, 45}, {34, 35, 38, 39, 42, 43, 46, 47}, {48, 49, 52, 53, 56, 57, 60, 61}, {50, 51, 54, 55, 58, 59, 62, 63}};
    static readonly int[,] Col4={
    {0, 8, 32, 40, 64, 72, 96, 104, 2, 10, 34, 42, 66, 74, 98, 106, 4, 12, 36, 44, 68, 76, 100, 108, 6, 14, 38, 46, 70, 78, 102, 110},
    {16, 24, 48, 56, 80, 88, 112, 120, 18, 26, 50, 58, 82, 90, 114, 122, 20, 28, 52, 60, 84, 92, 116, 124, 22, 30, 54, 62, 86, 94, 118, 126},
    {65, 73, 97, 105, 1, 9, 33, 41, 67, 75, 99, 107, 3, 11, 35, 43, 69, 77, 101, 109, 5, 13, 37, 45, 71, 79, 103, 111, 7, 15, 39, 47},
    {81, 89, 113, 121, 17, 25, 49, 57, 83, 91, 115, 123, 19, 27, 51, 59, 85, 93, 117, 125, 21, 29, 53, 61, 87, 95, 119, 127, 23, 31, 55, 63}};
    static int U32(byte[] b,int offset) {
        if(offset<0 || offset>b.Length-4)throw new IOException("Folha de ícones truncada.");
        uint value=BitConverter.ToUInt32(b,offset);if(value>int.MaxValue)throw new IOException("Offset de ícone inválido.");return (int)value;
    }
    static int Address32(int x,int y) { return Block32[(y>>3)&3,(x>>3)&7]*64+Col32[y&7,x&7]; }
    static int Address4(int x,int y) { int column=(y&15)>>2,sx=(x&31)^((column&1)!=0?4:0);return Block16[(y>>4)&7,(x>>5)&3]*512+Col4[y&3,sx]+column*128; }
    internal static Bitmap Decode(byte[] data,int resource,int id) {
        int descriptor=checked(resource+U32(data,resource+4)*4+id*64);
        int pixels=checked(resource+U32(data,descriptor)*4),palette=checked(resource+U32(data,descriptor+4)*4);
        if(descriptor<0 || descriptor>data.Length-64)throw new IOException("Descrição de ícone inválida.");
        ulong tex=BitConverter.ToUInt64(data,descriptor+48);
        int width=1<<(int)((tex>>26)&15),height=1<<(int)((tex>>30)&15);
        if(((tex>>20)&63)!=20 || width>64 || height>64 || width<8 || height<8)throw new IOException("Formato de ícone não reconhecido.");
        int transferWidth=U32(data,pixels+48),transferHeight=U32(data,pixels+52);
        if(transferWidth<1 || transferWidth>64 || transferHeight<1 || transferHeight>32 || transferWidth*transferHeight*8!=width*height)throw new IOException("Transferência de ícone não reconhecida.");
        if(pixels>data.Length-96-transferWidth*transferHeight*4 || palette>data.Length-160)throw new IOException("Pixels de ícone truncados.");
        uint[] vram=new uint[4096];
        for(int y=0;y<transferHeight;y++)for(int x=0;x<transferWidth;x++)vram[Address32(x,y)]=BitConverter.ToUInt32(data,pixels+96+(y*transferWidth+x)*4);
        var bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb);
        for(int y=0;y<height;y++)for(int x=0;x<width;x++) {
            int address=Address4(x,y),index=(int)((vram[address/8]>>((address&7)*4))&15),at=palette+96+index*4;
            bitmap.SetPixel(x,y,Color.FromArgb(Math.Min(255,(int)data[at+3]*2),data[at],data[at+1],data[at+2]));
        }
        return bitmap;
    }
    public GameButtonIcons(string gameExe) {
        try {
            if(string.IsNullOrEmpty(gameExe))return;
            string path=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(gameExe)),"gamedata","pzs3us1","00004.bin");
            var info=new FileInfo(path);if(!info.Exists || info.Length<64 || info.Length>4*1024*1024)return;
            byte[] data=File.ReadAllBytes(path);int resource=U32(data,32)&~3;
            if(U32(data,resource)<19 || U32(data,resource)>128)throw new IOException("Tabela de ícones inesperada.");
            string[] keys={"R2","+","/","↑","↓","←","→","□","△","○","×","L1","R1"};int[] indices={18,0,1,4,5,7,6,11,12,10,13,14,17};
            var hd=new HdMenuTextures(gameExe);
            for(int i=0;i<keys.Length;i++)icons.Add(keys[i],hd.Replace(data,resource,indices[i])??Decode(data,resource,indices[i]));
            foreach(int id in new[]{12,22,25}) {
                var native=Decode(data,resource,id);promptTriangleSizes[id]=native.Size;
                var high=hd.Replace(data,resource,id);promptTriangles[id]=high??native;if(high!=null)native.Dispose();
            }
            HdLoaded.AddRange(hd.Loaded);
        }catch(IOException){Dispose();}catch(UnauthorizedAccessException){Dispose();}catch(OverflowException){Dispose();}catch(ArgumentException){Dispose();}
    }
    static float TokenWidth(string token) { return token=="+"?22:token=="/"?14:30; }
    public bool HasIcons { get { return icons.Count>=11; } }
    public static float Measure(string command,float scale) { float width=0;foreach(string token in command.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries))width+=TokenWidth(token)+2;return Math.Max(0,width-2)*scale; }
    public bool Draw(Graphics g,string command,RectangleF box,float scale) {
        if(!HasIcons)return false;
        string[] tokens=command.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
        foreach(string token in tokens)if(!icons.ContainsKey(token))return false;
        float x=box.X,height=30*scale;
        foreach(string token in tokens) {
            float width=TokenWidth(token)*scale;
            // Crop native operator padding so + and / have the same visual weight as the buttons.
            if(token=="+" || token=="/") {
                float inkHeight=(token=="+"?20:24)*scale;
                DrawSingle(g,token,new RectangleF(x,box.Y+(box.Height-inkHeight)/2,width,inkHeight));
            }else g.DrawImage(icons[token],new RectangleF(x,box.Y+(box.Height-height)/2,width,height));
            x+=width+2*scale;
        }
        return true;
    }
    public bool DrawSingle(Graphics g,string token,RectangleF box) {
        Bitmap icon;if(!icons.TryGetValue(token,out icon))return false;
        Rectangle source;
        if(!singleBounds.TryGetValue(token,out source)) {
            int left=icon.Width,top=icon.Height,right=-1,bottom=-1;
            for(int y=0;y<icon.Height;y++)for(int x=0;x<icon.Width;x++)if(icon.GetPixel(x,y).A>8){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}
            if(right<left || bottom<top)return false;
            source=new Rectangle(left,top,right-left+1,bottom-top+1);singleBounds[token]=source;
        }
        float fit=Math.Min(box.Width/source.Width,box.Height/source.Height);
        float w=source.Width*fit,h=source.Height*fit;
        g.DrawImage(icon,new RectangleF(box.X+(box.Width-w)/2,box.Y+(box.Height-h)/2,w,h),source,GraphicsUnit.Pixel);return true;
    }
    // Locate only the green Triangle inside the native animation cell. Its hand/glow is a separate sprite.
    public RectangleF TriangleInk(RectangleF box,int texture,Rectangle uv) {
        Bitmap native;
        if(uv.Width<=0||uv.Height<=0||!promptTriangles.TryGetValue(texture,out native))
            return new RectangleF(box.X+box.Width*.18f,box.Y+box.Height*.34f,box.Width*.64f,box.Height*.42f);
        string key=texture+":"+uv.X+":"+uv.Y+":"+uv.Width+":"+uv.Height;
        RectangleF normalized;
        if(!triangleInkCache.TryGetValue(key,out normalized)) {
            Size original=promptTriangleSizes[texture];
            uv=Rectangle.FromLTRB(uv.Left*native.Width/original.Width,uv.Top*native.Height/original.Height,uv.Right*native.Width/original.Width,uv.Bottom*native.Height/original.Height);
            int left=native.Width,top=native.Height,right=-1,bottom=-1;
            TriangleInkScans++;
            for(int y=Math.Max(0,uv.Top);y<Math.Min(native.Height,uv.Bottom);y++)for(int x=Math.Max(0,uv.Left);x<Math.Min(native.Width,uv.Right);x++) {
                Color c=native.GetPixel(x,y);if(c.A>16&&c.G>60&&c.G>c.R*1.3f&&c.G>c.B*1.15f){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}
            }
            normalized=right<left||uv.Width<=0||uv.Height<=0?new RectangleF(.18f,.34f,.64f,.42f):new RectangleF((float)(left-uv.Left)/uv.Width,(float)(top-uv.Top)/uv.Height,(float)(right-left+1)/uv.Width,(float)(bottom-top+1)/uv.Height);
            // Native animation cells are finite. Bound unexpected/corrupt UV sequences too.
            if(triangleInkCache.Count>=256)triangleInkCache.Clear();
            triangleInkCache[key]=normalized;
        }
        return new RectangleF(box.X+normalized.X*box.Width,box.Y+normalized.Y*box.Height,normalized.Width*box.Width,normalized.Height*box.Height);
    }

    public bool DrawFaceSymbol(Graphics g,string token,RectangleF box) {
        Bitmap icon;if(!icons.TryGetValue(token,out icon))return false;
        Rectangle source;
        if(!symbolBounds.TryGetValue(token,out source)) {
            int left=icon.Width,top=icon.Height,right=-1,bottom=-1;
            for(int y=0;y<icon.Height;y++)for(int x=0;x<icon.Width;x++) {
                Color c=icon.GetPixel(x,y);bool match=token=="○"?c.R>70&&c.R>c.G*1.3f&&c.R>c.B*1.2f:c.R>70&&c.B>60&&c.R>c.G*1.2f&&c.B>c.G*1.1f;
                if(c.A>16&&match){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}
            }
            if(right<left)return false;source=new Rectangle(left,top,right-left+1,bottom-top+1);symbolBounds[token]=source;
        }
        // icons contains the HD replacement when installed, otherwise the native fallback.
        g.DrawImage(icon,box,source,GraphicsUnit.Pixel);return true;
    }
    public void Dispose() { foreach(var icon in icons.Values)icon.Dispose();icons.Clear();triangleInkCache.Clear();singleBounds.Clear();symbolBounds.Clear();foreach(var image in promptTriangles.Values)image.Dispose();promptTriangles.Clear(); }
}


// Native pause-menu artwork, loaded from the user's common battle resource.
sealed class GameMenuSkin : IDisposable {
    readonly Dictionary<int,Bitmap> parts=new Dictionary<int,Bitmap>();
    readonly Dictionary<int,Rectangle> bounds=new Dictionary<int,Rectangle>();
    readonly Bitmap[] frameTiles=new Bitmap[4];
    public readonly List<string> HdLoaded=new List<string>();
    public GameMenuSkin(string gameExe) {
        try {
            if(string.IsNullOrEmpty(gameExe))return;
            string path=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(gameExe)),"gamedata","pzs3us1","00005.bin");
            var file=new FileInfo(path);if(!file.Exists || file.Length<64 || file.Length>4*1024*1024)return;
            byte[] data=File.ReadAllBytes(path);int resource=BitConverter.ToInt32(data,28)&~3;
            if(resource<0 || resource>data.Length-8 || BitConverter.ToInt32(data,resource)!=47)return;
            var hd=new HdMenuTextures(gameExe);
            foreach(int id in new[]{2,16,17,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37})parts[id]=hd.Replace(data,resource,id)??GameButtonIcons.Decode(data,resource,id);
            HdLoaded.AddRange(hd.Loaded);
            Bitmap frame;if(parts.TryGetValue(2,out frame))for(int i=0;i<4;i++){frameTiles[i]=new Bitmap(frame);frameTiles[i].RotateFlip(i==0?RotateFlipType.RotateNoneFlipNone:i==1?RotateFlipType.RotateNoneFlipX:i==2?RotateFlipType.RotateNoneFlipY:RotateFlipType.RotateNoneFlipXY);}
        }catch(IOException){Dispose();}catch(UnauthorizedAccessException){Dispose();}catch(ArgumentException){Dispose();}catch(OverflowException){Dispose();}
    }
    public static string UiFontPath(string gameExe) {
        try {
            if(string.IsNullOrEmpty(gameExe))return null;
            string root=Path.GetDirectoryName(Path.GetFullPath(gameExe)),folder=Path.Combine(root,"fonts"),list=Path.Combine(folder,"fonts.tsv");
            int chosen=0;string settings=Path.Combine(root,"bt3_settings.txt");
            if(File.Exists(settings))foreach(string line in File.ReadAllLines(settings))if(line.StartsWith("text_font_id="))int.TryParse(line.Substring(13),out chosen);
            string fallback=null;
            foreach(string line in File.ReadAllLines(list)) {
                if(string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))continue;
                string[] fields=line.Split('\t');int uses=3;if(fields.Length<2 || (fields.Length>2 && !int.TryParse(fields[2],out uses)) || (uses&2)==0)continue;
                string candidate=Path.GetFullPath(Path.Combine(folder,fields[0].Replace('/',Path.DirectorySeparatorChar)));
                if(!candidate.StartsWith(Path.GetFullPath(folder)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase) || !File.Exists(candidate) || new FileInfo(candidate).Length>16*1024*1024)continue;
                if(fallback==null)fallback=candidate;uint id=2166136261;
                foreach(byte value in System.Text.Encoding.UTF8.GetBytes(fields[0]))id=unchecked((id^value)*16777619);
                if((int)(id&0x7fffffff)==chosen)return candidate;
            }
            return fallback;
        }catch(IOException){return null;}catch(UnauthorizedAccessException){return null;}catch(ArgumentException){return null;}
    }
    public void Strip(Graphics g,RectangleF box,bool selected,bool fitBand=false) {
        // Stretch only an interior source column; the native chamfer retains its proportions.
        Bitmap image;int id=selected?17:16;
        if(parts.TryGetValue(id,out image)) {
            float cap=Math.Min(box.Height,box.Width);
            int sourceTop=0,sourceHeight=image.Height;
            if(fitBand) {
                // The native row cell has a transparent bottom margin. Fit its visible band, not the cell.
                int column=image.Width*3/4,maxAlpha=0;
                for(int y=0;y<image.Height;y++)maxAlpha=Math.Max(maxAlpha,image.GetPixel(column,y).A);
                int first=image.Height,last=-1;
                for(int y=0;y<image.Height;y++)if(image.GetPixel(column,y).A>Math.Max(1,maxAlpha/5)){first=Math.Min(first,y);last=y;}
                if(last>=first){sourceTop=first;sourceHeight=last-first+1;}
            }
            using(var attributes=new ImageAttributes()) {
                attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                int left=(int)Math.Round(box.Left),top=(int)Math.Round(box.Top),join=(int)Math.Round(box.Left+cap),right=(int)Math.Round(box.Right),bottom=(int)Math.Round(box.Bottom);
                g.DrawImage(image,new Rectangle(left,top,join-left,bottom-top),0,sourceTop,image.Width,sourceHeight,GraphicsUnit.Pixel,attributes);
                if(right>join)g.DrawImage(image,new Rectangle(join,top,right-join,bottom-top),image.Width*.75f,sourceTop,Math.Max(1,image.Width/32f),sourceHeight,GraphicsUnit.Pixel,attributes);
            }
        }else using(var brush=new SolidBrush(selected?Color.FromArgb(185,184,128,57):Color.FromArgb(100,70,94,111)))g.FillRectangle(brush,box);
    }
    public void Frame(Graphics g,RectangleF box,float scale) {
        Bitmap image=frameTiles[0];if(image==null){using(var bg=new SolidBrush(Color.FromArgb(155,6,34,67)))g.FillRectangle(bg,box);using(var line=new Pen(Color.FromArgb(220,205,231,255),scale))g.DrawRectangle(line,box.X,box.Y,box.Width,box.Height);return;}
        // Assemble on an integer pixel grid, replacing alpha rather than blending adjacent tiles.
        // Mirrored sampling prevents transparent source-edge fringes at the nine-slice joins.
        int left=(int)Math.Round(box.Left),top=(int)Math.Round(box.Top);
        int width=Math.Max(1,(int)Math.Round(box.Right)-left),height=Math.Max(1,(int)Math.Round(box.Bottom)-top);
        int c=Math.Max(1,Math.Min(Math.Min(width,height)/2,(int)Math.Round(Math.Min(24*scale,Math.Min(width,height)*.16f))));
        float w=image.Width,h=image.Height;
        using(var assembled=new Bitmap(width,height,PixelFormat.Format32bppPArgb))
        using(var canvas=Graphics.FromImage(assembled))
        using(var attributes=new ImageAttributes()) {
            canvas.CompositingMode=System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            canvas.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            canvas.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.Half;
            attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
            using(var fill=new SolidBrush(image.GetPixel(image.Width-1,image.Height-1)))canvas.FillRectangle(fill,0,0,width,height);
            Rectangle[] target={new Rectangle(0,0,c,c),new Rectangle(width-c,0,c,c),new Rectangle(0,height-c,c,c),new Rectangle(width-c,height-c,c,c)};
            for(int i=0;i<4;i++)canvas.DrawImage(frameTiles[i],target[i],0,0,w,h,GraphicsUnit.Pixel,attributes);
            canvas.DrawImage(frameTiles[0],new Rectangle(c,0,width-2*c,c),w-2,0,1,h,GraphicsUnit.Pixel,attributes);
            canvas.DrawImage(frameTiles[2],new Rectangle(c,height-c,width-2*c,c),w-2,0,1,h,GraphicsUnit.Pixel,attributes);
            canvas.DrawImage(frameTiles[0],new Rectangle(0,c,c,height-2*c),0,h-2,w,1,GraphicsUnit.Pixel,attributes);
            canvas.DrawImage(frameTiles[1],new Rectangle(width-c,c,c,height-2*c),0,h-2,w,1,GraphicsUnit.Pixel,attributes);
            g.DrawImageUnscaled(assembled,left,top);
        }
    }

    bool Sprite(Graphics g,int id,RectangleF target,bool available,bool right) {
        Bitmap image;if(!parts.TryGetValue(id,out image))return false;Rectangle source;
        if(!bounds.TryGetValue(id,out source)) {
            int l=image.Width,t=image.Height,r=-1,b=-1;
            for(int y=0;y<image.Height;y++)for(int x=0;x<image.Width;x++)if(image.GetPixel(x,y).A>16){l=Math.Min(l,x);t=Math.Min(t,y);r=Math.Max(r,x);b=Math.Max(b,y);}
            if(r<l || b<t)return false;source=new Rectangle(l,t,r-l+1,b-t+1);bounds[id]=source;
        }
        float scale=Math.Min(target.Width/source.Width,target.Height/source.Height),w=source.Width*scale,h=source.Height*scale;
        Rectangle dest=Rectangle.Round(new RectangleF(right?target.Right-w:target.X+(target.Width-w)/2,target.Y+(target.Height-h)/2,w,h));
        using(var attributes=new ImageAttributes()) {
            if(!available){var matrix=new ColorMatrix();matrix.Matrix33=.55f;attributes.SetColorMatrix(matrix);}
            g.DrawImage(image,dest,source.X,source.Y,source.Width,source.Height,GraphicsUnit.Pixel,attributes);
        }
        return true;
    }
    public bool Cost(Graphics g,string text,bool skill,RectangleF box,bool available) {
        int number;if(!int.TryParse(text,out number) || number<1 || number>9)return false;
        return Sprite(g,(skill?19:28)+number,box,available,true);
    }
    public bool Bolt(Graphics g,RectangleF box,bool available) {return Sprite(g,19,box,available,false);}
    public void Dispose(){foreach(var value in parts.Values)value.Dispose();foreach(var tile in frameTiles)if(tile!=null)tile.Dispose();parts.Clear();bounds.Clear();}
}

// Per-pixel alpha is required for genuine translucent strips over the game.
// No input handling, game memory writes or screen capture takes place here.
static class AlphaHud {
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)] struct P { public int x,y; public P(int a,int b){x=a;y=b;} }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)] struct S { public int cx,cy;public S(int a,int b){cx=a;cy=b;} }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential,Pack=1)] struct Blend { public byte op,flags,alpha,format; }
    [System.Runtime.InteropServices.DllImport("user32.dll",SetLastError=true)] static extern bool UpdateLayeredWindow(IntPtr hwnd,IntPtr dst,ref P point,ref S size,IntPtr src,ref P origin,uint key,ref Blend blend,uint flags);
    [System.Runtime.InteropServices.DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
    [System.Runtime.InteropServices.DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hwnd,IntPtr dc);
    [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
    [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
    [System.Runtime.InteropServices.DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)] struct BitmapInfo {
        public uint headerSize;public int width,height;public ushort planes,bits;public uint compression,imageSize;public int xppm,yppm;public uint colors,important,color;
    }
    [System.Runtime.InteropServices.DllImport("gdi32.dll",SetLastError=true)] static extern IntPtr CreateDIBSection(IntPtr dc,ref BitmapInfo info,uint usage,out IntPtr bits,IntPtr section,uint offset);
    [System.Runtime.InteropServices.DllImport("kernel32.dll",EntryPoint="RtlMoveMemory")] static extern void CopyMemory(IntPtr destination,IntPtr source,UIntPtr count);
    sealed class Surface : IDisposable {
        public IntPtr Dc,Bits;IntPtr image,previous;public Size Size;
        public void Ensure(Size size){
            if(Size==size&&image!=IntPtr.Zero)return;
            Dispose();
            try{
                Dc=CreateCompatibleDC(IntPtr.Zero);if(Dc==IntPtr.Zero)throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                var info=new BitmapInfo {headerSize=40,width=size.Width,height=-size.Height,planes=1,bits=32};
                image=CreateDIBSection(Dc,ref info,0,out Bits,IntPtr.Zero,0);
                if(image==IntPtr.Zero)throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                previous=SelectObject(Dc,image);if(previous==IntPtr.Zero||previous==new IntPtr(-1))throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                Size=size;SurfaceCreates++;
            }catch{Dispose();throw;}
        }
        public void Dispose(){
            if(previous!=IntPtr.Zero&&previous!=new IntPtr(-1)&&Dc!=IntPtr.Zero)SelectObject(Dc,previous);
            if(image!=IntPtr.Zero)DeleteObject(image);if(Dc!=IntPtr.Zero)DeleteDC(Dc);
            image=previous=Dc=Bits=IntPtr.Zero;Size=System.Drawing.Size.Empty;
        }
    }
    static readonly Dictionary<System.Windows.Forms.Form,Surface> surfaces=new Dictionary<System.Windows.Forms.Form,Surface>();
    public static long SurfaceCreates {get;private set;}public static long Presents {get;private set;}public static long CopiedBytes {get;private set;}
    public static void Release(System.Windows.Forms.Form window){Surface surface;if(surfaces.TryGetValue(window,out surface)){surfaces.Remove(window);surface.Dispose();}}
    public static void Present(System.Windows.Forms.Form window,Bitmap bitmap){Present(window,bitmap,new Rectangle(Point.Empty,bitmap.Size),window.Location);}
    public static void Present(System.Windows.Forms.Form window,Bitmap bitmap,Rectangle region,Point screenPosition) {
        region=Rectangle.Intersect(region,new Rectangle(Point.Empty,bitmap.Size));if(region.Width<1||region.Height<1)return;
        Surface surface;
        if(!surfaces.TryGetValue(window,out surface)){surface=new Surface();surfaces.Add(window,surface);window.Disposed+=(o,e)=>Release(window);}
        surface.Ensure(region.Size);
        var pixels=bitmap.LockBits(region,ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);
        try{
            int stride=checked(region.Width*4);
            for(int y=0;y<region.Height;y++)CopyMemory(IntPtr.Add(surface.Bits,y*stride),IntPtr.Add(pixels.Scan0,y*pixels.Stride),new UIntPtr((uint)stride));
        }finally{bitmap.UnlockBits(pixels);}
        var pos=new P(screenPosition.X,screenPosition.Y);var origin=new P(0,0);var size=new S(region.Width,region.Height);var blend=new Blend {alpha=255,format=1};
        var bounds=new Rectangle(screenPosition,region.Size);if(window.Bounds!=bounds)window.Bounds=bounds;
        if(!UpdateLayeredWindow(window.Handle,IntPtr.Zero,ref pos,ref size,surface.Dc,ref origin,0,ref blend,2))throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
        Presents++;CopiedBytes+=(long)region.Width*region.Height*4;
    }
    sealed class ProbeWindow : System.Windows.Forms.Form {
        protected override System.Windows.Forms.CreateParams CreateParams {get {var p=base.CreateParams;p.ExStyle|=0x80000|0x80;return p;}}
    }
    public static void Check(string path) {
        using(var window=new ProbeWindow())using(var bitmap=new Bitmap(80,40,PixelFormat.Format32bppPArgb)) {
            window.CreateControl();using(var g=Graphics.FromImage(bitmap)){g.Clear(Color.Transparent);using(var brush=new SolidBrush(Color.FromArgb(90,60,100,130)))g.FillRectangle(brush,4,4,72,32);}
            Present(window,bitmap);File.WriteAllText(path,"PASS: UpdateLayeredWindow accepted a premultiplied alpha HUD on a hidden test window. No game was started or modified.\r\n");
        }
    }
}


// Replacement lookup uses content fingerprints plus the game's XXH3 texture names.
// The index contains hashes only; DDS/PNG artwork stays in the user's installation.
sealed class HdMenuTextures {
    readonly Dictionary<string,string[]> bindings;
    readonly Dictionary<string,string> paths=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
    public readonly List<string> Loaded=new List<string>();
    public HdMenuTextures(string exe) {
        try {
            if(string.IsNullOrEmpty(exe))return;
            string root=Path.GetDirectoryName(Path.GetFullPath(exe)),settings=Path.Combine(root,"bt3_settings.txt");
            if(File.Exists(settings))foreach(string line in File.ReadAllLines(settings))if(line.Trim()=="texture_pack=0")return;
            bindings=new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string,string[]>>(File.ReadAllText(Path.Combine(Program.Root,"assets","menu-textures.json")));
            string folder=Environment.GetEnvironmentVariable("BT3_TEXTURES");if(folder==null)folder=Path.Combine(root,"textures");if(!Directory.Exists(folder))return;
            foreach(string path in Directory.EnumerateFiles(folder,"*",SearchOption.AllDirectories)) {
                string ext=Path.GetExtension(path);if(!ext.Equals(".dds",StringComparison.OrdinalIgnoreCase) && !ext.Equals(".png",StringComparison.OrdinalIgnoreCase))continue;
                string key=Path.GetFileNameWithoutExtension(path);if(!paths.ContainsKey(key))paths.Add(key,path);
            }
        }catch(IOException){}catch(UnauthorizedAccessException){}catch(ArgumentException){}
    }
    public Bitmap Replace(byte[] data,int resource,int id) {
        try {
            if(bindings==null || paths.Count==0)return null;
            int descriptor=resource+BitConverter.ToInt32(data,resource+4)*4+id*64;
            int pixels=resource+BitConverter.ToInt32(data,descriptor)*4,palette=resource+BitConverter.ToInt32(data,descriptor+4)*4;
            int bytes=checked(BitConverter.ToInt32(data,pixels+48)*BitConverter.ToInt32(data,pixels+52)*4);
            if(bytes<1 || bytes>1024*1024 || pixels+96>data.Length-bytes || palette+96>data.Length-64)return null;
            byte[] source=new byte[bytes+64];Buffer.BlockCopy(data,pixels+96,source,0,bytes);Buffer.BlockCopy(data,palette+96,source,bytes,64);
            string hash;using(var sha=System.Security.Cryptography.SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(source)).Replace("-","").ToLowerInvariant();
            string[] names;if(!bindings.TryGetValue(hash,out names))return null;
            foreach(string name in names) {string path;if(!paths.TryGetValue(name,out path))continue;Bitmap image=DdsImage.Load(path);Loaded.Add("part "+id+": "+image.Width+"x"+image.Height+" "+path);return image;}
        }catch(IOException){}catch(UnauthorizedAccessException){}catch(ArgumentException){}catch(OverflowException){}
        return null;
    }
}

static class DdsImage {
    static byte[] Rgb565(int n) {return new[]{(byte)(((n>>11)&31)*255/31),(byte)(((n>>5)&63)*255/63),(byte)((n&31)*255/31),(byte)255};}
    public static Bitmap Load(string path) {
        if(Path.GetExtension(path).Equals(".png",StringComparison.OrdinalIgnoreCase)) {
            using(var input=new Bitmap(path)){if(input.Width>4096 || input.Height>4096)throw new IOException("HD image too large");var copy=new Bitmap(input.Width,input.Height,PixelFormat.Format32bppArgb);for(int y=0;y<copy.Height;y++)for(int x=0;x<copy.Width;x++){Color c=input.GetPixel(x,y);copy.SetPixel(x,y,Color.FromArgb(Math.Min(255,c.A*2),c.R,c.G,c.B));}return copy;}
        }
        byte[] b=File.ReadAllBytes(path);if(b.Length<128 || b[0]!=68 || b[1]!=68 || b[2]!=83 || b[3]!=32)throw new IOException("Invalid DDS");
        int height=BitConverter.ToInt32(b,12),width=BitConverter.ToInt32(b,16),four=BitConverter.ToInt32(b,84),flags=BitConverter.ToInt32(b,80);
        if(width<1 || height<1 || width>4096 || height>4096)throw new IOException("Invalid DDS dimensions");
        byte[] rgba=new byte[checked(width*height*4)];int at=128;
        if((flags&4)==0 && BitConverter.ToInt32(b,88)==32) {
            if(at>b.Length-rgba.Length)throw new IOException("Truncated DDS");bool redFirst=BitConverter.ToUInt32(b,92)==255;
            for(int i=0;i<rgba.Length;i+=4){rgba[i]=b[at+i+(redFirst?2:0)];rgba[i+1]=b[at+i+1];rgba[i+2]=b[at+i+(redFirst?0:2)];rgba[i+3]=(byte)Math.Min(255,b[at+i+3]*2);}
        } else {
            bool dxt1=four==0x31545844,dxt3=four==0x33545844,dxt5=four==0x35545844;
            if(!dxt1 && !dxt3 && !dxt5)throw new IOException("Unsupported DDS compression");int block=dxt1?8:16;
            for(int by=0;by<height;by+=4)for(int bx=0;bx<width;bx+=4) {
                if(at>b.Length-block)throw new IOException("Truncated DDS block");int color=at+(dxt1?0:8),c0=BitConverter.ToUInt16(b,color),c1=BitConverter.ToUInt16(b,color+2);
                byte[][] colors={Rgb565(c0),Rgb565(c1),new byte[4],new byte[4]};
                for(int channel=0;channel<3;channel++){colors[2][channel]=(byte)((!dxt1 || c0>c1)?(2*colors[0][channel]+colors[1][channel])/3:(colors[0][channel]+colors[1][channel])/2);colors[3][channel]=(byte)((!dxt1 || c0>c1)?(colors[0][channel]+2*colors[1][channel])/3:0);}
                colors[2][3]=255;colors[3][3]=(byte)((!dxt1 || c0>c1)?255:0);uint codes=BitConverter.ToUInt32(b,color+4);
                byte[] alphas=new byte[8];ulong alphaBits=0;
                if(dxt5){alphas[0]=b[at];alphas[1]=b[at+1];if(alphas[0]>alphas[1])for(int j=1;j<=6;j++)alphas[j+1]=(byte)(((7-j)*alphas[0]+j*alphas[1])/7);else {for(int j=1;j<=4;j++)alphas[j+1]=(byte)(((5-j)*alphas[0]+j*alphas[1])/5);alphas[6]=0;alphas[7]=255;}for(int j=0;j<6;j++)alphaBits|=(ulong)b[at+2+j]<<(8*j);}
                for(int y=0;y<4;y++)for(int x=0;x<4;x++) {
                    int pixel=y*4+x,xx=bx+x,yy=by+y;if(xx>=width || yy>=height)continue;byte[] c=colors[(codes>>(pixel*2))&3];
                    int alpha=dxt5?alphas[(alphaBits>>(pixel*3))&7]:dxt3?((b[at+pixel/2]>>((pixel&1)*4))&15)*17:c[3];int offset=(yy*width+xx)*4;
                    rgba[offset]=c[2];rgba[offset+1]=c[1];rgba[offset+2]=c[0];rgba[offset+3]=(byte)Math.Min(255,alpha*2);
                }
                at+=block;
            }
        }
        var bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb);var data=bitmap.LockBits(new Rectangle(0,0,width,height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
        try {System.Runtime.InteropServices.Marshal.Copy(rgba,0,data.Scan0,rgba.Length);}finally{bitmap.UnlockBits(data);}return bitmap;
    }
}

sealed class NativeMenuMetrics {
    readonly Dictionary<char,int> advance=new Dictionary<char,int>();
    public NativeMenuMetrics(string exe) {
        try {
            if(string.IsNullOrEmpty(exe))return;byte[] b=File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(exe),"gamedata","pzs3us1","00004.bin"));
            int f=BitConverter.ToInt32(b,4)&~3,table=f+BitConverter.ToInt32(b,f+20),count=BitConverter.ToInt32(b,f+24);
            if(count<1 || count>8192 || table<0 || table>b.Length-count*6)return;
            for(int i=0;i<count;i++){int p=table+i*6;char c=(char)BitConverter.ToUInt16(b,p);advance[c]=(BitConverter.ToUInt16(b,p+2)>>10)&31;}
        }catch(IOException){}catch(UnauthorizedAccessException){}catch(ArgumentException){}
    }
    int Cell(char c){int value;return advance.TryGetValue(c,out value)?value:11;}
    float Step(char c,float unit,float spacing) {
        float cell=Cell(c)*unit;
        // Tighten existing side bearings, retaining the original glyph shape.
        return char.IsWhiteSpace(c)?cell*spacing:cell-Math.Min(.75f,Math.Max(0,1-spacing)*5)*unit;
    }
    public float Measure(string text,float unit,float spacing=1){float total=0;foreach(char c in text)total+=Step(c,unit,spacing);return total;}
    public void Draw(Graphics g,string text,FontFamily family,Color color,RectangleF box,float unit,bool centered,bool centerInk=false,float spacing=1) {
        float em=13*unit;RectangleF hBounds;
        using(var test=new System.Drawing.Drawing2D.GraphicsPath()){test.AddString("H",family,0,100,PointF.Empty,StringFormat.GenericTypographic);hBounds=test.GetBounds();em=13*unit*100/hBounds.Height;}
        float width=Measure(text,unit,spacing),fit=Math.Min(1,box.Width/Math.Max(1,width));float cursor=centered?box.X+(box.Width-width*fit)/2:box.X;
        using(var test=new System.Drawing.Drawing2D.GraphicsPath()){test.AddString("H",family,0,em,PointF.Empty,StringFormat.GenericTypographic);hBounds=test.GetBounds();}
        if(centerInk && !string.IsNullOrWhiteSpace(text))using(var test=new System.Drawing.Drawing2D.GraphicsPath()){test.AddString(text,family,0,em,PointF.Empty,StringFormat.GenericTypographic);hBounds=test.GetBounds();}
        float top=box.Y+(box.Height-hBounds.Height)/2-hBounds.Y;
        foreach(char c in text) {
            float cell=Cell(c)*unit;
            if(!char.IsWhiteSpace(c))using(var path=new System.Drawing.Drawing2D.GraphicsPath()) {
                path.AddString(c.ToString(),family,0,em,PointF.Empty,StringFormat.GenericTypographic);RectangleF ink=path.GetBounds();
                if(ink.Width>0){float sx=Math.Min(fit,Math.Max(unit,cell-unit)*fit/ink.Width);
                    using(var matrix=new System.Drawing.Drawing2D.Matrix(sx,0,0,1,cursor+unit*fit-ink.X*sx,top))path.Transform(matrix);
                    using(var outline=new Pen(Color.FromArgb(32,43,94),Math.Max(1,unit*.55f))){outline.LineJoin=System.Drawing.Drawing2D.LineJoin.Round;g.DrawPath(outline,path);}using(var fill=new SolidBrush(color))g.FillPath(fill,path);
                }
            }
            cursor+=Step(c,unit,spacing)*fit;
        }
    }
    public void Wrapped(Graphics g,string text,FontFamily family,Color color,RectangleF box,float unit) {
        var lines=new List<string>();
        for(int attempt=0;attempt<5;attempt++) {
            lines.Clear();string line="";
            foreach(string word in text.Split(' ')){string next=line.Length==0?word:line+" "+word;if(line.Length>0 && Measure(next,unit)>box.Width){lines.Add(line);line=word;}else line=next;}
            if(line.Length>0)lines.Add(line);if(lines.Count<=2)break;unit*=.92f;
        }
        if(lines.Count>3){lines.RemoveRange(3,lines.Count-3);lines[2]+="…";}
        float rh=16*unit,yy=box.Y+(box.Height-lines.Count*rh)/2;
        foreach(string value in lines){Draw(g,value,family,color,new RectangleF(box.X,yy,box.Width,rh),unit,true);yy+=rh;}
    }
}
