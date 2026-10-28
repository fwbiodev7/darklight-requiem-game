"""Animate complete EditMode camera captures with the runtime idle cadence.

Only review GIFs are written. No sprite source or anatomical part is edited.
"""
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
PREVIEW=ROOT/'outputs/implementacao-fase2-20261017/unity-preview'

def phase(clock):
    interval=clock%14.3
    if 6<=interval<6.13 or 14.12<=interval<14.25:
        return 5
    return int(clock/.52)%5

def main():
    for skin in ['requiem','rock']:
        source=[Image.open(PREVIEW/f'{skin}-{i}.png').convert('RGB') for i in range(6)]
        order=[];durations=[]
        for step in range(1430):
            current=phase(step/100)
            if order and order[-1]==current:
                durations[-1]+=10
            else:
                order.append(current);durations.append(10)
        images=[source[index] for index in order]
        images[0].save(PREVIEW/f'{skin}-idle-unity.gif',save_all=True,append_images=images[1:],duration=durations,loop=0,optimize=False)
    print('Updated both complete-camera idle GIFs; two 130 ms blinks per 14.3 s.')

if __name__=='__main__':main()
