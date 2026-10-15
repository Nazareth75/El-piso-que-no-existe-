# El piso que no existe

Prototipo 2D en Unity **6.3 LTS (6000.3.16f1)**.

## Abrir y jugar

1. Clonar la rama `devmodule/dilan` y agregar la carpeta del repositorio desde Unity Hub.
2. Abrir `Assets/Scenes/SampleScene.unity`.
3. Pulsar Play y hacer clic dentro de la ventana Game.

- **A / D:** mover la cápsula Player a izquierda y derecha.
- El monstruo persigue al jugador y gira hacia él.
- Al tocar al jugador, aparece Game Over y se detiene el movimiento.
- **Reintentar:** devuelve ambos personajes a sus posiciones iniciales.

## Implementación

`Assets/Scripts/JuegoSimple.cs` controla movimiento, persecución, contacto y panel de Game Over. Usa Input System y físicas 2D. Las velocidades se pueden ajustar en el componente del Player.

La escena incluye las referencias y colisiones configuradas. `Tools > Configurar juego simple` permite volver a configurar los objetos llamados `Player`, `Square` y `personaje_caminar…`.

La animación del monstruo está configurada a 10 cuadros por segundo y en bucle. Las carpetas generadas por Unity se excluyen mediante `.gitignore`.
