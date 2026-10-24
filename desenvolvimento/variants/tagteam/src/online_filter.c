/* SPDX-License-Identifier: AGPL-3.0-or-later
   Only Port_PadRead is hooked. This translates the local 18-byte pad packet BEFORE net.c sample_local stores it.
   Battle decision/command builders, remote input and rollback re-simulation execute the original game code. */
typedef unsigned char u8;typedef unsigned short u16;typedef unsigned int u32;
/* The intermediate freestanding PE is never loaded as a DLL. No TLS is used by this code. */
unsigned long _tls_index;
#include "online_addresses.h"
#undef MAIL
#define tagBank (*(volatile unsigned *)(unsigned long long)0x1400ff00u)
#define MAIL (0x1400f000ull+tagBank)
#define CALL(name,ret,args) ((ret (*)args)(unsigned long long)ADDR_##name)
#define U32(p,o) (*(volatile u32 *)((u8 *)(p)+(o)))
#define I32(p,o) (*(volatile int *)((u8 *)(p)+(o)))
#define U8(p,o) (*(volatile u8 *)((u8 *)(p)+(o)))
static __attribute__((always_inline)) inline void neutral_stick(u8 *p){p[4]=128;p[5]=128;}
static __attribute__((always_inline)) inline void ui_hidden(void){U32((void *)MAIL,0x10c)=0;U32((void *)MAIL,0x18c)=0;for(int i=0;i<32;i++)U32((void *)MAIL,0x600+i*4)=0;for(int i=0;i<16;i++)U32((void *)MAIL,0x700+i*4)=0;for(int i=0;i<16;i++)U32((void *)MAIL,0x780+i*4)=0;for(int i=0;i<16;i++)U32((void *)MAIL,0x7c0+i*4)=0;for(int i=0;i<16;i++)U32((void *)MAIL,0x800+i*4)=0;for(int i=0;i<32;i++)U32((void *)MAIL,0x840+i*4)=0;for(int i=0;i<16;i++)U32((void *)MAIL,0x8c0+i*4)=0;}
static __attribute__((always_inline)) inline unsigned directions(u16 raw,const u8 *data){
    unsigned v=raw&0xf0;
    if(data[4]<78)v|=0x80;else if(data[4]>178)v|=0x20;
    if(data[5]<78)v|=0x10;else if(data[5]>178)v|=0x40;
    return v;
}
/* Fill display-only mailboxes. No data from these mailboxes is used to decide the game's actions. */
static void palette(void *chr,int side,int open,int fusion){
    u8 *b=(u8 *)MAIL+0x100+side*128;U32(b,0)++;
    U32(b,4)=(u32)(unsigned long long)chr;U32(b,12)=0;U32(b,16)=U32(chr,12);
    if(!open){U32(b,0)++;return;}
    int any=0,hasFusion=0,hasForms=0;
    for(int i=0;i<4;i++){int id=CALL(BtlParam_GetSlotId,int,(void *,int))(chr,i);hasForms|=id>=0&&id<255;}
    for(int i=0;i<3;i++){int id=CALL(BtlParam_GetUnkB4,int,(void *,int))(chr,i);if(id>=0&&id<255)hasFusion=1;}
    U32(b,20)=hasFusion;U32(b,24)=hasForms;
    for(int i=0;i<4;i++){
        int id=fusion?(i<3?CALL(BtlParam_GetUnkB4,int,(void *,int))(chr,i):255):CALL(BtlParam_GetSlotId,int,(void *,int))(chr,i);
        int cost=fusion?(i<3?CALL(BtlParam_GetCostAE,int,(void *,int))(chr,i):0):CALL(BtlParam_GetSlotCost,int,(void *,int))(chr,i);
        int valid=id>=0&&id<255;
        int allowed=0;
        if(open&&valid){int flag=CALL(BtlChar_TestFlag,int,(void *,int))(chr,0x10f);allowed=fusion?CALL(BtlAct_CanFuse,int,(void *,int,int,int,int *))(chr,i,1,!flag,(int *)0):CALL(BtlAct_CanTransform,int,(void *,int,int,int))(chr,i,1,!flag);}
        U32(b,32+i*16)=(u32)id;U32(b,36+i*16)=(u32)cost;U32(b,40+i*16)=(u32)allowed;U32(b,44+i*16)=(u32)valid;
        any|=valid;
    }
    U32(b,12)=(u32)(open&&(any||hasFusion)?(fusion?3:1):0);U32(b,0)++;
}
/* Read-only roster queries. Do not call GetSwitchTarget: it may modify the fighter. */
static int count_members(void *chr){int n=I32(chr,0x998);return n>=1&&n<=5?n:0;}
static int live_member(void *chr,int n){return n>=0&&n<count_members(chr)&&I32(chr,0x9a4+n*0xa4+8)&&I32(chr,0x9a4+n*0xa4+0x40)>0;}
static int current_target(void *chr){
    int n=I32(chr,0x9a0),count=count_members(chr),active=I32(chr,0x994);
    if(n>=0&&n<count&&n!=active&&live_member(chr,n))return n;
    for(int i=1;i<=count;i++){int next=(n+i)%count;if(next>=0&&next!=active&&live_member(chr,next))return next;}
    return -1;
}
static int can_switch(void *chr){
    int flag=CALL(BtlChar_TestFlag,int,(void *,int))(chr,0x10f);
    return CALL(BtlAct_CanSwitch,int,(void *,int,int))(chr,1,!flag);
}
static void switch_palette(void *chr,int side,int open){
    u8 *b=(u8 *)MAIL+0x100+side*128;U32(b,0)++;U32(b,4)=(u32)(unsigned long long)chr;U32(b,16)=U32(chr,12);U32(b,12)=0;
    U32(b,20)=0;U32(b,24)=0;int card=0,allowed=can_switch(chr),active=I32(chr,0x994);
    for(int n=0;n<count_members(chr)&&card<4;n++)if(n!=active&&live_member(chr,n)){
        int id=I32(chr,0x9a4+n*0xa4);
        if(id<0||id>=255)continue;
        U32(b,32+card*16)=id;U32(b,36+card*16)=n;U32(b,40+card*16)=allowed;U32(b,44+card*16)=1;card++;
    }
    for(int i=card;i<4;i++)U32(b,44+i*16)=0;
    U32(b,12)=open&&card?2:0;U32(b,0)++;
}
__declspec(dllexport) int PadFilter(int socket,u8 *data){
    int result=((int (*)(int,u8 *))(unsigned long long)TRAMP)(socket,data);
    if(result!=18||socket<0||socket>3)return result;
    tagBank=(socket>=2)?0x1000u:0;
    int online=CALL(Port_NetActive,int,(void))();if(online&&socket!=0)return result;
    unsigned flags=U32((void *)ADDR_gBattleWork,0x19f0);
    if(!U32((void *)ADDR_gBattleWork,0x19f8)||(flags&0xbe00u)){ui_hidden();return result;}
    int physical=online?CALL(Port_NetMe,int,(void))():socket;
    int quad=!online&&CALL(Port_QuadMode,int,(void))();
    U32((void *)MAIL,0x900)=quad?4:2;
    if(!quad&&socket>=2)return result;
    int actor=quad?CALL(Port_QuarterFighter,int,(int))(physical):physical;
    int side=physical&1;if(physical<0||physical>3||actor<0||actor>=10)return result;
    if(flags&0x4100u){int owner=CALL(BtlPause_GetMenuPad,int,(void))();if(owner<0||owner==physical){ui_hidden();return result;}}
    void *chr=CALL(BtlChar_FindBySide,void *,(int))(actor);
    if(!chr||I32(chr,0x1278)!=0||(quad&&I32(chr,4)!=physical))return result;
    u16 raw=(u16)~((u16)data[0]|((u16)data[1]<<8));u16 mapped=raw&0x0008u; /* Start remains physical; L3 becomes native L1 lock-on. Physical R3 is consumed; native L3/R3 are generated only by panel selections. */
    u8 *mail=(u8 *)MAIL+side*16;U32(mail,0)++;U32(mail,4)=(u32)(unsigned long long)chr;U32(mail,8)=raw;U32(mail,0)++;
    int action=I32(chr,0x948);int active=action>=0x106&&action<=0x13b;int skill=action>=0xfd&&action<=0x102;
    int combo=(action>=0x44&&action<=0x66)||(action>=0x70&&action<=0xad);
    int stickUp=data[5]<78,stickDown=data[5]>178;
    unsigned dirs=directions(raw,data);int charge=(raw&0x200)!=0;
    if(raw&1)mapped|=1; /* Select stays the original Select camera command. */
    if(raw&2)mapped|=0x400; /* L3 -> original L1: search/lock on to opponent. */
    if(raw&0x400)mapped|=0x800; /* L1 -> original ascend R1 */
    if(raw&0x100)mapped|=0x200; /* L2 -> original descend R2 */
    if(raw&0x4000)mapped|=0x4000; /* X stays original X. Native double-X grab is retained in this compatibility test. */
    int switching=(raw&0x80)&&!charge&&!active&&!skill;
    int open=!switching&&(raw&0x10)&&!charge&&!active&&!skill&&!(combo&&(raw&0x800));
    u8 *tab=(u8 *)MAIL+0x700+side*32;
    unsigned shoulders=raw&0xc00u,oldShoulders=U32(tab,4);U32(tab,4)=shoulders;
    if(!open){U32(tab,0)=0;U32(tab,20)=0;}
    if(open){
        int forms=0,fusions=0;
        for(int i=0;i<4;i++){int id=CALL(BtlParam_GetSlotId,int,(void *,int))(chr,i);forms|=id>=0&&id<255;}
        for(int i=0;i<3;i++){int id=CALL(BtlParam_GetUnkB4,int,(void *,int))(chr,i);fusions|=id>=0&&id<255;}
        if(!forms)U32(tab,0)=fusions?1:0;
        else if(!fusions)U32(tab,0)=0;
        else if(shoulders&~oldShoulders)U32(tab,0)^=1;
    }
    if(switching)switch_palette(chr,side,1);else palette(chr,side,open,(int)U32(tab,0));
    u8 *cards=(u8 *)MAIL+0x100+side*128;
    /* Only packet translation state: remember the last transmitted R3 direction,
       so its native release keeps the selector. Never write combat state. */
    u8 *selection=(u8 *)MAIL+0x500+side*16;
    unsigned previous=U32(selection,0), previousFace=U32(selection,4);
    u8 *formPulse=(u8 *)MAIL+0x800+side*32;
    U32(selection,0)=0;U32(selection,4)=0;
    u8 *sw=(u8 *)MAIL+0x600+side*64;
    unsigned face=raw&0xf000,oldFace=U32(sw,16);U32(sw,16)=switching?face:0;
    if(!face)U32(tab,20)=0;
    if(switching){
        mapped&=9u;neutral_stick(data);data[2]=data[3]=128;
        int slot=face==0x8000?0:face==0x1000?1:face==0x4000?2:face==0x2000?3:-1;
        if(slot>=0&&face!=oldFace&&U32(cards,44+slot*16)&&U32(cards,40+slot*16)){
            U32(sw,0)=U32(cards,36+slot*16)+1;U32(sw,4)=0;U32(sw,12)=0;U32(sw,28)=0;U32(sw,32)=0;U32(sw,36)=0;U32(sw,40)=1;
            U32(sw,20)=U32(chr,12);U32(sw,24)=U32(chr,0x994);
        }
    }else if(open&&U32(cards,12)){
        mapped&=9u;neutral_stick(data);data[2]=data[3]=128;
        int slot=-1;
        if((raw&0xf000)==0x8000)slot=0;else if((raw&0xf000)==0x1000)slot=1;
        else if((raw&0xf000)==0x4000)slot=2;else if((raw&0xf000)==0x2000)slot=3;
        if(slot>=0&&U32(cards,44+slot*16)&&U32(cards,40+slot*16)){
            unsigned face=raw&0xf000;
            if(U32(cards,12)==3){
                if(U32(tab,20)!=face&&!U32(tab,8)){
                    U32(tab,8)=1;U32(tab,12)=(u32)slot;U32(tab,16)=U32(chr,12);U32(tab,24)=0;U32(tab,28)=0;
                }
                U32(tab,20)=face;
            }else if(previousFace!=face&&!U32(formPulse,0)){ /* two-sample native press and release */
                U32(formPulse,0)=4;U32(formPulse,4)=slot==0?0x80u:slot==1?0x10u:slot==2?0x20u:0x40u;U32(formPulse,8)=U32(chr,12);
            }
            U32(selection,4)=face;
            if(slot==0)mapped|=0x80;else if(slot==1)mapped|=0x10;else if(slot==2)mapped|=0x20;else mapped|=0x40;
        }
    }else if(active){
        int slot=(action-0x106)%3;unsigned physical=slot==0?0x8000u:slot==1?0x1000u:0x2000u;
        if(raw&physical)mapped|=0x1000; /* native Triangle holds every original chargeable super; R2 no longer required */
        mapped|=raw&(charge?0xe0u:0x80u); /* Down/Right are blocked without R2, except a requested native boost below. */
        if(raw&0x10)mapped|=0x10;
        if(I32(chr,0x1594)>=2&&(raw&0x8000)){
            void *row=CALL(BtlSuper_GetPromptRow,void *,(void *,int))(chr,I32(chr,0x1594));
            if(row){int dir=(int)(signed char)U8(row,2);unsigned need=dir==0?0x10u:dir==1?0x40u:dir==2?0x80u:dir==3?0x20u:0;
                if(need&&(dirs&need))mapped|=0x8000|need; /* Explicit requested boost direction, including Down/Right. */
            }
        }
    }else if(skill){
        unsigned physical=((action-0xfd)&1)?0xa0u:0x50u;
        if(raw&physical)mapped|=0x2000; /* hold native Circle for active skill, independently of R2 */
    }else if(charge){
        mapped|=0x100; /* R2 -> original charge L2 */
        unsigned face=raw&0xb000;
        if(face==0x8000){mapped|=0x1000;neutral_stick(data);}
        else if(face==0x1000){mapped|=0x1010;neutral_stick(data);}
        else if(face==0x2000){mapped|=0x1040;neutral_stick(data);}
        else if(face==0){
            unsigned vertical=raw&0x50,horizontal=raw&0xa0;
            if(vertical&&!horizontal){mapped|=0x2000;neutral_stick(data);}
            else if(horizontal&&!vertical){mapped|=0x2010;neutral_stick(data);}
        }
    }else{
        mapped|=raw&0x80; /* Up opens forms; physical Down/Right without R2 are consumed. Analog movement is unchanged. */
        if(raw&0x8000)mapped|=0x8000;
        if(raw&0x1000)mapped|=0x1000;
        if(raw&0x800)mapped|=0x2000;
        if(raw&0x2000)mapped|=0x3000; /* Circle keeps counter stance; no special ground-grab alias. */
        if(combo&&(raw&0x800)&&(raw&0x10))mapped|=0x10;
        /* Counter stance contains native Triangle. Do not combine physical
           Circle with stick Up: the native input-94 test would turn it into a grab. */
        if((raw&0x2000u)&&stickUp){mapped&=(u16)~0x10u;data[5]=128;}
        /* Native combo cancel windows use a fresh, directionless guard press
           for action 0x68. Circle has priority over held Rush/Blast buttons;
           R1+Square is reserved for the native grab sequencer below. */
        if(combo&&(raw&0x2000u)&&(raw&0x8800u)!=0x8800u){
            mapped=(mapped&0xc09u)|0x2000u;neutral_stick(data);
        }else if(combo&&(raw&0x800u)&&!(raw&0x8000u)){
            /* Native directional vanishes: the left stick selects direction;
               R1 alone defaults to forward. No action permissions, costs or
               simulation state are changed. R1+Square keeps grab priority. */
            unsigned vanish=directions(0,data);
            if(!vanish)vanish=0x10u;
            mapped=(mapped&0xc09u)|0x2000u|vanish;
            neutral_stick(data);
        }

    }
    /* Translate vertical Z Dash to the original two-tap ASCEND/DESCEND input.
       Only private packet state is written; both peers run native double-tap tests. */
    u8 *vertical=(u8 *)MAIL+0x840+side*64;
    if(U32(vertical,4)!=U32(chr,12)){
        for(int i=0;i<8;i++)U32(vertical,i*4)=0;
        U32(vertical,4)=U32(chr,12);
    }
    int plain=!active&&!skill&&!open&&!switching&&!U32(tab,8)&&!U32(sw,0)&&!U32(formPulse,0);
    unsigned verticalWant=0;
    if(plain&&charge&&!(raw&0xf8fcu)){
        if((raw&0x500u)==0x400u)verticalWant=0x800u;
        else if((raw&0x500u)==0x100u)verticalWant=0x200u;
    }
    if(verticalWant){
        if(U32(vertical,8)!=verticalWant)U32(vertical,12)=(U32(chr,0x940)&(verticalWant==0x800u?0x800u:0x1000u))?8:6;
        unsigned phase=U32(vertical,12);
        mapped=(mapped&9u)|((phase>=7||phase==4||phase==3)?0:verticalWant);
        neutral_stick(data);data[2]=data[3]=128;
        if(phase)U32(vertical,12)--;
    }else {
        U32(vertical,12)=0;
        if(U32(vertical,8))U32(vertical,20)=2;
        /* The old physical double-tap route is disabled. If a second normal
           ascent/descent would become TAP2, wait for the native seven-frame
           window to expire. Release cancels it; continuous movement is unchanged. */
        if(plain&&!charge){
            unsigned nativeHeld=U32(chr,0x940),seen=U32(vertical,16);
            if((raw&0x400u)&&(mapped&0x800u)){
                if((seen&0x800u)&&!(nativeHeld&0x800u)&&U8(chr,0x687)<7)mapped&=(u16)~0x800u;
                U32(vertical,16)|=0x800u;
            }
            if((raw&0x100u)&&(mapped&0x200u)){
                if((seen&0x200u)&&!(nativeHeld&0x1000u)&&U8(chr,0x688)<7)mapped&=(u16)~0x200u;
                U32(vertical,16)|=0x200u;
            }
        }
    }
    if(!verticalWant&&U32(vertical,20)){mapped&=(u16)~0xa00u;U32(vertical,20)--;}
    U32(vertical,8)=verticalWant;
    /* Counter actions 0x67..0x69 only inspect guard release when their
       animation advances. A fixed two-sample release can end before that window.
       Keep releasing until native input is clear AND the counter action has left,
       then the existing physical R1 mapping resumes guard. Read-only acknowledgment. */
    u8 *guardReturn=(u8 *)MAIL+0x8c0+side*32;
    unsigned wasCircle=U32(guardReturn,0);U32(guardReturn,0)=raw&0x2000u;
    if(U32(guardReturn,8)!=U32(chr,12)){wasCircle=0;U32(guardReturn,4)=0;U32(guardReturn,12)=0;U32(guardReturn,8)=U32(chr,12);}
    int counterPose=action>=0x67&&action<=0x69;
    int releasePose=counterPose||action==0x3b||action==0x3d;
    int guardEligible=plain&&!charge&&!(raw&0xd000u);
    if(!guardEligible||!(raw&0x800u)||(raw&0x2000u)){U32(guardReturn,4)=0;U32(guardReturn,12)=0;}
    else {
        /* Also recover a counter already in progress after reattachment/pause:
           R1 alone must not silently keep a Circle-only pose alive. */
        if(!U32(guardReturn,4)&&(wasCircle||counterPose)){U32(guardReturn,4)=1;U32(guardReturn,12)=2;}
        if(U32(guardReturn,4)){
            if(U32(guardReturn,12)||releasePose||(U32(chr,0x73c)&1u)||(U32(chr,0x74c)&0x10u)){
                mapped&=(u16)~0x3000u;
                if(U32(guardReturn,12))U32(guardReturn,12)--;
            }else U32(guardReturn,4)=0;
        }
    }
    /* Complete a short native transformation pulse even after a quick face tap.
       Each phase lasts two packet samples, never long enough to become a fusion. */
    if(U32(formPulse,0)){
        if(charge||active||skill||switching||U32(tab,8)||U32(formPulse,8)!=U32(chr,12))U32(formPulse,0)=0;
        else {mapped=(mapped&9u)|U32(formPulse,4)|(U32(formPulse,0)>2?4u:0u);neutral_stick(data);data[2]=data[3]=128;U32(formPulse,0)--;}
    }
    /* R3 is tested on release, with its selector direction still held. This
       also handles releasing Up and the face button together after a tap. */
    if((previous&6)==4&&!(mapped&4)){
        mapped=(mapped&0x000bu)|(previous&0xf0u);neutral_stick(data);
    }
    /* Send ordinary right-stick selector pulses, await their observed result,
       then send the original L3+R3 chord. No writes to the selected member. */
    if(U32(sw,0)){
        int target=(int)U32(sw,0)-1;
        if((action>=0xf3&&action<=0xf8)||U32(sw,24)!=U32(chr,0x994)){U32(sw,0)=0;U32(sw,40)=5;}
        else if(++U32(sw,4)>120){U32(sw,0)=0;U32(sw,40)=7;}
        else if(charge||active||skill||(raw&8)||U32(sw,20)!=U32(chr,12)||!live_member(chr,target)||!can_switch(chr)){U32(sw,0)=0;U32(sw,40)=4;}
        else {
            int candidate=current_target(chr);data[2]=data[3]=128;
            if(candidate==target){
                unsigned phase=U32(sw,28),held=U32(chr,0x73c);
                if(phase==0){
                    if(U32(sw,36)>=4){U32(sw,0)=0;U32(sw,40)=8;}
                    else {U32(sw,36)++;U32(sw,28)=1;U32(sw,40)=2;mapped=(mapped&9u)|6;neutral_stick(data);}
                }else if(phase==1){
                    if(held&0x8000u){U32(sw,28)=2;U32(sw,32)=0;U32(sw,40)=3;mapped&=(u16)~6u;}
                    else {mapped=(mapped&9u)|6;neutral_stick(data);}
                }else {
                    mapped&=(u16)~6u;
                    if(!(held&0x8000u)&&++U32(sw,32)>=2)U32(sw,28)=0;
                }
            }
            else if(U32(sw,12)){
                if(candidate!=(int)U32(sw,8))U32(sw,12)=0;
                else if(U32(sw,12)==2){data[3]=255;U32(sw,12)=1;}
            }else {
                data[3]=255;U32(sw,8)=(u32)candidate;U32(sw,12)=2;
            }
        }
    }
    if(U32(tab,8)){
        unsigned held=U32(chr,0x73c),age=++U32(tab,28);
        int changed=U32(tab,16)!=U32(chr,12);
        if(changed||charge||active||skill||switching)U32(tab,24)=1;
        unsigned dir=U32(tab,12)==0?0x80u:U32(tab,12)==1?0x10u:0x20u;
        /* Packet reads can run ahead of battle/rollback. Wait for the game's
           own R3 edge, 30-frame L3+R3 cooldown, and >=13 since-press counter.
           The native fusion predicate runs at counter 12. Never force it. */
        mapped&=9u;neutral_stick(data);data[2]=data[3]=128;
        if(changed||action==0xf1||action==0xf2||age>180){U32(tab,8)=0;}
        else if(U32(tab,8)==1){
            if(U32(tab,24))U32(tab,8)=0;
            else if(!(held&0x2000u)&&U8(chr,0x7af)>=30){U32(tab,8)=2;mapped|=4u|dir;}
        }else if((held&0x2000u)&&U8(chr,0x7cd)>=13){U32(tab,8)=0;}
        else mapped|=4u|(U32(tab,24)?0:dir);
    }
    /* Local R1+Square priority -> native X/X, neutral/neutral, X/X packets.
       Two samples per phase preserve both edges when battle samples every other blank.
       No native command bits, attack masks or combat permissions are changed. */
    u8 *grab=(u8 *)MAIL+0x780+side*32;
    int groundMode=((raw&0x8800u)==0x8800u&&stickDown)?1:((raw&0x3000u)==0x1000u&&stickUp&&!(combo&&(raw&0x800u)))?2:0;
    /* Once selected, keep the ground-grab mode until the face/guard chord is
       released. Returning the stick to center must not start a double-X grab. */
    if(!groundMode&&U32(grab,12)==1&&(raw&0x8800u)==0x8800u)groundMode=1;
    if(!groundMode&&U32(grab,12)==2&&(raw&0x3000u)==0x1000u&&!(combo&&(raw&0x800u)))groundMode=2;
    int groundChord=groundMode!=0,groundWas=U32(grab,12)!=0;U32(grab,12)=(u32)groundMode;
    int chord=(raw&0x8800u)==0x8800u&&!groundChord,was=(int)U32(grab,0);U32(grab,0)=(u32)chord;
    int eligible=!charge&&!active&&!skill&&!open&&!switching&&!U32(tab,8)&&!U32(sw,0);
    if(chord&&!was&&eligible){U32(grab,4)=6;U32(grab,8)=U32(chr,12);}
    int emitted=0;
    if(U32(grab,4)){
        if(groundChord||!eligible||(!chord&&(raw&0xf000u))||U32(grab,8)!=U32(chr,12)||(action>=0xb7&&action<=0xbc))U32(grab,4)=0;
        else {
            mapped=(mapped&9u)|(U32(grab,4)==3||U32(grab,4)==4?0:0x4000u);
            neutral_stick(data);data[2]=data[3]=128;U32(grab,4)--;emitted=1;
        }
    }
    if(chord&&eligible&&!emitted){
        mapped&=9u;neutral_stick(data);data[2]=data[3]=128;
    } /* Consume the complete held chord: no Rush/Blast/Guard/counter leaks or repeats. */
    /* Optional alternate ground grab: physical R1+left-stick Down+Square becomes the
       original Triangle+forward packet (native input 94). No forced native action or permissions. */
    if((raw&0x2000u)&&!groundMode)U32(grab,16)=0;
    if(groundChord&&!groundWas&&eligible){
        U32(grab,16)=(previous&0x3000u)?4:2;U32(grab,20)=U32(chr,12);
    }
    if(U32(grab,16)){
        if(!eligible||U32(grab,20)!=U32(chr,12))U32(grab,16)=0;
        else {
            mapped=(mapped&9u)|0x10u|(U32(grab,16)>2?0:0x1000u);
            neutral_stick(data);data[2]=data[3]=128;U32(grab,16)--;
        }
    }else if(groundChord&&eligible){
        mapped=(mapped&9u)|0x1010u;neutral_stick(data);data[2]=data[3]=128;
    }
    /* Physical R3 -> original L1, neutral, L1. Only offer it when the
       unmodified decision table queried emergency input 46 in the last frame.
       The query flag is read-only: the game still checks health and permissions. */
    u8 *emergency=(u8 *)MAIL+0x7c0+side*32;
    int r3=(raw&4)!=0,previousR3=(int)U32(emergency,0);U32(emergency,0)=(u32)r3;
    int emergencyEligible=!charge&&!active&&!skill&&!open&&!switching&&!U32(tab,8)&&!U32(sw,0);
    if(r3&&!previousR3&&emergencyEligible&&(U8(chr,0x923)&0x40u)){
        U32(emergency,4)=6;U32(emergency,8)=U32(chr,12);U32(grab,4)=0;
    }
    if(U32(emergency,4)){
        if(!emergencyEligible||U32(emergency,8)!=U32(chr,12)||action==0x42)U32(emergency,4)=0;
        else {
            mapped=(mapped&9u)|(U32(emergency,4)==3||U32(emergency,4)==4?0:0x400u);
            neutral_stick(data);data[2]=data[3]=128;U32(emergency,4)--;
        }
    }
    U32(selection,0)=mapped;
    data[0]=(u8)~mapped;data[1]=(u8)~(mapped>>8);
    /* Diagnostic only: original and transmitted local packet words, no tags in the game input word. */
    U32((void *)MAIL,0x400)++;U32((void *)MAIL,0x404)=raw;U32((void *)MAIL,0x408)=mapped;U32((void *)MAIL,0x40c)=(u32)side;U32((void *)MAIL,0x410)=(u32)action;U32((void *)MAIL,0x400)++;
    return result;
}
