"""Maintainer tool: regenerate compatibility metadata AFTER reviewing game ABI changes.
This is never run by the launcher and is not required for game address-only updates.
Rebuild the native payload first when changing the filter; analysis/symbols.json must
describe the addresses used to compile that payload, even if the reviewed game moved.
"""
from pathlib import Path
import sys,json,struct,hashlib
if '--reviewed' not in sys.argv:raise SystemExit('Review native structure compatibility first; then use --reviewed.')
r=Path(__file__).resolve().parents[2];dev=r/'desenvolvimento';sys.path.insert(0,str(dev/'build_tools'))
import pefile
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
game=Path(sys.argv[sys.argv.index('--game')+1]) if '--game' in sys.argv else Path(r'C:\Users\vinis\OneDrive\Desktop\Tenkaichi3Decomp\Tenkaichi3Decomp.exe')
pe=pefile.PE(str(game));d=game.read_bytes();symbols={};at=pe.FILE_HEADER.PointerToSymbolTable;count=pe.FILE_HEADER.NumberOfSymbols;strings=at+count*18;i=0
while i<count:
 name,value,section,typ,storage,aux=struct.unpack_from('<8sIhHBB',d,at+i*18)
 if name[:4]==bytes(4):n=strings+int.from_bytes(name[4:],'little');name=d[n:d.index(b'\0',n)]
 name=name.rstrip(b'\0').decode(errors='replace')
 if section>0:
  rv=pe.sections[section-1].VirtualAddress+value
  if rv<pe.OPTIONAL_HEADER.SizeOfImage and (name not in symbols or typ==32):symbols[name]={'rva':rv,'type':typ,'section':section}
 i+=1+aux
cs=Cs(CS_ARCH_X86,CS_MODE_64);cs.detail=True;base=pe.OPTIONAL_HEADER.ImageBase
def flen(name):
 s=symbols[name];end=pe.sections[s['section']-1].VirtualAddress+pe.sections[s['section']-1].SizeOfRawData
 return min([x['rva'] for x in symbols.values() if x['type']==32 and x['section']==s['section'] and x['rva']>s['rva']]+[end])-s['rva']
def target(v):
 rv=v-base
 names=[n for n,s in symbols.items() if s['rva']<=rv]
 if not names:return '$image',rv
 n=max(names,key=lambda n:symbols[n]['rva']);return n,rv-symbols[n]['rva']
old=json.loads((dev/'compatibility.json').read_text());needed=json.loads((dev/'analysis/symbols.json').read_text());names=list(dict.fromkeys([f['name'] for f in old['functions']]+[n for n in needed if symbols[n]['type']==32]))
functions=[]
for name in names:
 rv=symbols[name]['rva'];length=flen(name);code=pe.get_data(rv,length);mask=bytearray(b'\xff'*length);refs=[]
 for ins in cs.disasm(code,base+rv):
  off=ins.address-base-rv
  fields=[]
  if ins.disp_size and any(o.type==3 and ins.reg_name(o.mem.base)=='rip' for o in ins.operands):fields.append((ins.disp_offset,ins.disp_size,ins.address+ins.size+ins.disp,'relative',ins.size-ins.disp_offset))
  if ins.disp_size and any(o.type==3 and o.mem.base==0 and base<=o.mem.disp<base+pe.OPTIONAL_HEADER.SizeOfImage for o in ins.operands):fields.append((ins.disp_offset,ins.disp_size,ins.disp,'absolute',0))
  if ins.imm_size:
   for o in ins.operands:
    if o.type==2 and (ins.mnemonic in ('mov','movabs','push','call') or ins.mnemonic.startswith('j')) and base<=o.imm<base+pe.OPTIONAL_HEADER.SizeOfImage:
     relative=ins.mnemonic=='call' or ins.mnemonic.startswith('j')
     if relative and rv<=o.imm-base<rv+length:continue
     fields.append((ins.imm_offset,ins.imm_size,o.imm,'relative' if relative else 'absolute',ins.size-ins.imm_offset))
  for pos,width,v,kind,bias in fields:
   n,add=target(v)
   if n=='$image':continue
   ref=dict(offset=off+pos,width=width,symbol=n,addend=add,kind=kind,bias=bias)
   for section in pe.sections:
    if section.VirtualAddress<=v-base<section.VirtualAddress+section.SizeOfRawData and not section.Characteristics&0xa0000000:
     content=pe.get_data(v-base,min(128,section.VirtualAddress+section.SizeOfRawData-(v-base)));end=content.find(b'\0')
     if end>=4 and all(32<=x<127 or x in (9,10,13) for x in content[:end]):ref['data_expected']=content[:end+1].hex()
   refs.append(ref);mask[off+pos:off+pos+width]=bytes(width)
 functions.append(dict(name=name,length=length,expected=code.hex(),mask=mask.hex(),references=refs))
payload=(dev/'SparkingControls.payload').read_bytes();dll=pefile.PE(str(dev/'analysis/online_filter.dll'));references=[]
for sec in dll.sections:
 if sec.Name.rstrip(b'\0')!=b'.text':continue
 for ins in cs.disasm(sec.get_data()[:sec.Misc_VirtualSize],0x14000000+sec.VirtualAddress):
  fields=[]
  if ins.imm_size:fields += [(ins.imm_offset,ins.imm_size,o.imm) for o in ins.operands if o.type==2]
  if ins.disp_size:fields += [(ins.disp_offset,ins.disp_size,ins.disp)]
  for pos,width,v in fields:
   if base<=v<base+0x4000000:
    n=min(needed,key=lambda n:abs(base+needed[n]['rva']-v));add=v-base-needed[n]['rva'];assert add==0 or (n=='gBattleWork' and add in [0x19f0,0x19f8]),(n,add)
    references.append(dict(offset=ins.address-0x14000000+pos,width=width,symbol=n,addend=add))
assert 24<=len(references)<=32
# NetActive's status load is deliberately discovered from its instruction, not a stale static RVA.
net=pe.get_data(symbols['Port_NetActive']['rva'],flen('Port_NetActive'));netrefs=[]
for ins in cs.disasm(net,base+symbols['Port_NetActive']['rva']):
 if ins.mnemonic=='mov' and any(o.type==3 and ins.reg_name(o.mem.base)=='rip' for o in ins.operands):netrefs.append(ins.address+ins.size+ins.disp-base)
assert len(netrefs)>=1
nettarget=netrefs[0];netname,netadd=target(base+nettarget)
meta=dict(packet_contract=old.get('packet_contract'),approved_history_sha256=old.get('approved_history_sha256',[]),format=2,payload_sha256=hashlib.sha256(payload).hexdigest(),baseline_game_sha256=hashlib.sha256(d).hexdigest(),functions=functions,game_references=references,globals=[n for n in needed if n.startswith(('g','s'))],online_symbol=netname,online_addend=netadd,entry_offset=0x1000,trampoline_length=6)
(dev/'compatibility.json').write_text(json.dumps(meta,separators=(',',':')),encoding='utf-8')
print('Built reviewed compatibility metadata:',len(functions),'functions;',len(references),'payload references')
