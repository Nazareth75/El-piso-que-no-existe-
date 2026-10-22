# UNIFRANZ: El Piso que no Existe (Piso7-2D)

## Descripción

Piso7-2D es un videojuego 2D de aventura, misterio y terror universitario, desarrollado en Unity. El jugador explora distintos escenarios, descubre lugares de la universidad e interactúa con elementos del entorno mientras avanza en la historia del Piso 7.

## Género

Aventura, misterio y terror.

## Plataforma

PC (Windows).

## Tecnologías

- Unity 6000.3.16f1
- C#
- Git y GitHub
- Aseprite para los recursos gráficos
- Python para herramientas de generación de escenas y exportación de arte

## Integrantes

- Nazaret – [Nazareth75]
- Dilan – [Pacus777]
- Katlen – [lpzekathlenlennyescalantesi-cyber]
- Jose Escarzo – [JoseEscarzo]

## Características implementadas

- Movimiento lateral del personaje.
- Caminar, correr y saltar.
- Cámara con seguimiento del jugador.
- Sistema de interacción con objetos y puertas.
- HUD para mostrar información del juego.
- Menú principal y recursos de interfaz.
- Fondos y animación de introducción de UNIFRANZ.
- Recorrido por diferentes espacios del campus universitario.

## Abrir el proyecto

1. Abrir Unity Hub.
2. Seleccionar **Add** y después **Add project from disk**.
3. Elegir la carpeta del proyecto `Piso7-2D`.
4. Abrirlo con Unity **6000.3.16f1**.

## Primer arranque

En Unity, utilizar el menú **Piso7** para ejecutar las herramientas de configuración:

1. **Configurar importación Pixel Art**.
2. **Crear escena Nivel 1 (Entrada)**.
3. Opcionalmente, crear la escena de introducción de UNIFRANZ.
4. Crear el recorrido **Descubre el campus** si se desea probar esa funcionalidad.

Después, abrir la escena correspondiente y pulsar **Play**.

## Controles

| Acción | Tecla |
|---|---|
| Moverse | A / D o flechas |
| Correr | Shift |
| Saltar | Espacio |
| Interactuar | E |

Los controles disponibles pueden variar según la escena.

## Estructura del proyecto

```text
Piso7-2D/
├── Assets/
│   ├── Art/
│   │   ├── Backgrounds/
│   │   ├── Campus/
│   │   ├── Player/
│   │   ├── Exported/
│   │   └── SourceAse/
│   ├── Resources/
│   ├── Scenes/
│   │   └── Campus/
│   ├── Scripts/
│   │   ├── Player/
│   │   ├── Camera/
│   │   ├── Interaction/
│   │   ├── Core/
│   │   ├── Campus/
│   │   └── Editor/
│   └── Prefabs/
├── Packages/
├── ProjectSettings/
├── Tools/
└── README.md
```

## Herramientas de arte

El proyecto incluye recursos gráficos y archivos originales de Aseprite. También cuenta con herramientas Python para exportar recursos y generar escenas.

## Flujo de trabajo con Git

- **main:** rama destinada a la versión estable.
- **develop:** rama de integración del desarrollo.
- **feature/***: ramas para trabajar en funcionalidades específicas.

Los cambios se integran mediante merge o Pull Request, revisando los conflictos antes de finalizar la integración.

## Estado del proyecto

Prototipo en desarrollo. Las próximas mejoras pueden incluir nuevos niveles, puzzles, sigilo, entidades y guardado de partidas.
## Integración del equipo en feature/nazaret

El recorrido comienza en `Assets/Scenes/MainMenu.unity` y continúa en `Campus_EntradaPrincipal`.
El prototipo de persecución de Dilan se conserva en `Assets/Scenes/Dilan_Prototipo.unity`, con Game Over, Victoria y Reintentar. Véase `README_Dilan.md`.
`Level1_Entrada` también está incluida en la lista de escenas de compilación.
Para repetir las pruebas básicas en Unity, guarda la escena y selecciona **Tools > Equipo > Validar integracion**. La herramienta entra y sale de Play, recorre las escenas y escribe el informe en `Library/TeamValidation/report.txt`; al finalizar restaura la escena anterior. Ejecuta las pruebas sin interactuar con el teclado hasta que finalicen.
