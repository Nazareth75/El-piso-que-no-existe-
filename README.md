# Piso7-2D

## Descripción

Piso7-2D es un videojuego 2D de terror, misterio y aventura. El jugador deberá explorar un entorno inquietante y cargado de secretos, avanzando por distintas secciones del escenario mientras descubre pistas sobre lo que ocurre en el "Piso 7". La ambientación busca generar tensión y curiosidad a través de la atmósfera visual y el diseño del escenario.

## Género

Terror, Misterio, Aventura.

## Plataforma objetivo

PC (Windows).

## Tecnologías

- Unity
- Git
- GitHub

## Integrantes

- Nazaret – [Nazareth75]
- Dilan – [Pacus777]
- Katlen – [lpzekathlenlennyescalantesi-cyber]
- Jose Escarzo – [JoseEscarzo]

## Estado del proyecto

Prototipo inicial.

## Estructura del proyecto

El proyecto está organizado dentro de la carpeta `Assets/` de la siguiente manera:

```
Assets/
├── Art/
│   ├── Backgrounds/   → Fondos utilizados en las escenas
│   ├── Characters/    → Sprites y assets de personajes
│   ├── Tiles/         → Tiles utilizados para construir el escenario
│   └── UI/            → Elementos gráficos de interfaz
├── Scenes/            → Escenas del juego (ej. Nivel01.unity)
├── Prefabs/           → Prefabs reutilizables del proyecto
└── Materials/         → Materiales utilizados en el proyecto
```

## Ejecución

1. Clonar este repositorio:
```
git clone https://github.com/JoseEscarzo/Piso7-2D.git
```
2. Abrir Unity Hub.
3. Click en "Añadir" → "Añadir proyecto desde el disco" y seleccionar la carpeta clonada.
4. Abrir el proyecto con la versión de Unity indicada (6000.3.16f1 o superior compatible).
5. Dentro de Unity, ir a `Assets/Scenes/` y abrir la escena `Nivel01.unity`.
6. Presionar Play para visualizar la escena inicial.

## Assets

## Flujo de trabajo

El proyecto utiliza un flujo de trabajo basado en tres niveles de ramas:

- **main**: representa la versión estable del proyecto. No se trabaja directamente sobre ella.
- **develop**: rama principal de desarrollo durante el semestre. Todos los cambios se integran progresivamente aquí.
- **feature/***: ramas específicas para desarrollar tareas puntuales (ej. `feature/escenario-inicial`, `feature/fondo-principal`, `feature/tiles-nivel01`). Una vez finalizada la tarea, se integran a `develop` mediante merge o Pull Request.
