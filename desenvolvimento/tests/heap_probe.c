#include <windows.h>
#include <psapi.h>
#include <stdio.h>
#include <stdlib.h>
static void *blocks[12000];
int main(int argc,char **argv){
    if(argc!=2)return 2;
    HANDLE heap=HeapCreate(0,0,0);if(!heap)return 3;
    ULONG before=0,after=0,lfh=2;SIZE_T size=0;
    HeapQueryInformation(heap,HeapCompatibilityInformation,&before,sizeof(before),&size);
    SetLastError(0);int enabled=HeapSetInformation(heap,HeapCompatibilityInformation,&lfh,sizeof(lfh));DWORD error=GetLastError();
    HeapQueryInformation(heap,HeapCompatibilityInformation,&after,sizeof(after),&size);
    /* The timing heap uses Windows defaults, as the game does; the LFH test
       above only checks availability on a separate, discarded heap. */
    HeapDestroy(heap);heap=HeapCreate(0,0,0);if(!heap)return 3;
    LARGE_INTEGER start,end,freq;QueryPerformanceFrequency(&freq);QueryPerformanceCounter(&start);
    for(int j=0;j<6;j++){
        for(int i=0;i<12000;i++){blocks[i]=HeapAlloc(heap,0,32+(i*17)%2048);if(!blocks[i])return 4;((volatile char *)blocks[i])[0]=(char)i;}
        for(int i=0;i<12000;i++)HeapFree(heap,0,blocks[(i*7)%12000]);
    }
    QueryPerformanceCounter(&end);
    ULONG timed_after=0;HeapQueryInformation(heap,HeapCompatibilityInformation,&timed_after,sizeof(timed_after),&size);
    char env[32]={0};GetEnvironmentVariableA("_NO_DEBUG_HEAP",env,sizeof(env));
    PROCESS_MEMORY_COUNTERS_EX memory={0};memory.cb=sizeof(memory);GetProcessMemoryInfo(GetCurrentProcess(),(PROCESS_MEMORY_COUNTERS *)&memory,sizeof(memory));
    FILE *f=fopen(argv[1],"wb");if(!f)return 5;
    fprintf(f,"{\"no_debug_heap\":\"%s\",\"lfh_enabled\":%d,\"heap_error\":%lu,\"heap_before\":%lu,\"heap_after\":%lu,\"timed_heap_after\":%lu,\"allocation_ms\":%.3f,\"private_mb\":%.3f}\n",env,enabled,(unsigned long)error,(unsigned long)before,(unsigned long)after,(unsigned long)timed_after,(double)(end.QuadPart-start.QuadPart)*1000/(double)freq.QuadPart,(double)memory.PrivateUsage/1048576);
    fclose(f);HeapDestroy(heap);return 0;
}
