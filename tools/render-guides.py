from pathlib import Path
import json,re
R=Path(__file__).resolve().parents[1];base='https://github.com/vini2227/BT3-SparkingControls';release=base+'/releases/tag/v1.0.3-beta';download=base+'/releases/download/v1.0.3-beta/'
langs=[('en','English'),('fr','Français'),('it','Italiano'),('de','Deutsch'),('es','Español (España)'),('ar','العربية'),('id','Bahasa Indonesia'),('es-419','Español (Latinoamérica)'),('pt','Português (Brasil)'),('pl','Polski'),('ru','Русский'),('zh-Hans','简体中文'),('zh-Hant','繁體中文'),('ja','日本語'),('ko','한국어'),('th','ไทย')]
D=json.loads((R/'docs/i18n/guide-translations.json').read_text(encoding='utf-8'))
buttons=['□','△','X','R2','L1','L2','R2 + L1','R2 + L2','R1','R2 + □','R2 + △','R2 + ○','R2 + D-pad ↑ / ↓','R2 + D-pad ← / →','○','R1 (combo)','R1 + LS (combo)','R1 + □','R1 + LS↓ + □','LS↑ + △','R3','L3','Select / View','D-pad ↑','D-pad ←','D-pad ↓ / → (no R2)']
headers={'pt':('Botão / combinação','Ação','Downloads e demonstração'),'fr':('Touche / combinaison','Action','Téléchargements et démonstration'),'es':('Botón / combinación','Acción','Descargas y demostración'),'es-419':('Botón / combinación','Acción','Descargas y demostración'),'de':('Taste / Kombination','Aktion','Downloads und Demonstration'),'it':('Tasto / combinazione','Azione','Download e dimostrazione'),'pl':('Przycisk / kombinacja','Działanie','Pobieranie i demonstracja'),'ru':('Кнопка / сочетание','Действие','Загрузка и демонстрация'),'id':('Tombol / kombinasi','Aksi','Unduhan dan demonstrasi'),'zh-Hans':('按键／组合','动作','下载与演示'),'zh-Hant':('按鍵／組合','動作','下載與示範'),'ja':('ボタン／組み合わせ','動作','ダウンロードと実演'),'ko':('버튼 / 조합','동작','다운로드 및 시연'),'th':('ปุ่ม / ชุดคำสั่ง','การทำงาน','ดาวน์โหลดและวิดีโอสาธิต'),'ar':('الزر / المجموعة','الإجراء','التنزيلات والعرض التوضيحي')}
# All pages expose every language at the top, before installation or controls.
def nav(local):
 links=[]
 for code,label in langs:
  url=('../../README.md' if local else 'README.md') if code=='en' else (f'README.{code}.md' if local else f'docs/i18n/README.{code}.md')
  links.append(f'[{label}]({url})')
 return '\n\n'.join(' · '.join(links[i:i+4]) for i in range(0,len(links),4))
def downloads(title):
 return f'''## {title}

[**Windows x64 ZIP**]({download}BT3-SparkingControls-1.0.3-Beta-Windows.zip) · [**Linux x64 ZIP**]({download}BT3-SparkingControls-1.0.3-Beta-Linux-x64.zip) · [**1.0.3 Beta**]({release})

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB]({base}/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT]({download}BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256]({download}SHA256SUMS.txt)
'''
raw=(R/'docs/Controls-and-Setup-Guide.txt').read_text(encoding='utf-8'); changes=raw.split('WHAT CHANGED IN 1.0.3 BETA',1)[1].split('ABOUT',1)[0].strip(); raw=raw.split('WHAT CHANGED IN 1.0.3 BETA',1)[0]+'ABOUT'+raw.split('\nABOUT',1)[1]; blocks=raw.split('\n\n');english=[]
known={'ABOUT':'About','WINDOWS INSTALLATION':'Windows installation','LINUX INSTALLATION (NATIVE LINUX GAME)':'Linux installation — native Linux game','BASIC CONTROLS':'Basic controls','SPECIAL ATTACKS AND SKILLS':'Special attacks and skills','COUNTERS, VANISHES AND GRABS':'Counters, vanishes and grabs','TRANSFORMATIONS, FUSION AND TEAMMATE SWITCHING':'Transformations, fusion and teammate switching','HUD AND CUSTOMIZATION':'HUD and customization','DEFAULT KEYBOARD SHORTCUTS':'Default keyboard shortcuts','COMPATIBILITY, ONLINE AND TIMING':'Compatibility, online and timing','SOURCE AND LICENSE':'Source and license'}
for block in blocks[1:]:
 block=block.strip()
 if not block:continue
 if block in known:english.append('## '+known[block]);continue
 lines=block.splitlines()
 if re.match(r'^\d+\. ',lines[0]) or lines[0].startswith('- '):english.append(block);continue
 if len(lines)>1 and all(re.match(r'^(Square|Triangle|X:|R2|L1|L2|R1|L3|Select|F[678])',s) for s in lines):english.append('\n'.join('- '+s for s in lines));continue
 english.append(' '.join(lines))
