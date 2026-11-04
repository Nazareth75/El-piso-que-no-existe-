"""Extrae los frames de un .ase de Aseprite a PNG."""

import struct
import zlib
from pathlib import Path

from PIL import Image

ASE = Path(r"C:\Users\Kathlen\Downloads\personaje_caminando_10frames.ase")
OUT = Path(r"C:\Users\Kathlen\Piso7-2D\Assets\Resources\Player\Personaje2")


def read_string(data, i):
    (n,) = struct.unpack_from("<H", data, i)
    i += 2
    text = data[i : i + n].decode("utf-8", "replace")
    return text, i + n


def main():
    data = ASE.read_bytes()
    size, magic, frames, width, height, depth = struct.unpack_from("<IHHHHH", data, 0)
    print(f"size={size} magic={magic:#x} frames={frames} {width}x{height} depth={depth}")
    if magic != 0xA5E0:
        raise SystemExit("no es un aseprite")

    pos = 128
    images = []
    palette = [(0, 0, 0, 0)] * 256

    for frame_i in range(frames):
        frame_size, frame_magic, old_chunks, duration = struct.unpack_from("<IHHI", data, pos)
        if frame_magic != 0xF1FA:
            raise SystemExit(f"frame {frame_i} magic {frame_magic:#x}")
        new_chunks = struct.unpack_from("<I", data, pos + 12)[0]
        chunks = new_chunks or old_chunks
        cursor = pos + 16
        cels = []
        for _ in range(chunks):
            chunk_size, chunk_type = struct.unpack_from("<IH", data, cursor)
            body = data[cursor + 6 : cursor + chunk_size]
            if chunk_type == 0x2019:
                parse_palette(body, palette)
            elif chunk_type == 0x2005:
                cels.append(parse_cel(body, depth, palette))
            cursor += chunk_size
        canvas = Image.new("RGBA", (width, height), (0, 0, 0, 0))
        for cel in cels:
            if cel is None:
                continue
            img, x, y = cel
            canvas.alpha_composite(img, (x, y))
        images.append(canvas)
        print(f"frame {frame_i} cels={len(cels)} duration={duration}")
        pos += frame_size

    OUT.mkdir(parents=True, exist_ok=True)
    # Recorte común para que la animación no salte.
    boxes = [im.getbbox() for im in images]
    boxes = [b for b in boxes if b]
    left = min(b[0] for b in boxes)
    top = min(b[1] for b in boxes)
    right = max(b[2] for b in boxes)
    bottom = max(b[3] for b in boxes)
    print(f"trim {left},{top},{right},{bottom} -> {right-left}x{bottom-top}")
    for i, im in enumerate(images):
        cropped = im.crop((left, top, right, bottom))
        path = OUT / f"frame_{i:02d}.png"
        cropped.save(path)
        print("saved", path.name, cropped.size)


def parse_palette(body, palette):
    _size, first, last = struct.unpack_from("<III", body, 0)
    i = 16
    for index in range(first, last + 1):
        if i + 2 > len(body):
            break
        flags = struct.unpack_from("<H", body, i)[0]
        i += 2
        r, g, b, a = body[i : i + 4]
        i += 4
        if flags & 1:
            _, i = read_string(body, i)
        if index < len(palette):
            palette[index] = (r, g, b, a)


def parse_cel(body, depth, palette):
    layer, x, y = struct.unpack_from("<Hhh", body, 0)
    cel_type = struct.unpack_from("<H", body, 7)[0]
    payload = body[16:]
    if cel_type == 1:
        print("  linked cel", layer)
        return None
    if cel_type == 0:
        w, h = struct.unpack_from("<HH", payload, 0)
        raw = payload[4:]
    elif cel_type == 2:
        w, h = struct.unpack_from("<HH", payload, 0)
        raw = zlib.decompress(payload[4:])
    else:
        print("  cel type", cel_type)
        return None
    img = pixels_to_image(raw, w, h, depth, palette)
    return img, x, y


def pixels_to_image(raw, w, h, depth, palette):
    img = Image.new("RGBA", (w, h))
    px = img.load()
    if depth == 32:
        step = 4
        for i in range(w * h):
            o = i * step
            if o + 4 > len(raw):
                break
            px[i % w, i // w] = tuple(raw[o : o + 4])
    elif depth == 8:
        for i, index in enumerate(raw[: w * h]):
            px[i % w, i // w] = palette[index]
    elif depth == 16:
        for i in range(w * h):
            v, a = raw[i * 2], raw[i * 2 + 1]
            px[i % w, i // w] = (v, v, v, a)
    return img


if __name__ == "__main__":
    main()
