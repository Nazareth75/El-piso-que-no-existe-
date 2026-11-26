import os
import cv2
import numpy as np
from PIL import Image

raiz = r"C:\Users\Kathlen\Piso7-Mapa"
mapa_ruta = os.path.join(raiz, "Assets", "Art", "mapa_campus.png")
im = cv2.imread(mapa_ruta, cv2.IMREAD_COLOR)
alto, ancho = im.shape[:2]

# Recuadros ajustados a cada persona dibujada. El resto del plano no se toca.
personas = [
    (176, 188, 230, 252),
    (312, 224, 354, 274),
    (270, 126, 316, 166),
    (632, 280, 676, 322),
    (712, 280, 760, 322),
    (304, 372, 346, 405),
    (268, 400, 314, 448),
    (346, 400, 396, 448),
    (604, 100, 640, 148),
    (914, 96, 956, 150),
    (838, 218, 888, 276),
    (848, 422, 884, 470),
    (620, 210, 680, 275),
]

mascara = np.zeros((alto, ancho), np.uint8)
for x0, y0, x1, y1 in personas:
    mascara[y0:y1, x0:x1] = 255

limpio = cv2.inpaint(im, mascara, 2, cv2.INPAINT_TELEA)
cv2.imwrite(mapa_ruta, limpio)
cv2.imwrite(os.path.join(raiz, "Assets", "StreamingAssets", "Campus", "mapa.png"), limpio)

rgb = cv2.cvtColor(limpio, cv2.COLOR_BGR2RGB)
r = rgb[:, :, 0].astype(np.int16)
g = rgb[:, :, 1].astype(np.int16)
b = rgb[:, :, 2].astype(np.int16)
lum = (r + g + b) / 3
mx = np.maximum(np.maximum(r, g), b)
mn = np.minimum(np.minimum(r, g), b)
piso = (lum >= 145) & (lum <= 235) & ((mx - mn) < 62) & ~((g > r + 18) & (g > b + 8)) & ~((r > 210) & (b < 140))

# Huecos de puertas que el cristal tapa, para que el recorrido entre.
puertas = [
    (824, 430, 896, 468),
]
for x0, y0, x1, y1 in puertas:
    piso[y0:y1, x0:x1] = True

solido = ~piso
kernel = np.ones((3, 3), np.uint8)
solido_u8 = (solido.astype(np.uint8) * 255)
solido_u8 = cv2.dilate(solido_u8, kernel, iterations=1)
solido = solido_u8 > 0
for x0, y0, x1, y1 in puertas:
    solido[y0:y1, x0:x1] = False

paso = 4
filas = alto // paso
columnas = ancho // paso
celdas = np.zeros((filas, columnas), np.uint8)
for y in range(filas):
    for x in range(columnas):
        bloque = solido[y * paso:(y + 1) * paso, x * paso:(x + 1) * paso]
        if bloque.mean() > 0.45:
            celdas[y, x] = 1

# Une celdas sólidas en franjas horizontales.
rectos = []
visitado = np.zeros_like(celdas)
for y in range(filas):
    x = 0
    while x < columnas:
        if celdas[y, x] == 0 or visitado[y, x]:
            x += 1
            continue
        x1 = x
        while x1 < columnas and celdas[y, x1] == 1 and not visitado[y, x1]:
            x1 += 1
        alto_franja = 1
        while y + alto_franja < filas:
            if np.all(celdas[y + alto_franja, x:x1] == 1) and np.all(visitado[y + alto_franja, x:x1] == 0):
                alto_franja += 1
            else:
                break
        visitado[y:y + alto_franja, x:x1] = 1
        rectos.append((x * paso, y * paso, (x1 - x) * paso, alto_franja * paso))
        x = x1

salida = os.path.join(raiz, "Assets", "StreamingAssets", "Campus", "colision.txt")
with open(salida, "w", encoding="utf-8") as archivo:
    archivo.write(f"{ancho} {alto}\n")
    for x, y, w, h in rectos:
        archivo.write(f"{x} {y} {w} {h}\n")

vista = limpio.copy()
vista[solido] = (vista[solido] * 0.35 + np.array([0, 40, 0])).astype(np.uint8)
cv2.imwrite(os.path.join(raiz, "Assets", "Art", "_mask.png"), vista)
print("personas", len(personas), "rectangulos", len(rectos), "solido", int(solido.mean() * 100))
