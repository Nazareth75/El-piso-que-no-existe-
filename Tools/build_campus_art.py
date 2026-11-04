"""Fondos laterales del campus UNIFRANZ La Paz.

Misma medida de antes: 450 x 136 px de arte, x4, PPU 100.
La recepción copia el recorrido real: sillas, muro de colores,
mostrador turquesa, ascensor y escalera de vidrio.
"""

import uuid
from pathlib import Path
from PIL import Image

SCALE = 4
ART_W = 450
ART_H = 136
FLOOR = 16
DOOR_H = 68
CEILING = 120

ROOT = Path(__file__).resolve().parents[1]
FONDOS = ROOT / "Assets" / "Art" / "Campus" / "Descubre" / "Fondos"
PUNTOS = ROOT / "Assets" / "Art" / "Campus" / "Descubre" / "Puntos"

INK = (32, 36, 42)
WHITE = (248, 249, 250)
WALL = (236, 238, 240)
WALL_DK = (214, 218, 222)
TEAL = (18, 168, 154)
TEAL_DK = (10, 118, 110)
METAL = (150, 156, 164)
METAL_DK = (78, 84, 92)
GLASS = (176, 214, 220)
LED = (255, 252, 236)
TILE = (228, 232, 234)
GROUT = (186, 194, 198)
PLANT = (46, 140, 72)
PLANT_DK = (28, 96, 52)
WOOD = (150, 108, 70)
ORANGE = (228, 87, 46)

SPECTRUM = [
    (176, 32, 48), (214, 70, 36), (230, 126, 32), (236, 190, 48),
    (150, 190, 48), (36, 168, 96), (28, 168, 158), (36, 150, 186),
    (40, 108, 196), (78, 72, 186), (132, 58, 176),
]

GLYPH = {
    "A": ["010", "101", "111", "101", "101"],
    "B": ["110", "101", "110", "101", "110"],
    "C": ["011", "100", "100", "100", "011"],
    "D": ["110", "101", "101", "101", "110"],
    "E": ["111", "100", "110", "100", "111"],
    "F": ["111", "100", "110", "100", "100"],
    "G": ["011", "100", "101", "101", "011"],
    "H": ["101", "101", "111", "101", "101"],
    "I": ["111", "010", "010", "010", "111"],
    "J": ["001", "001", "001", "101", "010"],
    "K": ["101", "110", "100", "110", "101"],
    "L": ["100", "100", "100", "100", "111"],
    "M": ["101", "111", "111", "101", "101"],
    "N": ["101", "111", "111", "101", "101"],
    "O": ["010", "101", "101", "101", "010"],
    "P": ["110", "101", "110", "100", "100"],
    "R": ["110", "101", "110", "101", "101"],
    "S": ["011", "100", "010", "001", "110"],
    "T": ["111", "010", "010", "010", "010"],
    "U": ["101", "101", "101", "101", "111"],
    "V": ["101", "101", "101", "010", "010"],
    "W": ["101", "101", "111", "111", "101"],
    "Y": ["101", "101", "010", "010", "010"],
    "Z": ["111", "001", "010", "100", "111"],
    "1": ["010", "110", "010", "010", "111"],
    "2": ["111", "001", "111", "100", "111"],
    "4": ["101", "101", "111", "001", "001"],
    "5": ["111", "100", "111", "001", "111"],
    " ": ["000", "000", "000", "000", "000"],
}


