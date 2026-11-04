"""Escribe las escenas del recorrido para que se puedan abrir sin el menú del editor."""

import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCENE_DIR = ROOT / "Assets" / "Scenes" / "Campus"
SPRITE_MAT = "a97c105638bdf8b4a8650670310a4cd3"
LIGHT2D = "073797afb82c5a1438f328866b10b3f0"
PAN = "5410a0e50dfb01346a623ff33304cc7d"
HOTSPOT = "cfb24e0ba285a3c46a75d640f785d152"
PLACE = "467ede20a059aca4f9611ba01aac6b5d"
OVERLAY = "b7eb460f97baea24280beab6ae95d034"
ELEVATOR = "c3a8e1f04b6d4a2e9f17c5d8b0a4e672"

def load_fondos():
    folder = ROOT / "Assets" / "Art" / "Campus" / "Descubre" / "Fondos"
    fondos = {}
    for meta in folder.glob("*.png.meta"):
        text = meta.read_text(encoding="utf-8")
        guid = ""
        file_id = "21300000"
        for line in text.splitlines():
            if line.startswith("guid:"):
                guid = line.split(":", 1)[1].strip()
            if ": " in line and line.strip().endswith(("0", "1", "2", "3", "4", "5", "6", "7", "8", "9")):
                key, value = line.strip().split(":", 1)
                if key.endswith("_0"):
                    file_id = value.strip()
        fondos[meta.name[: -len(".png.meta")]] = (guid, file_id)
    return fondos
PUNTOS = {
    "avanzar": ("f201743f38f301047b189cd7e9001fab", "-4134804419177649633"),
    "atras": ("2b8b65e6d9fee1746909e60d3e263350", "-8712400438132310798"),
    "subir": ("0b32d049f9859f849a13c102ad63bc52", "-3895435749759585146"),
    "bajar": ("a23213a40a36b674eacd82a52b90927d", "1469709276852900075"),
}

# elevator: posición del ascensor, o None si esa escena no tiene puertas de ascensor.
# La recepción lo tiene donde está en la visita: después del muro de colores.
PLACES = [
    ("Campus_EntradaPrincipal", "entrada_principal", "Planta baja", "Entrada principal", 10.7, None,
     [("avanzar", 12.1, "Campus_Recepcion", "Recepcion")]),
    ("Campus_Recepcion", "recepcion", "Planta baja", "Recepcion", 11, 13.44,
     [("atras", 1.2, "Campus_EntradaPrincipal", "Entrada"),
      ("avanzar", 3.4, "Campus_HallPrincipal", "Hall"),
      ("subir", 15.6, "Campus_Escaleras", "Escaleras")]),
    ("Campus_HallPrincipal", "hall_principal", "Planta baja", "Hall principal", 9, None,
     [("atras", 1.2, "Campus_Recepcion", "Recepcion"),
      ("avanzar", 3.48, "Campus_Admisiones", "Admisiones"),
      ("avanzar", 6.92, "Campus_Clinica", "Clinica"),
      ("avanzar", 10.68, "Campus_Cafeteria", "Cafeteria"),
      ("avanzar", 13.88, "Campus_Auditorio", "Auditorio")]),
    ("Campus_Admisiones", "admisiones", "Planta baja", "Admisiones", 9, None,
     [("atras", 1.4, "Campus_HallPrincipal", "Hall")]),
    ("Campus_Clinica", "clinica", "Planta baja", "Clinica odontologica", 9, None,
     [("atras", 1.4, "Campus_HallPrincipal", "Hall")]),
    ("Campus_Cafeteria", "cafeteria", "Planta baja", "Cafeteria", 9, None,
     [("atras", 1.4, "Campus_HallPrincipal", "Hall")]),
    ("Campus_Auditorio", "auditorio", "Planta baja", "Auditorio", 9, None,
     [("atras", 1.4, "Campus_HallPrincipal", "Hall")]),
    ("Campus_Escaleras", "escaleras", "Planta baja", "Escaleras", 9, None,
     [("atras", 1.4, "Campus_Recepcion", "Recepcion"),
      ("bajar", 6.2, "Campus_Subsuelo", "Subsuelo"),
      ("subir", 12.8, "Campus_Mezanine", "Mezanine")]),
    ("Campus_Subsuelo", "subsuelo", "Subsuelo", "Osteoteca", 9, 2.16,
     [("subir", 16.2, "Campus_Escaleras", "Planta baja")]),
    ("Campus_Mezanine", "mezanine", "Mezanine", "Cajas", 9, 2.16,
     [("bajar", 13.8, "Campus_Escaleras", "Planta baja"),
      ("subir", 16.2, "Campus_PasilloPiso1", "Piso 1")]),
    ("Campus_PasilloPiso1", "pasillo_piso1", "Piso 1", "Pasillo principal", 9, 2.16,
     [("bajar", 13.6, "Campus_Mezanine", "Mezanine"),
      ("subir", 14.8, "Campus_Piso2", "Piso 2"),
      ("avanzar", 16.4, "Campus_LabHistologia", "Laboratorio")]),
    ("Campus_LabHistologia", "lab_histologia", "Piso 1", "Lab. de Histologia", 9, None,
     [("atras", 1.4, "Campus_PasilloPiso1", "Pasillo")]),
    ("Campus_Piso2", "piso2", "Piso 2", "Lab. de Sistemas", 9, 2.16,
     [("bajar", 14.6, "Campus_PasilloPiso1", "Piso 1"),
      ("subir", 16.4, "Campus_Piso3", "Piso 3")]),
    ("Campus_Piso3", "piso3", "Piso 3", "Aulas", 9, 2.16,
     [("bajar", 14.6, "Campus_Piso2", "Piso 2"),
      ("subir", 16.4, "Campus_Piso4", "Piso 4")]),
    ("Campus_Piso4", "piso4", "Piso 4", "Sala juridica", 9, 2.16,
     [("bajar", 14.6, "Campus_Piso3", "Piso 3"),
      ("subir", 16.4, "Campus_Piso5", "Piso 5")]),
    ("Campus_Piso5", "piso5", "Piso 5", "Biblioteca", 9, 2.16,
     [("bajar", 15.8, "Campus_Piso4", "Piso 4")]),
]


