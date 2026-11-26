"""Registra las entradas de aulas en los cuatro planos y verifica su acceso por el pasillo."""
from pathlib import Path
from collections import deque
import json
import math
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
PLANOS = ROOT / 'Assets/StreamingAssets/Pisos'
# Puertas en las imágenes originales proporcionadas por el usuario (no se modifica el arte).
SALAS = {
 2: [
 ('Sala MAC',1,295,375),('Centro de desarrollo de software',1,600,375),
 ('Laboratorio de computación 1',1,970,375),('Laboratorio de computación 3',1,1135,375),
 ('Laboratorio de informática 2',1,1305,375),('Laboratorio de postgrado',1,285,565),
 ('Laboratorio de computación 2',1,935,710),
 ('Aula 201',0,1390,355),('Aula 202',0,1390,505),('Aula 203',0,1390,680),
 ('Aula 204',0,1245,710),('Aula 205',0,325,610)],
 3: [
 ('Sala de semiología',2,310,390),('Aula 301',0,510,390),('Aula 302',0,685,390),
 ('Aula 303',0,920,390),('Aula 304',0,1165,390),('Aula 305',0,1390,410),
 ('Aula 306',0,325,540),('Aula 307',0,330,610),('Aula 308',0,920,720),
 ('Aula 309',0,1350,600),('Aula 310',0,1370,685)],
 4: [
 ('Aula 401',0,310,390),('Aula 402',0,510,390),('Aula 403',0,675,390),
 ('Aula 404',0,920,390),('Aula 405',0,1165,390),('Aula 406',0,1390,410),
 ('Aula 407',0,325,530),('Aula 408',0,325,610),('Aula 409',0,1360,590),
 ('Aula 410',0,1360,685)],
 5: [
 ('Aula 501',0,310,390),('Aula 502',0,510,390),('Aula 503',0,675,390),
 ('Aula 504',0,920,390),('Aula 505',0,1165,390),('Aula 506',0,1390,410),
 ('Aula 507',0,325,530),('Aula 508',0,920,720)]
}

def suelo(n):
    lines=(PLANOS/f'piso{n}.txt').read_text().splitlines()
    width,height=map(float,lines[0].split())
    walls=[]; stairs=[]
    for line in lines[1:]:
        p=line.split()
        if not p: continue
        if p[0]=='grada': stairs.append((float(p[1])/width*40,(height-float(p[2]))/width*40))
        elif len(p)==4: walls.append(tuple(map(float,p)))
    # La geometría conserva las coordenadas normalizadas de los mapas originales.
    walls=[(x/width*40,(height-y-h)/width*40,w/width*40,h/width*40) for x,y,w,h in walls]
    max_y=40*height/width
    def free(x,y):
        cy=y+.32
        return (.7<x<39.3 and .5<y<max_y-.7 and
                not any(a-.34<x<a+w+.34 and b-.34<cy<b+h+.34 for a,b,w,h in walls))
    step=.125
    cells={(x,y) for x in range(320) for y in range(math.ceil(max_y/step)) if free(x*step,y*step)}
    start=min(cells,key=lambda p:(p[0]*step-stairs[0][0])**2+(p[1]*step-stairs[0][1])**2)
    seen={start}; q=deque([start])
    while q:
        x,y=q.popleft()
        for p in ((x+1,y),(x-1,y),(x,y+1),(x,y-1)):
            if p in cells and p not in seen: seen.add(p); q.append(p)
    return free,seen,step,stairs

def main():
    result=[]
    for floor,entries in SALAS.items():
        width,height=Image.open(PLANOS/f'piso{floor}.png').size
        free,seen,step,stairs=suelo(floor)
        for index,(name,kind,x,y) in enumerate(entries):
            wx=x/width*40; wy=(height-y)/width*40
            candidate=min(seen,key=lambda p:(p[0]*step-wx)**2+(p[1]*step-wy)**2)
            sx,sy=candidate[0]*step,candidate[1]*step
            distance=math.hypot(sx-wx,sy-wy)
            assert distance<1.9, (floor,name,'La entrada está demasiado lejos del pasillo',distance)
            # Mantener una distancia entre entradas para que E seleccione la sala correcta.
            result.append(dict(id=f'aula_{floor}_{index+1:02}',piso=floor,tipo=kind,nombre=name,
                               x=round(sx,4),y=round(sy,4),puertaX=x,puertaY=y))
            print(f'{floor}: {name:40} {sx:5.2f}, {sy:5.2f}  ajuste {distance:.2f}')
    assert len(result)==41
    (PLANOS/'aulas.json').write_text(json.dumps({'salas':result},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('41 entradas registradas en pasillos transitables.')

if __name__=='__main__': main()