class Art:
    def __init__(self):
        self.px = [[None for _ in range(ART_W)] for _ in range(ART_H)]

    def put(self, x, y, color):
        if color is None or not (0 <= x < ART_W and 0 <= y < ART_H):
            return
        self.px[y][x] = color

    def rect(self, x, y, w, h, color):
        x, y, w, h = int(x), int(y), int(w), int(h)
        for yy in range(y, y + h):
            row = self.px[yy] if 0 <= yy < ART_H else None
            if row is None:
                continue
            for xx in range(x, x + w):
                if 0 <= xx < ART_W:
                    row[xx] = color

    def frame(self, x, y, w, h, color):
        self.rect(x, y, w, 1, color)
        self.rect(x, y + h - 1, w, 1, color)
        self.rect(x, y, 1, h, color)
        self.rect(x + w - 1, y, 1, h, color)

    def text(self, x, y, word, color, scale=2):
        cx = int(x)
        for ch in word:
            glyph = GLYPH.get(ch, GLYPH[" "])
            for row, bits in enumerate(glyph):
                for col, bit in enumerate(bits):
                    if bit == "1":
                        self.rect(cx + col * scale, y + (4 - row) * scale, scale, scale, color)
            cx += 4 * scale

    def save(self, path):
        img = Image.new("RGBA", (ART_W * SCALE, ART_H * SCALE), (0, 0, 0, 0))
        pix = img.load()
        for y in range(ART_H):
            for x in range(ART_W):
                color = self.px[y][x]
                if color is None:
                    continue
                r, g, b = color
                py = (ART_H - 1 - y) * SCALE
                px = x * SCALE
                for sy in range(SCALE):
                    for sx in range(SCALE):
                        pix[px + sx, py + sy] = (r, g, b, 255)
        path.parent.mkdir(parents=True, exist_ok=True)
        img.save(path)


def campus(a):
    a.rect(0, FLOOR, ART_W, CEILING - FLOOR, WALL)
    a.rect(0, CEILING, ART_W, ART_H - CEILING, (224, 226, 228))
    a.rect(20, CEILING + 3, ART_W - 40, 3, LED)
    a.rect(20, CEILING + 8, ART_W - 40, 2, LED)
    a.rect(0, 0, ART_W, FLOOR, TILE)
    for x in range(0, ART_W, 22):
        a.rect(x, 0, 1, FLOOR - 1, GROUT)
    a.rect(0, FLOOR - 2, ART_W, 2, WHITE)


def sign(a, x, y, word):
    a.text(x, y, word, INK, 2)
    width = len(word) * 8
    a.rect(x, y - 3, width, 2, ORANGE)


def plant(a, x, y):
    a.rect(x + 8, y, 12, 8, (110, 74, 48))
    a.rect(x, y + 8, 28, 14, PLANT_DK)
    a.rect(x + 4, y + 18, 22, 16, PLANT)
    a.rect(x + 8, y + 30, 14, 12, (78, 176, 96))


def mesh_chair(a, x, y):
    a.rect(x + 2, y, 2, 12, METAL_DK)
    a.rect(x + 16, y, 2, 9, METAL_DK)
    a.rect(x, y + 9, 20, 3, METAL)
    a.rect(x + 15, y + 12, 2, 16, METAL_DK)
    for i in range(4):
        a.rect(x + 4, y + 15 + i * 3, 10, 1, (186, 192, 198))


def teal_desk(a, x, y, w=100):
    a.rect(x, y, w, 28, TEAL)
    a.rect(x, y + 24, w, 4, TEAL_DK)
    a.rect(x + 4, y + 28, w - 8, 3, WHITE)
    a.rect(x + 10, y + 8, 16, 8, WHITE)


def rainbow(a, x, y, h, count=24):
    for i in range(count):
        a.rect(x + i * 5, y, 3, h, SPECTRUM[i % len(SPECTRUM)])


