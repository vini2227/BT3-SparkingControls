using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;

static class LinuxPlatform {
    public static readonly string Package=AppDomain.CurrentDomain.BaseDirectory;
    static void CopyResource(string source,string target) {
        byte[] data=File.ReadAllBytes(source);
        if(File.Exists(target))using(var sha=System.Security.Cryptography.SHA256.Create())if(Convert.ToBase64String(sha.ComputeHash(data))==Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(target))))return;
        string temp=target+"."+Guid.NewGuid().ToString("N")+".tmp";
        File.WriteAllBytes(temp,data);
        if(File.Exists(target))File.Replace(temp,target,null);else File.Move(temp,target);
    }
    public static string DataHome {get {string value=Environment.GetEnvironmentVariable("XDG_DATA_HOME");return string.IsNullOrEmpty(value)?Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".local","share"):value;}}
    public static string SettingsRoot {get {string path=Path.Combine(DataHome,"BT3-SparkingControls","linux");Directory.CreateDirectory(path);return path;}}
    public static string PreviousRoot {get{return Path.Combine(DataHome,"BT3-SparkingControls","linux-online-v0.1.37");}}
    public static string Prepare() {
        string home=Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if(string.IsNullOrEmpty(home))home=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".local","share");
        string root=Path.Combine(home,"BT3-SparkingControls","linux-runtime");Directory.CreateDirectory(root);
        foreach(string file in new[]{"profile-linux.json","libbt3sparking.so","compatibility-linux.json","compatibility-linux-legacy.json"})CopyResource(Path.Combine(Package,file),Path.Combine(root,file));
        Directory.CreateDirectory(Path.Combine(root,"assets"));
        foreach(string file in Directory.GetFiles(Path.Combine(Package,"assets")))CopyResource(file,Path.Combine(root,"assets",Path.GetFileName(file)));
        return root;
    }
}

