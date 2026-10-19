"""Identify intact whole figures without changing source pixels or assembling anatomy."""
import numpy as np

def identify(im, rows=4, columns=6, minimum_area=1500, alpha_threshold=128):
    alpha=np.array(im.getchannel('A'))
    parents=[];runs=[];previous=[]
    def find(i):
        while parents[i]!=i:
            parents[i]=parents[parents[i]];i=parents[i]
        return i
    for y,line in enumerate(alpha>=alpha_threshold):
        changes=np.diff(np.concatenate(([False],line,[False])).astype(np.int8))
        current=[]
        for start,end in zip(np.where(changes==1)[0],np.where(changes==-1)[0]):
            x0,x1=int(start),int(end);i=len(parents);parents.append(i)
            current.append((x0,x1,i));runs.append((x0,y,x1,i))
            for p0,p1,pi in previous:
                if p0>x1:break
                if p1>=x0:parents[find(pi)]=find(i)
        previous=current
    boxes={}
    for x0,y,x1,i in runs:
        k=find(i)
        if k not in boxes:boxes[k]=[x0,y,x1,y+1,0]
        b=boxes[k];b[0]=min(b[0],x0);b[1]=min(b[1],y);b[2]=max(b[2],x1);b[3]=max(b[3],y+1);b[4]+=x1-x0
    bodies=[(k,b) for k,b in boxes.items() if b[4]>minimum_area]
    expected=rows*columns
    if len(bodies)!=expected:raise ValueError(f'Expected {expected} complete connected figures; found {len(bodies)}')
    bodies.sort(key=lambda p:(p[1][1]+p[1][3])*.5)
    ordered=[]
    for row in range(rows):ordered.extend(sorted(bodies[row*columns:row*columns+columns],key=lambda p:p[1][0]))
    owners={key:index+1 for index,(key,_) in enumerate(ordered)}
    # Small disconnected energy flecks belong to the closest complete silhouette.
    # This labels existing source pixels only; no character elements are moved.
    for k,b in boxes.items():
        if k in owners:continue
        def distance(item):
            _,c=item;dx=max(c[0]-b[2],b[0]-c[2],0);dy=max(c[1]-b[3],b[1]-c[3],0)
            return dx*dx+dy*dy
        owners[k]=owners[min(ordered,key=distance)[0]]
    owner=np.zeros(alpha.shape,dtype=np.uint8)
    for x0,y,x1,i in runs:owner[y,x0:x1]=owners[find(i)]
    frames=[]
    for index,(k,body) in enumerate(ordered):
        yy,xx=np.where(owner==index+1)
        x0,y0,x1,y1=int(xx.min()),int(yy.min()),int(xx.max()+1),int(yy.max()+1)
        touches=x0==0 or y0==0 or x1==im.width or y1==im.height
        # Anchor to the two boots of the main connected body, never to energy flecks.
        foot_y=body[3];ys,xs=np.where(owner[max(0,foot_y-3):foot_y]==index+1)
        foot_x=float(xs.mean()+.5)
        frames.append(dict(index=index,grid=[x0,y0,x1,y1],rect=[max(0,x0-2),max(0,y0-2),min(im.width,x1+2),min(im.height,y1+2)],
            touchesExtractionEdge=touches,bodyHeight=body[3]-body[1],anchor=[foot_x,foot_y],componentArea=body[4]))
    return frames,owner
