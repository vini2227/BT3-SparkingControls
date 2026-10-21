from pathlib import Path
import json,struct,argparse,hashlib
from elf_image import Elf
from unicorn import Uc,UC_ARCH_X86,UC_MODE_64,UC_HOOK_CODE,UC_HOOK_MEM_WRITE
from unicorn.x86_const import *
parser=argparse.ArgumentParser();parser.add_argument('--game',required=True);parser.add_argument('--resolved-profile');options=parser.parse_args()
root=Path(__file__).resolve().parents[1];lib=Elf(root/'libbt3sparking.so');game=Elf(options.game)
profile=json.loads(Path(options.resolved_profile).read_text()) if options.resolved_profile else json.loads((root/'profile-linux.json').read_text());meta=json.loads((root/'native-layout.json').read_text());patch=dict(meta['hooks'][0]);patch['address']=game.syms['Port_PadRead'][0];patch['trampoline']=(bytes.fromhex(patch['expected'])+b'\xe9'+struct.pack('<i',patch['address']+6-(0x0800e000+11))).hex()
assert hashlib.sha256(game.d).hexdigest()==profile['game_sha256']
base=0x60000000;low=0x08000000;stack=0x70000000;stop=0x50000000
count=0
def case(enabled=True,bad=False):
 global count
 u=Uc(UC_ARCH_X86,UC_MODE_64);game.load(u,0);lib.load(u,base)
 for a,size in [(low,0x10000),(stack,0x10000),(stop,0x10000)]:u.mem_map(a,size)
 table={};names={};writes=[];protect=[];status=[];exitcode=[]
 def string(a):
  data=bytearray()
  while len(data)<1024:
   b=bytes(u.mem_read(a+len(data),1))
   if b==b'\0':return bytes(data).decode()
   data+=b
  raise AssertionError('unterminated string')
 # Resolve external libc PLT/GOT entries to explicit emulator stubs.
 for section in lib.sections:
  if section[1]!=4:continue
  for at in range(section[4],section[4]+section[5],24):
   offset,info,add=struct.unpack_from('<QQq',lib.d,at);kind=info&0xffffffff;index=info>>32
   if kind not in (1,6,7):continue
   name,value,size=lib.tables[section[6]][index]
   if value or not name:continue
   if name not in names:names[name]=stop+0x1000+len(names)*16
   address=names[name];table[address]=name;u.mem_write(base+offset,struct.pack('<Q',address+add))
 resolved=','.join(format(value,'x') for value in profile.get('runtime_addresses') or [])
 u.mem_write(stop+0x8300,resolved.encode()+b'\0')
 u.mem_write(stop+0x8000,b'1\0');u.mem_write(stop+0x8100,b'/emulated/status\0')
 if bad:u.mem_write(patch['address'],b'\x90')
 def code(uc,a,size,arg):
  if a==stop:uc.emu_stop();return
  if a not in table:return
  name=table[a];args=[uc.reg_read(reg) for reg in [UC_X86_REG_RDI,UC_X86_REG_RSI,UC_X86_REG_RDX,UC_X86_REG_RCX,UC_X86_REG_R8,UC_X86_REG_R9]]
  assert uc.reg_read(UC_X86_REG_RSP)%16==8
  result=0
  if name=='getenv':
   key=string(args[0]);result=(stop+0x8000 if enabled else 0) if key=='BT3_ADDON_GAME' else stop+0x8100 if key=='BT3_ADDON_STATUS' else stop+0x8300 if key=='BT3_ADDON_SYMBOLS' and resolved else 0
  elif name=='memcmp':result=int(bytes(uc.mem_read(args[0],args[2]))!=bytes(uc.mem_read(args[1],args[2])))
  elif name=='mmap':assert args[:3]==[low,0x10000,3];assert args[3]&0x100000;result=low
  elif name=='mprotect':protect.append(args[:3])
  elif name=='memcpy':uc.mem_write(args[0],bytes(uc.mem_read(args[1],args[2])));result=args[0]
  elif name=='memset':uc.mem_write(args[0],bytes([args[1]&255])*args[2]);result=args[0]
  elif name=='fopen':result=stop+0x8200
  elif name=='fputs':status.append(string(args[0]))
  elif name in ('fclose','fprintf'):pass
  elif name=='_exit':exitcode.append(args[0]);uc.emu_stop();return
  else:raise AssertionError('unexpected native call '+name)
  sp=uc.reg_read(UC_X86_REG_RSP);ret=struct.unpack('<Q',uc.mem_read(sp,8))[0]
  uc.reg_write(UC_X86_REG_RSP,sp+8);uc.reg_write(UC_X86_REG_RAX,result);uc.reg_write(UC_X86_REG_RIP,ret)
 def write(uc,access,a,size,value,arg):
  if 0x20000000<=a<0x23000000:writes.append((a,size))
 u.hook_add(UC_HOOK_CODE,code);u.hook_add(UC_HOOK_MEM_WRITE,write)
 sp=stack+0xff08;u.mem_write(sp,struct.pack('<Q',stop));u.reg_write(UC_X86_REG_RSP,sp)
 u.emu_start(base+lib.syms['install'][0],stop,count=100000)
 if not enabled:assert writes==[] and status==[] and protect==[] and exitcode==[];count+=1;return
 if bad:assert writes==[] and exitcode==[126] and status[0].startswith('ERROR');count+=1;return
 assert exitcode==[] and status==['READY'];count+=1
 expected=bytes.fromhex(patch['expected']);codebytes=bytes(u.mem_read(patch['address'],len(expected)))
 assert codebytes[0]==0xe9 and patch['address']+5+struct.unpack_from('<i',codebytes,1)[0]==low+0xe100;count+=1
 assert bytes(u.mem_read(low+0xe000,len(bytes.fromhex(patch['trampoline']))))==bytes.fromhex(patch['trampoline']);count+=1
 assert bytes(u.mem_read(low+0xe100,2))==b'\x48\xb8' and struct.unpack('<Q',u.mem_read(low+0xe102,8))[0]==base+lib.syms['PadFilter'][0];count+=1
 assert protect[0]==[low,0xf000,5] and protect[-1][2]==5;count+=1
 assert all(patch['address']<=a and a+size<=patch['address']+len(expected) for a,size in writes);count+=1
 # The private mailbox remains writable; no other game entry receives a patch.
 assert not any(a<=low+0xf000<a+n for a,n,flags in protect if flags==5);count+=1
case();case(False);case(True,True)
text='PASS: '+str(count)+' Linux loader checks: valid constructor, one pad detour/trampoline, RX/RW boundaries, disabled preload passthrough, incompatible signature rejected before patching.\nOS calls were emulator stubs; real dlopen/LD_PRELOAD execution is NOT verified.\n'
print(text);(root/'analysis/loader-test-results.txt').write_text(text)
