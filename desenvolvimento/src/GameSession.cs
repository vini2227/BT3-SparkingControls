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
    public sealed class Snapshot {
        public int Pid;public IntPtr Handle;public Profile Profile;public int Generation;
    }
    readonly object gate=new object();readonly ManualResetEvent ready=new ManualResetEvent(false);
    readonly string gamePath,startPath;readonly bool probe;
    readonly List<Snapshot> retained=new List<Snapshot>();
    volatile bool stopping,done;Snapshot latest;Exception failure;Thread worker;
    public bool Done {get{return done;}}
    public Exception Failure {get{lock(gate)return failure;}}
    public Snapshot Latest {get{lock(gate)return latest;}}
    public GameSession(string exe,string probeHost=null){gamePath=Path.GetFullPath(exe);startPath=probeHost??gamePath;probe=probeHost!=null;}
    public void Start(){worker=new Thread(Run);worker.IsBackground=true;worker.Name="BT3 game restart session";worker.Start();ready.WaitOne();if(Failure!=null)throw new IOException("Could not prepare game session.",Failure);}
    void Run(){
        var live=new HashSet<uint>();Native.PROCESS_INFORMATION first=new Native.PROCESS_INFORMATION();
        try {
            Native.STARTUPINFO si=new Native.STARTUPINFO();si.cb=Marshal.SizeOf(si);
            string command="\""+startPath+"\""+(probe?" \""+gamePath+"\"":"");
            // DEBUG_PROCESS propagates to relaunches made by the game itself.
            if(!Native.CreateProcess(startPath,new StringBuilder(command),IntPtr.Zero,IntPtr.Zero,false,1,IntPtr.Zero,Path.GetDirectoryName(gamePath),ref si,out first))throw new IOException("CreateProcess: "+Marshal.GetLastWin32Error());
            DebugSetProcessKillOnExit(false);
            int generation=0;
            while(!stopping){
                DebugEvent e;
                if(!WaitForDebugEvent(out e,250)){
                    int error=Marshal.GetLastWin32Error();if(error==121)continue;
                    throw new IOException("WaitForDebugEvent: "+error);
                }
                uint status=0x00010002;
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
                            Program.SessionLog("Prepared game generation "+generation+"; PID "+e.Pid+"; base 0x"+e.ImageBase.ToInt64().ToString("X")+"; packet hook 0x"+p.allocation.ToString("X")+"; before first instruction.");
                            if(probe){
                                File.WriteAllText(Path.Combine(Program.Root,"restart-probe-results.txt"),"PASS: descendant game creation was intercepted and its local packet hook was applied and read back before the first instruction. PID "+e.Pid+"; allocation 0x"+p.allocation.ToString("X")+".\r\nThe game child was terminated at its creation event; no game code executed. This is not an online gameplay test.\r\n");
                                Native.TerminateProcess(e.Process,0);
                            }
                            ready.Set();
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
                }finally{if(!ContinueDebugEvent(e.Pid,e.Tid,status))throw new IOException("ContinueDebugEvent: "+Marshal.GetLastWin32Error());}
                if(live.Count==0)break;
            }
        }catch(Exception ex){lock(gate)failure=ex;try{Program.SessionLog(ex.ToString());}catch{}}
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
