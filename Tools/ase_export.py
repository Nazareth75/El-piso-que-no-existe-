"""Export Aseprite .ase/.aseprite files to PNG frames and spritesheets."""
from __future__ import annotations

import argparse
import struct
import zlib
from pathlib import Path

from PIL import Image


CHUNK_LAYER = 0x2004
CHUNK_CEL = 0x2005
CHUNK_TAGS = 0x2018
CHUNK_PALETTE = 0x2019
CHUNK_OLD_PALETTE = 0x0004
CHUNK_OLD_PALETTE2 = 0x0011
CHUNK_COLOR_PROFILE = 0x2007


def read_string(data: bytes, offset: int) -> tuple[str, int]:
    length = struct.unpack_from("<H", data, offset)[0]
    offset += 2
    text = data[offset : offset + length].decode("utf-8", errors="replace")
    return text, offset + length


def decode_cel_pixels(
    raw: bytes,
    width: int,
    height: int,
    color_depth: int,
    palette: list[tuple[int, int, int, int]] | None,
) -> Image.Image:
    if color_depth == 32:
        expected = width * height * 4
        pixels = raw[:expected]
        img = Image.frombytes("RGBA", (width, height), pixels)
        return img
    if color_depth == 16:
        # Grayscale + alpha
        out = bytearray(width * height * 4)
        for i in range(width * height):
            v = raw[i * 2]
            a = raw[i * 2 + 1]
            o = i * 4
            out[o : o + 4] = bytes((v, v, v, a))
        return Image.frombytes("RGBA", (width, height), bytes(out))
    if color_depth == 8:
        if not palette:
            palette = [(i, i, i, 255) for i in range(256)]
        out = bytearray(width * height * 4)
        for i, idx in enumerate(raw[: width * height]):
            r, g, b, a = palette[idx]
            o = i * 4
            out[o : o + 4] = bytes((r, g, b, a))
        return Image.frombytes("RGBA", (width, height), bytes(out))
    raise ValueError(f"Unsupported color depth: {color_depth}")


