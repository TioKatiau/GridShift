"""Publica os arquivos da entrega usando a credencial Git já configurada.
Nunca grava nem imprime a credencial. Não cria permissões nem tokens.
"""
import json, os, subprocess, sys, urllib.request, urllib.error
from pathlib import Path

repo='TioKatiau/GridShift'
env=dict(os.environ,GIT_TERMINAL_PROMPT='0',GCM_INTERACTIVE='Never')
auth=subprocess.run(['git','credential','fill'],input='protocol=https\nhost=github.com\n\n',text=True,capture_output=True,env=env)
fields=dict(line.split('=',1) for line in auth.stdout.splitlines() if '=' in line)
token=fields.get('password')
if not token:
    print('NO_CONFIGURED_CREDENTIAL');sys.exit(2)

def request(method,url,data=None,kind='application/json'):
    headers={'Authorization':'Bearer '+token,'Accept':'application/vnd.github+json','User-Agent':'GridShift-delivery','Content-Type':kind}
    body=json.dumps(data).encode() if isinstance(data,dict) else data
    try:
        with urllib.request.urlopen(urllib.request.Request(url,data=body,headers=headers,method=method),timeout=180) as r:return json.load(r)
    except urllib.error.HTTPError as e:
        print('HTTP',e.code,e.read().decode()[:500]);raise SystemExit(3)

if sys.argv[1]=='check':
    print('AuthenticatedLogin='+request('GET','https://api.github.com/user')['login'])
elif sys.argv[1]=='release':
    sha=sys.argv[2]
    body=Path('Docs/Entrega26/Release.md').read_text(encoding='utf-8-sig')
    result=request('POST',f'https://api.github.com/repos/{repo}/releases',{'tag_name':'v26.0','target_commitish':sha,'name':'GridShift 26.0 — entrega acadêmica','body':body,'draft':False,'prerelease':True})
    Path('Docs/Entrega26/github-release.json').write_text(json.dumps({k:result[k] for k in ('id','html_url','upload_url','tag_name')},indent=2),encoding='utf-8')
    print(result['html_url'])
elif sys.argv[1]=='upload':
    from urllib.parse import quote
    release=json.loads(Path('Docs/Entrega26/github-release.json').read_text())
    path=Path(sys.argv[2]); url=release['upload_url'].split('{')[0]+'?name='+quote(path.name)
    result=request('POST',url,path.read_bytes(),'application/octet-stream')
    print(result['browser_download_url'])
