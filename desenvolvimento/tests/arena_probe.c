#include <windows.h>
#include <stdint.h>
#include <stdio.h>
static void *arena;
/* Match the game's startup reservation and fallback, before main executes. */
__attribute__((constructor))static void reserve_arena(void){
    arena=VirtualAlloc((void *)(uintptr_t)0x13000000,0x0c000000,MEM_RESERVE|MEM_WRITE_WATCH,PAGE_READWRITE);
    if(!arena){
        uintptr_t address=0x30000000;MEMORY_BASIC_INFORMATION info;
        while(address<0x7f000000&&VirtualQuery((void *)address,&info,sizeof(info))){
            uintptr_t base=((uintptr_t)info.BaseAddress+0xffff)&~(uintptr_t)0xffff,end=(uintptr_t)info.BaseAddress+info.RegionSize;
            if(info.State==MEM_FREE&&base+0x0c000000<=end&&base+0x0c000000<=0x7f000000){arena=VirtualAlloc((void *)base,0x0c000000,MEM_RESERVE|MEM_WRITE_WATCH,PAGE_READWRITE);if(arena)break;}
            address=end>address?end:address+0x10000;
        }
    }
}
int main(int argc,char **argv){
    if(argc!=2||!arena)return 2;
    FILE *file=fopen(argv[1],"wb");if(!file)return 3;
    fprintf(file,"{\"arena\":%llu,\"preferred\":%s}\n",(unsigned long long)(uintptr_t)arena,arena==(void *)(uintptr_t)0x13000000?"true":"false");fclose(file);
    VirtualFree(arena,0,MEM_RELEASE);return 0;
}