def parse_ase(path: Path) -> dict:
    data = path.read_bytes()
    if len(data) < 128:
        raise ValueError("File too small")

    file_size, magic, frame_count, width, height, color_depth = struct.unpack_from(
        "<IHHHHI", data, 0
    )[:6]
    # Correct unpack:
    file_size = struct.unpack_from("<I", data, 0)[0]
    magic = struct.unpack_from("<H", data, 4)[0]
    frame_count = struct.unpack_from("<H", data, 6)[0]
    width = struct.unpack_from("<H", data, 8)[0]
    height = struct.unpack_from("<H", data, 10)[0]
    color_depth = struct.unpack_from("<H", data, 12)[0]
    flags = struct.unpack_from("<I", data, 14)[0]
    speed = struct.unpack_from("<H", data, 18)[0]

    if magic != 0xA5E0:
        raise ValueError(f"Not an ASE file (magic={hex(magic)})")

    palette: list[tuple[int, int, int, int]] = [(0, 0, 0, 0)] * 256
    layers: list[dict] = []
    tags: list[dict] = []
    frames: list[dict] = []

    pos = 128
    for frame_index in range(frame_count):
        if pos + 16 > len(data):
            break
        frame_bytes = struct.unpack_from("<I", data, pos)[0]
        frame_magic = struct.unpack_from("<H", data, pos + 4)[0]
        old_chunks = struct.unpack_from("<H", data, pos + 6)[0]
        duration = struct.unpack_from("<H", data, pos + 8)[0]
        new_chunks = struct.unpack_from("<I", data, pos + 12)[0]
        chunk_count = new_chunks if old_chunks == 0xFFFF else old_chunks
        if frame_magic != 0xF1FA:
            raise ValueError(f"Bad frame magic at {pos}: {hex(frame_magic)}")

        cels: list[dict] = []
        cpos = pos + 16
        for _ in range(chunk_count):
            if cpos + 6 > len(data):
                break
            chunk_size = struct.unpack_from("<I", data, cpos)[0]
            chunk_type = struct.unpack_from("<H", data, cpos + 4)[0]
            body = cpos + 6
            end = cpos + chunk_size

            if chunk_type in (CHUNK_OLD_PALETTE, CHUNK_OLD_PALETTE2):
                # Skip detailed old palette; prefer 0x2019
                pass
            elif chunk_type == CHUNK_PALETTE:
                # DWORD new size, DWORD first, DWORD last, BYTE[8] reserved...
                first = struct.unpack_from("<I", data, body + 4)[0]
                last = struct.unpack_from("<I", data, body + 8)[0]
                p = body + 20
                for i in range(first, last + 1):
                    flags_c = struct.unpack_from("<H", data, p)[0]
                    r, g, b, a = data[p + 2], data[p + 3], data[p + 4], data[p + 5]
                    p += 6
                    if flags_c & 1:
                        _, p = read_string(data, p)
                    if 0 <= i < 256:
                        palette[i] = (r, g, b, a)
            elif chunk_type == CHUNK_LAYER:
                flags_l = struct.unpack_from("<H", data, body)[0]
                layer_type = struct.unpack_from("<H", data, body + 2)[0]
                child_level = struct.unpack_from("<H", data, body + 4)[0]
                blend = struct.unpack_from("<H", data, body + 10)[0]
                opacity = data[body + 12]
                name, _ = read_string(data, body + 16)
                layers.append(
                    {
                        "flags": flags_l,
                        "type": layer_type,
                        "child": child_level,
                        "blend": blend,
                        "opacity": opacity,
                        "name": name,
                        "visible": bool(flags_l & 1),
                    }
                )
            elif chunk_type == CHUNK_TAGS and frame_index == 0:
                tag_count = struct.unpack_from("<H", data, body)[0]
                p = body + 10
                for _t in range(tag_count):
                    from_f = struct.unpack_from("<H", data, p)[0]
                    to_f = struct.unpack_from("<H", data, p + 2)[0]
                    direction = data[p + 4]
                    p += 17
                    name, p = read_string(data, p)
                    tags.append(
                        {
                            "name": name,
                            "from": from_f,
                            "to": to_f,
                            "direction": direction,
                        }
                    )
            elif chunk_type == CHUNK_CEL:
                layer_index = struct.unpack_from("<H", data, body)[0]
                x = struct.unpack_from("<h", data, body + 2)[0]
                y = struct.unpack_from("<h", data, body + 4)[0]
                opacity = data[body + 6]
                cel_type = struct.unpack_from("<H", data, body + 7)[0]
                z_index = struct.unpack_from("<h", data, body + 9)[0]
                extra = body + 16
                if cel_type == 0:  # raw image
                    cw = struct.unpack_from("<H", data, extra)[0]
                    ch = struct.unpack_from("<H", data, extra + 2)[0]
                    raw = data[extra + 4 : end]
                    img = decode_cel_pixels(raw, cw, ch, color_depth, palette)
                    cels.append(
                        {
                            "layer": layer_index,
                            "x": x,
                            "y": y,
                            "opacity": opacity,
                            "z": z_index,
                            "image": img,
                        }
                    )
                elif cel_type == 2:  # compressed image
                    cw = struct.unpack_from("<H", data, extra)[0]
                    ch = struct.unpack_from("<H", data, extra + 2)[0]
                    compressed = data[extra + 4 : end]
                    raw = zlib.decompress(compressed)
                    img = decode_cel_pixels(raw, cw, ch, color_depth, palette)
                    cels.append(
                        {
                            "layer": layer_index,
                            "x": x,
                            "y": y,
                            "opacity": opacity,
                            "z": z_index,
                            "image": img,
                        }
                    )
                elif cel_type == 1:  # linked cel
                    link = struct.unpack_from("<H", data, extra)[0]
                    cels.append(
                        {
                            "layer": layer_index,
                            "x": x,
                            "y": y,
                            "opacity": opacity,
                            "z": z_index,
                            "link": link,
                        }
                    )
                # cel_type 3 = compressed tilemap — skip for now

            cpos = end

        frames.append({"duration": duration or speed or 100, "cels": cels})
        pos += frame_bytes

    # Resolve linked cels
    for fi, frame in enumerate(frames):
        resolved = []
        for cel in frame["cels"]:
            if "link" in cel:
                link_i = cel["link"]
                if 0 <= link_i < len(frames):
                    for src in frames[link_i]["cels"]:
                        if src.get("layer") == cel["layer"] and "image" in src:
                            linked = dict(cel)
                            linked["image"] = src["image"]
                            linked["x"] = src["x"]
                            linked["y"] = src["y"]
                            del linked["link"]
                            resolved.append(linked)
                            break
            else:
                resolved.append(cel)
        frame["cels"] = resolved

    return {
        "width": width,
        "height": height,
        "color_depth": color_depth,
        "flags": flags,
        "layers": layers,
        "tags": tags,
        "frames": frames,
        "path": path,
    }


