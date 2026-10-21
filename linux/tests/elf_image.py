from pathlib import Path
import struct
class Elf:
 def __init__(self,p):
  self.d=Path(p).read_bytes();e=struct.unpack_from('<16sHHIQQQIHHHHHH',self.d);assert e[2]==62
  self.sections=[struct.unpack_from('<IIQQQQIIQQ',self.d,e[6]+i*e[11]) for i in range(e[12])];self.syms={};self.tables={}
  self.ph=[struct.unpack_from('<IIQQQQQQ',self.d,e[5]+i*e[9]) for i in range(e[10])]
  for idx,s in enumerate(self.sections):
   if s[1] not in (2,11):continue
   st=self.sections[s[6]];names=self.d[st[4]:st[4]+st[5]];table=[]
   for at in range(s[4],s[4]+s[5],24):
    n,info,other,ndx,v,z=struct.unpack_from('<IBBHQQ',self.d,at);name=names[n:names.find(b'\0',n)].decode(errors='replace');table.append((name,v,z))
    if name and v:self.syms[name]=(v,z)
   self.tables[idx]=table
 def load(self,u,base):
  low=min(p[3] for p in self.ph if p[0]==1)&~4095;size=(max(p[3]+p[6] for p in self.ph if p[0]==1)+4095)&~4095;u.mem_map(base+low,size-low)
  for p in self.ph:
   if p[0]==1:u.mem_write(base+p[3],self.d[p[2]:p[2]+p[5]])
  for s in self.sections:
   if s[1]!=4:continue
   for at in range(s[4],s[4]+s[5],24):
    offset,info,add=struct.unpack_from('<QQq',self.d,at);kind=info&0xffffffff;index=info>>32
    if kind==8:u.mem_write(base+offset,struct.pack('<Q',base+add))
    elif kind in (6,7,1):
     name,v,size=self.tables[s[6]][index]
     if v:u.mem_write(base+offset,struct.pack('<Q',base+v+add))
