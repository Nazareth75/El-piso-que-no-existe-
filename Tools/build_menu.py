"""Menú pixel art con paleta noche/arena (no el verde de la referencia) y sonidos."""

import math
import struct
import uuid
import wave
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets" / "Resources" / "Menu"

NAVY = (18, 36, 58, 255)
NAVY_DOT = (32, 58, 88, 255)
SAND = (232, 196, 148, 255)
SAND_DK = (196, 148, 96, 255)
TERRA = (166, 84, 52, 255)
MORTAR = (74, 46, 38, 255)
CREAM = (255, 244, 224, 255)
CREAM_HI = (255, 250, 240, 255)
LIP = (122, 72, 48, 255)
INK = (22, 36, 64, 255)
GOLD = (232, 176, 72, 255)
AMBER = (196, 112, 48, 255)
LEAF = (214, 146, 64, 255)

FONT = {
    "A": ["01110", "10001", "10001", "11111", "10001", "10001", "10001"],
    "C": ["01110", "10001", "10000", "10000", "10000", "10001", "01110"],
    "D": ["11110", "10001", "10001", "10001", "10001", "10001", "11110"],
    "F": ["11111", "10000", "10000", "11110", "10000", "10000", "10000"],
    "L": ["10000", "10000", "10000", "10000", "10000", "10000", "11111"],
    "Q": ["01110", "10001", "10001", "10001", "10101", "10010", "01101"],
    "T": ["11111", "00100", "00100", "00100", "00100", "00100", "00100"],
    "X": ["10001", "10001", "01010", "00100", "01010", "10001", "10001"],
    "Z": ["11111", "00001", "00010", "00100", "01000", "10000", "11111"],
    "E": ["11111", "10000", "10000", "11110", "10000", "10000", "11111"],
    "G": ["01110", "10001", "10000", "10111", "10001", "10001", "01110"],
    "I": ["11111", "00100", "00100", "00100", "00100", "00100", "11111"],
    "J": ["00111", "00010", "00010", "00010", "10010", "10010", "01100"],
    "N": ["10001", "11001", "10101", "10011", "10001", "10001", "10001"],
    "O": ["01110", "10001", "10001", "10001", "10001", "10001", "01110"],
    "P": ["11110", "10001", "10001", "11110", "10000", "10000", "10000"],
    "R": ["11110", "10001", "10001", "11110", "10100", "10010", "10001"],
    "S": ["01111", "10000", "10000", "01110", "00001", "00001", "11110"],
    "U": ["10001", "10001", "10001", "10001", "10001", "10001", "01110"],
    " ": ["00000", "00000", "00000", "00000", "00000", "00000", "00000"],
}


def text_width(text, scale):
    return len(text) * 6 * scale - scale


def draw_text(img, text, cx, cy, color, scale=2):
    x = int(cx - text_width(text, scale) / 2)
    y = int(cy - 7 * scale / 2)
    for ch in text:
        glyph = FONT[ch]
        for gy, row in enumerate(glyph):
            for gx, bit in enumerate(row):
                if bit != "1":
                    continue
                for sy in range(scale):
                    for sx in range(scale):
                        px = x + gx * scale + sx
                        py = y + gy * scale + sy
                        if 0 <= px < img.width and 0 <= py < img.height:
                            img.putpixel((px, py), color)
        x += 6 * scale


def rounded_cut(px, py, w, h):
    steps = (4, 2, 1)
    for i, inset in enumerate(steps):
        if py == i or py == h - 1 - i:
            if px < inset or px >= w - inset:
                return True
    return False


def draw_button(w, h, pressed=False):
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    lip = 3 if not pressed else 1
    face_h = h - lip
    for y in range(h):
        for x in range(w):
            if rounded_cut(x, y, w, h):
                continue
            if y >= face_h:
                img.putpixel((x, y), LIP)
                continue
            yy = y + (2 if pressed else 0)
            if yy <= 2:
                color = CREAM_HI
            else:
                color = CREAM
            if x < 2 or yy > face_h - 3:
                color = SAND
            img.putpixel((x, y), color)
    return img