def compose_frame(doc: dict, frame_index: int) -> Image.Image:
    canvas = Image.new("RGBA", (doc["width"], doc["height"]), (0, 0, 0, 0))
    frame = doc["frames"][frame_index]
    # Draw by layer order
    cels = sorted(frame["cels"], key=lambda c: (c["layer"], c.get("z", 0)))
    for cel in cels:
        layer_i = cel["layer"]
        if layer_i < len(doc["layers"]) and not doc["layers"][layer_i]["visible"]:
            continue
        if "image" not in cel:
            continue
        img = cel["image"]
        opacity = cel["opacity"]
        if opacity < 255:
            img = img.copy()
            alpha = img.split()[3].point(lambda a: a * opacity // 255)
            img.putalpha(alpha)
        canvas.alpha_composite(img, (cel["x"], cel["y"]))
    return canvas


def export_doc(doc: dict, out_dir: Path, name: str, sheet_cols: int | None = None) -> None:
    out_dir.mkdir(parents=True, exist_ok=True)
    frames_dir = out_dir / "frames"
    frames_dir.mkdir(exist_ok=True)

    images: list[Image.Image] = []
    for i in range(len(doc["frames"])):
        img = compose_frame(doc, i)
        images.append(img)
        img.save(frames_dir / f"{name}_{i:03d}.png")

    # Trim common empty border while keeping shared canvas across frames
    bbox = None
    for img in images:
        b = img.getbbox()
        if b is None:
            continue
        if bbox is None:
            bbox = list(b)
        else:
            bbox[0] = min(bbox[0], b[0])
            bbox[1] = min(bbox[1], b[1])
            bbox[2] = max(bbox[2], b[2])
            bbox[3] = max(bbox[3], b[3])

    if bbox:
        cropped = [img.crop(tuple(bbox)) for img in images]
    else:
        cropped = images

    # Individual cropped frames
    for i, img in enumerate(cropped):
        img.save(frames_dir / f"{name}_trim_{i:03d}.png")

    # Spritesheet
    n = len(cropped)
    if n == 0:
        return
    fw, fh = cropped[0].size
    cols = sheet_cols or min(n, 8)
    rows = (n + cols - 1) // cols
    sheet = Image.new("RGBA", (cols * fw, rows * fh), (0, 0, 0, 0))
    for i, img in enumerate(cropped):
        sheet.paste(img, ((i % cols) * fw, (i // cols) * fh))
    sheet_path = out_dir / f"{name}_sheet.png"
    sheet.save(sheet_path)

    # Static first frame / preview
    cropped[0].save(out_dir / f"{name}_preview.png")

    # Meta for Unity (frame size + tags)
    meta_lines = [
        f"source={doc['path'].name}",
        f"canvas={doc['width']}x{doc['height']}",
        f"frame_size={fw}x{fh}",
        f"frame_count={n}",
        f"sheet_cols={cols}",
        f"sheet_rows={rows}",
        f"sheet={sheet_path.name}",
    ]
    for tag in doc["tags"]:
        meta_lines.append(
            f"tag={tag['name']};from={tag['from']};to={tag['to']};dir={tag['direction']}"
        )
    for i, fr in enumerate(doc["frames"]):
        meta_lines.append(f"duration[{i}]={fr['duration']}")
    if doc["layers"]:
        meta_lines.append("layers=" + ",".join(l["name"] for l in doc["layers"]))
    (out_dir / f"{name}_meta.txt").write_text("\n".join(meta_lines), encoding="utf-8")
    print(f"Exported {name}: {n} frames, sheet {cols}x{rows}, cell {fw}x{fh}")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("inputs", nargs="+", type=Path)
    parser.add_argument("-o", "--out", type=Path, required=True)
    args = parser.parse_args()

    for src in args.inputs:
        if not src.exists():
            print(f"MISSING: {src}")
            continue
        name = src.stem.strip().replace(" ", "_")
        print(f"Parsing {src.name} ...")
        doc = parse_ase(src)
        print(
            f"  {doc['width']}x{doc['height']} depth={doc['color_depth']} "
            f"frames={len(doc['frames'])} layers={len(doc['layers'])} tags={len(doc['tags'])}"
        )
        for layer in doc["layers"]:
            print(f"    layer: {layer['name']} visible={layer['visible']}")
        for tag in doc["tags"]:
            print(f"    tag: {tag['name']} [{tag['from']}-{tag['to']}]")
        export_doc(doc, args.out / name, name)


if __name__ == "__main__":
    main()
