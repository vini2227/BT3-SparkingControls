from pathlib import Path
import json,hashlib
root=Path(__file__).resolve().parents[1]
path=root/'profile-linux.json';p=json.loads(path.read_text());p['payload_sha256']=hashlib.sha256((root/'libbt3sparking.so').read_bytes()).hexdigest();p['control_source_sha256']=hashlib.sha256((root/'src/online_filter.c').read_bytes()).hexdigest();path.write_text(json.dumps(p,indent=2)+'\n')
print('Updated Linux library/source hashes; supported game version is unchanged.')