def blit(dst, src, x, y):
    dst.alpha_composite(src, (x, y))


def icon_home():
    img = Image.new("RGBA", (22, 18), (0, 0, 0, 0))

    def p(x, y):
        if 0 <= x < img.width and 0 <= y < img.height:
            img.putpixel((x, y), INK)

    for y in range(8):
        p(11 - y, y)
        p(10 - y, y)
        p(11 + y, y)
        p(12 + y, y)
    for x in range(3, 20):
        p(x, 7)
        p(x, 8)
    for y in range(8, 17):
        p(5, y)
        p(6, y)
        p(15, y)
        p(16, y)
    for x in range(5, 17):
        p(x, 16)
    for y in range(11, 17):
        p(9, y)
        p(12, y)
    for x in range(9, 13):
        p(x, 11)
    return img


def icon_mail():
    img = Image.new("RGBA", (22, 16), (0, 0, 0, 0))
    px = img.putpixel
    for x in range(2, 20):
        px((x, 2), INK)
        px((x, 13), INK)
    for y in range(2, 14):
        px((2, y), INK)
        px((19, y), INK)
    for i in range(8):
        px((3 + i, 3 + i), INK)
        px((18 - i, 3 + i), INK)
    return img


def icon_exit():
    img = Image.new("RGBA", (22, 16), (0, 0, 0, 0))
    px = img.putpixel
    for y in range(3, 13):
        px((3, y), INK)
        px((4, y), INK)
    for x in range(3, 12):
        px((x, 3), INK)
        px((x, 12), INK)
    for x in range(8, 18):
        px((x, 7), INK)
        px((x, 8), INK)
    for i in range(5):
        px((14 + i, 4 + i), INK)
        px((14 + i, 11 - i), INK)
    return img


