#include <windows.h>
#include <shellapi.h>
#include <stdio.h>
#include <wchar.h>
int WINAPI WinMain(HINSTANCE a,HINSTANCE b,LPSTR command,int show){
    int argc=0;LPWSTR *argv=CommandLineToArgvW(GetCommandLineW(),&argc);if(argc!=2)return 2;
    char verbose[16]={0};GetEnvironmentVariableA("BT3_GS_VERBOSE",verbose,sizeof(verbose));if(verbose[0]!='1')return 3;
    setvbuf(stdout,NULL,_IONBF,0);setvbuf(stderr,NULL,_IONBF,0);
    LARGE_INTEGER begin,end,freq;QueryPerformanceFrequency(&freq);QueryPerformanceCounter(&begin);
    for(int i=0;i<5000;i++)fprintf(stderr,"time: renderer-capture-test %d: game 1.0 ms, texture 2.0 ms, pipeline 3.0 ms, submit 4.0 ms\n",i);
    QueryPerformanceCounter(&end);
    fprintf(stderr,"capture_write_ms=%.3f\n",(double)(end.QuadPart-begin.QuadPart)*1000/(double)freq.QuadPart);
    fprintf(stdout,"CAPTURE_STDOUT_OK\n");fprintf(stderr,"CAPTURE_STDERR_OK\n");
    wchar_t line[32768];_snwprintf(line,32768,L"\"%ls\"",argv[1]);
    STARTUPINFOW info={0};PROCESS_INFORMATION process;info.cb=sizeof(info);info.dwFlags=STARTF_USESTDHANDLES;
    info.hStdInput=GetStdHandle(STD_INPUT_HANDLE);info.hStdOutput=GetStdHandle(STD_OUTPUT_HANDLE);info.hStdError=GetStdHandle(STD_ERROR_HANDLE);
    if(!CreateProcessW(argv[1],line,NULL,NULL,TRUE,CREATE_NO_WINDOW,NULL,NULL,&info,&process))return 4;
    WaitForSingleObject(process.hProcess,INFINITE);DWORD code=0;GetExitCodeProcess(process.hProcess,&code);CloseHandle(process.hThread);CloseHandle(process.hProcess);LocalFree(argv);return (int)code;
}
