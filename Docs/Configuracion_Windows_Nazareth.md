# Aporte de Nazareth — configuración de Windows

Proyecto: El piso que no existe. Unity 6000.3.16f1.
Rama: feature/nazaret. Configuración preparada para que Dilan exporte y pruebe Windows Standalone.

## Configuración de pantalla

| Ajuste | Valor |
|---|---|
| Resolución inicial | 1280 × 720 (16:9) |
| Modo inicial | Windowed / ventana |
| Usar resolución nativa como predeterminada | Desactivado |
| Ventana redimensionable | Activada |
| Permitir cambio a pantalla completa | Activado |

La ventana inicial de 720p permite probar el juego en pantallas de distintas resoluciones. El menú del juego conserva su opción de pantalla completa. No se cambiaron los ajustes de Android ni el diseño de las escenas.

## Escena inicial y lista de compilación

MainMenu es la primera escena habilitada (índice 0). El flujo previsto es MainMenu → Jugar → selección de personaje → Campus_EntradaPrincipal → Campus_Recepcion.

Se verificaron las 20 escenas habilitadas, sus archivos .meta y sus GUID. No hay rutas duplicadas ni destinos serializados de transición que falten en la lista. No se encontraron perfiles de compilación personalizados en Assets que sobrescriban esta lista.

Se conservan SampleScene, Level1_Entrada y Dilan_Prototipo para mantener el trabajo del equipo y las referencias existentes; no son la escena inicial. No se eliminó ni reordenó el contenido del equipo.

Escenas habilitadas:

0. `Assets/Scenes/MainMenu.unity`
1. `Assets/Scenes/Campus/Campus_EntradaPrincipal.unity`
2. `Assets/Scenes/Campus/Campus_Recepcion.unity`
3. `Assets/Scenes/Campus/Campus_HallPrincipal.unity`
4. `Assets/Scenes/Campus/Campus_Admisiones.unity`
5. `Assets/Scenes/Campus/Campus_Clinica.unity`
6. `Assets/Scenes/Campus/Campus_Cafeteria.unity`
7. `Assets/Scenes/Campus/Campus_Auditorio.unity`
8. `Assets/Scenes/Campus/Campus_Escaleras.unity`
9. `Assets/Scenes/Campus/Campus_Subsuelo.unity`
10. `Assets/Scenes/Campus/Campus_Mezanine.unity`
11. `Assets/Scenes/Campus/Campus_PasilloPiso1.unity`
12. `Assets/Scenes/Campus/Campus_LabHistologia.unity`
13. `Assets/Scenes/Campus/Campus_Piso2.unity`
14. `Assets/Scenes/Campus/Campus_Piso3.unity`
15. `Assets/Scenes/Campus/Campus_Piso4.unity`
16. `Assets/Scenes/Campus/Campus_Piso5.unity`
17. `Assets/Scenes/SampleScene.unity`
18. `Assets/Scenes/Level1_Entrada.unity`
19. `Assets/Scenes/Dilan_Prototipo.unity`

## Entrega a Dilan

1. Actualizar su rama con este aporte mediante el flujo Git del equipo.
2. En Unity, abrir File → Build Profiles y seleccionar Windows, arquitectura Intel 64-bit (x86_64). Activar ese perfil/plataforma si todavía no lo está.
3. Comprobar que el perfil usa la lista global anterior, con MainMenu primero, y revisar Player Settings → Resolution and Presentation.
4. Generar el build en una carpeta de salida, por ejemplo Builds/Windows/ElPisoQueNoExiste.exe.
5. Probar el ejecutable fuera de Unity: menú, ambos personajes, movimiento, ataque, transiciones, tamaño de ventana y pantalla completa.
6. Entregar la carpeta completa del build, incluida la carpeta de datos y las bibliotecas que Unity genere; no entregar únicamente el .exe.

## Verificación y límites de este aporte

Verificación realizada sobre archivos de configuración: resolución, modo ventana, opciones de ventana, MainMenu en índice 0, existencia/GUID de las 20 escenas y destinos serializados de transición incluidos. La selección del perfil activo en el editor se deja explícitamente al responsable de exportación; no se afirma haber cambiado el perfil activo.

No se generó ni se probó un ejecutable Windows en esta tarea: corresponde al aporte de Dilan. Tampoco se generó un APK ni se comprobaron controles táctiles: corresponde al aporte de Android. Esta configuración aún requiere la prueba del ejecutable en distintas resoluciones para confirmar su comportamiento fuera del editor.