def go(fid, name, components, tag="Untagged"):
    comps = "\n".join(f"  - component: {{fileID: {c}}}" for c in components)
    return f"""--- !u!1 &{fid}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
{comps}
  m_Layer: 0
  m_Name: {name}
  m_TagString: {tag}
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
"""


def transform(fid, go_id, x, y, z=0):
    return f"""--- !u!4 &{fid}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {x}, y: {y}, z: {z}}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
"""


def sprite_renderer(fid, go_id, guid, file_id, order):
    return f"""--- !u!212 &{fid}
SpriteRenderer:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 1
  m_SmallMeshCulling: 1
  m_ForceMeshLod: -1
  m_MeshLodSelectionBias: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 2100000, guid: {SPRITE_MAT}, type: 2}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_GlobalIlluminationMeshLod: 0
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: {order}
  m_MaskInteraction: 0
  m_Sprite: {{fileID: {file_id}, guid: {guid}, type: 3}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_SpriteSortPoint: 0
"""


def box(fid, go_id, w, h, trigger=0):
    return f"""--- !u!61 &{fid}
BoxCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  serializedVersion: 3
  m_Density: 1
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_ForceSendLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ForceReceiveLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ContactCaptureLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_CallbackLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_IsTrigger: {trigger}
  m_UsedByEffector: 0
  m_CompositeOperation: 0
  m_CompositeOrder: 0
  m_Offset: {{x: 0, y: 0}}
  m_SpriteTilingProperty:
    border: {{x: 0, y: 0, z: 0, w: 0}}
    pivot: {{x: 0.5, y: 0.5}}
    oldSize: {{x: 0, y: 0}}
    newSize: {{x: 0, y: 0}}
    adaptiveTilingThreshold: 0
    drawMode: 0
    adaptiveTiling: 0
  m_AutoTiling: 0
  m_Size: {{x: {w}, y: {h}}}
  m_EdgeRadius: 0
"""


