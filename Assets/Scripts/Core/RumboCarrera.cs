using UnityEngine;

namespace Piso7.Core
{
    /// <summary>
    /// Guarda cuántas decisiones empujan a cada carrera.
    /// Sistemas, Medicina y la tercera empiezan en cero.
    /// </summary>
    public class RumboCarrera : MonoBehaviour
    {
        public const string NombreTercera = "Por definir";

        public static RumboCarrera Instancia { get; private set; }

        public int Sistemas { get; private set; }
        public int Medicina { get; private set; }
        public int Tercera { get; private set; }
        public string UltimaDecision { get; private set; }
        public bool ClinicaElegida { get; private set; }
        public bool AyudoEnClinica { get; private set; }

        AudioSource _audio;
        AudioClip _aviso;

        public static RumboCarrera Asegurar()
        {
            if (Instancia != null)
                return Instancia;

            var go = new GameObject("RumboCarrera");
            return go.AddComponent<RumboCarrera>();
        }

        void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this;
            DontDestroyOnLoad(gameObject);
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
            _aviso = CrearAviso();
        }

        public void ElegirClinica(bool ayudar)
        {
            if (ClinicaElegida)
                return;

            ClinicaElegida = true;
            AyudoEnClinica = ayudar;
            if (ayudar)
                SumarMedicina();
            else
                Anotar("Seguiste de largo en la clínica. Medicina sigue en " + Medicina + ".");
        }

        public void SumarSistemas()
        {
            Sistemas++;
            UltimaDecision = "Ingeniería de Sistemas";
            Anotar("Ingeniería de Sistemas +1.");
            _audio.PlayOneShot(_aviso, 0.45f);
        }

        public void SumarMedicina()
        {
            Medicina++;
            UltimaDecision = "Medicina";
            Anotar("Medicina +1.");
            _audio.PlayOneShot(_aviso, 0.45f);
        }

        public void Reiniciar()
        {
            Sistemas = 0;
            Medicina = 0;
            Tercera = 0;
            UltimaDecision = null;
            ClinicaElegida = false;
            AyudoEnClinica = false;
        }

        public string Resumen()
        {
            return "Sistemas " + Sistemas + "   ·   Medicina " + Medicina + "   ·   " + NombreTercera + " " + Tercera;
        }

        void Anotar(string frase)
        {
            GameSession.Ensure().SetObjective(frase + "  " + Resumen());
            if (RecorridoNiveles.Asegurar() != null)
                RecorridoNiveles.Instancia.Refrescar();
        }

        static AudioClip CrearAviso()
        {
            const int muestrasPorSegundo = 22050;
            const float duracion = 0.28f;
            int total = Mathf.CeilToInt(muestrasPorSegundo * duracion);
            var datos = new float[total];
            for (int i = 0; i < total; i++)
            {
                float t = i / (float)muestrasPorSegundo;
                float nota = t < 0.12f ? 523.25f : 659.25f;
                float dentro = t < 0.12f ? t / 0.12f : (t - 0.12f) / 0.16f;
                float envolvente = Mathf.Sin(Mathf.Clamp01(dentro) * Mathf.PI);
                datos[i] = Mathf.Sin(2f * Mathf.PI * nota * t) * envolvente * 0.35f;
            }

            var clip = AudioClip.Create("AvisoCarrera", total, 1, muestrasPorSegundo, false);
            clip.SetData(datos, 0);
            return clip;
        }
    }
}
