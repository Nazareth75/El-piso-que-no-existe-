using UnityEditor;
using UnityEngine;

public static class VerificarSonidoCampus
{
    [MenuItem("Sonido/Verificar biblioteca de 40 sonidos")]
    public static void Verificar()
    {
        int cantidad = 0;
        foreach (string nombre in SonidoCampus.Biblioteca)
        {
            var clip = Resources.Load<AudioClip>("AudioCampus/" + nombre);
            if (clip == null || clip.samples <= 0 || clip.channels != 1 || clip.frequency < 22050)
                throw new System.Exception("Clip no válido: " + nombre);
            clip.LoadAudioData();
            var datos = new float[clip.samples];
            if (!clip.GetData(datos, 0)) throw new System.Exception("No se puede decodificar: " + nombre);
            float pico = 0f;
            foreach (float muestra in datos)
            {
                if (float.IsNaN(muestra) || float.IsInfinity(muestra)) throw new System.Exception("Muestra no válida: " + nombre);
                pico = Mathf.Max(pico, Mathf.Abs(muestra));
            }
            if (pico < .001f || pico > .99f) throw new System.Exception("Nivel de audio no válido: " + nombre + " " + pico);
            cantidad++;
        }
        Debug.Log("SONIDO_CAMPUS_OK: " + cantidad + " clips cargados y decodificados, sin silencio ni saturación en los archivos.");
        if (Application.isPlaying && SonidoCampus.Instancia != null)
        {
            if (SonidoCampus.Instancia.ClipsCargados != cantidad) throw new System.Exception("El gestor no cargó toda la biblioteca");
            SonidoCampus.Emitir("mision", .35f);
        }
    }
}