// Xlib's unsigned long is 64 bits on this x86-64-only port.
// Input shape is empty; the overlay never receives controller/keyboard/mouse events.
static class LinuxDesktop {
    const string X="libX11.so.6";
    [StructLayout(LayoutKind.Sequential)] struct VisualInfo {
        public IntPtr visual;public ulong visualid;public int screen,depth,klass;
        public ulong red,green,blue;public int colormapSize,bits;
    }
    [StructLayout(LayoutKind.Sequential)] struct Attributes {
        public ulong backgroundPixmap,backgroundPixel,borderPixmap,borderPixel;
        public int bitGravity,winGravity,backingStore;public ulong backingPlanes,backingPixel;
        public int saveUnder;public long eventMask,doNotPropagateMask;public int overrideRedirect;
        public ulong colormap,cursor;
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int ErrorHandler(IntPtr display,IntPtr error);
    static readonly ErrorHandler IgnoreXError=(display,error)=>0;
    [DllImport(X)] static extern IntPtr XSetErrorHandler(ErrorHandler handler);
    [DllImport(X)] static extern IntPtr XOpenDisplay(string name);
    [DllImport(X)] static extern int XDefaultScreen(IntPtr display);
    [DllImport(X)] static extern ulong XRootWindow(IntPtr display,int screen);
    [DllImport(X)] static extern ulong XInternAtom(IntPtr display,string name,int onlyIfExists);
    [DllImport(X)] static extern int XGetWindowProperty(IntPtr display,ulong window,ulong property,long offset,long length,int delete,ulong requested,out ulong actual,out int format,out ulong count,out ulong remaining,out IntPtr data);
    [DllImport(X)] static extern int XFree(IntPtr data);
    [DllImport(X)] static extern int XQueryTree(IntPtr display,ulong window,out ulong root,out ulong parent,out IntPtr children,out uint count);
    [DllImport(X)] static extern int XGetGeometry(IntPtr display,ulong window,out ulong root,out int x,out int y,out uint width,out uint height,out uint border,out uint depth);
    [DllImport(X)] static extern int XTranslateCoordinates(IntPtr display,ulong source,ulong dest,int x,int y,out int dx,out int dy,out ulong child);
    [DllImport(X)] static extern int XMatchVisualInfo(IntPtr display,int screen,int depth,int klass,out VisualInfo visual);
    [DllImport(X)] static extern ulong XCreateColormap(IntPtr display,ulong root,IntPtr visual,int allocation);
    [DllImport(X)] static extern ulong XCreateWindow(IntPtr display,ulong parent,int x,int y,uint width,uint height,uint border,int depth,uint klass,IntPtr visual,ulong mask,ref Attributes attributes);
    [DllImport(X)] static extern int XStoreName(IntPtr display,ulong window,string name);
    [DllImport(X)] static extern IntPtr XCreateGC(IntPtr display,ulong window,ulong mask,IntPtr values);
    [DllImport(X)] static extern int XMoveResizeWindow(IntPtr display,ulong window,int x,int y,uint width,uint height);
    [DllImport(X)] static extern int XMapRaised(IntPtr display,ulong window);
    [DllImport(X)] static extern int XRaiseWindow(IntPtr display,ulong window);
    [DllImport(X)] static extern int XFlush(IntPtr display);
    [DllImport(X)] static extern int XSync(IntPtr display,int discard);
    [DllImport(X)] static extern IntPtr XCreateImage(IntPtr display,IntPtr visual,uint depth,int format,int offset,IntPtr data,uint width,uint height,int pad,int stride);
    [DllImport(X)] static extern int XPutImage(IntPtr display,ulong window,IntPtr gc,IntPtr image,int sx,int sy,int dx,int dy,uint width,uint height);
    [DllImport(X)] static extern int XDestroyImage(IntPtr image);
    [DllImport(X)] static extern int XFreeGC(IntPtr display,IntPtr gc);
    [DllImport(X)] static extern int XDestroyWindow(IntPtr display,ulong window);
    [DllImport(X)] static extern int XFreeColormap(IntPtr display,ulong colormap);
    [DllImport(X)] static extern int XCloseDisplay(IntPtr display);
    [DllImport("libXext.so.6")] static extern void XShapeCombineRectangles(IntPtr display,ulong window,int kind,int x,int y,IntPtr rectangles,int count,int operation,int ordering);
    [DllImport("libc.so.6")] static extern IntPtr malloc(UIntPtr size);
    [DllImport("libc.so.6")] static extern void free(IntPtr pointer);
    static IntPtr display,gc;static ulong root,window,colormap,gameWindow;static int screen,gamePid;static VisualInfo visual;
    [DllImport(X)] static extern int XQueryKeymap(IntPtr display,byte[] keys);
    [DllImport(X)] static extern byte XKeysymToKeycode(IntPtr display,ulong symbol);
    static readonly byte[] keyboard=new byte[32];
    static ulong Keysym(int key){
        if(key>=0x70&&key<=0x87)return (ulong)(0xffbe+key-0x70);
        if(key>=0x41&&key<=0x5a)return (ulong)(key+32);
        if(key>=0x30&&key<=0x39)return (ulong)key;
        if(key>=0x60&&key<=0x69)return (ulong)(0xffb0+key-0x60);
        switch(key){case 8:return 0xff08;case 9:return 0xff09;case 13:return 0xff0d;case 16:return 0xffe1;case 17:return 0xffe3;case 18:return 0xffe9;case 19:return 0xff13;case 20:return 0xffe5;case 27:return 0xff1b;case 32:return 32;case 33:return 0xff55;case 34:return 0xff56;case 35:return 0xff57;case 36:return 0xff50;case 37:return 0xff51;case 38:return 0xff52;case 39:return 0xff53;case 40:return 0xff54;case 45:return 0xff63;case 46:return 0xffff;case 106:return 0xffaa;case 107:return 0xffab;case 109:return 0xffad;case 110:return 0xffae;case 111:return 0xffaf;case 144:return 0xff7f;case 145:return 0xff14;case 186:return 59;case 187:return 61;case 188:return 44;case 189:return 45;case 190:return 46;case 191:return 47;case 192:return 96;case 219:return 91;case 220:return 92;case 221:return 93;case 222:return 39;default:return 0;}
    }
    public static bool IsSupportedKey(int key){return Keysym(key)!=0;}
    public static void RefreshKeys(){Open();XQueryKeymap(display,keyboard);}
    static bool SymbolDown(ulong symbol){byte code=XKeysymToKeycode(display,symbol);return code!=0&&(keyboard[code>>3]&(1<<(code&7)))!=0;}
    public static bool KeyDown(int key){ulong symbol=Keysym(key);if(symbol==0||display==IntPtr.Zero)return false;return SymbolDown(symbol)||((key==16||key==17||key==18)&&SymbolDown(symbol+1));}
    static void Open() {
        if(display!=IntPtr.Zero)return;
        display=XOpenDisplay(null);if(display==IntPtr.Zero)throw new IOException("An X11 display is required (X11 session or working XWayland). Native Wayland overlays are not implemented.");
        XSetErrorHandler(IgnoreXError);screen=XDefaultScreen(display);root=XRootWindow(display,screen);
    }
    static ulong[] Property(ulong target,string name) {
        IntPtr data;ulong type,count,remaining;int format;
        if(XGetWindowProperty(display,target,XInternAtom(display,name,0),0,16384,0,0,out type,out format,out count,out remaining,out data)!=0)return new ulong[0];
        try{if(format!=32 || count>16384)return new ulong[0];var values=new ulong[(int)count];for(int i=0;i<values.Length;i++)values[i]=unchecked((ulong)Marshal.ReadInt64(data,i*8));return values;}
        finally{if(data!=IntPtr.Zero)XFree(data);}
    }
    static ulong Find(ulong parent,int pid,int depth) {
        foreach(ulong id in Property(parent,"_NET_CLIENT_LIST")){var value=Property(id,"_NET_WM_PID");if(value.Length>0 && value[0]==(ulong)pid)return id;}
        if(depth==0)return 0;
        ulong rr,pp;IntPtr children;uint count;
        if(XQueryTree(display,parent,out rr,out pp,out children,out count)==0)return 0;
        try{for(int i=0;i<count && i<4096;i++){ulong id=unchecked((ulong)Marshal.ReadInt64(children,i*8));var value=Property(id,"_NET_WM_PID");if(value.Length>0 && value[0]==(ulong)pid)return id;ulong nested=Find(id,pid,depth-1);if(nested!=0)return nested;}}
        finally{if(children!=IntPtr.Zero)XFree(children);}return 0;
    }
    public static bool GameBounds(int pid,out Rectangle bounds) {
        Open();bounds=Rectangle.Empty;
        if(gamePid!=pid || gameWindow==0){gamePid=pid;gameWindow=Find(root,pid,3);}
        if(gameWindow==0)return false;
        var active=Property(root,"_NET_ACTIVE_WINDOW");if(active.Length>0 && active[0]!=gameWindow)return false;
        ulong rr,child;int x,y,dx,dy;uint width,height,border,depth;
        if(XGetGeometry(display,gameWindow,out rr,out x,out y,out width,out height,out border,out depth)==0){gameWindow=0;return false;}
        if(width<1 || height<1 || width>16384 || height>16384)return false;
        if(XTranslateCoordinates(display,gameWindow,root,0,0,out dx,out dy,out child)==0)return false;
        bounds=new Rectangle(dx,dy,(int)width,(int)height);return true;
    }
    static void Create(Rectangle bounds) {
        Open();if(window!=0)return;
        if(XMatchVisualInfo(display,screen,32,4,out visual)==0)throw new IOException("X11 32-bit ARGB visual unavailable.");
        colormap=XCreateColormap(display,root,visual.visual,0);
        var attrs=new Attributes {backgroundPixel=0,borderPixel=0,overrideRedirect=1,colormap=colormap};
        window=XCreateWindow(display,root,bounds.X,bounds.Y,(uint)bounds.Width,(uint)bounds.Height,0,32,1,visual.visual,2|8|512|8192,ref attrs);
        if(window==0)throw new IOException("Cannot create the X11 overlay.");
        XStoreName(display,window,"BT3 Sparking Controls HUD");gc=XCreateGC(display,window,0,IntPtr.Zero);
        XShapeCombineRectangles(display,window,2,0,0,IntPtr.Zero,0,0,0);
    }
    [DllImport(X)] static extern int XUnmapWindow(IntPtr display,ulong window);
    static IntPtr cachedImage,cachedPixels;static Size imageSize;static byte[] imageRow;static Rectangle shownBounds;static bool mapped;
    static void ReleaseImage(){if(cachedImage!=IntPtr.Zero)XDestroyImage(cachedImage);else if(cachedPixels!=IntPtr.Zero)free(cachedPixels);cachedImage=cachedPixels=IntPtr.Zero;imageSize=Size.Empty;imageRow=null;}
    public static void Hide(){if(display!=IntPtr.Zero&&window!=0&&mapped){XUnmapWindow(display,window);XFlush(display);mapped=false;}}
    public static void Present(Rectangle bounds,Bitmap bitmap) {
        Create(bounds);
        if(shownBounds!=bounds){XMoveResizeWindow(display,window,bounds.X,bounds.Y,(uint)bitmap.Width,(uint)bitmap.Height);shownBounds=bounds;}
        if(cachedImage==IntPtr.Zero||imageSize!=bitmap.Size){
            ReleaseImage();int stride=checked(bitmap.Width*4);cachedPixels=malloc((UIntPtr)(uint)checked(stride*bitmap.Height));if(cachedPixels==IntPtr.Zero)throw new OutOfMemoryException();
            cachedImage=XCreateImage(display,visual.visual,32,2,0,cachedPixels,(uint)bitmap.Width,(uint)bitmap.Height,32,stride);
            if(cachedImage==IntPtr.Zero){ReleaseImage();throw new IOException("Cannot create X11 image.");}
            imageSize=bitmap.Size;imageRow=new byte[stride];
        }
        var locked=bitmap.LockBits(new Rectangle(Point.Empty,bitmap.Size),ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb);
        try{for(int y=0;y<bitmap.Height;y++){Marshal.Copy(IntPtr.Add(locked.Scan0,y*locked.Stride),imageRow,0,imageRow.Length);Marshal.Copy(imageRow,0,IntPtr.Add(cachedPixels,y*imageRow.Length),imageRow.Length);}}
        finally{bitmap.UnlockBits(locked);}
        XPutImage(display,window,gc,cachedImage,0,0,0,0,(uint)bitmap.Width,(uint)bitmap.Height);
        if(!mapped){XMapRaised(display,window);mapped=true;}XFlush(display);
    }
    public static void Close() {
        if(display==IntPtr.Zero)return;
        ReleaseImage();mapped=false;shownBounds=Rectangle.Empty;
        if(gc!=IntPtr.Zero)XFreeGC(display,gc);if(window!=0)XDestroyWindow(display,window);if(colormap!=0)XFreeColormap(display,colormap);
        XSync(display,0);XCloseDisplay(display);display=gc=IntPtr.Zero;root=window=colormap=gameWindow=0;gamePid=0;
    }
}
