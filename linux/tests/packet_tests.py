from pathlib import Path
import sys,json,struct,hashlib,random,argparse
from unicorn import Uc,UC_ARCH_X86,UC_MODE_64,UC_HOOK_CODE,UC_HOOK_MEM_WRITE
from unicorn.x86_const import *
parser=argparse.ArgumentParser();parser.add_argument('--game',required=True);parser.add_argument('--resolved-profile');options=parser.parse_args()
R=Path(__file__).resolve().parents[1];p=json.loads(Path(options.resolved_profile).read_text()) if options.resolved_profile else json.loads((R/'profile-linux.json').read_text());s=json.loads((R/'analysis/symbols.json').read_text())
from elf_image import Elf

game=Elf(options.game);s={name:{'rva':game.syms[name][0]-0x20000000} for name in s};library=Elf(R/'libbt3sparking.so');base=0x20000000;libbase=0x60000000
assert hashlib.sha256(game.d).hexdigest()==p['game_sha256']
layout=json.loads((R/'native-layout.json').read_text());assert len(layout['hooks'])==1 and layout['hooks'][0]['name']=='Port_PadRead'
for _testHelper in ['BtlInput_AddDoubleTaps', 'BtlAct_Action67to69', 'BtlMember_SpendKi', 'BtlParam_GetKiDrain67', 'BtlAnim_Advance', 'BtlMember_IsKiEmpty', 'BtlAct_Request', 'BtlMove_Step', 'BtlMove_ApplyGravity', 'BtlChar_SetFlag', 'BtlChar_SetVibration', 'BtlAct_GetCurrent']:s[_testHelper]={'rva':game.syms[_testHelper][0]-base}
checks=0
def run_suite(allocated):
 global checks
 u=Uc(UC_ARCH_X86,UC_MODE_64);game.load(u,0);library.load(u,libbase)
 if p.get('runtime_addresses'):u.mem_write(libbase+library.syms['bt3_symbols'][0],b''.join(struct.pack('<Q',value) for value in p['runtime_addresses']))
 u.mem_map(allocated,0x10000);chr=0x4000000;obj=chr+0x4000;row=chr+0x5000;mask=chr+0x6000;buf=0x50002000;stack=0x70000000;stop=0x50000000
 for a,n in [(chr,0x10000),(0x50000000,0x10000),(stack,0x10000)]:u.mem_map(a,n)
 current={'net':1,'side':0,'active':1,'flags':0,'action':0,'stage':0,'owner':0,'ids':[4,5,6,255],'result':18,'rowdir':0,'roster':[(0,10000,1)],'member':0,'candidate':1,'switchable':1,'fusions':[255,255,255],'fuseable':1,'inputHeld':0,'emergency':0,'fusionAge':0,'switchUp':30,'recordHeld':0,'ascendAge':100,'descendAge':100,'nativeCommand':0,'story':0,'storyState':3,'cue':-1};calls=[];packet=b'';trace=[]
 def adr(name):return base+s[name]['rva']
 def regargs():return [u.reg_read(v) for v in [UC_X86_REG_RDI,UC_X86_REG_RSI,UC_X86_REG_RDX,UC_X86_REG_RCX]]
 handlers={
  adr('Port_NetActive'):lambda a:current['net'],adr('Port_NetMe'):lambda a:current['side'],adr('BtlPause_GetMenuPad'):lambda a:current['owner'],
  adr('BtlChar_FindBySide'):lambda a:chr if a[0]==current['side'] else 0,
  adr('BtlParam_GetSlotId'):lambda a:current['ids'][a[1]],adr('BtlParam_GetSlotCost'):lambda a:(a[1]+1)*100000,
  adr('BtlChar_TestFlag'):lambda a:0,adr('BtlAct_CanTransform'):lambda a:1,
  adr('BtlParam_GetUnkB4'):lambda a:current['fusions'][a[1]],adr('BtlParam_GetCostAE'):lambda a:(a[1]+2)*100000,adr('BtlAct_CanFuse'):lambda a:current['fuseable'],adr('BtlSuper_GetPromptRow'):lambda a:row,adr('BtlAct_CanSwitch'):lambda a:current['switchable'],adr('BtlMember_GetGauge'):lambda a:chr+0x9a4+a[1]*0xa4+0x40,
 }
 def original(args):u.mem_write(args[1],packet);return current['result']
 handlers[allocated+0xe000]=original
 def hook(uc,a,size,arg):
  global checks
  trace.append(a)
  if len(trace)>32:trace.pop(0)
  if a==stop:uc.emu_stop();return
  if a in handlers:
   assert uc.reg_read(UC_X86_REG_RSP)%16==8,('ABI stack',hex(a));args=regargs();calls.append((a,args));result=handlers[a](args)
   sp=uc.reg_read(UC_X86_REG_RSP);ret=struct.unpack('<Q',uc.mem_read(sp,8))[0]
   for reg in [UC_X86_REG_RDI,UC_X86_REG_RSI,UC_X86_REG_RDX,UC_X86_REG_RCX,UC_X86_REG_R8,UC_X86_REG_R9,UC_X86_REG_R10,UC_X86_REG_R11]:uc.reg_write(reg,0x11223344)
   uc.reg_write(UC_X86_REG_RAX,result);uc.reg_write(UC_X86_REG_RSP,sp+8);uc.reg_write(UC_X86_REG_RIP,ret);checks+=1
 def write(uc,access,a,size,value,arg):
  assert not base<=a<base+0x3000000,('write to game globals',hex(a))
  assert allow_native[0] or not chr<=a<chr+0x10000,('write to fighter/native test state',hex(a))
 allow_native=[False]
 u.hook_add(UC_HOOK_CODE,hook);u.hook_add(UC_HOOK_MEM_WRITE,write)
 def set32(a,v):u.mem_write(a,struct.pack('<I',v))
 def packet_for(raw,lx=128,ly=128):return struct.pack('<H',(~raw)&65535)+bytes([128,128,lx,ly])+b'\0'*12
 def invoke(raw=0,lx=128,ly=128,socket=0,**opts):
  nonlocal packet
  global checks
  current.update(opts);packet=packet_for(raw,lx,ly);calls.clear();u.mem_write(chr,b'\0'*0x2000);set32(chr, current['side']);set32(chr+0x948,current['action']);set32(chr+0x1594,current['stage']);u.mem_write(row+2,bytes([current['rowdir']]))
  u.mem_write(chr+0x923,bytes([64 if current['emergency'] else 0]))
  set32(chr+0x73c,current['inputHeld']);set32(chr+0x74c,current['nativeCommand'])
  set32(chr+0x940,current['recordHeld']);u.mem_write(chr+0x687,bytes([current['ascendAge'],current['descendAge']]))
  u.mem_write(chr+0x7af,bytes([current['switchUp']]))
  u.mem_write(chr+0x7cd,bytes([current['fusionAge']]))
  set32(chr+0x998,len(current['roster']));set32(chr+0x994,current['member']);set32(chr+0x9a0,current['candidate'])
  for n,(id,hp,present) in enumerate(current['roster']):
   set32(chr+0x9a4+n*0xa4,id);set32(chr+0x9a4+n*0xa4+8,present);set32(chr+0x9a4+n*0xa4+0x40,hp)
  set32(adr('gBtlChars'),chr+0x7000 if current['story'] else 0);set32(adr('gBtlSeq'),chr+0x7100 if current['story'] else 0);set32(chr+0x7000,2);set32(chr+0x7004,chr);set32(chr+0x7100,current['storyState']);set32(adr('gHudPrompt'),chr+0x8000 if current['cue']>=0 else 0);set32(chr+0x8290,max(0,current['cue']))
  set32(adr('gBattleWork')+0x19f8,current['active']);set32(adr('gBattleWork')+0x19f0,current['flags'])
  sp=stack+0xf008;u.mem_write(sp,struct.pack('<Q',stop));u.reg_write(UC_X86_REG_RSP,sp);u.reg_write(UC_X86_REG_RDI,socket);u.reg_write(UC_X86_REG_RSI,buf)
  for reg in [UC_X86_REG_RBX,UC_X86_REG_RBP,UC_X86_REG_R12,UC_X86_REG_R13,UC_X86_REG_R14,UC_X86_REG_R15]:u.reg_write(reg,0x12345678)
  try:u.emu_start(libbase+library.syms['PadFilter'][0],stop,count=100000)
  except Exception:
   print('RIP',hex(u.reg_read(UC_X86_REG_RIP)),'trace',[hex(x) for x in trace],'calls',calls);raise
  assert u.reg_read(UC_X86_REG_RIP)==stop;assert u.reg_read(UC_X86_REG_RSP)==sp+8;assert u.reg_read(UC_X86_REG_RAX)==current['result']
  for reg in [UC_X86_REG_RBX,UC_X86_REG_RBP,UC_X86_REG_R12,UC_X86_REG_R13,UC_X86_REG_R14,UC_X86_REG_R15]:assert u.reg_read(reg)==0x12345678
  got=bytes(u.mem_read(buf,18));assert got[6:]==packet[6:];checks+=22;return got
 def held(packet):return (~struct.unpack_from('<H',packet)[0])&65535
 # Story fight fallback, non-fight bypass and native confirmation ownership.
 for net,side,port in [(0,0,0),(0,1,1),(1,0,0),(1,1,0)]:
  assert held(invoke(0x8200,net=net,side=side,socket=port,active=0,story=1,storyState=3,flags=0))==0x1100;checks+=1
  for state in [0,1,2,4,5]:
   assert invoke(0x8200,socket=port,storyState=state)==packet;checks+=1
 for cue in [0,1,2]:
  assert held(invoke(4,net=0,side=0,active=0,story=1,storyState=3,cue=cue))==4;checks+=1
 invoke(0,net=1,side=0,active=1,story=0,cue=-1)
 for opt in [{'net':0,'active':0},{'net':1,'active':0},{'active':1,'flags':0x400},{'flags':0x4100,'owner':0}]:
  got=invoke(0xa300,lx=201,ly=40,**opt);assert got==packet;checks+=1
 invoke(net=1,active=1,flags=0,owner=0)
 got=invoke(0xa300,socket=1);assert got==packet and len(calls)==2;checks+=1
 got=invoke(0x8200,flags=0x4100,owner=1);assert held(got)==0x1100;checks+=1
 invoke(flags=0,owner=0)
 # Expected native physical chords. They are delivered to BOTH peers as ordinary PS2 pad bytes.
 for physical,expected in [(0x200,0x100),(0x400,0x800),(0x100,0x200),(0x800,0x2000),(0x2000,0x3000),(0x1000,0x1000),(0x8000,0x8000),(0x4000,0x4000),(1,1),(2,0x400),(4,0),(0x8200,0x1100),(0x1200,0x1110),(0x2200,0x1140),(0x210,0x2100),(0x240,0x2100),(0x280,0x2110),(0x220,0x2110)]:
  got=invoke(physical);assert held(got)==expected,(hex(physical),hex(held(got)),hex(expected));checks+=1
 for side in [0,1]:
  got=invoke(0x8200,side=side);assert held(got)==0x1100;mail=allocated+0xf000+side*16;assert struct.unpack('<II',u.mem_read(mail+4,8))==(chr,0x8200);checks+=2
 invoke(side=0)
 # Native selectors must not be changed accidentally by the analog stick or a second directional input.
 for raw in [0x8200|n for n in [0x10,0x20,0x40,0x80]]:
  got=invoke(raw,lx=0,ly=255);assert held(got)==0x1100 and got[4:6]==b'\x80\x80';checks+=1
 for act,physical in [(0x106,0x8000),(0x107,0x1000),(0x108,0x2000)]:
  got=invoke(physical,action=act);assert held(got)&0x1000 and not held(got)&0x100;checks+=1
 for act,physical in [(0xfd,0x10),(0xfe,0x80)]:
  got=invoke(physical,action=act);assert held(got)&0x2000 and not held(got)&0x100;checks+=1
 got=invoke(0x8010,action=0x106,stage=2,rowdir=0);assert held(got)&0x9010==0x9010;checks+=1
 got=invoke(0x8840,action=0,stage=0);assert held(got)==0x4000;checks+=1
 for expected in [0x4000,0,0,0x4000,0x4000,0]:assert held(invoke(0))==expected;checks+=1
 got=invoke(0x2010,ids=[255]*4);assert held(got)==0x3000;checks+=1
 invoke(0,ids=[4,5,6,255])
 invoke(0)
 got=invoke(0x1000,ly=0);assert held(got)==0x1010 and got[4:6]==b'\x80\x80';checks+=2
 got=invoke(0x10);assert held(got)==0 and struct.unpack('<I',u.mem_read(allocated+0xf10c,4))[0]==1;checks+=2
 for face,expected in [(0x8000,0x84),(0x1000,0x14),(0x4000,0x24)]:
  invoke(0x10)
  assert held(invoke(face|0x10))==expected;checks+=1
  assert held(invoke(face|0x10))==expected;checks+=1
  for _ in range(15):assert held(invoke(face|0x10))==expected&~4;checks+=1
  invoke(0x10)
  assert held(invoke(face|0x10))==expected;checks+=1
  for value in [expected,expected&~4,expected&~4,0]:assert held(invoke(0))==value;checks+=1
 got=invoke(0);assert struct.unpack('<I',u.mem_read(allocated+0xf10c,4))[0]==0;checks+=1

 # Switch panel: up to four live reserves, no attack commands leak out of face selection.
 team=[(0,10000,1),(4,10000,1),(5,10000,1),(6,10000,1),(7,10000,1)]
 invoke(0,roster=team,member=0,candidate=1)
 got=invoke(0x80);assert held(got)==0 and struct.unpack('<I',u.mem_read(allocated+0xf10c,4))[0]==2;checks+=2
 box=bytes(u.mem_read(allocated+0xf100,128))
 assert [struct.unpack_from('<I',box,32+n*16)[0] for n in range(4)]==[4,5,6,7];checks+=1
 got=invoke(0x8080);assert held(got)==6 and got[2:6]==bytes([128])*4;checks+=2
 for _ in range(16):assert held(invoke(0x8080))==6;checks+=1
 assert held(invoke(0,inputHeld=0x8000))==0;checks+=1
 invoke(0,action=0xf3);assert struct.unpack('<II',u.mem_read(allocated+0xf600,8))[0]==0;checks+=1
 invoke(0,action=0,inputHeld=0)
 # Non-current reserve: two-sample stick pulse, wait for native acknowledgment,
 # then L3+R3 once. Regular movement continues after closing the palette while waiting.
 got=invoke(0x1080);assert held(got)==0 and got[3]==255;checks+=2
 for sample in range(8):
  got=invoke(0x20,lx=255);assert held(got)==0 and got[3]==(255 if sample==0 else 128) and got[4]==255;checks+=2
 got=invoke(0,candidate=2);assert held(got)==6;checks+=1
 assert held(invoke(0,inputHeld=0x8000))==0;checks+=1
 invoke(0,action=0xf3);invoke(0,action=0,inputHeld=0)
 # Use the actual game's NextSwitchTarget machine code for multiple-reserve navigation.
 def next_candidate():
  sp=stack+0xf008;u.mem_write(sp,struct.pack('<Q',stop));u.reg_write(UC_X86_REG_RSP,sp);u.reg_write(UC_X86_REG_RDI,chr)
  allow_native[0]=True
  try:u.emu_start(adr('BtlMember_NextSwitchTarget'),stop,count=100000)
  finally:allow_native[0]=False
  assert u.reg_read(UC_X86_REG_RIP)==stop
  return struct.unpack('<I',u.mem_read(chr+0x9a0,4))[0]
 for side in [0,1]:
  for card,face in enumerate([0x8000,0x1000,0x4000,0x2000]):
   invoke(0,side=side,candidate=1)
   got=invoke(face|0x80);candidate=1;switched=False
   for _ in range(20):
    if held(got)&6==6:
     assert candidate==card+1;switched=True;checks+=1;break
    if got[3]==255:
     candidate=next_candidate();checks+=1
    got=invoke(0x80,candidate=candidate)
   assert switched;checks+=1
   invoke(0,inputHeld=0x8000);invoke(0,action=0xf3);invoke(0,action=0,inputHeld=0)
 invoke(0,side=0,candidate=1)
 # Defeated/absent slots are not offered; active member excluded.
 got=invoke(0x80,roster=[(0,10000,1),(4,0,1),(5,10000,0),(6,20000,1)])
 box=bytes(u.mem_read(allocated+0xf100,128));assert struct.unpack_from('<I',box,32)[0]==6 and struct.unpack_from('<I',box,60)[0]==0;checks+=2
 invoke(0,roster=team,candidate=1)
 got=invoke(0x8080,switchable=0);assert held(got)==0 and got[3]==128;checks+=2
 invoke(0,switchable=1)
 # Pending selection is canceled by R2, local pause, roster/active changes or timeout.
 for cancel in [{'member':1},{'switchable':0},{'flags':0x4100,'owner':0},{'active':0}]:
  invoke(0,net=1,active=1,flags=0,member=0,switchable=1,candidate=1)
  invoke(0x2080)
  invoke(0,**cancel)
  assert struct.unpack('<I',u.mem_read(allocated+0xf600,4))[0]==0;checks+=1
 invoke(0,net=1,active=1,flags=0,member=0,switchable=1,candidate=1)
 invoke(0x2080)
 got=invoke(0x200);assert held(got)==0x100 and struct.unpack('<I',u.mem_read(allocated+0xf600,4))[0]==0;checks+=2
 invoke(0)
 invoke(0x2080)
 for _ in range(125):
  got=invoke(0);assert held(got)&6==0;checks+=1
 assert struct.unpack('<I',u.mem_read(allocated+0xf600,4))[0]==0;checks+=1
 invoke(0,roster=[(0,10000,1)],member=0,candidate=1)



 # Confirmation must not vanish merely because one local packet was generated.
 invoke(0,roster=team,candidate=1,action=0,side=0,inputHeld=0)
 invoke(0x8080)
 for _ in range(25):
  assert held(invoke(0))==6;checks+=1
 assert struct.unpack('<I',u.mem_read(allocated+0xf600,4))[0]==2;checks+=1
 assert held(invoke(0,inputHeld=0x8000))==0;checks+=1
 invoke(0,action=0xf3);assert struct.unpack('<I',u.mem_read(allocated+0xf628,4))[0]==5;checks+=1
 invoke(0,action=0,inputHeld=0)
 # Retry only after the native input is seen and its release is observed.
 invoke(0x8080)
 for attempt in range(4):
  assert struct.unpack('<I',u.mem_read(allocated+0xf624,4))[0]==attempt+1;checks+=1
  assert held(invoke(0,inputHeld=0x8000))==0;checks+=1
  assert held(invoke(0,inputHeld=0))==0;checks+=1
  assert held(invoke(0,inputHeld=0))==0;checks+=1
  got=invoke(0)
  assert (held(got)==6) == (attempt<3);checks+=1
 assert struct.unpack('<I',u.mem_read(allocated+0xf628,4))[0]==8;checks+=1
 invoke(0,roster=[(0,10000,1)])

 # Explicit offline swap acknowledgment on both physical ports.
 for localSide in [0,1]:
  def portinvoke(raw=0,**opts):return invoke(raw,socket=localSide,**opts)
  portinvoke(0,net=0,active=0,side=localSide)
  portinvoke(0,net=0,active=1,side=localSide)
  # Confirmation must not vanish merely because one local packet was generated.
  portinvoke(0,roster=team,candidate=1,action=0,side=localSide,inputHeld=0)
  portinvoke(0x8080)
  for _ in range(25):
   assert held(portinvoke(0))==6;checks+=1
  assert struct.unpack('<I',u.mem_read(allocated+0xf600+localSide*64,4))[0]==2;checks+=1
  assert held(portinvoke(0,inputHeld=0x8000))==0;checks+=1
  portinvoke(0,action=0xf3);assert struct.unpack('<I',u.mem_read(allocated+0xf628+localSide*64,4))[0]==5;checks+=1
  portinvoke(0,action=0,inputHeld=0)
  # Retry only after the native input is seen and its release is observed.
  portinvoke(0x8080)
  for attempt in range(4):
   assert struct.unpack('<I',u.mem_read(allocated+0xf624+localSide*64,4))[0]==attempt+1;checks+=1
   assert held(portinvoke(0,inputHeld=0x8000))==0;checks+=1
   assert held(portinvoke(0,inputHeld=0))==0;checks+=1
   assert held(portinvoke(0,inputHeld=0))==0;checks+=1
   got=portinvoke(0)
   assert (held(got)==6) == (attempt<3);checks+=1
  assert struct.unpack('<I',u.mem_read(allocated+0xf628+localSide*64,4))[0]==8;checks+=1
  portinvoke(0,roster=[(0,10000,1)])
 

 # Shoulder tabs only toggle once per press and only for a fighter with fusions.
 invoke(0,net=1,active=1,flags=0,side=0,action=0,fusions=[21,22,255])
 got=invoke(0x10);assert held(got)==0;checks+=1
 got=invoke(0x410);assert held(got)==0;checks+=1
 assert struct.unpack('<I',u.mem_read(allocated+0xf10c,4))[0]==3;checks+=1
 for _ in range(18):
  invoke(0x410);assert struct.unpack('<I',u.mem_read(allocated+0xf10c,4))[0]==3;checks+=1
 invoke(0x10);invoke(0x810);assert struct.unpack('<I',u.mem_read(allocated+0xf10c,4))[0]==1;checks+=1
 invoke(0x10);invoke(0x810);assert struct.unpack('<I',u.mem_read(allocated+0xf10c,4))[0]==3;checks+=1
 invoke(0);invoke(0x10);assert struct.unpack('<I',u.mem_read(allocated+0xf10c,4))[0]==1;checks+=1
 # A face tap requests a genuine long R3 + direction hold, never a short transformation.
 # Test all three fusion selectors on host, guest and both offline pad ports.
 for net,side,socket in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  for face,expected in [(0x8000,0x84),(0x1000,0x14),(0x4000,0x24)]:
   invoke(0,net=net,side=side,socket=socket,fusions=[21,22,23])
   invoke(0x10,socket=socket);invoke(0x410,socket=socket);invoke(0x10,socket=socket)
   first=invoke(0x10|face,socket=socket);assert held(first)==expected;checks+=1
   for age in range(13):
    for _ in range(2):assert held(invoke(0,socket=socket,inputHeld=0x2000,fusionAge=age))==expected;checks+=1
   # Release comes only after 14 held packets: it cannot be a <13-frame transform tap.
   released=held(invoke(0,socket=socket,inputHeld=0x2000,fusionAge=13));assert released&4==0;checks+=1
   assert held(invoke(0,socket=socket,inputHeld=0,fusionAge=0))==0;checks+=1
 invoke(0,net=1,side=0,fusions=[21,255,255],fuseable=0)
 invoke(0x10);invoke(0x410);invoke(0x10)
 assert held(invoke(0x8010))==0;checks+=1
 invoke(0,fuseable=1,fusions=[255,255,255])
 invoke(0x410);assert struct.unpack('<I',u.mem_read(allocated+0xf10c,4))[0]==1;checks+=1
 invoke(0)


 # Native grab: two samples per press/release phase on host/guest/offline ports.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,action=0,active=1)
  invoke(0x800,socket=port)
  got=invoke(0x8800,socket=port,lx=255,ly=0);assert held(got)==0x4000 and got[4:6]==bytes([128,128]);checks+=2
  for expected in [0x4000,0,0,0x4000,0x4000]:assert held(invoke(0x8800,socket=port))==expected;checks+=1
  for _ in range(12):assert held(invoke(0x8800,socket=port))==0;checks+=1
  invoke(0,socket=port)
  invoke(0x8800,socket=port);assert held(invoke(0x200,socket=port))==0x100;checks+=1
  assert struct.unpack('<I',u.mem_read(allocated+0xf784+side*32,4))[0]==0;checks+=1
  invoke(0,socket=port)
  invoke(0x8800,socket=port);invoke(0,socket=port,action=0xb7)
  assert struct.unpack('<I',u.mem_read(allocated+0xf784+side*32,4))[0]==0;checks+=1
 invoke(0,net=1,side=0,action=0)


 # Complete grab chord has packet priority over extra attack/counter buttons.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  for act in [0,0x44,0x70,0xad]:
   for chord in [0x8800,0x9800,0xa800]:
    invoke(0,net=net,side=side,socket=port,action=act)
    assert held(invoke(chord,socket=port))==0x4000;checks+=1
    for expected in [0x4000,0,0,0x4000,0x4000]:assert held(invoke(chord,socket=port))==expected;checks+=1
    for _ in range(4):assert held(invoke(chord,socket=port))==0;checks+=1
    invoke(0,socket=port)
  assert held(invoke(0x800,socket=port,action=0))==0x2000;checks+=1
  assert held(invoke(0x8000,socket=port))==0x8000;checks+=1
  assert held(invoke(0x8a00,socket=port))==0x1100;checks+=1
 invoke(0,net=1,side=0,action=0)


 # Empty categories are skipped on host, guest, and both offline ports.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  box=allocated+0xf100+side*128
  for ids,fusions,want in [([255]*4,[21,255,255],3),([4,255,255,255],[255]*3,1),([255]*4,[255]*3,0)]:
   invoke(0,net=net,side=side,socket=port,action=0,ids=ids,fusions=fusions)
   for chord in [0x10,0x410,0x10,0x810,0x10]:
    invoke(chord,socket=port)
    assert struct.unpack('<I',u.mem_read(box+12,4))[0]==want;checks+=1
   assert struct.unpack('<II',u.mem_read(box+20,8))==(int(want==3),int(want==1));checks+=1
 invoke(0,net=1,side=0,socket=0,ids=[4,5,6,255],fusions=[255]*3)
 # Physical R3 does not leak the original R3 to transformations, fusions or team switching.
 # It offers the native emergency chord only during the game's own input-46 window.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,action=0,active=1,emergency=0)
  assert held(invoke(4,socket=port))==0;checks+=1
  invoke(0,socket=port,emergency=1)
  assert held(invoke(4,socket=port))==0x400;checks+=1
  for value in [0x400,0,0,0x400,0x400]:assert held(invoke(4,socket=port))==value;checks+=1
  for _ in range(12):assert held(invoke(4,socket=port))==0;checks+=1
  invoke(0,socket=port)
  assert held(invoke(6,socket=port))==0x400;checks+=1
  for value in [0x400,0,0,0x400,0x400]:assert held(invoke(6,socket=port))==value;checks+=1
  invoke(0,socket=port)
  invoke(4,socket=port);invoke(0,socket=port,action=0x42)
  assert struct.unpack('<I',u.mem_read(allocated+0xf7c4+side*32,4))[0]==0;checks+=1
  invoke(0,socket=port,action=0)
  invoke(4,socket=port);assert held(invoke(0x200,socket=port))==0x100;checks+=1
  assert struct.unpack('<I',u.mem_read(allocated+0xf7c4+side*32,4))[0]==0;checks+=1
  invoke(0,socket=port,emergency=0)
 invoke(0,net=1,side=0,action=0,emergency=0)

 # Unmodified battle D-pad Down/Right must not leak without R2.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,action=0,flags=0,active=1)
  for direction in [0x20,0x40,0x60]:
   for face,expected in [(0,0),(0x8000,0x8000),(0x1000,0x1000),(0x800,0x2000)]:
    got=invoke(face|direction,socket=port,lx=201,ly=90)
    assert held(got)==expected and got[4:6]==bytes([201,90]);checks+=1
  assert held(invoke(0x240,socket=port))==0x2100;checks+=1
  assert held(invoke(0x220,socket=port))==0x2110;checks+=1
  for direction,rowdir in [(0x40,1),(0x20,3)]:
   assert held(invoke(direction,socket=port,action=0x106,stage=0))&0x60==0;checks+=1
   value=held(invoke(0x8000|direction,socket=port,action=0x106,stage=2,rowdir=rowdir))
   assert value&0x9000==0x9000 and value&direction;checks+=1
  # Grounded grab remains Circle + forward, including analog forward.
  invoke(0,socket=port,action=0,stage=0)
  assert held(invoke(0x1000,socket=port,action=0,stage=0,ly=0))==0x1010;checks+=1
  got=invoke(0x60,socket=port,flags=0x4100,owner=side);assert got==packet;checks+=1
 invoke(0,net=1,side=0,socket=0,flags=0,owner=0,action=0,stage=0,rowdir=0)
 # Alternate ground grab is an ordinary native Circle+forward chord.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,flags=0,action=0,stage=0)
  for _ in range(6):
   got=invoke(0x8800,socket=port,lx=128,ly=255)
   assert held(got)==0x1010 and got[4:6]==bytes([128,128]);checks+=1
  assert held(invoke(0,socket=port))==0;checks+=1
  assert held(invoke(0x2010,socket=port,ids=[255]*4))==0x3000;checks+=1
  assert held(invoke(0x40,socket=port))==0;checks+=1
  invoke(0,socket=port,ids=[4,5,6,255])
  assert held(invoke(0x8800,socket=port))==0x4000;checks+=1
  assert held(invoke(0x8800,ly=255,socket=port))==0x1010;checks+=1
  assert held(invoke(0x8800,ly=255,socket=port))==0x1010;checks+=1
  assert held(invoke(0x8a40,socket=port))==0x1100;checks+=1
  got=invoke(0x8800,ly=255,socket=port,flags=0x4100,owner=side);assert got==packet;checks+=1
  invoke(0,socket=port,flags=0,action=0)
 invoke(0,net=1,side=0,socket=0,ids=[4,5,6,255],owner=0)
 # Ground grab must create a fresh guard edge even when R1 was held first.
 groundSequences=[]
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,flags=0,action=0,stage=0,ids=[4,5,6,255])
  guard=held(invoke(0x800,socket=port));assert guard==0x2000;checks+=1
  release=held(invoke(0x8800,ly=255,socket=port));assert release==0x10;checks+=1
  assert held(invoke(0x800,socket=port))==0x10;checks+=1
  press=held(invoke(0x800,socket=port));assert press==0x1010;checks+=1
  assert held(invoke(0x800,socket=port))==0x1010;checks+=1
  assert held(invoke(0x800,socket=port))==0x2000;checks+=1
  invoke(0,socket=port)
  assert held(invoke(0x8800,ly=255,socket=port))==0x1010;checks+=1
  invoke(0,socket=port);invoke(0x800,socket=port);invoke(0x8800,ly=255,socket=port)
  assert held(invoke(0x8200,socket=port))==0x1100;checks+=1
  assert struct.unpack('<I',u.mem_read(allocated+0xf790+side*32,4))[0]==0;checks+=1
  groundSequences.append((guard,release,press));invoke(0,socket=port)
 invoke(0,net=1,side=0,socket=0,flags=0,action=0,stage=0,owner=0)
 # D-pad Down is NOT the alternate ground grab selector.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,action=0,stage=0)
  sequence=[held(invoke(0x8840,socket=port)) for _ in range(6)]
  assert sequence==[0x4000,0x4000,0,0,0x4000,0x4000] and 0x1010 not in sequence;checks+=1
  invoke(0,socket=port)
  assert held(invoke(0x8800,ly=179,socket=port))==0x1010;checks+=1
  invoke(0,socket=port)
  assert held(invoke(0x8800,ly=178,socket=port))==0x4000;checks+=1
  invoke(0,socket=port)
 invoke(0,net=1,side=0,socket=0,action=0,stage=0)
 # Triangle + physical left-stick Up works in either button order.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,flags=0,action=0,stage=0)
  assert held(invoke(0x1000,ly=0,socket=port))==0x1010;checks+=1
  assert held(invoke(0x1000,ly=0,socket=port))==0x1010;checks+=1
  invoke(0,socket=port)
  assert held(invoke(0x1000,socket=port))==0x1000;checks+=1
  release=held(invoke(0x1000,ly=0,socket=port));assert release==0x10;checks+=1
  assert held(invoke(0,ly=128,socket=port))==0x10;checks+=1
  press=held(invoke(0,ly=128,socket=port));assert press==0x1010;checks+=1
  assert held(invoke(0,socket=port))==0x1010;checks+=1
  assert held(invoke(0,socket=port))==0;checks+=1
  assert held(invoke(0x2200,ly=0,socket=port))==0x1140;checks+=1
  invoke(0,socket=port)
  assert held(invoke(0x8840,socket=port))==0x4000;checks+=1
  invoke(0,socket=port);invoke(0,socket=port)
 invoke(0,net=1,side=0,socket=0,flags=0,action=0,stage=0,owner=0)
 # Keep a selected ground grab while centering the stick; never leak double X.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,action=0,stage=0)
  assert held(invoke(0x8800,ly=255,socket=port))==0x1010;checks+=1
  for _ in range(4):assert held(invoke(0x8800,ly=128,socket=port))==0x1010;checks+=1
  assert held(invoke(0,socket=port))==0;checks+=1
  assert held(invoke(0,socket=port))==0;checks+=1
  assert held(invoke(0x8840,socket=port))==0x4000;checks+=1
  invoke(0,socket=port);invoke(0,socket=port)
 invoke(0,net=1,side=0,socket=0,action=0,stage=0)
 # Circle + stick Up is no longer a ground-grab shortcut; counter stance is preserved.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,action=0,stage=0)
  got=invoke(0x2000,ly=0,socket=port)
  assert held(got)==0x3000 and got[5]==128;checks+=1
  assert struct.unpack('<I',u.mem_read(allocated+0xf78c+side*32,4))[0]==0;checks+=1
  invoke(0,socket=port)
  assert held(invoke(0x1000,ly=0,socket=port))==0x1010;checks+=1
  invoke(0,socket=port)
 invoke(0,net=1,side=0,socket=0,action=0,stage=0)
 # L3 performs native L1 search/lock-on in battle; old L3/R3 shortcuts never leak.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,action=0,stage=0,emergency=0,flags=0)
  assert held(invoke(2,socket=port))==0x400;checks+=1
  assert held(invoke(2,socket=port))==0x400;checks+=1
  assert held(invoke(0,socket=port))==0;checks+=1
  assert held(invoke(6,socket=port))==0x400;checks+=1
  assert held(invoke(0x402,socket=port))==0xc00;checks+=1
  got=invoke(2,socket=port,flags=0x4100,owner=side);assert got==packet;checks+=1
  invoke(0,socket=port,flags=0)
 invoke(0,net=1,side=0,socket=0,flags=0,owner=0)
 # Fusion waits for actual simulation counters, not an assumed number of pad reads.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,action=0,stage=0,inputHeld=0,fusionAge=0,switchUp=0,ids=[255]*4,fusions=[21,255,255],fuseable=1)
  invoke(0x10,socket=port)
  assert held(invoke(0x8010,socket=port))==0;checks+=1
  for cooldown in [5,15,29]:assert held(invoke(0,socket=port,switchUp=cooldown))==0;checks+=1
  assert held(invoke(0,socket=port,switchUp=30))==0x84;checks+=1
  for _ in range(24):assert held(invoke(0,socket=port,inputHeld=0x2000,fusionAge=3))==0x84;checks+=1
  assert held(invoke(0,socket=port,fusionAge=12))==0x84;checks+=1
  assert held(invoke(0,socket=port,fusionAge=13))&4==0;checks+=1
  invoke(0,socket=port,inputHeld=0,fusionAge=0)
  invoke(0x10,socket=port);invoke(0x8010,socket=port)
  assert held(invoke(0,socket=port,action=0xf1))&4==0;checks+=1
  assert struct.unpack('<I',u.mem_read(allocated+0xf708+side*32,4))[0]==0;checks+=1
  invoke(0,socket=port,action=0)
  invoke(0x10,socket=port);invoke(0x8010,socket=port)
  for _ in range(181):value=held(invoke(0,socket=port,inputHeld=0,fusionAge=0))
  assert value&4==0;checks+=1
  assert struct.unpack('<I',u.mem_read(allocated+0xf708+side*32,4))[0]==0;checks+=1
  invoke(0,socket=port)
 invoke(0,net=1,side=0,socket=0,action=0,inputHeld=0,fusionAge=0,switchUp=30,ids=[4,5,6,255],fusions=[255]*3)
 random.seed(19)
 for _ in range(200):
  raw=random.randrange(65536);got=invoke(raw,lx=random.randrange(256),ly=random.randrange(256),net=0,active=0);assert got==packet;checks+=1
 # New controls must also work offline, independently for each physical port.
 for side in [0,1]:
  for raw,expected in [(0x200,0x100),(0x8200,0x1100),(0x1200,0x1110),(0x2200,0x1140),(0x210,0x2100),(0x280,0x2110),(0x800,0x2000),(0x2000,0x3000)]:
   got=invoke(raw,net=0,active=1,socket=side,side=side,flags=0,action=0,stage=0)
   assert held(got)==expected,(side,hex(raw),hex(held(got)));checks+=1
 invoke(net=1,active=1,flags=0,action=0,stage=0,side=0)
 # Exercise the actual current game's unmodified remap + command builder with canonical packets.
 masks=[0]*31
 for i,v in {0:0x2000,1:0x4000,2:0x1000,3:0x8000,4:0x10,5:0x40,6:0x80,7:0x20,9:0x100,11:0x800,12:0x200,13:4,14:0x400,15:6,20:0xf000,23:1,24:0x1000,27:0x2000,28:0x4000,29:0x1000,30:0x8000}.items():masks[i]=v
 u.mem_write(mask,b''.join(struct.pack('<I',v) for v in masks))
 # Permit the native remap/cmd functions to mark only their own scratch/counters if needed, not this packet filter.
 def native(name,args):
  sp=stack+0xf008;u.mem_write(sp,struct.pack('<Q',stop));u.reg_write(UC_X86_REG_RSP,sp)
  for reg,v in zip([UC_X86_REG_RDI,UC_X86_REG_RSI,UC_X86_REG_RDX,UC_X86_REG_RCX],args):u.reg_write(reg,v)
  u.emu_start(adr(name),stop,count=100000);assert u.reg_read(UC_X86_REG_RIP)==stop;return u.reg_read(UC_X86_REG_RAX)
 def commands(raw,prev=0):return native('BtlInput_BuildCommands',[chr,native('BtlInput_RemapButtons',[raw,mask]),native('BtlInput_RemapButtons',[prev,mask])])
 for raw,expected in [(0x8200,0x200),(0x1200,0x400),(0x2200,0x800),(0x210,0x4000),(0x280,0x8000)]:
  got=held(invoke(raw));assert commands(got,0x100)&expected;assert commands(got,got)&expected==0;checks+=2
 assert commands(0x4000,0)&0x20000;checks+=1
 assert commands(0,0x4000)&0x20000==0;checks+=1
 assert commands(0x4000,0)&0x20000;checks+=1
 assert commands(0x400,0)&0x400000;assert commands(0,0x400)&0x400000==0;assert commands(0x400,0)&0x400000;checks+=3
 # Reproduce sampling the native pad every other blank. The former one-blank
 # release vanishes at one phase; the revised pulses preserve both edges for all
 # phases, network input delays and repeated rollback reads of the recorded pad.
 def grab_predicate(stream,stride,phase):
  previous=0;oldCommand=0;age=100;found=False
  for raw in stream[phase::stride]:
   u.mem_write(chr+0x67d,bytes([min(255,age)]))
   command=commands(raw,previous);set32(chr+0x74c,command);set32(chr+0x750,oldCommand);set32(chr+0x754,command&~oldCommand)
   allow_native[0]=True
   try:found|=bool(native('BtlInput_TestAction',[chr,92,1]))
   finally:allow_native[0]=False
   age=0 if raw&0x4000 and not previous&0x4000 else min(255,age+1)
   previous=raw;oldCommand=command
  return found
 assert not grab_predicate([0]*8+[0x4000,0,0x4000]+[0]*12,2,0);checks+=1
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,action=0,flags=0,stage=0)
  sequence=[held(invoke(0x8800,socket=port)) for _ in range(6)]
  for delay in [0,1,2,3,6,12,30]:
   for stride in [1,2]:
    for phase in range(stride):
     stream=[0]*(8+delay)+sequence+[0]*12
     assert grab_predicate(stream,stride,phase),(net,side,delay,stride,phase,sequence);checks+=1
     assert grab_predicate(stream,stride,phase);checks+=1
  invoke(0,socket=port)
 # Audit other synthesized combinations with native predicates under one-/two-
 # blank sampling and constant network input delay. No simulation state is written
 # by the filter: only this isolated fixture seeds native input counters.
 def recognized(stream,move,stride,phase):
  previous=0;previousCmd=0;age=100;seen=False
  for raw in stream[phase::stride]:
   bits=native('BtlInput_RemapButtons',[raw,mask]);oldBits=native('BtlInput_RemapButtons',[previous,mask])
   set32(chr+0x73c,bits);set32(chr+0x740,oldBits);set32(chr+0x744,bits&~oldBits);set32(chr+0x748,oldBits&~bits)
   command=commands(raw,previous);set32(chr+0x74c,command);set32(chr+0x750,previousCmd);set32(chr+0x754,command&~previousCmd)
   u.mem_write(chr+0x7af,bytes([30]));u.mem_write(chr+0x7cd,bytes([min(255,age)]));u.mem_write(chr+0x8b6,bytes([min(255,age)]))
   allow_native[0]=True
   try:seen|=bool(native('BtlInput_TestAction',[chr,move,1]))
   finally:allow_native[0]=False
   relevant=0x400 if move==46 else 4
   age=0 if raw&relevant and not previous&relevant else min(255,age+1)
   previous=raw;previousCmd=command
  return seen
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  cases=[]
  for face,move in [(0x8000,100),(0x1000,101),(0x4000,102)]:
   invoke(0,net=net,side=side,socket=port,action=0,flags=0,stage=0,ids=[4,5,6,255],fusions=[255]*3,emergency=0)
   invoke(0x10,socket=port)
   seq=[held(invoke(face|0x10,socket=port))]+[held(invoke(0,socket=port)) for _ in range(5)]
   cases.append((move,seq))
  invoke(0,net=net,side=side,socket=port,action=0,flags=0,emergency=1)
  seq=[held(invoke(4,socket=port))]+[held(invoke(0,socket=port)) for _ in range(7)]
  cases.append((46,seq))
  invoke(0,net=net,side=side,socket=port,action=0,emergency=0)
  first=held(invoke(0x1000,socket=port))
  seq=[first]*4+[held(invoke(0x1000,ly=0,socket=port))]+[held(invoke(0,socket=port)) for _ in range(5)]
  cases.append((94,seq))
  for move,seq in cases:
   for delay in [0,1,2,3,6,12,30]:
    for stride in [1,2]:
     for phase in range(stride):
      stream=[0]*(8+delay)+seq+[0]*8
      assert recognized(stream,move,stride,phase),(move,net,side,delay,stride,phase,seq);checks+=1
      assert recognized(stream,move,stride,phase);checks+=1
  invoke(0,socket=port)
 # Exchange/rollback-facing handshake audit: neither team confirmation nor fusion
 # may disappear while the native fighter is still reporting an earlier input.
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  for lag in [0,1,2,3,6,12,30]:
   invoke(0,net=net,side=side,socket=port,active=0)
   invoke(0,socket=port,active=1,action=0,flags=0,roster=team,member=0,candidate=1,ids=[255]*4,fusions=[21,255,255],switchable=1,inputHeld=0,fusionAge=0,switchUp=30,emergency=0)
   samples=[invoke(0x1080,socket=port),invoke(0,socket=port)]
   assert all(sample[3]==255 for sample in samples);checks+=1
   for _ in range(lag):assert invoke(0,socket=port)[3]==128;checks+=1
   assert held(invoke(0,socket=port,candidate=2))==6;checks+=1
   for _ in range(lag):assert held(invoke(0,socket=port))==6;checks+=1
   assert held(invoke(0,socket=port,inputHeld=0x8000))==0;checks+=1
   invoke(0,socket=port,action=0xf3)
   invoke(0,socket=port,active=0)
   invoke(0,socket=port,active=1,action=0,roster=[(0,10000,1)],member=0,candidate=1,inputHeld=0,fusionAge=0,switchUp=0)
   invoke(0x10,socket=port);assert held(invoke(0x8010,socket=port))==0;checks+=1
   for _ in range(lag):assert held(invoke(0,socket=port))==0;checks+=1
   assert held(invoke(0,socket=port,switchUp=30))==0x84;checks+=1
   for _ in range(lag):assert held(invoke(0,socket=port))==0x84;checks+=1
   for age in [1,3,8,4,10,12]:assert held(invoke(0,socket=port,inputHeld=0x2000,fusionAge=age))==0x84;checks+=1
   assert held(invoke(0,socket=port,fusionAge=13))&4==0;checks+=1
   invoke(0,socket=port,action=0xf1)
  invoke(0,socket=port,active=0)
 invoke(0,net=1,side=0,socket=0,active=1,action=0,flags=0,roster=[(0,10000,1)],member=0,candidate=1,ids=[4,5,6,255],fusions=[255]*3,inputHeld=0,fusionAge=0,switchUp=30,emergency=0)
 # Run the actual native command builder on the emitted held/released/pressed sequence.
 for guard,release,press in groundSequences:
  guardCommand=commands(guard,0);releaseCommand=commands(release,guard);pressCommand=commands(press,release)
  assert guardCommand&0x10 and not releaseCommand&0x10 and pressCommand&0x10==0;checks+=1
  assert native('BtlInput_RemapButtons',[press,mask])&0x10;checks+=1
 # Execute the real input-94 predicate used to start the native ground grab.
 def ground_input(raw,previous):
  heldBits=native('BtlInput_RemapButtons',[raw,mask]);previousBits=native('BtlInput_RemapButtons',[previous,mask])
  set32(chr+0x73c,heldBits);set32(chr+0x744,heldBits&~previousBits)
  allow_native[0]=True
  try:return native('BtlInput_TestAction',[chr,94,1])
  finally:allow_native[0]=False
 assert ground_input(0x2010,0)==0;checks+=1 # Old guard+forward translation was wrong.
 assert ground_input(0x1010,0x10)==1;checks+=1
 assert ground_input(0x1010,0x1010)==0;checks+=1
 assert ground_input(0x1110,0)==0;checks+=1 # Charge still excludes this move.
 # Include the game's real analog merge before testing input 94. The previous
 # regression was invisible to button-only tests, since Up came from the stick.
 def packet_ground_input(packet):
  raw=held(packet);bits=native('BtlInput_RemapButtons',[raw,mask]);u.mem_write(buf+0x100,packet[4:6])
  merged=native('BtlInput_MergeStick',[chr,bits,buf+0x100])
  set32(chr+0x73c,merged);set32(chr+0x744,merged)
  allow_native[0]=True
  try:return native('BtlInput_TestAction',[chr,94,1])
  finally:allow_native[0]=False
 # Control: the old Circle counter + analog Up packet really could trigger a grab.
 assert packet_ground_input(packet_for(0x3000,128,0))==1;checks+=1
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  invoke(0,net=net,side=side,socket=port,action=0,stage=0)
  for lx in [0,128,255]:
   packet=invoke(0x2000,lx=lx,ly=0,socket=port)
   assert held(packet)==0x3000 and packet[5]==128;checks+=1
   assert packet_ground_input(packet)==0;checks+=1
  invoke(0,socket=port)
  assert packet_ground_input(invoke(0x1000,ly=0,socket=port))==1;checks+=1
  for _ in range(4):invoke(0,socket=port)
  assert packet_ground_input(invoke(0x8800,ly=255,socket=port))==1;checks+=1
  invoke(0,socket=port)
  assert held(invoke(0x2200,ly=0,socket=port))==0x1140;checks+=1
  invoke(0,socket=port)
 # Actual native command word must contain LOCKON_P only on L3's press.
 invoke(0,net=1,side=0,socket=0,flags=0,action=0,stage=0)
 got=held(invoke(2));assert commands(got,0)&0x400000;checks+=1
 assert commands(got,got)&0x400000==0;checks+=1
 invoke(0)
 # Combo Circle uses the SAME native input-96 as the previous R1, without
 # held attack/directional buttons stealing priority. R1 uses native vanishes.
 def native_combo_test(packet,inputId,prev=0):
  raw=held(packet);bits=native('BtlInput_RemapButtons',[raw,mask])
  u.mem_write(buf+0x100,packet[4:6]);bits=native('BtlInput_MergeStick',[chr,bits,buf+0x100])
  cmd=commands(raw,prev);prevCmd=commands(prev,prev)
  set32(chr+0x73c,bits);set32(chr+0x74c,cmd);set32(chr+0x754,cmd&~prevCmd)
  allow_native[0]=True
  try:return native('BtlInput_TestAction',[chr,inputId,1])
  finally:allow_native[0]=False
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  for act in [0x44,0x66,0x70,0xad]:
   invoke(0,net=net,side=side,socket=port,action=act,flags=0,stage=0)
   for physical in [0x2000,0xa000,0x3000,0xb000,0x6000]:
    got=invoke(physical,lx=0,ly=0,socket=port)
    assert held(got)==0x2000 and got[4:6]==bytes([128,128]);checks+=1
    assert native_combo_test(got,96)==1;checks+=1
    assert native_combo_test(got,96,0x2000)==0;checks+=1 # A held guard is not a fresh press.
    invoke(0,socket=port)
   got=invoke(0x800,socket=port);assert held(got)==0x2010 and got[4:6]==bytes([128,128]);checks+=1
   assert native_combo_test(got,96)==0;checks+=1
   assert native_combo_test(got,32)==1;checks+=1
   assert native_combo_test(got,32,0x2010)==0;checks+=1 # Requires a fresh guard press, no auto-repeat.
   invoke(0,socket=port)
   got=invoke(0x1800,socket=port);assert held(got)==0x2010;checks+=1
   assert native_combo_test(got,32)==1;checks+=1
   for lx,ly,rawdir,inputId in [(128,0,0x10,32),(128,255,0x40,29),(0,128,0x80,30),(255,128,0x20,31)]:
    for physical in [0x800,0x1800]:
     invoke(0,socket=port)
     got=invoke(physical,lx=lx,ly=ly,socket=port)
     assert held(got)==0x2000|rawdir and got[4:6]==bytes([128,128]);checks+=1
     assert native_combo_test(got,inputId)==1;checks+=1
     assert native_combo_test(got,96)==0;checks+=1
   # Complete grab chord retains priority even with the analog pointed up.
   invoke(0,socket=port)
   assert held(invoke(0x8800,ly=0,socket=port))==0x4000;checks+=1
   for _ in range(6):invoke(0,socket=port)
  invoke(0,socket=port,action=0)
  assert held(invoke(0x800,socket=port))==0x2000;checks+=1
  assert held(invoke(0x2000,socket=port))==0x3000;checks+=1
  assert held(invoke(0x8200,socket=port))==0x1100;checks+=1
  invoke(0,socket=port)
 invoke(0,net=1,side=0,socket=0,action=0,flags=0,stage=0)
 # Regression: run the real native double-tap detector and action predicates.
 # Its counters are fixture-owned; the packet translator must never write them.
 def simulate_vertical(physical,net,side,port,stride=1,phase=0):
  invoke(0,net=net,side=side,socket=port,active=0,action=0,flags=0,stage=0)
  prev=0;age=[100,100];prevAge=[100,100];recognizedIds=[];emitted=[]
  for i,raw in enumerate(physical):
   out=invoke(raw,socket=port,active=1,recordHeld=prev,ascendAge=prevAge[0],descendAge=prevAge[1]);emitted.append(held(out))
   if i%stride!=phase:continue
   bits=native('BtlInput_RemapButtons',[held(out),mask])
   tapped=native('BtlInput_AddDoubleTaps',[chr,bits,prev,0]);set32(chr+0x73c,tapped);set32(chr+0x744,tapped&~prev)
   allow_native[0]=True
   try:
    recognizedIds.append((bool(native('BtlInput_TestAction',[chr,15,1])),bool(native('BtlInput_TestAction',[chr,17,1]))))
   finally:allow_native[0]=False
   for j,bit in enumerate([0x800,0x1000]):prevAge[j]=age[j];age[j]=0 if bits&bit and not prev&bit else min(100,age[j]+1)
   prev=tapped
  return recognizedIds,emitted
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  for ascend,button,chord,nativeBit in [(True,0x400,0x600,0x800),(False,0x100,0x300,0x200)]:
   for stride in [1,2]:
    for phase in range(stride):
     for lead in [0,2,18]:
      states,stream=simulate_vertical([0]*8+[button]*lead+[chord]*24+[0]*6,net,side,port,stride,phase)
      assert any(v[0 if ascend else 1] for v in states),(net,side,ascend,stride,phase,lead,stream);checks+=1
      assert not any(v[1 if ascend else 0] for v in states);checks+=1
     states,stream=simulate_vertical([0]*8+[button]*2+[0]*2+[button]*26+[0]*6,net,side,port,stride,phase)
     assert not any(any(v) for v in states),(ascend,stride,phase,stream);checks+=1
     assert nativeBit in stream[-12:-6];checks+=1 # second held press eventually resumes ordinary movement
     states,stream=simulate_vertical([0]*8+[chord]*20+[button]*25+[0]*6,net,side,port,stride,phase)
     end=stream[28:];assert 0 in end[:3] and nativeBit in end[-12:-6];checks+=1
  # Guard -> counter -> release Circle while retaining R1: real native guard
  # predicate must go false, then true, for BOTH one-/two-blank sample phases.
  for stride in [1,2]:
   for phase in range(stride):
    invoke(0,net=net,side=side,socket=port,active=0,recordHeld=0,ascendAge=100,descendAge=100)
    physical=[0]*8+[0x800]*6+[0x2800]*6+[0x800]*8+[0]*4;output=[]
    for raw in physical:output.append(held(invoke(raw,socket=port,active=1,action=0x38)))
    assert output[20:24]==[0,0,0x2000,0x2000],output;checks+=1
    sampled=output[phase::stride];guardStates=[]
    for j,raw in enumerate(sampled):
     set32(chr+0x74c,commands(raw,sampled[j-1] if j else 0));allow_native[0]=True
     try:guardStates.append(bool(native('BtlInput_TestAction',[chr,34,1])))
     finally:allow_native[0]=False
    a=next(i for i in range(20,len(output)) if i%stride==phase)//stride
    assert guardStates[a] is False and guardStates[a+1 if stride==2 else a+2] is True;checks+=1
  invoke(0,socket=port,action=0,recordHeld=0,ascendAge=100,descendAge=100)
 invoke(0,net=1,side=0,socket=0,action=0,flags=0,stage=0)
 # Execute the ACTUAL counter handler, with its animation completion stubbed.
 # Unlike a guard-bit-only assertion, this reproduces the native wait until
 # BtlAnim_Advance succeeds before testing the release predicate.
 counterWindow=[False];requested=[]
 custom={
  'BtlMember_SpendKi':lambda a:1,'BtlParam_GetKiDrain67':lambda a:0,
  'BtlAnim_Advance':lambda a:int(counterWindow[0]),'BtlMember_IsKiEmpty':lambda a:0,
  'BtlAct_Request':lambda a:requested.append(a[1]) or 0,
  'BtlMove_Step':lambda a:0,'BtlMove_ApplyGravity':lambda a:0,
  'BtlChar_SetFlag':lambda a:0,'BtlChar_SetVibration':lambda a:0,
  'BtlAct_GetCurrent':lambda a:current['action']}
 savedHandlers={adr(name):handlers.get(adr(name)) for name in custom}
 for name,fn in custom.items():handlers[adr(name)]=fn
 def counter_tick(raw,previous,window):
  counterWindow[0]=window;requested[:]=[];set32(chr+0x74c,commands(raw,previous))
  allow_native[0]=True
  try:native('BtlAct_Action67to69',[chr,1])
  finally:allow_native[0]=False
  return 0xb in requested
 # Control: the former two-sample release finishes BEFORE animation completion.
 invoke(0,action=0x67,recordHeld=0,inputHeld=0,nativeCommand=0)
 assert not any(counter_tick(v,0,i==8) for i,v in enumerate([0,0]+[0x2000]*8));checks+=1
 for net,side,port in [(1,0,0),(1,1,0),(0,0,0),(0,1,1)]:
  for pose in [0x67,0x68,0x69]:
   for holdIndex,holdLength in enumerate([1,2,17,180]):
    stride=1 if holdIndex%2==0 else 2;phase=holdIndex%stride
    delay=[0,4,12][(pose+holdIndex)%3];windowDelay=[0,3,11,35][holdIndex]
    invoke(0,net=net,side=side,socket=port,active=0,recordHeld=0,inputHeld=0,nativeCommand=0)
    for _ in range(4):invoke(0x800,socket=port,active=1,action=0x38)
    for _ in range(holdLength):assert held(invoke(0x2800,socket=port,action=pose))==0x3000;checks+=1
    action=pose;nativeHeld=5;nativeCmd=commands(0x3000);previous=0x3000;stream=[];exitAt=None;guardAt=None
    for i in range(windowDelay+delay+24):
     out=held(invoke(0x800,socket=port,action=action,inputHeld=nativeHeld,nativeCommand=nativeCmd));stream.append(out)
     if action==pose:assert not out&0x3000,('early guard resume',pose,holdLength,i);checks+=1
     elif out&0x2000 and guardAt is None:guardAt=i
     if i%stride!=phase:continue
     delivered=stream[i-delay] if i>=delay else 0x3000
     nativeHeld=native('BtlInput_RemapButtons',[delivered,mask]);nativeCmd=commands(delivered,previous)
     if action==pose and counter_tick(delivered,previous,i>=windowDelay):action=0xb;exitAt=i
     previous=delivered
    assert exitAt is not None and guardAt is not None and guardAt>exitAt,(pose,holdLength,delay,stride,phase,exitAt,guardAt,stream);checks+=1
    assert all(v==0x2000 for v in stream[-4:]),stream;checks+=1
  # Re-press Circle cancels the guard-return wait immediately. Other explicit
  # attacks and charge must never be swallowed by a pending return.
  invoke(0x2800,socket=port,action=0x67,inputHeld=5,nativeCommand=0x10)
  assert held(invoke(0x800,socket=port))==0;checks+=1
  assert held(invoke(0x2800,socket=port))==0x3000;checks+=1
  assert held(invoke(0,socket=port))==0;checks+=1
  invoke(0x2800,socket=port,action=0x67);invoke(0x800,socket=port)
  assert held(invoke(0x8200,socket=port))==0x1100;checks+=1
  # Recovery also works after reattachment/pause discarded the Circle edge.
  invoke(0,socket=port,active=0);assert held(invoke(0x800,socket=port,active=1,action=0x67))==0;checks+=1
  invoke(0,socket=port,action=0,inputHeld=0,nativeCommand=0)
 for address,old in savedHandlers.items():
  if old is None:del handlers[address]
  else:handlers[address]=old
 invoke(0,net=1,side=0,socket=0,action=0,inputHeld=0,nativeCommand=0,recordHeld=0,ascendAge=100,descendAge=100)
 # Exact native fusion predicates 104/105/106, including cooldown and frame 12.
 for inputId,raw in [(104,0x84),(105,0x14),(106,0x24)]:
  bits=native('BtlInput_RemapButtons',[raw,mask]);set32(chr+0x73c,bits)
  for since,cooldown,expected in [(11,30,0),(12,29,0),(12,30,1),(13,30,0)]:
   u.mem_write(chr+0x7cd,bytes([since]));u.mem_write(chr+0x7af,bytes([cooldown]));allow_native[0]=True
   try:assert native('BtlInput_TestAction',[chr,inputId,1])==expected;checks+=1
   finally:allow_native[0]=False
 # Reproduce Goku turning SSJ when a fusion hold is released too early.
 set32(chr+0x748,0x2000);set32(chr+0x73c,native('BtlInput_RemapButtons',[0x80,mask]))
 u.mem_write(chr+0x7af,bytes([30]));u.mem_write(chr+0x7cd,bytes([7]));allow_native[0]=True
 try:assert native('BtlInput_TestAction',[chr,100,1])==1;checks+=1
 finally:allow_native[0]=False
 # Completed fusion now releases only after native count 13: none of the
 # short transformation selectors (including base-form return) can match.
 set32(chr+0x73c,0);u.mem_write(chr+0x7cd,bytes([13]));allow_native[0]=True
 try:
  for inputId in range(99,104):assert native('BtlInput_TestAction',[chr,inputId,1])==0;checks+=1
 finally:allow_native[0]=False
 return 'PASS Linux System V allocation '+hex(allocated)

lines=[run_suite(a) for a in [0x08000000]]
lines+=['PASS: '+str(checks)+' packet/ABI/native-command checks; new controls offline on both ports; menus preserved; online local side only, relocation, held techniques/skills, transform packets, switch panel and native teammate navigation/acknowledgment, cancellation and timeout, native swap acknowledgment/release and bounded retries online and on both offline ports, fusion tabs and native long-hold packets on host/guest and both offline ports, native R1+Square priority without attack/guard/counter leaks, R3 emergency shortcut in native eligibility window and no original R3 leakage, native boost chord, vertical Z Dash and disabled legacy double taps, guard return after Circle release with actual native counter animation/exit tests and varied hold durations/delayed input acknowledgment, no writes to fighter or game globals.','NOT TESTED: a live online match, actual ping/rollback behavior, physical-controller timing or opponent compatibility end-to-end.']
print('\n'.join(lines));(R/'analysis/packet-test-results.txt').write_text('\n'.join(lines)+'\n',encoding='utf-8')
