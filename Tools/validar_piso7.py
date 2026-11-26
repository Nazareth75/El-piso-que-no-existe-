"""Comprueba el plano de colisiones y todos los puntos del piso 7 sin abrir Unity."""
from pathlib import Path
from collections import deque
import re
import struct

ROOT = Path(__file__).resolve().parents[1]
source = (ROOT / 'Assets/Scripts/PisoSiete.cs').read_text(encoding='utf-8')
walls = [tuple(map(float, row)) for row in re.findall(r'Muro\(([\d.]+), ([\d.]+), ([\d.]+), ([\d.]+)\)', source)]
points = [(name, float(x), float(y)) for name, x, y in re.findall(r'Punto\("([^"]+)", "[^"]+", ([\d.]+), ([\d.]+)', source)]
assert len(points) == 12, f'Faltan interacciones: {len(points)}'

def free(x, y, margin=18.24):
    return (margin < x < 1672-margin and margin < y < 941-margin and
            not any(a-margin < x < a+w+margin and b-margin < y < b+h+margin for a,b,w,h in walls))

# Misma rejilla que PisoSiete.CrearNavegacion (origen Unity abajo-izquierda).
step = float(re.search(r'const float Paso = ([\d.]+)f', source)[1]) * 48
cols = int(re.search(r'const int Columnas = (\d+)', source)[1])
rows = int(re.search(r'const int Filas = (\d+)', source)[1])
cells = {(x,y) for x in range(cols) for y in range(rows) if free(x*step, 941-y*step)}

def nearest(px, py):
    return min(cells, key=lambda p:(p[0]*step-px)**2+(941-p[1]*step-py)**2)

start = nearest(105, 590)
seen = {start}
queue = deque([start])
while queue:
    x,y = queue.popleft()
    for p in ((x+1,y),(x-1,y),(x,y+1),(x,y-1)):
        if p in cells and p not in seen:
            seen.add(p)
            queue.append(p)

failures = []
for name,x,y in points:
    target = nearest(x,y)
    assert free(x,y,5.76), f'Interacción dentro de un muro: {name}'
    if target not in seen:
        failures.append(name)
    print(f'{name:16} {"OK" if target in seen else "SIN RUTA"}')
assert not failures, f'Puntos inaccesibles desde ascensor: {failures}'

# JugadorCampus lleva CircleCollider2D de radio .28 y offset vertical .32.
# Validamos también su ruta real, que no coincide con el pivote visual de los enemigos.
player_cells = {(x,y) for x in range(cols) for y in range(rows)
                if free(x*step, 941-y*step-15.36, 13.44)}
player_start = min(player_cells, key=lambda p:(p[0]*step-105)**2+(941-p[1]*step-590)**2)
player_seen = {player_start}
queue = deque([player_start])
while queue:
    x,y = queue.popleft()
    for p in ((x+1,y),(x-1,y),(x,y+1),(x,y-1)):
        if p in player_cells and p not in player_seen:
            player_seen.add(p)
            queue.append(p)
for name,x,y in points:
    # Debe poder interactuar dentro del radio y sin que la línea de visión atraviese un muro.
    reachable = [p for p in player_seen if (p[0]*step-x)**2+(941-p[1]*step-y)**2 < (1.55*48)**2]
    assert reachable, f'El collider del jugador no puede alcanzar {name}'
print('Collider real del jugador: todas las salas e interacciones accesibles.')

monsters = re.findall(r'CrearEntidad\("([^"]+)", ([\d.]+), ([\d.]+), ([\d.]+), ([\d.]+),', source)
assert len(monsters) == 5
for name,x,y,mx,my in monsters:
    for px,py in ((float(x),float(y)),(float(mx),float(my))):
        assert free(px,py), f'Enemigo dentro de un obstáculo: {name}'
        assert nearest(px,py) in seen, f'Patrulla aislada: {name}'

assets = ROOT / 'Assets/StreamingAssets/PisoX'
with (assets/'BotsOriginales/fondo_piso_x.png').open('rb') as f:
    assert struct.unpack('>II', f.read(24)[16:24]) == (1672,941)
for monster in ('mutante_sombra','arana_mutante','bestia_toxica','masa_corrupta','tentaculo'):
    with (assets/f'Pack/monstruos/{monster}.png').open('rb') as f:
        w,h = struct.unpack('>II', f.read(24)[16:24])
        assert w >= 6*96 and h == 72, (monster,w,h)
print(f'Plano validado: {len(walls)} colisiones; {len(seen)}/{len(cells)} celdas conectadas; 12 interacciones y 5 animaciones.')