def paint_icon_button(icon, pressed=False):
    base = draw_button(52, 44, pressed)
    iw, ih = icon.size
    blit(base, icon, (52 - iw) // 2, (40 - ih) // 2 + (1 if pressed else 0))
    return base


def draw_background():
    w, h = 480, 270
    img = Image.new("RGBA", (w, h), NAVY)
    for y in range(h):
        for x in range(w):
            if (x + y) % 2 == 0 and (x // 2 + y // 3) % 3 == 0:
                img.putpixel((x, y), NAVY_DOT)
            elif (x * 3 + y) % 17 == 0:
                img.putpixel((x, y), (24, 46, 72, 255))

    thick = 18
    for y in range(h):
        for x in range(w):
            if not (x < thick or y < thick or x >= w - thick or y >= h - thick):
                continue
            img.putpixel((x, y), MORTAR)

    brick_h = 9
    row = 0
    y = 0
    while y < h:
        offset = 0 if row % 2 == 0 else 14
        x = -offset
        while x < w:
            bw = 22 + ((row * 3 + x // 7) % 3) * 6
            for by in range(y + 1, min(h, y + brick_h - 1)):
                for bx in range(max(0, x + 1), min(w, x + bw - 1)):
                    if not (bx < thick or by < thick or bx >= w - thick or by >= h - thick):
                        continue
                    band = (bx // 5 + by) % 4
                    color = SAND if band != 0 else SAND_DK
                    if by == y + 1:
                        color = (244, 214, 170, 255)
                    img.putpixel((bx, by), color)
            x += bw
        y += brick_h
        row += 1

    for i in range(thick, w - thick):
        img.putpixel((i, thick), TERRA)
        img.putpixel((i, h - thick - 1), TERRA)
    for i in range(thick, h - thick):
        img.putpixel((thick, i), TERRA)
        img.putpixel((w - thick - 1, i), TERRA)

    hang_vines(img)
    draw_text(img, "UNIFRANZ", w // 2, 42, CREAM, 3)
    draw_text(img, "EL PISO QUE NO EXISTE", w // 2, 68, GOLD, 1)
    return img


def hang_vines(img):
    w, _ = img.size
    specs = [
        (36, 54, 1),
        (58, 36, -1),
        (92, 48, 1),
        (390, 58, -1),
        (424, 34, 1),
        (448, 46, -1),
        (150, 28, 1),
        (330, 30, -1),
    ]
    for x0, length, drift in specs:
        x = x0
        y = 18
        for i in range(length):
            y += 1
            if i % 6 == 0:
                x += drift
            if 20 < x < w - 20 and y < img.height - 24:
                img.putpixel((x, y), AMBER)
                img.putpixel((x + 1, y), GOLD)
            if i % 5 == 3 and 20 < x < w - 24:
                leaf = LEAF if i % 2 == 0 else GOLD
                for lx, ly in ((2, 0), (3, 1), (2, 2), (4, 1), (-2, 1), (-3, 2)):
                    px, py = x + lx * drift, y + ly
                    if 20 < px < w - 20 and 18 < py < img.height - 24:
                        img.putpixel((px, py), leaf)


def draw_panel():
    w, h = 280, 168
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    for y in range(h):
        for x in range(w):
            if rounded_cut(x, y, w, h):
                continue
            if x < 3 or y < 3 or x >= w - 3 or y >= h - 3:
                img.putpixel((x, y), LIP)
            else:
                img.putpixel((x, y), CREAM)
    return img


def meta_sprite(path, guid, width, height):
    Path(str(path) + ".meta").write_text(
        f"""fileFormatVersion: 2
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
  spriteGenerateFallbackPhysicsShape: 0
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
      name: {path.stem}_0
      rect:
        serializedVersion: 2
        x: 0
        y: 0
        width: {width}
        height: {height}
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
      {path.stem}_0: 21300000
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
""",
        encoding="utf-8",
        newline="\n",
    )


def save_png(img, name):
    path = OUT / name
    img.save(path)
    meta = Path(str(path) + ".meta")
    guid = uuid.uuid4().hex
    if meta.exists():
        for line in meta.read_text(encoding="utf-8").splitlines():
            if line.startswith("guid:"):
                guid = line.split(":", 1)[1].strip()
    meta_sprite(path, guid, img.width, img.height)
    return guid


def env_sample(i, n):
    a = min(1.0, i / 80.0)
    b = min(1.0, (n - i) / 200.0)
    return a * b


def mix_tone(buffer, freq, start, dur, rate, vol, kind):
    n = int(rate * dur)
    s0 = int(start * rate)
    for i in range(n):
        idx = s0 + i
        if idx >= len(buffer):
            break
        t = i / rate
        wave_s = math.sin(2 * math.pi * freq * t)
        if kind == "soft":
            wave_s = 0.75 * wave_s + 0.25 * math.sin(2 * math.pi * freq * 2 * t)
        sample = wave_s * vol * env_sample(i, n)
        buffer[idx] += sample


def midi(note):
    return 440.0 * (2 ** ((note - 69) / 12))


def build_music(path, melody, bass, beat=0.42, vol=0.18):
    rate = 22050
    length = beat * max(sum(d for _, d in melody), sum(d for _, d in bass))
    buffer = [0.0] * int(rate * (length + 0.05))
    t = 0.0
    for note, dur in bass:
        mix_tone(buffer, midi(note), t, beat * dur, rate, vol * 0.55, "sine")
        t += beat * dur
    t = 0.0
    for note, dur in melody:
        if note is not None:
            mix_tone(buffer, midi(note), t, beat * dur * 0.92, rate, vol, "soft")
        t += beat * dur
    peak = max(1e-6, max(abs(s) for s in buffer))
    scale = min(1.0, 0.9 / peak)
    samples = [max(-1, min(1, s * scale)) for s in buffer]
    write_wav(path, samples, rate)


def write_wav(path, samples, rate):
    with wave.open(str(path), "w") as handle:
        handle.setnchannels(1)
        handle.setsampwidth(2)
        handle.setframerate(rate)
        frames = b"".join(
            struct.pack("<h", max(-32767, min(32767, int(s * 32767)))) for s in samples
        )
        handle.writeframes(frames)
    meta = Path(str(path) + ".meta")
    if meta.exists():
        return
    meta.write_text(
        f"""fileFormatVersion: 2
guid: {uuid.uuid4().hex}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 7
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 1
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {{}}
  forceToMono: 1
  normalize: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
""",
        encoding="utf-8",
        newline="\n",
    )


def blip(path, freq, dur, vol):
    rate = 22050
    n = int(rate * dur)
    samples = []
    for i in range(n):
        t = i / rate
        env = (1 - i / n) ** 2
        samples.append(math.sin(2 * math.pi * freq * t) * vol * env)
    write_wav(path, samples, rate)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    fondo = draw_background()
    fondo_guid = None

    def labeled(text, scale, pressed=False):
        img = draw_button(168, 52, pressed)
        draw_text(img, text, 84, 24 + (2 if pressed else 0), INK, scale)
        return img

    save_png(labeled("JUGAR", 3), "boton_jugar.png")
    save_png(labeled("PAUSA", 3), "boton_pausa.png")
    save_png(labeled("OPCIONES", 2), "boton_opciones.png")
    save_png(labeled("PERSONAJE", 2), "boton_personaje.png")
    save_png(labeled("JUGAR", 3, True), "boton_jugar_pressed.png")
    save_png(labeled("PAUSA", 3, True), "boton_pausa_pressed.png")
    save_png(labeled("OPCIONES", 2, True), "boton_opciones_pressed.png")
    save_png(labeled("PERSONAJE", 2, True), "boton_personaje_pressed.png")
    save_png(draw_button(168, 52), "boton_vacio.png")
    save_png(draw_button(168, 52, True), "boton_vacio_pressed.png")

    save_png(paint_icon_button(icon_home()), "boton_casa.png")
    save_png(paint_icon_button(icon_mail()), "boton_sobre.png")
    save_png(paint_icon_button(icon_exit()), "boton_salir.png")
    save_png(paint_icon_button(icon_home(), True), "boton_casa_pressed.png")
    save_png(paint_icon_button(icon_mail(), True), "boton_sobre_pressed.png")
    save_png(paint_icon_button(icon_exit(), True), "boton_salir_pressed.png")
    save_png(draw_panel(), "panel.png")

    def stamp(src, ax, ay, w, h):
        resized = src.resize((w, h), Image.Resampling.NEAREST)
        x = int(round(ax * fondo.width - w / 2))
        y = int(round((1 - ay) * fondo.height - h / 2))
        fondo.alpha_composite(resized, (x, y))

    stamp(labeled("JUGAR", 3), 0.50, 0.64, 116, 36)
    stamp(labeled("PERSONAJE", 2), 0.50, 0.48, 116, 36)
    stamp(labeled("OPCIONES", 2), 0.50, 0.32, 116, 36)
    stamp(paint_icon_button(icon_mail()), 0.78, 0.13, 39, 33)
    stamp(paint_icon_button(icon_exit()), 0.90, 0.13, 39, 33)
    fondo_guid = save_png(fondo, "fondo.png")

    blip(OUT / "click.wav", 740, 0.07, 0.45)
    blip(OUT / "hover.wav", 520, 0.04, 0.22)
    melody = [
        (74, 1), (77, 1), (81, 1), (77, 1),
        (76, 1), (79, 1), (84, 1), (79, 1),
        (74, 1), (77, 1), (82, 1), (81, 1),
        (79, 1), (77, 1), (74, 1), (69, 1),
    ]
    bass = [(50, 4), (48, 4), (46, 4), (50, 4)]
    build_music(OUT / "musica.wav", melody, bass, 0.34, 0.2)
    campus_melody = [
        (74, 2), (None, 2), (77, 2), (None, 2),
        (72, 2), (None, 2), (69, 2), (None, 2),
    ]
    campus_bass = [(50, 4), (48, 4), (45, 4), (50, 4)]
    build_music(OUT / "campus.wav", campus_melody, campus_bass, 0.46, 0.12)
    print("fondo", fondo_guid)


if __name__ == "__main__":
    main()
