# UNIFRANZ: El Piso que no Existe (Piso7-2D)

Videojuego 2D de terror, misterio y aventura universitaria, desarrollado con Unity 6000.3.16f1.

## Descripción del proyecto

*El Piso que no Existe* es un videojuego de aventura lateral en 2D ambientado en un entorno universitario. El jugador deberá explorar diferentes escenarios, descubrir lugares y pistas, e interactuar con elementos del entorno mientras avanza en el misterio del Piso 7.

El juego utiliza un estilo visual pixel art y busca generar curiosidad y suspenso mediante sus escenarios, la exploración y el desarrollo progresivo de la historia.

## Género

- Terror
- Misterio
- Aventura
- Exploración

## Plataforma y tecnologías

- **Plataforma objetivo:** PC (Windows).
- **Motor de desarrollo:** Unity 6000.3.16f1.
- **Lenguaje de programación:** C#.
- **Control de versiones:** Git y GitHub.
- **Recursos gráficos:** Pixel art y archivos de Aseprite.
- **Herramientas auxiliares:** Python y Pillow para la exportación de recursos gráficos.

## Integrantes y roles

El desarrollo del videojuego se realizó de manera colaborativa, distribuyendo las responsabilidades entre los integrantes para trabajar en la programación, los escenarios, los recursos visuales y la organización del proyecto.

- **Nazaret — Coordinación y gestión del proyecto:** organización de las actividades, seguimiento de los avances y coordinación del trabajo del equipo.
- **Dilan — Diseño de escenarios y niveles:** colaboración en la construcción de escenarios, distribución de los elementos del entorno y preparación de los niveles.
- **Katlen — Programación e integración del juego:** implementación e integración de scripts, escenas y recursos en Unity, además de la preparación de funcionalidades para el funcionamiento del videojuego.
- **Jose Escarzo — Diseño visual y recursos gráficos:** colaboración en la preparación de elementos visuales, organización de recursos gráficos y ambientación del juego.

Los roles descritos representan una distribución propuesta de responsabilidades y deben ajustarse si las tareas realizadas por cada integrante fueron diferentes.

## Funcionalidades implementadas

El prototipo incluye las siguientes funcionalidades y elementos:

- Movimiento lateral del personaje.
- Caminar, correr y saltar.
- Cámara con seguimiento del jugador.
- Interacción con objetos y elementos del escenario.
- Sistema de vida y resistencia con interfaz HUD.
- Escenarios y recursos gráficos con estilo pixel art.
- Introducción visual animada.
- Escena de entrada universitaria.
- Recorrido para explorar diferentes lugares del campus.
- Puntos interactivos para cambiar de escena.
- Herramientas del Editor de Unity para configurar la importación de recursos y generar escenas.

Las funcionalidades disponibles pueden variar según la escena que se haya generado y la versión actual del proyecto.

## Controles del juego

| Acción | Tecla |
|---|---|
| Mover al personaje | A / D o flechas |
| Correr | Shift |
| Saltar | Espacio |
| Interactuar | E |
| Cambiar de lugar en el recorrido del campus | Clic en un punto interactivo |

## Requisitos para ejecutar el proyecto

- Unity Hub.
- Unity Editor 6000.3.16f1.
- Una computadora compatible con Unity.
- Git, si se desea clonar el repositorio.
- Los recursos del proyecto incluidos en el repositorio.

## Cómo abrir el proyecto

1. Clonar el repositorio desde GitHub o descargar sus archivos.
2. Abrir Unity Hub.
3. Seleccionar **Añadir proyecto desde el disco**.
4. Elegir la carpeta raíz del proyecto.
5. Abrirlo con Unity 6000.3.16f1.
6. Esperar a que Unity termine de importar los recursos.

## Primer arranque

Para preparar la escena inicial desde el Editor de Unity:

1. Abrir el menú **Piso7** en la barra superior.
2. Seleccionar **1. Configurar importación Pixel Art**.
3. Seleccionar **2. Crear escena Nivel 1 (Entrada)**.
4. Presionar **Play** para ejecutar la escena.

### Introducción animada

