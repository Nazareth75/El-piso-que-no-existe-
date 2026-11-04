# Android: configuración y controles táctiles

Responsable de este aporte: Dilan (Pacus777), en `devmodule/dilan`.
Base integrada: `feature/nazaret`, commit `605e588`.

## Abrir y generar el APK

1. Abrir la raíz del proyecto con Unity **6000.3.16f1**.
2. Instalar en Unity Hub los módulos Android Build Support, Android SDK & NDK Tools y OpenJDK de esa versión.
3. Guardar las escenas abiertas y salir de Play Mode.
4. Elegir **Tools > Equipo > Generar APK Android**.
5. Esperar el resultado en Console. Por defecto, el archivo queda en `Builds/Android/ElPisoQueNoExiste.apk`.

La exportación usa las escenas habilitadas en Build Settings y exige que `MainMenu` sea la primera.
Genera un APK instalable (no AAB), con IL2CPP y ARM64, Android mínimo API 25.
El identificador es `com.pacus777.elpisoquenoexiste`. Usa la firma de depuración local de Android para pruebas académicas; no es una firma de publicación en Play Store.
La orientación permite ambas posiciones horizontales y desactiva ambas verticales.

## Controles

Los botones se crean automáticamente en Android y en el editor para facilitar las pruebas. En Windows compilado no se muestran.
Se ajustan al área segura de la pantalla.

| Botón | Uso |
| --- | --- |
| IZQ / DER | Mantener para caminar |
| CORRER | Mantener junto con una dirección |
| SALTAR | Tocar para saltar |
| ATACAR | Tocar para golpear; el enemigo requiere tres golpes dentro del alcance |
| USAR | Tocar cerca de una puerta, objeto o ascensor |
| PAUSA | Abrir el menú; Jugar permite reanudar |

El menú, la selección de personaje, las opciones, el resultado de combate y los pisos del ascensor usan botones de interfaz compatibles con toque.
Los botones de movimiento admiten dedos simultáneos. El movimiento se libera al salir del botón, perder foco, pausar la aplicación o cambiar de escena.
Los controles originales de teclado y mando se conservan.

## Validación realizada el 22 de octubre de 2026

- Exportación completada: Unity informó `Build Finished, Result: Success`.
- APK generado en `Builds/Android/ElPisoQueNoExiste.apk`, de 45.461.926 bytes, con bibliotecas ARM64 de Unity e IL2CPP.
- Prueba de integración en editor completada: controles ocultos en menú y visibles en juego; movimiento y salto reales; dos dedos sobre un botón; liberación al salir, pausar o perder foco; pausa y reanudación.
- La primera exportación falló al descargar `kotlin-compiler-31.10.0.jar` desde Google por `Connection reset`; el reintento terminó correctamente.
- Pendiente: instalar y probar el APK en un celular o emulador autorizado. Las pruebas del editor no constituyen evidencia de ejecución Android.

La prueba del editor se puede repetir desde **Tools > Equipo > Probar controles tactiles en editor**. El informe se guarda en `Library/mobile-controls-validation.txt`.

## Pruebas fuera de Unity (pendientes hasta registrar evidencia real)

Instalar el APK en un dispositivo ARM64 con Android compatible. Con ADB:

```text
adb install -r ElPisoQueNoExiste.apk
adb shell am start -n com.pacus777.elpisoquenoexiste/com.unity3d.player.UnityPlayerActivity
```

Si la instalación usa otro punto de entrada, abrir el juego tocando su icono en el dispositivo.

- Comprobar menú inicial y selección de ambos personajes.
- Mantener DER + CORRER; soltar cada dedo y comprobar que no sigue moviéndose.
- Caminar y saltar, atacar al enemigo tres veces, continuar tras la victoria y reintentar tras una derrota.
- Usar una puerta y el ascensor; visitar otra escena y confirmar que no quedan botones atascados.
- Abrir pausa, reanudar, cambiar volumen y volver al menú.
- Girar el teléfono: debe mantenerse horizontal. Comprobar botones junto a recortes de pantalla.
- Minimizar y regresar: no debe mantenerse pulsada ninguna dirección.
- Registrar modelo, versión Android, resultado, problemas y capturas del juego ejecutándose en el dispositivo.

Una compilación correcta no demuestra por sí sola que se completaron estas pruebas.
El documento final del equipo debe añadir capturas reales de Windows y Android y las pruebas que realice Jose.

## Referencias

- https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/android/getting-started/class-player-settings
- https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/scripting-backends/il2cpp/il2cpp-introduction
