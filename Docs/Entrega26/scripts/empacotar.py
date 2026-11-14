from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
root=Path('Builds/GridShift26_Windows')
with ZipFile('Builds/GridShift26_Windows.zip','w',ZIP_DEFLATED,compresslevel=6) as out:
 for p in root.rglob('*'):
  if p.is_file():out.write(p,'GridShift26_Windows/'+p.relative_to(root).as_posix())
print('ZIP ready')
