"""Build a Windows ICO: native 32-bit DIB frames below 256px, PNG at 256px.
The bitmap frames also work with .NET Framework Icon.ToBitmap / GDI consumers.
The high-resolution PNG artwork is unchanged.
"""
from pathlib import Path
import sys,io,struct
d=Path(__file__).resolve().parents[1];sys.path.insert(0,str(d/'build_tools'))
from PIL import Image
image=Image.open(d/'assets/dragon-ball-three-stars.png').convert('RGBA');sizes=[16,20,24,32,40,48,64,96,128,256]
bitmap=io.BytesIO();image.save(bitmap,format='ICO',sizes=[(n,n) for n in sizes],bitmap_format='bmp');data=bitmap.getvalue();frames={}
for i in range(struct.unpack_from('<H',data,4)[0]):
 width,height,colors,reserved,planes,bpp,length,offset=struct.unpack_from('<BBBBHHII',data,6+i*16);frames[width or 256]=data[offset:offset+length]
large=io.BytesIO();image.resize((256,256),Image.Resampling.LANCZOS).save(large,format='PNG');frames[256]=large.getvalue()
header=bytearray(struct.pack('<HHH',0,1,len(sizes)));offset=6+16*len(sizes);content=bytearray()
for n in sizes:
 frame=frames[n];header+=struct.pack('<BBBBHHII',n if n<256 else 0,n if n<256 else 0,0,0,1,32,len(frame),offset);content+=frame;offset+=len(frame)
(d/'assets/dragon-ball-three-stars.ico').write_bytes(header+content)
print('Windows icon: nine native DIB frames + high-resolution 256px PNG.')