HEADER = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {fileID: 0}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 10
  m_Fog: 0
  m_FogColor: {r: 0.5, g: 0.5, b: 0.5, a: 1}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {r: 0.212, g: 0.227, b: 0.259, a: 1}
  m_AmbientEquatorColor: {r: 0.114, g: 0.125, b: 0.133, a: 1}
  m_AmbientGroundColor: {r: 0.047, g: 0.043, b: 0.035, a: 1}
  m_AmbientIntensity: 1
  m_AmbientMode: 3
  m_SubtractiveShadowColor: {r: 0.42, g: 0.478, b: 0.627, a: 1}
  m_SkyboxMaterial: {fileID: 0}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {fileID: 0}
  m_SpotCookie: {fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {fileID: 0}
  m_Sun: {fileID: 0}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 13
  m_BakeOnSceneLoad: 0
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 0
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {fileID: 0}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 2
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 1
    m_PVRFilteringGaussRadiusAO: 1
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {fileID: 0}
  m_LightingSettings: {fileID: 0}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    buildHeightMesh: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {fileID: 0}
"""


def scene_text(place, fondos):
    name, fondo, piso, titulo, cam_x, elevator_x, points = place
    guid, file_id = fondos[fondo]
    parts = [HEADER]
    roots = []

    # Camera 100
    parts.append(go(100, "Main Camera", [103, 102, 101], "MainCamera"))
    parts.append(f"""--- !u!114 &101
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {PAN}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Piso7.Campus.CampusPanCamera
  startX: {cam_x}
  y: 2.72
  panSpeed: 7
""")
    parts.append(f"""--- !u!81 &102
AudioListener:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100}}
  m_Enabled: 1
