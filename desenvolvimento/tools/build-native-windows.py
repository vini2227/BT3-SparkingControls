from pathlib import Path
import sys,struct,json,hashlib,shutil,subprocess,os
r=Path(__file__).resolve().parents[2];out=r/'desenvolvimento';out.mkdir(exist_ok=True);src=out/'src';src.mkdir(exist_ok=True);analysis=out/'analysis';analysis.mkdir(exist_ok=True)
sys.path.insert(0,str(out/'build_tools'))
import pefile
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
from keystone import Ks,KS_ARCH_X86,KS_MODE_64
game=Path(os.environ.get('BT3_GAME',r'Tenkaichi3Decomp.exe'));d=game.read_bytes();pe=pefile.PE(data=d);image=pe.OPTIONAL_HEADER.ImageBase;assert image==0x20000000
at=pe.FILE_HEADER.PointerToSymbolTable;count=pe.FILE_HEADER.NumberOfSymbols;strings=at+count*18;i=0;symbols={}
while i<count:
 name,value,section,typ,storage,aux=struct.unpack_from('<8sIhHBB',d,at+i*18)
 if name[:4]==b'\0'*4:n=strings+struct.unpack('<I',name[4:])[0];name=d[n:d.index(b'\0',n)]
 name=name.rstrip(b'\0').decode(errors='replace')
 if section>0:symbols[name]={'rva':pe.sections[section-1].VirtualAddress+value}
 i+=1+aux
needed=['BtlParam_GetUnkB4','BtlParam_GetCostAE','BtlAct_CanFuse','BtlAct_CanSwitch','BtlMember_NextSwitchTarget','BtlMember_GetGauge','Port_PadRead','Port_NetActive','Port_NetMe','BtlPause_GetMenuPad','BtlChar_FindBySide','BtlChar_GetObj','BtlParam_GetSlotId','BtlParam_GetSlotCost','BtlAct_CanTransform','BtlChar_TestFlag','BtlSuper_GetPromptRow','gBattleWork','gBtlChars','gBtlObjTbl','gHud','gBtlSeq','gHudPrompt','sAspectMilli','sHudLayout','BtlInput_RemapButtons','BtlInput_BuildCommands','BtlInput_TestSwitch']
needed+=['BtlInput_TestAction','BtlInput_MergeStick']
assert all(n in symbols for n in needed)
(analysis/'symbols.json').write_text(json.dumps({n:symbols[n] for n in needed},indent=2),encoding='utf-8')
head=['/* Test-only, exact installed Windows executable. */','#define ALLOC 0x14000000u','#define MAIL 0x1400f000u','#define TRAMP 0x1400e000u']
head+=['#define ADDR_'+n+' 0x%xu'%(image+symbols[n]['rva']) for n in needed]
(src/'online_addresses.h').write_text('\n'.join(head)+'\n',encoding='utf-8')
c=(src/'online_filter.c').read_text(encoding='utf-8-sig')
zig=Path(os.environ.get('ZIG',str(r/'arquivo/anteriores/linux-port/upstream/toolchain/zig-windows-x86_64-0.13.0/zig.exe')))
subprocess.run([str(zig),'cc','-target','x86_64-windows-gnu','-shared','-nostdlib','-ffreestanding','-fno-builtin','-fno-stack-protector','-fno-jump-tables','-O2','-Wall','-Wextra','-Werror','-Wl,--image-base,0x14000000','-Wl,--entry,PadFilter','-o',str(analysis/'online_filter.dll'),str(src/'online_filter.c')],check=True,cwd=r)
dll=pefile.PE(str(analysis/'online_filter.dll'));assert dll.OPTIONAL_HEADER.ImageBase==0x14000000;assert not getattr(dll,'DIRECTORY_ENTRY_IMPORT',[])
payload=bytearray(0xf000)
for section in dll.sections:
 if section.Name.rstrip(b'\0') in [b'.text',b'.rdata',b'.data',b'.pdata',b'.xdata']:
  assert section.VirtualAddress+section.Misc_VirtualSize<0xe000
  payload[section.VirtualAddress:section.VirtualAddress+section.SizeOfRawData]=section.get_data()
entry=next(e.address for e in dll.DIRECTORY_ENTRY_EXPORT.symbols if e.name==b'PadFilter')
cs=Cs(CS_ARCH_X86,CS_MODE_64);cs.detail=True;ks=Ks(KS_ARCH_X86,KS_MODE_64);site=symbols['Port_PadRead']['rva'];original=pe.get_data(site,32);length=0
for ins in cs.disasm(original,image+site):
 assert not ins.mnemonic.startswith('j') and ins.mnemonic!='call' and 'rip' not in ins.op_str
 length+=ins.size
 if length>=5:break
tramp=original[:length]+bytes(ks.asm('jmp '+str(image+site+length),0x1400e000+length)[0]);payload[0xe000:0xe000+len(tramp)]=tramp
rel=[];seen=set()
for block in getattr(dll,'DIRECTORY_ENTRY_BASERELOC',[]):
 for fix in block.entries:
  if fix.type in [3,10] and fix.rva<0xe000:rel.append({'offset':fix.rva,'sign':1,'width':8 if fix.type==10 else 4});seen.add(fix.rva)
for section in dll.sections:
 if section.Name.rstrip(b'\0')!=b'.text':continue
 for ins in cs.disasm(section.get_data()[:section.Misc_VirtualSize],0x14000000+section.VirtualAddress):
  if ins.imm_size and ins.imm_size in [4,8] and not ins.mnemonic.startswith('j') and ins.mnemonic!='call':
   for op in ins.operands:
    if op.type==2 and 0x14000000<=op.imm<0x14010000:
     off=ins.address-0x14000000+ins.imm_offset
     if off not in seen:rel.append({'offset':off,'sign':1,'width':ins.imm_size});seen.add(off)
  if ins.disp_size==4 and 0x14000000<=ins.disp<0x14010000:
   off=ins.address-0x14000000+ins.disp_offset
   if off not in seen:rel.append({'offset':off,'sign':1,'width':4});seen.add(off)
rel.append({'offset':0xe000+length+1,'sign':-1,'width':4})
replacement=bytes(ks.asm('jmp '+str(0x14000000+entry),image+site)[0])+b'\x90'*(length-5)
profile={'format':2,'game_sha256':hashlib.sha256(d).hexdigest(),'payload_sha256':hashlib.sha256(payload).hexdigest(),'image_base':image,'allocation':0x14000000,'allocation_size':0x10000,'patches':[{'rva':site,'expected':original[:length].hex(),'replacement':replacement.hex()}],'symbols':{n:symbols[n]['rva'] for n in needed if n.startswith('g') or n.startswith('s')},'relocations':sorted(rel,key=lambda x:x['offset'])}
profile['symbols']['online_mode']=symbols['sMode']['rva']
(out/'SparkingControls.payload').write_bytes(payload);(out/'profile.json').write_text(json.dumps(profile,indent=2),encoding='utf-8')
(analysis/'layout.json').write_text(json.dumps({'game':str(game),'game_sha256':profile['game_sha256'],'entry_offset':entry,'trampoline_length':length,'relocations':len(rel),'imports':0},indent=2),encoding='utf-8')
print('Online test packet translator built:',out);print('Game SHA256:',profile['game_sha256']);print('One patch: Port_PadRead; relocations:',len(rel));print('Entry:',hex(entry))

