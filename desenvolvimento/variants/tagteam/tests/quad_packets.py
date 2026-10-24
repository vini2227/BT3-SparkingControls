from pathlib import Path
import sys,json,struct,hashlib,random

import pefile
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
from unicorn import Uc,UC_ARCH_X86,UC_MODE_64,UC_HOOK_CODE,UC_HOOK_MEM_WRITE
from unicorn.x86_const import *
R=Path(__file__).resolve().parents[1];p=json.loads((R/'profile.json').read_text());s=json.loads((R/'analysis/symbols.json').read_text());layout=json.loads((R/'analysis/layout.json').read_text());game=pefile.PE(layout['game']);base=p['image_base'];code=(R/'SparkingControls.payload').read_bytes();entry=layout['entry_offset']
if '--resolved-root' in sys.argv:
 root=Path(sys.argv[sys.argv.index('--resolved-root')+1]);p=json.loads((root/'resolved-test-profile.json').read_text(encoding='utf-8-sig'));code=(root/'resolved-test.payload').read_bytes()
 data=bytes(game.__data__);at=game.FILE_HEADER.PointerToSymbolTable;count=game.FILE_HEADER.NumberOfSymbols;strings=at+count*18;i=0;resolved={}
 while i<count:
  name,value,section,typ,storage,aux=struct.unpack_from('<8sIhHBB',data,at+i*18)
  if name[:4]==bytes(4):off=strings+int.from_bytes(name[4:],'little');name=data[off:data.index(b'\0',off)]
  name=name.rstrip(b'\0').decode(errors='replace')
  if section>0:resolved[name]={'rva':game.sections[section-1].VirtualAddress+value}
  i+=1+aux
 s={name:resolved[name] for name in s}
assert hashlib.sha256(Path(layout['game']).read_bytes()).hexdigest()==p['game_sha256'], 'Resolve the updated executable first with --compat-test and pass --resolved-root CACHE'
assert len(p['patches'])==1 and p['patches'][0]['rva']==s['Port_PadRead']['rva']
cs=Cs(CS_ARCH_X86,CS_MODE_64)
dll=pefile.PE(str(R/'analysis/online_filter.dll'))
for section in dll.sections:
 if section.Name.rstrip(b'\0')==b'.text':
  for ins in cs.disasm(section.get_data()[:section.Misc_VirtualSize],0x14000000+section.VirtualAddress):assert 'gs:' not in ins.op_str and 'fs:' not in ins.op_str
# Resolve extra test-only helpers without changing the injected address table.
_at=game.FILE_HEADER.PointerToSymbolTable;_end=_at+game.FILE_HEADER.NumberOfSymbols*18;_i=0
while _i<game.FILE_HEADER.NumberOfSymbols:
 _name,_value,_section,_type,_storage,_aux=struct.unpack_from('<8sIhHBB',bytes(game.__data__),_at+_i*18)
 if _name[:4]==bytes(4):
  _off=_end+int.from_bytes(_name[4:],'little');_name=bytes(game.__data__)[_off:].split(bytes(1),1)[0]
 _name=_name.rstrip(bytes(1)).decode(errors='replace')
 if _name in ['BtlInput_AddDoubleTaps', 'BtlAct_Action67to69', 'BtlMember_SpendKi', 'BtlParam_GetKiDrain67', 'BtlAnim_Advance', 'BtlMember_IsKiEmpty', 'BtlAct_Request', 'BtlMove_Step', 'BtlMove_ApplyGravity', 'BtlChar_SetFlag', 'BtlChar_SetVibration', 'BtlAct_GetCurrent'] and _section>0:s[_name]={'rva':game.sections[_section-1].VirtualAddress+_value}
 _i+=1+_aux
