from pathlib import Path
import json, importlib.util
from PIL import Image
from whole_figure_components import identify

root=Path(__file__).resolve().parents[2]/'outputs/fidelidade-original-20261018/pacote-v6'
print(json.dumps(dict(scipy=bool(importlib.util.find_spec('scipy')))))
for path in sorted(root.glob('*caminhada-*.png')):
    if 'ownership' in path.name:
        continue
    image=Image.open(path).convert('RGBA')
    counts={}
    for threshold in [180,220,250,254,255]:
        try:
            figures,_=identify(image,rows=8,columns=8,minimum_area=650,alpha_threshold=threshold)
            counts[threshold]=dict(figures=len(figures),edges=sum(f['touchesExtractionEdge'] for f in figures))
        except ValueError as error:
            counts[threshold]=str(error).split('found ')[-1]
    print(json.dumps(dict(file=path.name,thresholds=counts)))
