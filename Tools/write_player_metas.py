"""Copia el retrato del personaje 1 y escribe los metas de sprite."""

import shutil
import uuid
from pathlib import Path

from PIL import Image

from build_menu import meta_sprite

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "Assets" / "Art" / "Player" / "Frames" / "mujer_universitaria_1_trim_000.png"
ONE = ROOT / "Assets" / "Resources" / "Player" / "Personaje1"
TWO = ROOT / "Assets" / "Resources" / "Player" / "Personaje2"


def ensure(path):
    im = Image.open(path)
    meta = Path(str(path) + ".meta")
    guid = uuid.uuid4().hex
    if meta.exists():
        for line in meta.read_text(encoding="utf-8").splitlines():
            if line.startswith("guid:"):
                guid = line.split(":", 1)[1].strip()
    meta_sprite(path, guid, im.width, im.height)
    print(path.name, im.size, guid)


def main():
    ONE.mkdir(parents=True, exist_ok=True)
    dest = ONE / "retrato.png"
    shutil.copyfile(SRC, dest)
    ensure(dest)
    for png in sorted(TWO.glob("frame_*.png")):
        ensure(png)


if __name__ == "__main__":
    main()