checks=0
def run_suite(allocated):
 global checks
 u=Uc(UC_ARCH_X86,UC_MODE_64);u.mem_map(base,(game.OPTIONAL_HEADER.SizeOfImage+4095)&~4095);u.mem_write(base,game.get_memory_mapped_image())
 u.mem_map(allocated,p['allocation_size']);relocated=bytearray(code);delta=allocated-p['allocation']
 for fix in p['relocations']:
  fmt='<q' if fix['width']==8 else '<i';v=struct.unpack_from(fmt,relocated,fix['offset'])[0]+delta*fix['sign'];struct.pack_into(fmt,relocated,fix['offset'],v)
 u.mem_write(allocated,bytes(relocated));u.mem_protect(allocated,0xf000,5);chr=0x4000000;obj=chr+0x4000;row=chr+0x5000;mask=chr+0x6000;buf=0x50002000;stack=0x70000000;stop=0x50000000
 for a,n in [(chr,0x10000),(0x50000000,0x10000),(stack,0x10000)]:u.mem_map(a,n)
 current={'net':1,'side':0,'active':1,'flags':0,'action':0,'stage':0,'owner':0,'ids':[4,5,6,255],'result':18,'rowdir':0,'roster':[(0,10000,1)],'member':0,'candidate':1,'switchable':1,'fusions':[255,255,255],'fuseable':1,'inputHeld':0,'emergency':0,'fusionAge':0,'switchUp':30,'recordHeld':0,'ascendAge':100,'descendAge':100,'nativeCommand':0};calls=[];packet=b'';trace=[]
 def adr(name):return base+s[name]['rva']
 def regargs():return [u.reg_read(v) for v in [UC_X86_REG_RCX,UC_X86_REG_RDX,UC_X86_REG_R8,UC_X86_REG_R9]]
 handlers={
  adr('Port_NetActive'):lambda a:current['net'],adr('Port_NetMe'):lambda a:current['side'],adr('BtlPause_GetMenuPad'):lambda a:current['owner'],
  adr('BtlChar_FindBySide'):lambda a:chr if a[0]==current['side'] else 0,
  adr('BtlParam_GetSlotId'):lambda a:current['ids'][a[1]],adr('BtlParam_GetSlotCost'):lambda a:(a[1]+1)*100000,
  adr('BtlChar_TestFlag'):lambda a:0,adr('BtlAct_CanTransform'):lambda a:1,
  adr('BtlParam_GetUnkB4'):lambda a:current['fusions'][a[1]],adr('BtlParam_GetCostAE'):lambda a:(a[1]+2)*100000,adr('BtlAct_CanFuse'):lambda a:current['fuseable'],adr('BtlSuper_GetPromptRow'):lambda a:row,adr('BtlAct_CanSwitch'):lambda a:current['switchable'],adr('BtlMember_GetGauge'):lambda a:chr+0x9a4+a[1]*0xa4+0x40,
 }
 handlers[adr('Port_QuadMode')]=lambda a:current.get('quad',0)
 handlers[adr('Port_QuarterFighter')]=lambda a: [0,1,4,7][a[0]]
 handlers[adr('BtlChar_FindBySide')]=lambda a:chr if a[0]==([0,1,4,7][current['side']] if current.get('quad') else current['side']) else 0
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
   for reg in [UC_X86_REG_RCX,UC_X86_REG_RDX,UC_X86_REG_R8,UC_X86_REG_R9,UC_X86_REG_R10,UC_X86_REG_R11]:uc.reg_write(reg,0x11223344)
   uc.reg_write(UC_X86_REG_RAX,result);uc.reg_write(UC_X86_REG_RSP,sp+8);uc.reg_write(UC_X86_REG_RIP,ret);checks+=1
 def write(uc,access,a,size,value,arg):
  assert not base<=a<base+game.OPTIONAL_HEADER.SizeOfImage,('write to game globals',hex(a))
  assert allow_native[0] or not chr<=a<chr+0x10000,('write to fighter/native test state',hex(a))
 allow_native=[False]
 u.hook_add(UC_HOOK_CODE,hook);u.hook_add(UC_HOOK_MEM_WRITE,write)
 def set32(a,v):u.mem_write(a,struct.pack('<I',v))
 def packet_for(raw,lx=128,ly=128):return struct.pack('<H',(~raw)&65535)+bytes([128,128,lx,ly])+b'\0'*12
 def invoke(raw=0,lx=128,ly=128,socket=0,**opts):
  nonlocal packet
  global checks
  current.update(opts);packet=packet_for(raw,lx,ly);calls.clear();u.mem_write(chr,b'\0'*0x2000);set32(chr, [0,1,4,7][current['side']] if current.get('quad') else current['side']);set32(chr+4,current['side']);set32(chr+0x948,current['action']);set32(chr+0x1594,current['stage']);u.mem_write(row+2,bytes([current['rowdir']]))
  u.mem_write(chr+0x923,bytes([64 if current['emergency'] else 0]))
  set32(chr+0x73c,current['inputHeld']);set32(chr+0x74c,current['nativeCommand'])
  set32(chr+0x940,current['recordHeld']);u.mem_write(chr+0x687,bytes([current['ascendAge'],current['descendAge']]))
  u.mem_write(chr+0x7af,bytes([current['switchUp']]))
  u.mem_write(chr+0x7cd,bytes([current['fusionAge']]))
  set32(chr+0x998,len(current['roster']));set32(chr+0x994,current['member']);set32(chr+0x9a0,current['candidate'])
  for n,(id,hp,present) in enumerate(current['roster']):
   set32(chr+0x9a4+n*0xa4,id);set32(chr+0x9a4+n*0xa4+8,present);set32(chr+0x9a4+n*0xa4+0x40,hp)
  set32(adr('gBattleWork')+0x19f8,current['active']);set32(adr('gBattleWork')+0x19f0,current['flags'])
  sp=stack+0xf008;u.mem_write(sp,struct.pack('<Q',stop));u.reg_write(UC_X86_REG_RSP,sp);u.reg_write(UC_X86_REG_RCX,socket);u.reg_write(UC_X86_REG_RDX,buf)
  for reg in [UC_X86_REG_RBX,UC_X86_REG_RBP,UC_X86_REG_RSI,UC_X86_REG_RDI,UC_X86_REG_R12,UC_X86_REG_R13,UC_X86_REG_R14,UC_X86_REG_R15]:u.reg_write(reg,0x12345678)
  for i in range(6,16):u.reg_write(UC_X86_REG_XMM0+i,0x123456789abcdef123456789abcdef)
  try:u.emu_start(allocated+entry,stop,count=100000)
  except Exception:
   print('RIP',hex(u.reg_read(UC_X86_REG_RIP)),'trace',[hex(x) for x in trace],'calls',calls);raise
  assert u.reg_read(UC_X86_REG_RIP)==stop;assert u.reg_read(UC_X86_REG_RSP)==sp+8;assert u.reg_read(UC_X86_REG_RAX)==current['result']
  for reg in [UC_X86_REG_RBX,UC_X86_REG_RBP,UC_X86_REG_RSI,UC_X86_REG_RDI,UC_X86_REG_R12,UC_X86_REG_R13,UC_X86_REG_R14,UC_X86_REG_R15]:assert u.reg_read(reg)==0x12345678
  for i in range(6,16):assert u.reg_read(UC_X86_REG_XMM0+i)==0x123456789abcdef123456789abcdef
  got=bytes(u.mem_read(buf,18));assert got[6:]==packet[6:];checks+=22;return got
 def held(packet):return (~struct.unpack_from('<H',packet)[0])&65535
 # Four-player variant: disjoint banks, noncontiguous actors and local packets.
 current.update(net=0,quad=1,flags=0,active=1,action=0)
 for player in range(4):
  current['side']=player
  invoke(0,socket=player)
  invoke(0x200,socket=player)
  bank=allocated+0xf000+(player//2)*0x1000
  assert int.from_bytes(u.mem_read(bank+(player%2)*16+8,4),'little')==0x200
  assert int.from_bytes(u.mem_read(bank+0x900,4),'little')==4
  assert ((~int.from_bytes(u.mem_read(buf,2),'little'))&0xffff)&0x100
  checks+=3
 for player in range(4):
  bank=allocated+0xf000+(player//2)*0x1000
  assert int.from_bytes(u.mem_read(bank+(player%2)*16+8,4),'little')==0x200;checks+=1
 current.update(quad=0,side=0)
 return 'PASS allocation '+hex(allocated)

lines=[run_suite(a) for a in [0x14000000,0x13000000,0x60000000]]
print('PASS four-controller banks, native packet output and RX protection:',checks)
