using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// Windows holds newly created descendants before their first instruction.
// Only matching game images receive the same local packet hook; no battle hook is added.
sealed class GameSession : IDisposable {
    [StructLayout(LayoutKind.Explicit,Size=176)] struct DebugEvent {
        [FieldOffset(0)] public uint Code; [FieldOffset(4)] public uint Pid; [FieldOffset(8)] public uint Tid;
        [FieldOffset(16)] public IntPtr File; [FieldOffset(24)] public IntPtr Process;
        [FieldOffset(32)] public IntPtr Thread; [FieldOffset(40)] public IntPtr ImageBase;
        [FieldOffset(16)] public uint ExceptionCode;
    }
    [DllImport("kernel32.dll",SetLastError=true)] static extern bool WaitForDebugEvent(out DebugEvent e,uint ms);
    [DllImport("kernel32.dll",SetLastError=true)] static extern bool ContinueDebugEvent(uint pid,uint tid,uint status);
    [DllImport("kernel32.dll")] static extern bool DebugSetProcessKillOnExit(bool kill);
    [DllImport("kernel32.dll",SetLastError=true)] static extern bool DebugActiveProcessStop(uint pid);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool QueryFullProcessImageName(IntPtr process,int flags,StringBuilder name,ref int size);
    [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr OpenProcess(uint access,bool inherit,uint pid);
    [DllImport("kernel32.dll",SetLastError=true)] static extern uint SuspendThread(IntPtr thread);
    [DllImport("kernel32.dll",SetLastError=true)] static extern bool GetExitCodeProcess(IntPtr process,out uint code);
    [DllImport("kernel32.dll",SetLastError=true)] static extern bool CheckRemoteDebuggerPresent(IntPtr process,[MarshalAs(UnmanagedType.Bool)]out bool present);
    [DllImport("kernel32.dll",EntryPoint="GetEnvironmentStringsW",SetLastError=true)] static extern IntPtr GetEnvironmentStrings();
    [DllImport("kernel32.dll",EntryPoint="FreeEnvironmentStringsW")] static extern bool FreeEnvironmentStrings(IntPtr block);
    [StructLayout(LayoutKind.Sequential)]struct MemoryStatus {public uint Length,Load;public ulong TotalPhysical,AvailablePhysical,TotalPage,AvailablePage,TotalVirtual,AvailableVirtual,AvailableExtended;}
    [DllImport("kernel32.dll")]static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
    [StructLayout(LayoutKind.Sequential)]struct SecurityAttributes {public int Length;public IntPtr Descriptor;[MarshalAs(UnmanagedType.Bool)]public bool Inherit;}
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr CreateFile(string path,uint access,uint share,ref SecurityAttributes attributes,uint creation,uint flags,IntPtr template);
    public static string SystemMemoryPerformance {get{var status=new MemoryStatus();status.Length=(uint)Marshal.SizeOf(status);return GlobalMemoryStatusEx(ref status)?" system_memory_load="+status.Load+" system_available_mb="+(status.AvailablePhysical/1048576)+" system_commit_available_mb="+(status.AvailablePage/1048576):" system_memory=unavailable";}}
    public sealed class Snapshot {
        public int Pid;public IntPtr Handle;public Profile Profile;public int Generation;
    }
    readonly object gate=new object();readonly ManualResetEvent ready=new ManualResetEvent(false);
    readonly string gamePath,startPath;readonly bool probe,diagnostics,quietDiagnostics,offlineDetach,holdDetachProbe;
    public bool Detached {get;private set;}
    public bool DetachedThreadHoldVerified {get;private set;}
    public string RendererLogPath {get;private set;}
    readonly List<Snapshot> retained=new List<Snapshot>();
    volatile bool stopping,done;Snapshot latest;Exception failure;Thread worker;
    long debugEvents,debugExceptions,debugStrings,debugLoads,debugThreads,debugTicks,debugMaxTicks;
    // Performance counters add no log writes while Windows holds a debug event.
    public string Performance {get{return " debug_events="+Interlocked.Read(ref debugEvents)+" debug_exceptions="+Interlocked.Read(ref debugExceptions)+" debug_strings="+Interlocked.Read(ref debugStrings)+" debug_loads="+Interlocked.Read(ref debugLoads)+" debug_threads="+Interlocked.Read(ref debugThreads)+" debug_hold_ms="+(Interlocked.Read(ref debugTicks)*1000.0/Stopwatch.Frequency).ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+" debug_max_ms="+(Interlocked.Read(ref debugMaxTicks)*1000.0/Stopwatch.Frequency).ToString("F2",System.Globalization.CultureInfo.InvariantCulture);}}
    public bool Done {get{return done;}}
    public Exception Failure {get{lock(gate)return failure;}}
    public Snapshot Latest {get{lock(gate)return latest;}}
    internal static string ChildEnvironment(bool diagnostics=false,bool quiet=false){
        var values=new SortedDictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        // Preserve Win32's hidden per-drive current-directory entries (=C:, etc.)
        // as well as regular variables and their original Unicode values.
        IntPtr inherited=GetEnvironmentStrings();if(inherited==IntPtr.Zero)throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        try{
            IntPtr cursor=inherited;
            while(Marshal.ReadInt16(cursor)!=0){string line=Marshal.PtrToStringUni(cursor);int separator=line.IndexOf('=',1);if(separator>0)values[line.Substring(0,separator)]=line.Substring(separator+1);cursor=IntPtr.Add(cursor,checked((line.Length+1)*2));}
        }finally{FreeEnvironmentStrings(inherited);}
        // DEBUG_PROCESS otherwise enables the Windows diagnostic heap, which
        // disables LFH and makes allocation-heavy texture packs drastically slower.
        // Change only the child's environment; descendants inherit this policy.
        values["_NO_DEBUG_HEAP"]="1";
        if(quiet)values.Remove("BT3_GS_VERBOSE");
        if(diagnostics)values["BT3_GS_VERBOSE"]="1";
        var block=new StringBuilder();foreach(var entry in values)block.Append(entry.Key).Append('=').Append(entry.Value).Append('\0');
        block.Append('\0');return block.ToString();
    }
    public GameSession(string exe,string probeHost=null,bool captureDiagnostics=false,bool suppressDiagnostics=false,bool detachAfterHook=false,bool detachedProbe=false){
        if(detachedProbe&&!detachAfterHook)throw new ArgumentException("Detached probe requires offline detach mode.");
        if(detachAfterHook&&probeHost!=null)throw new ArgumentException("Offline detach test requires a direct game launch.");
        gamePath=Path.GetFullPath(exe);startPath=probeHost??gamePath;probe=probeHost!=null;diagnostics=captureDiagnostics;quietDiagnostics=suppressDiagnostics;offlineDetach=detachAfterHook;holdDetachProbe=detachedProbe;
    }
    public void Start(){worker=new Thread(Run);worker.IsBackground=true;worker.Name="BT3 game restart session";worker.Start();ready.WaitOne();if(Failure!=null)throw new IOException("Could not prepare game session.",Failure);}
    void Run(){
        var live=new HashSet<uint>();Native.PROCESS_INFORMATION first=new Native.PROCESS_INFORMATION();bool heldForDetach=false;
        try {
            Native.STARTUPINFO si=new Native.STARTUPINFO();si.cb=Marshal.SizeOf(si);
            string command="\""+startPath+"\""+(probe?" \""+gamePath+"\"":"");
            // Keep pre-first-instruction restart tracking, without opting the game
            // into the Windows diagnostic allocator. No heap flags are patched.
            IntPtr environment=Marshal.StringToHGlobalUni(ChildEnvironment(diagnostics,quietDiagnostics)),output=IntPtr.Zero,input=IntPtr.Zero;bool created;int createError;
            try{
                if(diagnostics){
                    string folder=Path.Combine(Program.Root,"renderer",DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));Directory.CreateDirectory(folder);RendererLogPath=Path.Combine(folder,"game-renderer.log");
                    var security=new SecurityAttributes{Length=Marshal.SizeOf(typeof(SecurityAttributes)),Inherit=true};
                    output=CreateFile(RendererLogPath,0x40000000,7,ref security,2,0x80,IntPtr.Zero);
                    if(output==new IntPtr(-1))throw new IOException("Could not create renderer log: "+Marshal.GetLastWin32Error());
                    input=CreateFile("NUL",0x80000000,7,ref security,3,0x80,IntPtr.Zero);
                    if(input==new IntPtr(-1))throw new IOException("Could not prepare diagnostic standard input: "+Marshal.GetLastWin32Error());
                    si.flags|=0x100;si.output=si.error=output;si.input=input;
                    Program.SessionLog("Renderer diagnostics enabled: "+RendererLogPath+"; existing native timing counters; normal launch leaves this disabled.");
                }
                created=Native.CreateProcess(startPath,new StringBuilder(command),IntPtr.Zero,IntPtr.Zero,diagnostics,0x401,environment,Path.GetDirectoryName(gamePath),ref si,out first);
                createError=created?0:Marshal.GetLastWin32Error();
            }finally{Marshal.FreeHGlobal(environment);if(output!=IntPtr.Zero&&output!=new IntPtr(-1))Native.CloseHandle(output);if(input!=IntPtr.Zero&&input!=new IntPtr(-1))Native.CloseHandle(input);}
            if(!created)throw new IOException("CreateProcess: "+createError);
            DebugSetProcessKillOnExit(false);
            int generation=0;
            while(!stopping){
                DebugEvent e;
                if(!WaitForDebugEvent(out e,250)){
                    int error=Marshal.GetLastWin32Error();if(error==121)continue;
                    throw new IOException("WaitForDebugEvent: "+error);
                }
                uint status=0x00010002;uint detachPid=0;
                long eventStarted=Stopwatch.GetTimestamp();Interlocked.Increment(ref debugEvents);
                if(e.Code==1)Interlocked.Increment(ref debugExceptions);else if(e.Code==8)Interlocked.Increment(ref debugStrings);else if(e.Code==6)Interlocked.Increment(ref debugLoads);else if(e.Code==2)Interlocked.Increment(ref debugThreads);
                try {
                    if(e.Code==3){
                        live.Add(e.Pid);
                        int length=32768;var path=new StringBuilder(length);
                        if(!QueryFullProcessImageName(e.Process,0,path,ref length))throw new IOException("Could not identify the new process.");
                        bool matching=string.Equals(Path.GetFullPath(path.ToString()),gamePath,StringComparison.OrdinalIgnoreCase);
                        if(matching){
                            Profile p=Program.PatchBeforeFirstFrame(e.Process,e.ImageBase.ToInt64(),gamePath);
                            IntPtr readHandle=OpenProcess(0x410,false,e.Pid);if(readHandle==IntPtr.Zero)throw new IOException("Could not retain the game memory reader.");
                            var snapshot=new Snapshot {Pid=(int)e.Pid,Handle=readHandle,Profile=p,Generation=++generation};
                            lock(gate){latest=snapshot;retained.Add(snapshot);}
                            Program.SessionLog("Prepared game generation "+generation+"; PID "+e.Pid+"; base 0x"+e.ImageBase.ToInt64().ToString("X")+"; packet hook 0x"+p.allocation.ToString("X")+"; before first instruction; game_sha256="+p.game_sha256+".");
                            if(probe){
                                File.WriteAllText(Path.Combine(Program.Root,"restart-probe-results.txt"),"PASS: descendant game creation was intercepted and its local packet hook was applied and read back before the first instruction. PID "+e.Pid+"; allocation 0x"+p.allocation.ToString("X")+".\r\nThe game child was terminated at its creation event; no game code executed. This is not an online gameplay test.\r\n");
                                Native.TerminateProcess(e.Process,0);
                            }
                            if(offlineDetach){
                                if(e.Pid!=first.pid||live.Count!=1)throw new IOException("Offline detach test requires the initial game process.");
                                if(SuspendThread(first.thread)==0xffffffff)throw new IOException("Could not hold offline handoff before the first instruction.");
                                heldForDetach=true;detachPid=e.Pid;
                            }else ready.Set();
                        }
                        if(e.File!=IntPtr.Zero)Native.CloseHandle(e.File);
                    }else if(e.Code==6){if(e.File!=IntPtr.Zero)Native.CloseHandle(e.File);}
                    else if(e.Code==5){live.Remove(e.Pid);Program.SessionLog("Game tree process exited; PID "+e.Pid);}
                    else if(e.Code==1){
                        // Pass all real application exceptions to the game/OS unchanged.
                        if(e.ExceptionCode!=0x80000003 && e.ExceptionCode!=0x80000004)status=0x80010001;
                    }
                }catch{
                    if(e.Code==3)Native.TerminateProcess(e.Process,1);
                    throw;
                }finally{
                    bool continued=ContinueDebugEvent(e.Pid,e.Tid,status);int continueError=continued?0:Marshal.GetLastWin32Error();
                    long ticks=Stopwatch.GetTimestamp()-eventStarted;Interlocked.Add(ref debugTicks,ticks);if(ticks>Interlocked.Read(ref debugMaxTicks))Interlocked.Exchange(ref debugMaxTicks,ticks);
                    if(!continued)throw new IOException("ContinueDebugEvent: "+continueError);
                }
                if(detachPid!=0){
                    if(!DebugActiveProcessStop(detachPid))throw new IOException("Could not detach offline test: "+Marshal.GetLastWin32Error());
                    live.Remove(detachPid);bool attached;
                    if(!CheckRemoteDebuggerPresent(first.process,out attached)||attached)throw new IOException("Offline detach verification failed.");
                    uint heldCount=SuspendThread(first.thread);
                    if(heldCount==0xffffffff)throw new IOException("Could not verify detached thread hold.");
                    uint balancedCount=Native.ResumeThread(first.thread);
                    if(heldCount!=1||balancedCount!=2)throw new IOException("Detached thread suspend count was not preserved.");
                    DetachedThreadHoldVerified=true;Detached=true;
                    if(holdDetachProbe&&!Native.TerminateProcess(first.process,0))throw new IOException("Could not terminate isolated detach probe.");
                    if(!holdDetachProbe&&Native.ResumeThread(first.thread)!=1){Native.TerminateProcess(first.process,1);throw new IOException("Could not resume detached offline test.");}
                    heldForDetach=false;ready.Set();
                    while(!stopping){uint code;if(!GetExitCodeProcess(first.process,out code))throw new IOException("Could not observe detached offline process.");if(code!=259)break;Thread.Sleep(250);}
                    break;
                }
                if(live.Count==0)break;
            }
        }catch(Exception ex){if((holdDetachProbe||heldForDetach)&&first.process!=IntPtr.Zero)Native.TerminateProcess(first.process,1);lock(gate)failure=ex;try{Program.SessionLog(ex.ToString());}catch{}}
        finally{
            foreach(uint pid in live)DebugActiveProcessStop(pid);
            if(first.thread!=IntPtr.Zero)Native.CloseHandle(first.thread);
            if(first.process!=IntPtr.Zero)Native.CloseHandle(first.process);
            done=true;ready.Set();
        }
    }
    public void Dispose(){
        stopping=true;if(worker!=null&&!worker.Join(3000))throw new IOException("Game session worker did not finish.");
        lock(gate)foreach(var item in retained)if(item.Handle!=IntPtr.Zero)Native.CloseHandle(item.Handle);
        ready.Dispose();
    }
}
