# Como trabajar desde ahora

El juego compartido debe mantenerse en feature/nazaret-windows (o una rama comun basada en ella). Jugador, enemigos, escenas y correcciones comunes se desarrollan alli. Antes de actualizar una rama: git status, guardar cambios propios y git fetch origin.

Para pasar las mejoras a Android:
 git switch feature/nazaret-apk
 git merge feature/nazaret-windows

No hacer el merge inverso de APK hacia Windows. Si una correccion general se hizo primero en APK, crear un commit que contenga solo esa correccion y usar git cherry-pick <commit> en Windows.

Los botones tactiles ya estan condicionados a UNITY_ANDROID/UNITY_IOS: compartir ese codigo no los muestra en Windows. No duplicar escenas solo para ocultar controles. Las configuraciones especificas de Android permanecen en su rama; los builds se generan fuera del repositorio.

Para descargar por primera vez: git clone https://github.com/Nazareth75/El-piso-que-no-existe-.git
Para actualizar una copia existente: git fetch origin, git switch <rama>, git pull --ff-only origin <rama>.
No es necesario descargar el proyecto completo para cada cambio. Cerrar Unity antes de cambiar de rama y reabrirlo despues; no cambiar durante Play ni compilaciones.

Windows: entregar toda la carpeta Windows, incluyendo exe, UnityPlayer.dll y ElPiso7_Data. Android: instalar el APK. El RAR es solo un contenedor, hay que extraer Windows completo antes de ejecutar.