De forma opcional, se puede seleccionar:

**Piso7 → 3. Crear escena Intro UNIFRANZ**

Esta opción permite generar la escena de introducción visual animada.

### Recorrido: Descubre el campus

Para probar el recorrido universitario:

1. Seleccionar **Piso7 → 4. Crear recorrido Descubre el campus**.
2. Abrir la escena correspondiente dentro de `Assets/Scenes/Campus/`.
3. Presionar **Play**.
4. Utilizar A/D para recorrer el escenario.
5. Hacer clic en los puntos interactivos naranjas para cambiar de escena.

Los nombres exactos de las escenas deben comprobarse en el proyecto de Unity.

## Estructura del proyecto

Los recursos principales se organizan dentro de la carpeta `Assets/`.

```text
Piso7-2D/
├── Assets/
│   ├── Art/
│   │   ├── Player/
│   │   ├── Backgrounds/
│   │   │   ├── Entrada/
│   │   │   └── IntroUNIFRANZ/
│   │   ├── Campus/
│   │   │   └── Descubre/
│   │   ├── SourceAse/
│   │   └── Exported/
│   ├── Scenes/
│   │   └── Campus/
│   ├── Scripts/
│   │   ├── Player/
│   │   ├── Camera/
│   │   ├── Interaction/
│   │   ├── Core/
│   │   ├── Campus/
│   │   └── Editor/
│   ├── Prefabs/
│   └── Materials/
├── Tools/
│   └── ase_export.py
└── README.md
```

Esta estructura representa las principales categorías de recursos. La organización exacta puede variar según los archivos y las escenas incorporados en la versión actual del proyecto.

## Relación con el diseño del juego

| Elemento del diseño | Implementación prevista o disponible |
|---|---|
| Vista lateral 2D | Cámara ortográfica |
| Movimiento, carrera y salto | Control del personaje |
| Interacción con objetos | Sistema de interacción |
| Vida y resistencia | Sistema de sesión e interfaz HUD |
| Exploración universitaria | Escenas de entrada y recorrido |
| Introducción visual | Escena de introducción |
| Misterio del Piso 7 | Desarrollo narrativo y futuras ampliaciones |

## Recursos gráficos y pixel art

El proyecto utiliza recursos gráficos de estilo pixel art, incluyendo personajes, fondos y elementos visuales del entorno.

Para conservar la apariencia de los sprites se recomienda utilizar:

- **Filter Mode:** Point (no filter).
- **Compression:** None.
- **Sprite Mode:** Single o Multiple, según el recurso.
- **Pixels Per Unit (PPU):** ajustar de acuerdo con las dimensiones de los sprites y la escala del escenario.

El proyecto incluye recursos de Aseprite y herramientas auxiliares para exportar gráficos. Para utilizar `Tools/ase_export.py` se requiere Python y la biblioteca Pillow.

## Flujo de trabajo con Git y GitHub

El equipo utiliza Git y GitHub para gestionar los archivos, organizar el desarrollo y compartir los avances.

- **`main`:** rama principal del proyecto.
- **`develop`:** rama destinada a integrar los avances del desarrollo.
- **`feature/*`:** ramas para trabajar en funcionalidades o tareas específicas.

Los cambios se pueden revisar e integrar mediante *merge* o *Pull Request*. Antes de integrar modificaciones, se recomienda comprobar los archivos afectados y resolver cualquier conflicto.

## Estado actual del proyecto

El videojuego se encuentra en etapa de prototipo y desarrollo.

Entre las posibles mejoras para futuras entregas se encuentran:

- Ampliar los niveles y escenarios.
- Incorporar acertijos y desafíos.
- Desarrollar mecánicas de sigilo.
- Añadir entidades y eventos relacionados con el Piso 7.
- Ampliar la historia y la exploración.
- Implementar opciones de guardado y más niveles.

## Objetivo del proyecto

Desarrollar un videojuego 2D que combine exploración, aventura y misterio en un entorno universitario, aplicando conocimientos de programación, diseño de escenarios, integración de recursos y trabajo colaborativo mediante herramientas de desarrollo y control de versiones.
