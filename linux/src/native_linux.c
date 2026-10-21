#define _GNU_SOURCE
#include <stdint.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <sys/mman.h>
#include <unistd.h>
#include "online_addresses.h"
extern int PadFilter(int socket,unsigned char *data);
static const unsigned char expected[]={243,15,30,250,65,87};
uintptr_t bt3_symbols[30]={0x20110f80UL,0x20110ff0UL,0x200cac40UL,0x200caf80UL,0x200f5260UL,0x200f5030UL,0x202bc980UL,0x202ed5e0UL,0x202ebaf0UL,0x201cdad0UL,0x200f1a50UL,0x200f1a90UL,0x20110e40UL,0x20110e60UL,0x200ca930UL,0x200e9e40UL,0x20114580UL,0x22af4ff8UL,0x2063a2d4UL,0x22adf870UL,0x20640500UL,0x2063a2f8UL,0x20640518UL,0x20640358UL,0x20cea528UL,0x20100b00UL,0x20100f20UL,0x20100e10UL,0x20102a10UL,0x20100eb0UL};
static void status(const char *message){const char *path=getenv("BT3_ADDON_STATUS");if(path){FILE *f=fopen(path,"w");if(f){fputs(message,f);fclose(f);}}}
static void fail(const char *message){status(message);fprintf(stderr,"BT3 Sparking Controls: %s\n",message);_exit(126);}
__attribute__((constructor)) static void install(void){
 if(!getenv("BT3_ADDON_GAME"))return;
 const char *resolved=getenv("BT3_ADDON_SYMBOLS");
 if(resolved){for(int i=0;i<30;i++){uintptr_t value=0;int digits=0;while(*resolved&&*resolved!=','){unsigned char ch=(unsigned char)*resolved++;unsigned n=ch>='0'&&ch<='9'?ch-'0':ch>='a'&&ch<='f'?ch-'a'+10:16;if(n>15||++digits>8)fail("ERROR: invalid resolved symbols");value=(value<<4)|n;}if(!digits||value<0x20000000UL||value>=0x30000000UL)fail("ERROR: symbol out of range");bt3_symbols[i]=value;if(i<29){if(*resolved++!=',')fail("ERROR: incomplete symbols");}else if(*resolved)fail("ERROR: extra symbols");}}
 if(memcmp((void *)ADDR_Port_PadRead,expected,sizeof(expected)))fail("ERROR: unsupported Port_PadRead signature");
 unsigned char *region=mmap((void *)0x08000000UL,0x10000,PROT_READ|PROT_WRITE,MAP_PRIVATE|MAP_ANONYMOUS|MAP_FIXED_NOREPLACE,-1,0);
 if(region!=(void *)0x08000000UL)fail("ERROR: private low-memory allocation unavailable");
 memcpy(region+0xe000,expected,sizeof(expected));region[0xe006]=0xe9;int32_t tail=(int32_t)(ADDR_Port_PadRead+sizeof(expected)-(0x0800e000UL+sizeof(expected)+5));memcpy(region+0xe007,&tail,4);
 region[0xe100]=0x48;region[0xe101]=0xb8;uintptr_t filter=(uintptr_t)&PadFilter;memcpy(region+0xe102,&filter,8);region[0xe10a]=0xff;region[0xe10b]=0xe0;
 if(mprotect(region,0xf000,PROT_READ|PROT_EXEC))fail("ERROR: cannot protect private code");
 uintptr_t start=ADDR_Port_PadRead&~4095UL,end=(ADDR_Port_PadRead+sizeof(expected)+4095UL)&~4095UL;
 if(mprotect((void *)start,end-start,PROT_READ|PROT_WRITE|PROT_EXEC))fail("ERROR: cannot prepare local pad hook");
 unsigned char *p=(void *)ADDR_Port_PadRead;int32_t jump=(int32_t)(0x0800e100UL-(ADDR_Port_PadRead+5));p[0]=0xe9;memcpy(p+1,&jump,4);memset(p+5,0x90,sizeof(expected)-5);
 if(mprotect((void *)start,end-start,PROT_READ|PROT_EXEC))fail("ERROR: cannot restore game text protection");
 __builtin___clear_cache((char *)start,(char *)end);status("READY");
}
