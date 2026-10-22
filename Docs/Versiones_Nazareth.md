# Versiones de Nazareth

Ramas derivadas de feature/nazaret:
- feature/nazaret-windows: exportar Windows Standalone x64, 1280x720, ventana redimensionable.
- feature/nazaret-apk: exportar APK Android horizontal, IL2CPP ARM64; Tools > Equipo > Generar APK Android.

Ambas conservan las mismas correcciones. Los controles tactiles solo se activan al compilar para Android/iOS; Windows usa teclado y raton. El enemigo requiere tres ataques (F o clic derecho). Se limita el movimiento al suelo; si hay una caida excepcional se recupera al personaje y aparecen opciones para continuar o volver al menu. Menu / Pausa permite salir al menu durante el recorrido.

Los sprites hijos BotonAcceso usan escala local X 0.0882, Y 0.0948, Z 0.1886; conservan posicion y area de interaccion.

Validacion en Unity 6000.3.16f1: 316 comprobaciones aprobadas, informe Library/PlayableValidation/report.txt. Incluye ambos personajes, combate, victoria, derrota, transiciones Campus, limite del suelo y recuperacion al menu. Esta validacion en editor no sustituye la prueba de un APK en dispositivo ni de un ejecutable exportado. Los builds anteriores deben regenerarse para incorporar estas correcciones.
