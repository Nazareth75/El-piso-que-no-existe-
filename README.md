# UNIFRANZ: El Piso que no Existe (Piso7-2D)

Proyecto Unity **6000.3.16f1** — aventura 2D lateral pixel art (misterio / terror universitario).

## Qué hay listo

- Arte exportado desde tus `.ase` (personaje, entrada, intro animada)
- Scripts de movimiento, cámara, interacción, HUD y sesión
- Menú del Editor **Piso7** para montar la escena del Nivel 1

## Abrir el proyecto

1. Abre **Unity Hub**
2. Add → carpeta `C:\Users\Kathlen\Piso7-2D`
3. Abre con editor **6000.3.16f1** (ya coincide con tu instalador)

## Primer arranque (importante)

En Unity, barra de menú:

1. **Piso7 → 1. Configurar importación Pixel Art**
2. **Piso7 → 2. Crear escena Nivel 1 (Entrada)**
3. Pulsa **Play**

Opcional: **Piso7 → 3. Crear escena Intro UNIFRANZ** (fondo animado de 5 frames).

Recorrido lateral de Descubre el campus (fondos y puntos, sin personaje):

4. **Piso7 → 4. Crear recorrido Descubre el campus**
5. Abre `Assets/Scenes/Campus/Campus_EntradaPrincipal` y pulsa **Play**
6. **A / D** recorre el lugar. **Clic** en un punto naranja cambia de escena.

## Controles

| Acción | Teclado |
|--------|---------|
| Mover | A / D o flechas |
| Correr | Shift (gasta resistencia) |
| Saltar | Espacio |
| Interactuar | E |

## Estructura de carpetas

```
Assets/
  Art/
    Player/           personaje (sheet + frames Walk)
    Backgrounds/
      Entrada/        fondo entrada universitaria
      IntroUNIFRANZ/  animación 5 frames
    Campus/Descubre/
      Fondos/         lugares laterales del recorrido
      Puntos/         flechas para cambiar de escena
      medidas.json    escala respecto al personaje (2.2 u)
    SourceAse/        archivos .ase originales
    Exported/         exportación completa desde Tools/ase_export.py
  Scripts/
    Player/ Camera/ Interaction/ Core/ Campus/ Editor/
  Scenes/             Level1_Entrada (menú 2)
  Scenes/Campus/      recorrido Descubre (menú 4)
```

## Relación con el GDD

| Diseño | Implementación actual |
|--------|------------------------|
| Vista lateral 2D | Cámara ortográfica + follow |
| Caminar / correr / saltar | `PlayerController2D` |
| Interactuar objetos | `PlayerInteractor` + pickups/puertas |
| Vida y resistencia | `GameSession` + HUD |
| Nivel 1: primer día / universidad | Escena `Level1_Entrada` |
| Intro visual UNIFRANZ | Escena `Intro_UNIFRANZ` |

Pendiente para siguientes entregas: puzzles, sigilo, entidades, Piso 7, guardado múltiple, más niveles.

## Assets Aseprite

El proyecto incluye el paquete `com.unity.2d.aseprite`. También puedes arrastrar los `.ase` de `Art/SourceAse/` al Project y Unity los importará.

Si reexportas desde Aseprite manualmente:

```
Tools/ase_export.py  (Python + Pillow)
```

## Notas de pixel art

- Filter Mode: **Point (no filter)**
- Compression: **None**
- Sprite Mode: Single (frames) / Multiple (sheet)
- PPU base: **100** (ajustable)

El sprite del personaje es grande; el builder lo escala a ~0.55 para jugabilidad. Ajusta `Visual` scale si quieres otro tamaño respecto al fondo.
