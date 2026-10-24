"""Maintainer-only generation of reviewed Windows variant profiles.
Usage: python build-variants-windows.py --reviewed --characters EXE --tagteam EXE
Ordinary users never run this: selecting an EXE uses bundled automatic detection.
"""
from pathlib import Path
import argparse,sys,os,shutil,json,struct,subprocess
D=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(D/'build_tools'))
import pefile
ap=argparse.ArgumentParser();ap.add_argument('--reviewed',action='store_true');ap.add_argument('--characters',type=Path);ap.add_argument('--tagteam',type=Path);args=ap.parse_args()
if not args.reviewed:ap.error('Review native interfaces and data layouts before generating profiles.')

def scopes(game,meta):
 pe=pefile.PE(str(game));b=game.read_bytes();at=pe.FILE_HEADER.PointerToSymbolTable;count=pe.FILE_HEADER.NumberOfSymbols;st=at+18*count;scope='';names={};i=0
 while i<count:
  n,v,sec,t,storage,aux=struct.unpack_from('<8sIhHBB',b,at+18*i)
  if n[:4]==bytes(4):q=st+int.from_bytes(n[4:],'little');n=b[q:b.index(b'\0',q)]
  n=n.rstrip(b'\0').decode(errors='replace')
  if n=='.file' and aux:scope=b[at+(i+1)*18:at+(i+1+aux)*18].rstrip(b'\0').decode(errors='replace')
  if 0<sec<=len(pe.sections) and pe.sections[sec-1].VirtualAddress+v<pe.OPTIONAL_HEADER.SizeOfImage:
   if t==32:scope=n
   if n not in names or t==32:names[n]=scope
  i+=1+aux
 for f in meta['functions']:
  for r in f['references']:
   if r.get('symbol') in names:r['symbol_scope']=names[r['symbol']]

for kind,game in [('characters',args.characters),('tagteam',args.tagteam)]:
 if game is None:continue
 game=game.resolve();out=D/'variants'/kind;build=D
 if kind=='tagteam':
  build=D/'analysis/variant-native/desenvolvimento'
  for directory in ['src','tools','analysis']:(build/directory).mkdir(parents=True,exist_ok=True)
  for name in ['online_filter.c','online_addresses.h']:shutil.copy2(out/'src'/name,build/'src'/name)
  shutil.copy2(D/'compatibility.json',build/'compatibility.json')
  script=(D/'tools/build-native-windows.py').read_text()
  script=script.replace("needed+=['BtlInput_TestAction','BtlInput_MergeStick']","needed+=['BtlInput_TestAction','BtlInput_MergeStick','Port_QuadMode','Port_QuarterFighter']").replace("'allocation_size':0x10000","'allocation_size':0x11000")
  (build/'tools/build-native-windows.py').write_text(script)
  env=os.environ.copy();env['BT3_GAME']=str(game);env['PYTHONPATH']=str(D/'build_tools')+os.pathsep+env.get('PYTHONPATH','')
  subprocess.run([sys.executable,str(build/'tools/build-native-windows.py')],env=env,check=True)
 script=(D/'tools/build-compatibility-windows.py').read_text().replace('assert len(references)==24','assert 24<=len(references)<=64')
 script=script.replace("(dev/'compatibility.json').write_text(json.dumps(meta,separators=(',',':')),encoding='utf-8')",'')
 extras=['Port_CharaFormTarget','Port_CharaName','Port_CharaBase','Port_CharaPkg','sample_local'] if kind=='characters' else ['Port_FightPad','BtlChar_Get','BtlChar_GetCount','Port_QuadMode','Port_QuarterFighter','sample_local']
 script=script.replace('functions=[]','names+='+repr(extras)+';names=list(dict.fromkeys(names));functions=[]')
 if kind=='characters':script=script.replace("globals=[n for n in needed if n.startswith(('g','s'))]","globals=[n for n in needed if n.startswith(('g','s'))]+['sChara','sRepl','sReplOf']")
 sys.argv=['builder','--reviewed','--game',str(game)];ns={'__file__':str(build/'tools/build-compatibility-windows.py')};exec(script,ns)
 meta=ns['meta'];meta['approved_history_sha256']=[];meta['packet_contract']=None;scopes(game,meta)
 out.mkdir(parents=True,exist_ok=True);(out/'compatibility.json').write_text(json.dumps(meta,separators=(',',':')))
 for name in ['profile.json','SparkingControls.payload']:shutil.copy2(build/name,out/name)
 print('Reviewed variant:',kind)
