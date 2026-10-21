from pathlib import Path
import json,sys,struct,hashlib
import argparse
args=argparse.ArgumentParser(description="Regenerate reviewed Linux compatibility metadata; inspect game changes first.")
args.add_argument("--game",required=True,type=Path);args.add_argument("--output",required=True,type=Path);args=args.parse_args()
l=Path(__file__).resolve().parents[1];sys.path.insert(0,str(l/'tests'));from elf_image import Elf
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
names=[f['name'] for f in json.loads((l/'compatibility-linux.json').read_text())['functions']]
# Header order is the native ABI of the resolved-address environment record.
header=(l/'src/online_addresses.h').read_text();import re
runtime=re.findall(r'#define ADDR_(\w+)',header)
(l/'runtime-symbols.json').write_text(json.dumps(runtime,indent=2),encoding='utf-8')
cs=Cs(CS_ARCH_X86,CS_MODE_64);cs.detail=True
for path,out in [(args.game,args.output)]:
 e=Elf(path);syms=dict(e.syms)
 def section(v):return next((s for s in e.sections if s[1]!=8 and s[3]<=v<s[3]+s[5]),None)
 def read(v,n):
  sec=section(v);assert sec is not None and v+n<=sec[3]+sec[5],hex(v);return e.d[sec[4]+v-sec[3]:sec[4]+v-sec[3]+n]
 def target(v):
  sec=section(v);candidates=[n for n,(a,z) in syms.items() if a<=v and (sec is None or sec[3]<=a<sec[3]+sec[5])]
  if not candidates:return '$image',v-0x20000000
  name=max(candidates,key=lambda n:syms[n][0]);return name,v-syms[name][0]
 functions=[]
 for name in names:
  rv,length=syms[name];assert 5<=length<100000;code=read(rv,length);mask=bytearray(b'\xff'*length);refs=[]
  for ins in cs.disasm(code,rv):
   fields=[]
   if ins.disp_size and any(o.type==3 and ins.reg_name(o.mem.base)=='rip' for o in ins.operands):fields.append((ins.disp_offset,ins.disp_size,ins.address+ins.size+ins.disp,'relative',ins.size-ins.disp_offset))
   if ins.disp_size and any(o.type==3 and o.mem.base==0 and 0x20000000<=o.mem.disp<0x28000000 for o in ins.operands):fields.append((ins.disp_offset,ins.disp_size,ins.disp,'absolute',0))
   if ins.imm_size and (ins.mnemonic in ('mov','movabs','push','call') or ins.mnemonic.startswith('j')):
    for o in ins.operands:
     if o.type==2 and 0x20000000<=o.imm<0x28000000:
      relative=ins.mnemonic=='call' or ins.mnemonic.startswith('j')
      if relative and rv<=o.imm<rv+length:continue
      fields.append((ins.imm_offset,ins.imm_size,o.imm,'relative' if relative else 'absolute',ins.size-ins.imm_offset))
   for pos,width,v,kind,bias in fields:
    symbol,addend=target(v);ref=dict(offset=ins.address-rv+pos,width=width,symbol=symbol,addend=addend,kind=kind,bias=bias)
    sec=section(v)
    if sec is not None and not sec[2]&5:
     content=read(v,min(128,sec[3]+sec[5]-v));end=content.find(b'\0')
     if end>=4 and all(32<=x<127 or x in (9,10,13) for x in content[:end]):ref['data_expected']=content[:end+1].hex()
    refs.append(ref);mask[ins.address-rv+pos:ins.address-rv+pos+width]=bytes(width)
  functions.append(dict(name=name,length=length,expected=code.hex(),mask=mask.hex(),references=refs))
 meta=dict(format=3,baseline_game_sha256=hashlib.sha256(e.d).hexdigest(),functions=functions,runtime_symbols=runtime,globals=list(json.loads((l/'profile-linux.json').read_text())['symbols']),online_symbol='sMode',online_addend=0,trampoline_length=6)
 Path(out).write_text(json.dumps(meta,separators=(',',':')),encoding='utf-8');print(out,len(functions),'functions',len(runtime),'runtime symbols')