def elevator(a, x, y=FLOOR, w=36):
    a.rect(x - 2, y, w + 4, DOOR_H + 2, (196, 200, 204))
    a.rect(x, y, w // 2 - 1, DOOR_H, (220, 224, 226))
    a.rect(x + w // 2, y, w // 2, DOOR_H, (186, 192, 198))
    a.rect(x + w // 2 - 1, y, 2, DOOR_H, (120, 126, 132))
    a.rect(x + w + 3, y + 28, 6, 14, (48, 52, 58))
    a.rect(x + w + 4, y + 34, 4, 4, (250, 206, 64))
    a.text(x - 4, y + DOOR_H + 4, "ASCENSOR", INK, 1)


def glass_stairs(a, x, y=FLOOR, steps=7):
    for i in range(steps):
        sx = x + i * 11
        sy = y + i * 8
        a.rect(sx, sy, 13, 8, (232, 234, 236))
        a.rect(sx, sy + 6, 13, 2, WHITE)
        a.rect(sx + 2, sy + 8, 2, 16, GLASS)
    for i in range(steps * 11):
        ry = y + 16 + int(i * 8 / 11)
        a.put(x + i, ry, METAL)
        a.put(x + i, ry + 1, METAL)


def door(a, x, y, w, h, label=None):
    a.rect(x, y, w, h, WHITE)
    a.frame(x, y, w, h, METAL_DK)
    a.rect(x + 3, y + 3, w - 6, h - 6, GLASS)
    a.rect(x + w // 2, y + 3, 1, h - 6, WHITE)
    a.rect(x + w - 7, y + 28, 2, 5, ORANGE)
    if label:
        a.text(x, y + h + 3, label, INK, 1)


def entrada():
    a = Art()
    for y in range(FLOOR, ART_H):
        t = (y - FLOOR) / (ART_H - FLOOR)
        color = tuple(int((186 + (96 - 186) * t) if i == 0 else (214 + (156 - 214) * t) if i == 1 else (228 + (196 - 228) * t)) for i in range(3))
        a.rect(0, y, ART_W, 1, color)
    for i, h in enumerate((48, 62, 80, 70, 54, 66)):
        a.rect(i * 36, FLOOR, 40, h, (132, 118, 110))
        a.rect(i * 36 + 10, FLOOR + h - 8, 16, 8, WHITE)
    a.rect(150, FLOOR, 250, 100, WALL)
    a.frame(150, FLOOR, 250, 100, INK)
    a.rect(160, 50, 70, 48, (120, 156, 168))
    for k in range(4):
        a.rect(166, 56 + k * 10, 58, 2, WHITE)
    a.text(166, 92, "UNIFRANZ", INK, 2)
    a.rect(166, 86, 64, 3, ORANGE)
    door(a, 268, FLOOR, 70, DOOR_H)
    a.rect(0, 0, ART_W, FLOOR, (196, 190, 180))
    for x in range(0, ART_W, 18):
        a.rect(x, 2, 8, FLOOR - 4, (176, 170, 160))
    a.rect(0, 0, ART_W, 4, (82, 86, 92))
    plant(a, 78, FLOOR)
    return a


def recepcion():
    a = Art()
    campus(a)
    plant(a, 40, FLOOR)
    for i in range(4):
        mesh_chair(a, 78 + i * 24, FLOOR)
    rainbow(a, 188, FLOOR + 8, 88, 22)
    teal_desk(a, 198, FLOOR, 96)
    sign(a, 200, 108, "RECEPCION")
    elevator(a, 318)
    glass_stairs(a, 368)
    return a


def hall():
    a = Art()
    campus(a)
    sign(a, 170, 108, "HALL")
    plant(a, 40, FLOOR)
    for x, label in ((70, "ADMISION"), (156, "CLINICA"), (250, "CAFE"), (330, "AUDITORIO")):
        door(a, x, FLOOR, 34, DOOR_H, label)
    return a


def escaleras():
    a = Art()
    campus(a)
    sign(a, 150, 108, "ESCALERAS")
    glass_stairs(a, 150, FLOOR, 12)
    door(a, 300, FLOOR + 56, 40, 40, "1")
    return a


def pasillo():
    a = Art()
    campus(a)
    elevator(a, 36)
    sign(a, 160, 108, "PISO 1")
    glass_stairs(a, 332, FLOOR, 4)
    for x in (120, 180, 240, 300):
        door(a, x, FLOOR, 30, DOOR_H)
    a.rect(392, FLOOR, 36, DOOR_H, ORANGE)
    a.rect(396, FLOOR + 3, 28, DOOR_H - 6, GLASS)
    a.text(394, FLOOR + DOOR_H + 3, "LAB", INK, 1)
    return a


def laboratorio():
    a = Art()
    campus(a)
    sign(a, 140, 108, "HISTOLOGIA")
    for i in range(6):
        x = 90 + i * 42
        a.rect(x, FLOOR, 36, 52, WHITE)
        a.frame(x, FLOOR, 36, 52, METAL)
        a.rect(x + 3, FLOOR + 14, 30, 1, METAL)
        a.rect(x + 3, FLOOR + 30, 30, 1, METAL)
        a.rect(x + 6, FLOOR + 4, 8, 6, ORANGE)
        a.rect(x + 18, FLOOR + 18, 8, 6, (48, 110, 180))
        a.rect(x + 8, FLOOR + 34, 8, 6, (180, 60, 56))
    a.rect(360, FLOOR + 36, 28, 36, WHITE)
    a.frame(360, FLOOR + 36, 28, 36, INK)
    a.rect(364, FLOOR + 58, 20, 8, ORANGE)
    return a


def subsuelo():
    a = Art()
    campus(a)
    elevator(a, 36)
    sign(a, 160, 108, "OSTEOTECA")
    for i in range(4):
        x = 140 + i * 58
        a.rect(x, FLOOR, 48, 70, (232, 234, 236))
        a.frame(x, FLOOR, 48, 70, METAL_DK)
        a.rect(x + 16, FLOOR + 8, 16, 12, WHITE)
        a.rect(x + 10, FLOOR + 22, 28, 8, (236, 232, 224))
        a.rect(x + 18, FLOOR + 34, 8, 18, WHITE)
        a.rect(x + 8, FLOOR + 48, 10, 6, (230, 226, 218))
        a.rect(x + 28, FLOOR + 48, 10, 6, (230, 226, 218))
    glass_stairs(a, 380, FLOOR, 5)
    return a


def mezanine():
    a = Art()
    campus(a)
    elevator(a, 36)
    sign(a, 150, 108, "CAJAS")
    a.rect(140, FLOOR, 180, 32, (210, 214, 218))
    a.frame(140, FLOOR, 180, 32, METAL)
    a.rect(150, FLOOR + 8, 40, 10, WHITE)
    a.rect(210, FLOOR + 8, 40, 10, WHITE)
    a.rect(270, FLOOR + 8, 40, 10, WHITE)
    a.text(150, FLOOR + 40, "POSTGRADO", INK, 1)
    glass_stairs(a, 340, FLOOR, 6)
    return a


def piso3():
    a = Art()
    campus(a)
    elevator(a, 36)
    sign(a, 150, 108, "PISO 3")
    for i in range(4):
        x = 120 + i * 58
        a.rect(x, FLOOR, 40, 14, WOOD)
        a.rect(x + 6, FLOOR + 14, 4, 10, (110, 78, 48))
        a.rect(x + 30, FLOOR + 14, 4, 10, (110, 78, 48))
        mesh_chair(a, x + 8, FLOOR + 24)
    glass_stairs(a, 360, FLOOR, 6)
    return a


def piso2():
    a = Art()
    campus(a)
    elevator(a, 36)
    sign(a, 160, 108, "SISTEMAS")
    glass_stairs(a, 360, FLOOR, 6)
    for i in range(5):
        x = 130 + i * 52
        a.rect(x, FLOOR, 40, 22, METAL_DK)
        a.rect(x + 4, FLOOR + 22, 32, 20, (36, 42, 54))
        a.rect(x + 8, FLOOR + 26, 24, 12, (120, 190, 210))
        a.rect(x + 14, FLOOR + 8, 12, 4, METAL)
    return a


def piso4():
    a = Art()
    campus(a)
    elevator(a, 36)
    sign(a, 150, 108, "JURIDICA")
    a.rect(160, FLOOR, 200, 16, WOOD)
    a.rect(170, FLOOR + 16, 180, 10, (120, 82, 48))
    a.rect(230, FLOOR + 30, 8, 28, WOOD)
    a.rect(250, FLOOR + 48, 28, 6, WOOD)
    a.rect(262, FLOOR + 36, 4, 14, METAL)
    a.rect(248, FLOOR + 34, 12, 4, METAL)
    a.rect(272, FLOOR + 34, 12, 4, METAL)
    glass_stairs(a, 360, FLOOR, 6)
    return a


def piso5():
    a = Art()
    campus(a)
    elevator(a, 36)
    sign(a, 150, 108, "BIBLIOTECA")
    for i in range(6):
        x = 140 + i * 40
        a.rect(x, FLOOR, 34, 78, (92, 64, 46))
        for row, color in enumerate(((180, 60, 50), (40, 90, 160), (40, 130, 70), (200, 160, 50), (120, 50, 90))):
            a.rect(x + 3, FLOOR + 6 + row * 14, 28, 10, color)
    glass_stairs(a, 390, FLOOR, 4)
    return a


def admisiones():
    a = Art()
    campus(a)
    sign(a, 140, 108, "ADMISIONES")
    teal_desk(a, 140, FLOOR, 180)
    for i in range(3):
        mesh_chair(a, 150 + i * 28, FLOOR + 32)
    return a


def clinica():
    a = Art()
    campus(a)
    sign(a, 140, 108, "ODONTOLOGIA")
    a.rect(160, FLOOR, 70, 16, (210, 230, 232))
    a.rect(170, FLOOR + 16, 40, 10, WHITE)
    a.rect(200, FLOOR + 26, 16, 28, METAL)
    a.rect(190, FLOOR + 50, 36, 8, (180, 220, 224))
    a.rect(250, FLOOR, 8, 70, METAL)
    a.rect(230, FLOOR + 62, 48, 6, WHITE)
    return a


def cafeteria():
    a = Art()
    campus(a)
    sign(a, 160, 108, "CAFETERIA")
    a.rect(80, FLOOR, 120, 30, (120, 72, 48))
    a.rect(86, FLOOR + 24, 108, 4, (90, 54, 36))
    for i in range(3):
        a.rect(100 + i * 28, FLOOR + 28, 10, 8, WHITE)
    for i in range(3):
        a.rect(230 + i * 50, FLOOR, 36, 16, WOOD)
        mesh_chair(a, 236 + i * 50, FLOOR + 16)
    return a


def auditorio():
    a = Art()
    campus(a)
    sign(a, 160, 108, "AUDITORIO")
    a.rect(300, FLOOR, 120, 8, WOOD)
    a.rect(320, FLOOR + 8, 80, 40, (40, 44, 52))
    for row in range(4):
        for col in range(6):
            a.rect(40 + col * 28, FLOOR + row * 16, 22, 10, (70, 78, 92))
    return a


def main():
    fondos = {
        "entrada_principal.png": entrada(),
        "recepcion.png": recepcion(),
        "hall_principal.png": hall(),
        "escaleras.png": escaleras(),
        "pasillo_piso1.png": pasillo(),
        "lab_histologia.png": laboratorio(),
        "subsuelo.png": subsuelo(),
        "mezanine.png": mezanine(),
        "piso2.png": piso2(),
        "piso3.png": piso3(),
        "piso4.png": piso4(),
        "piso5.png": piso5(),
        "admisiones.png": admisiones(),
        "clinica.png": clinica(),
        "cafeteria.png": cafeteria(),
        "auditorio.png": auditorio(),
    }
    for name, art in fondos.items():
        path = FONDOS / name
        art.save(path)
        ensure_meta(path)
        print("fondo", name)


def ensure_meta(png_path):
    meta_path = Path(str(png_path) + ".meta")
    if meta_path.exists():
        return
    guid = uuid.uuid4().hex
    meta_path.write_text(f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites:
    - serializedVersion: 2
      name: {png_path.stem}_0
      rect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 1800
        height: 544
      alignment: 0
      pivot: {{x: 0.5, y: 0.5}}
      border: {{x: 0, y: 0, z: 0, w: 0}}
      customData: 
      outline: []
      physicsShape: []
      tessellationDetail: -1
      bones: []
      spriteID: {uuid.uuid4().hex[:16]}0000000000000000
      internalID: 21300000
      vertices: []
      indices: 
      edges: []
      weights: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable:
      {png_path.stem}_0: 21300000
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
""", encoding="utf-8", newline="\n")
    print("meta", png_path.name, guid)


if __name__ == "__main__":
    main()