--- !u!20 &103
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100}}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 2
  m_BackGroundColor: {{r: 0.45, g: 0.62, b: 0.78, a: 1}}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_Iso: 200
  m_ShutterSpeed: 0.005
  m_Aperture: 16
  m_FocusDistance: 10
  m_FocalLength: 50
  m_BladeCount: 5
  m_Curvature: {{x: 2, y: 11}}
  m_BarrelClipping: 0.25
  m_Anamorphism: 0
  m_SensorSize: {{x: 36, y: 24}}
  m_LensShift: {{x: 0, y: 0}}
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
  orthographic size: 2.72
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {{fileID: 0}}
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
    parts.append(transform(104, 100, cam_x, 2.72, -10))
    roots.append(104)

    parts.append(go(200, "Global Light 2D", [202, 201]))
    parts.append(f"""--- !u!114 &201
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 200}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {LIGHT2D}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Unity.RenderPipelines.Universal.2D.Runtime::UnityEngine.Rendering.Universal.Light2D
  m_ComponentVersion: 2
  m_LightType: 4
  m_BlendStyleIndex: 0
  m_FalloffIntensity: 0.5
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_Intensity: 1
  m_LightVolumeIntensity: 1
  m_LightVolumeEnabled: 0
  m_ApplyToSortingLayers: 00000000
  m_LightCookieSprite: {{fileID: 0}}
  m_DeprecatedPointLightCookieSprite: {{fileID: 0}}
  m_LightOrder: 0
  m_AlphaBlendOnOverlap: 0
  m_OverlapOperation: 0
  m_NormalMapDistance: 3
  m_NormalMapQuality: 2
  m_UseNormalMap: 0
  m_ShadowsEnabled: 0
  m_ShadowIntensity: 0.75
  m_ShadowSoftness: 0.3
  m_ShadowSoftnessFalloffIntensity: 0.5
  m_ShadowVolumeIntensityEnabled: 0
  m_ShadowVolumeIntensity: 0.75
  m_LocalBounds:
    m_Center: {{x: 0, y: 0, z: 0}}
    m_Extent: {{x: 1, y: 1, z: 0}}
  m_PointLightInnerAngle: 360
  m_PointLightOuterAngle: 360
  m_PointLightInnerRadius: 0
  m_PointLightOuterRadius: 1
  m_ShapeLightParametricSides: 5
  m_ShapeLightParametricAngleOffset: 0
  m_ShapeLightParametricRadius: 1
  m_ShapeLightFalloffSize: 0.5
  m_ShapeLightFalloffOffset: {{x: 0, y: 0}}
  m_ShapePath:
  - {{x: -0.5, y: -0.5, z: 0}}
  - {{x: 0.5, y: -0.5, z: 0}}
  - {{x: 0.5, y: 0.5, z: 0}}
  - {{x: -0.5, y: 0.5, z: 0}}
""")
    parts.append(transform(202, 200, 0, 0, 0))
    roots.append(202)

    parts.append(go(300, "Fondo", [302, 301]))
    parts.append(sprite_renderer(301, 300, guid, file_id, -20))
    parts.append(transform(302, 300, 9, 2.72, 0))
    roots.append(302)

    parts.append(go(400, "Lugar", [402, 401]))
    parts.append(f"""--- !u!114 &401
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 400}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {PLACE}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Piso7.Campus.CampusPlaceInfo
  piso: {piso}
  nombre: {titulo}
""")
    parts.append(transform(402, 400, 0, 0, 0))
    roots.append(402)

    parts.append(go(500, "Recorrido", [502, 501]))
    parts.append(f"""--- !u!114 &501
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 500}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {OVERLAY}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Piso7.Campus.CampusOverlay
  title: {{fileID: 0}}
  hint: {{fileID: 0}}
  labelRoot: {{fileID: 0}}
  font: {{fileID: 0}}
""")
    parts.append(transform(502, 500, 0, 0, 0))
    roots.append(502)

    parts.append(go(600, "Suelo", [602, 601]))
    parts.append(box(601, 600, 18, 0.4))
    parts.append(transform(602, 600, 9, 0.44, 0))
    roots.append(602)

    n = 700
    for kind, x, target, label in points:
        pguid, pfile = PUNTOS[kind]
        parts.append(go(n, f"Punto_{label}", [n + 3, n + 1, n + 2, n + 4]))
        parts.append(sprite_renderer(n + 1, n, pguid, pfile, 8))
        parts.append(box(n + 2, n, 0.95, 0.95, 1))
        parts.append(f"""--- !u!114 &{n + 4}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {n}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {HOTSPOT}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Piso7.Campus.CampusHotspot
  targetScene: {target}
  label: {label}
""")
        # GameObject component list must include the script. Fix by rewriting go components.
        parts.append(transform(n + 3, n, x, 1.0, 0))
        roots.append(n + 3)
        n += 10

    if elevator_x is not None:
        parts.append(go(900, "Ascensor", [903, 901, 902]))
        parts.append(box(901, 900, 1.4, 2.5, 1))
        parts.append(f"""--- !u!114 &902
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 900}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {ELEVATOR}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Piso7.Campus.CampusElevator
""")
        parts.append(transform(903, 900, elevator_x, 1.9, 0))
        roots.append(903)

    parts.append(player_block(spawn_x(points)))
    roots.append(1104)

    # The go() above for points omitted the script component. Patch below in a cleaner builder.
    root_yaml = "\n".join(f"  - {{fileID: {r}}}" for r in roots)
    parts.append(f"""--- !u!1660057539 &9223372036854775807
SceneRoots:
  m_ObjectHideFlags: 0
  m_Roots:
{root_yaml}
""")
    return "\n".join(parts), name


def spawn_x(points, preferred=6.0):
    xs = [px for _, px, _, _ in points]
    for x in (6.0, 8.0, 9.0, 5.0, 4.2, 10.0):
        if all(abs(x - hx) > 1.15 for hx in xs):
            return x
    return preferred


