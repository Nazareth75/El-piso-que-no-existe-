"""Escena del menú, para abrirla en el editor y ver el marco."""

import sys
import uuid
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from write_campus_scenes import HEADER, go, sprite_renderer, transform

ROOT = Path(__file__).resolve().parents[1]
SCENE = ROOT / "Assets" / "Scenes" / "MainMenu.unity"
META = ROOT / "Assets" / "Scenes" / "MainMenu.unity.meta"
FONDO_META = ROOT / "Assets" / "Resources" / "Menu" / "fondo.png.meta"


def guid_of(path):
    for line in path.read_text(encoding="utf-8").splitlines():
        if line.startswith("guid:"):
            return line.split(":", 1)[1].strip()
    raise SystemExit(f"sin guid en {path}")


def main():
    fondo = guid_of(FONDO_META)
    parts = [HEADER]
    parts.append(go(100, "Main Camera", [103, 102], "MainCamera"))
    parts.append("""--- !u!81 &102
AudioListener:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100}
  m_Enabled: 1
--- !u!20 &103
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 2
  m_BackGroundColor: {r: 0.07, g: 0.14, b: 0.23, a: 1}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_Iso: 200
  m_ShutterSpeed: 0.005
  m_Aperture: 16
  m_FocusDistance: 10
  m_FocalLength: 50
  m_BladeCount: 5
  m_Curvature: {x: 2, y: 11}
  m_BarrelClipping: 0.25
  m_Anamorphism: 0
  m_SensorSize: {x: 36, y: 24}
  m_LensShift: {x: 0, y: 0}
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.3
  far clip plane: 1000
  field of view: 60
  orthographic: 1
  orthographic size: 1.35
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {fileID: 0}
  m_TargetDisplay: 0
  m_TargetEye: 3
  m_HDR: 1
  m_AllowMSAA: 1
  m_AllowDynamicResolution: 0
  m_ForceIntoRT: 0
  m_OcclusionCulling: 0
  m_StereoConvergence: 10
  m_StereoSeparation: 0.022
""")
    parts.append(transform(104, 100, 0, 0, -10))
    parts.append(go(300, "FondoMenu", [302, 301]))
    parts.append(sprite_renderer(301, 300, fondo, "21300000", 0))
    parts.append(transform(302, 300, 0, 0, 0))
    parts.append("""--- !u!1660057539 &9223372036854775807
SceneRoots:
  m_ObjectHideFlags: 0
  m_Roots:
  - {fileID: 104}
  - {fileID: 302}
""")
    SCENE.parent.mkdir(parents=True, exist_ok=True)
    SCENE.write_text("\n".join(parts), encoding="utf-8", newline="\n")
    if not META.exists():
        META.write_text(
            f"""fileFormatVersion: 2
guid: {uuid.uuid4().hex}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
""",
            encoding="utf-8",
            newline="\n",
        )
    print(SCENE.name, guid_of(META))


if __name__ == "__main__":
    main()
