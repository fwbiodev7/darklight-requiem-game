from pathlib import Path
import json, sys
from PIL import Image
from whole_figure_components import identify

for spec in json.loads(Path(sys.argv[1]).read_text(encoding='utf-8')):
    image = Image.open(spec['source']).convert('RGBA')
    result = dict(id=spec['id'], size=list(image.size), alpha=list(image.getchannel('A').getextrema()))
    try:
        figures, _ = identify(image,rows=spec['rows'],columns=spec['columns'],minimum_area=650)
        result.update(figures=len(figures),edgeFrames=[f['index'] for f in figures if f['touchesExtractionEdge']])
    except ValueError as error:
        result['error']=str(error)
    print(json.dumps(result))