def player_block(x):
    frames = [
        "805b501c517c1914d80c676c9f228b20",
        "bd58bbe6c755bfa40aff18a2c97f935f",
        "c4eeac17358b47d42bdfe142edd8f2e3",
        "d43bbc5558cfe3747850bc62ff9c25ef",
        "cbec33c377c5dc1479e576bc22cb3c6f",
        "aa3d1259706cdea40a53215427182d0d",
        "bbca7eca855e8db44b0f328a0614cfda",
        "85b95d931b9cfb540bac50b09d28019a",
        "8d1718f1fae4a2e41a512644b12ac4a5",
        "f6b0daf15fd6f7c4e849553110dacda4",
    ]
    frame_yaml = "\n".join(
        f"  - {{fileID: 21300000, guid: {guid}, type: 3}}" for guid in frames
    )
    return f"""--- !u!1 &1100
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 1104}}
  - component: {{fileID: 1103}}
  - component: {{fileID: 1102}}
  - component: {{fileID: 1101}}
  - component: {{fileID: 1105}}
  m_Layer: 0
  m_Name: Player
  m_TagString: Player
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!114 &1105
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1100}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: b7ceee24919e00843a8c114a08844fc9, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Piso7.Interaction.PlayerInteractor
  radius: 1.4
  interactableMask:
    serializedVersion: 2
    m_Bits: 4294967295
  origin: {{fileID: 0}}
--- !u!114 &1101
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1100}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: dfd7c3e1902ae18419c089de7706f4e3, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Piso7.Player.PlayerController2D
  walkSpeed: 4.5
  runSpeed: 7.5
  jumpForce: 9
  groundedRememberTime: 0.1
  jumpBufferTime: 0.1
  groundCheck: {{fileID: 0}}
  groundCheckRadius: 0.12
  groundLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  runStaminaCostPerSecond: 12
  staminaRecoverPerSecond: 8
--- !u!70 &1102
CapsuleCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1100}}
  m_Enabled: 1
  serializedVersion: 3
  m_Density: 1
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_ForceSendLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ForceReceiveLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ContactCaptureLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_CallbackLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_IsTrigger: 0
  m_UsedByEffector: 0
  m_CompositeOperation: 0
  m_CompositeOrder: 0
  m_Offset: {{x: 0, y: 0}}
  m_Size: {{x: 0.7, y: 2.05}}
  m_Direction: 0
--- !u!50 &1103
Rigidbody2D:
  serializedVersion: 5
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1100}}
  m_BodyType: 0
  m_Simulated: 1
  m_UseFullKinematicContacts: 0
  m_UseAutoMass: 0
  m_Mass: 1
  m_LinearDamping: 0
  m_AngularDamping: 0.05
  m_GravityScale: 3
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_Interpolate: 1
  m_SleepingMode: 1
  m_CollisionDetection: 1
  m_Constraints: 4
--- !u!4 &1104
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1100}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {x}, y: 1.7, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: 1114}}
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!1 &1110
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 1114}}
  - component: {{fileID: 1112}}
  - component: {{fileID: 1111}}
  m_Layer: 0
  m_Name: Visual
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!114 &1111
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1110}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: e32139e679496a34ab3ec8476fc78a88, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Piso7.Player.PlayerWalkAnimator
  walkFrames:
{frame_yaml}
  idleFrame: {{fileID: 21300000, guid: 805b501c517c1914d80c676c9f228b20, type: 3}}
  framesPerSecond: 10
  moveThreshold: 0.15
--- !u!212 &1112
SpriteRenderer:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1110}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 1
  m_SmallMeshCulling: 1
  m_ForceMeshLod: -1
  m_MeshLodSelectionBias: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 2100000, guid: a97c105638bdf8b4a8650670310a4cd3, type: 2}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_GlobalIlluminationMeshLod: 0
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 10
  m_MaskInteraction: 0
  m_Sprite: {{fileID: 21300000, guid: 805b501c517c1914d80c676c9f228b20, type: 3}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_SpriteSortPoint: 0
--- !u!4 &1114
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1110}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 0.55, y: 0.55, z: 0.55}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 1104}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
"""


def meta(guid):
    return f"""fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def main():
    SCENE_DIR.mkdir(parents=True, exist_ok=True)
    fondos = load_fondos()
    missing = [p[1] for p in PLACES if p[1] not in fondos]
    if missing:
        raise SystemExit("Faltan fondos: " + ", ".join(missing))
    for place in PLACES:
        text, name = scene_text(place, fondos)
        scene_path = SCENE_DIR / f"{name}.unity"
        meta_path = SCENE_DIR / f"{name}.unity.meta"
        scene_path.write_text(text, encoding="utf-8", newline="\n")
        if not meta_path.exists():
            meta_path.write_text(meta(uuid.uuid4().hex), encoding="utf-8", newline="\n")
        print(name)


if __name__ == "__main__":
    main()
