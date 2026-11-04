def record_crops(script):
    markers=('crops=[];pivots=[];heights=[]','crops.append(Image.fromarray(rgba));pivots.append','height=atlas.height)')
    assert all(marker in script for marker in markers)
    script=script.replace(markers[0],markers[0]+';source_rects=[]')
    script=script.replace(markers[1],"source_rects.append(dict(source=file,rect=[x0,y0,x1-x0,y1-y0]));"+markers[1])
    return script.replace(markers[2],'height=atlas.height,sourceRects=source_rects)')