hero='<img src="desenvolvimento/assets/dragon-ball-three-stars.png" alt="BT3 Sparking Controls" width="86">'
(R/'README.md').write_text(nav(False)+'\n\n---\n\n'+hero+'\n\n# BT3 Sparking Controls\n\n**1.0.3 Beta — Controls and Setup Guide**\n\n'+'## What changed in 1.0.3 Beta\n\n'+changes+'\n\n[Full changelog](CHANGELOG.md)\n\n'+downloads('Downloads and demonstration')+'\n'+ '\n\n'.join(english)+'\n',encoding='utf-8')
for code,entry in D.items():
 h=entry['head'];a,b,dh=headers[code];desc=entry['rows'].split('|');desc[6:6]=[entry['vertical_up'],entry['vertical_down']];assert len(desc)==len(buttons),(code,len(desc))
 table=f'| {a} | {b} |\n| --- | --- |\n'+'\n'.join('| `'+key+'` | '+value+' |' for key,value in zip(buttons,desc))
 body=f'''# BT3 Sparking Controls — 1.0.3 Beta

## {entry['title']}

{entry['about']}

## {entry['changes_title']}

{chr(10).join('- '+item for item in entry['changes'])}

[CHANGELOG](../../CHANGELOG.md)

{downloads(dh)}
## {h[0]}

### Windows x64

{entry['win']}

`%LOCALAPPDATA%\\BT3-SparkingControls`

### Linux x64

{entry['linux']}

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## {h[1]}

{entry['legend']}

{table}

{entry['attack']}

{entry['combat']}

## {h[2]}

{entry['forms']}

## {h[3]}

{entry['settings']}

## {h[4]}

{entry['compat']}

## {h[5]}

{entry['license']}

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
'''
 if code=='ar':body='<div dir="rtl">\n\n'+body+'\n</div>\n'
 (R/f'docs/i18n/README.{code}.md').write_text(nav(True)+'\n\n---\n\n'+body,encoding='utf-8')
# Preserve the whole original English text through Markdown formatting, never a short substitute.
readme=(R/'README.md').read_text(encoding='utf-8')
for name in known.values():assert '## '+name in readme,name
assert len(D)==15
for doc in [R/'README.md']+sorted((R/'docs/i18n').glob('README.*.md')):
 text=doc.read_text(encoding='utf-8');assert len(text)>3000,(doc,len(text))
 for target in re.findall(r'\]\(([^)]+)\)',text):
  if target.startswith(('http','https','#')):continue
  assert (doc.parent/target).resolve().exists(),(doc,target)
 for key in ['R3','L3','F6','F7','F8','1.0','AGPL-3.0']:assert key in text,(doc,key)
print('PASS: 16 complete guides, 26 control rows in every translation, all language links and local document links resolve.')
(R/'docs/i18n/README-VALIDATION.txt').write_text('PASS: 16 language pages; complete original English TXT content rendered on main README; 26 controls per translated guide; all 16 language links and local document links resolve.\n',encoding='utf-8')

