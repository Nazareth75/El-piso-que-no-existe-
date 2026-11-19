using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace Piso7.Interaction
{
    public class AcertijoSistemas : MonoBehaviour, IInteractable
    {
        [SerializeField] int computadora = 1;
        static int puntos;
        static int sistemas;
        static AcertijoSistemas abiertaActual;
        public UnityEvent alSumarSistemas = new UnityEvent();
        static readonly bool[] completados = new bool[4];
        static readonly Pregunta[] tecnologia = {
            new Pregunta("¿Qué convierte código en instrucciones?", "Compilador", "Monitor", "Teclado"), new Pregunta("¿Qué almacena datos temporalmente?", "RAM", "CPU", "Mouse"), new Pregunta("¿Qué significa URL?", "Dirección web", "Virus local", "Unidad de red"), new Pregunta("¿Cuál es un sistema operativo?", "Linux", "HTML", "USB"), new Pregunta("¿Qué protege una red?", "Firewall", "Impresora", "Pantalla") };
        static readonly Pregunta[] acertijos = {
            new Pregunta("Tengo teclas pero no abro puertas.", "Teclado", "Candado", "Reloj"), new Pregunta("Viajo por cables sin ser tren.", "Datos", "Ascensor", "Papel"), new Pregunta("Tengo memoria pero no pienso.", "Computadora", "Libro", "Silla"), new Pregunta("Soy una ventana sin vidrio.", "Navegador", "Puerta", "Espejo"), new Pregunta("Me apagas y sigo guardando todo.", "Disco duro", "Lámpara", "Parlante") };
        static readonly string[] pasos = { "Encender equipo", "Iniciar sesión", "Abrir programa", "Guardar trabajo" };
        static readonly string[] colores = { "ROJO", "AZUL", "VERDE", "AMARILLO" };
        bool abierto, fallo, ganado;
        int[] orden = new int[5];
        int[] botones = new int[4];
        int actual, siguiente;
        bool[] usados = new bool[4];
        int[] memoria = new int[4];
        float mostrarMemoriaHasta;
        float pistaHasta;
        int ignorarEHasta;
        int bloquearInteraccionHasta;

        bool EnZona(float x)
        {
            var zona = GetComponent<Collider2D>();
            return zona != null && Mathf.Abs(x - transform.position.x) <= zona.bounds.extents.x;
        }

        public string Prompt => completados[computadora - 1] ? "Ya completado" : "Presiona E para usar computadora " + computadora;
        public bool CanInteract
        {
            get
            {
                var jugador = GameObject.FindGameObjectWithTag("Player");
                return abiertaActual == null && !abierto && Time.frameCount > bloquearInteraccionHasta && jugador != null && EnZona(jugador.transform.position.x);
            }
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (abiertaActual != null || Cercana(interactor.transform.position.x) != this) return;
            abierto = true;
            abiertaActual = this;
            ignorarEHasta = Time.frameCount + 1;
            if (!completados[computadora - 1]) Iniciar();
        }

        void Update()
        {
            if (!abierto || Time.frameCount <= ignorarEHasta || Piso7.Core.GameMenu.BloqueaJuego) return;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) Cerrar();
        }

        void Cerrar()
        {
            abierto = false;
            abiertaActual = null;
            bloquearInteraccionHasta = Time.frameCount + 1;
        }

        static AcertijoSistemas Cercana(float x)
        {
            AcertijoSistemas mejor = null;
            float distancia = float.MaxValue;
            foreach (var candidata in FindObjectsByType<AcertijoSistemas>(FindObjectsSortMode.None))
            {
                if (!candidata.EnZona(x)) continue;
                float d = Mathf.Abs(candidata.transform.position.x - x);
                if (d < distancia) { distancia = d; mejor = candidata; }
            }
            return mejor;
        }

        void Iniciar()
        {
            fallo = ganado = false; actual = siguiente = 0;
            for (int i = 0; i < 5; i++) orden[i] = i;
            Mezclar(orden); Array.Clear(usados, 0, usados.Length);
            for (int i = 0; i < 4; i++) botones[i] = memoria[i] = i;
            Mezclar(botones); Mezclar(memoria);
            mostrarMemoriaHasta = Time.unscaledTime + 2.5f;
        }

        void OnGUI()
        {
            if (Piso7.Core.GameMenu.BloqueaJuego) return;
            Color anterior = GUI.contentColor;
            GUI.contentColor = Color.black;
            try
            {
                if (computadora == 1) GUI.Label(new Rect(155, 46, 250, 28), "Puntos: " + puntos + "   Sistemas: " + sistemas);
                GUI.contentColor = Color.white;
                if (Time.time < pistaHasta) GUI.Box(new Rect(Screen.width - 310, 42, 280, 38), "PISTA OBTENIDA: BIBLIOTECA ->");
                if (!abierto) return;
                var caja = new Rect(Screen.width / 2 - 260, Screen.height / 2 - 180, 520, 360);
                GUI.Box(caja, "COMPUTADORA " + computadora);
                if (ganado) { Mensaje(caja, "Pista obtenida\n¡Completado! +5 puntos"); return; }
                if (completados[computadora - 1]) { Mensaje(caja, "Ya completado"); return; }
                if (fallo) { Mensaje(caja, "Fallaste. Puedes reintentar."); return; }
                if (computadora == 1) Preguntas(caja, tecnologia);
                else if (computadora == 2) Preguntas(caja, acertijos);
                else if (computadora == 3) Ordenar(caja); else Memoria(caja);
            }
            finally { GUI.contentColor = anterior; }
        }

        void Preguntas(Rect caja, Pregunta[] lista)
        {
            var p = lista[orden[actual]];
            GUI.Label(new Rect(caja.x + 30, caja.y + 45, 460, 55), (actual + 1) + "/5  " + p.texto);
            for (int i = 0; i < 3; i++) if (GUI.Button(new Rect(caja.x + 65, caja.y + 115 + i * 48, 390, 36), p.opciones[i])) Responder(i == 0);
        }

        void Responder(bool correcta) { if (!correcta) { fallo = true; return; } if (++actual == 5) Ganar(); }

        void Ordenar(Rect caja)
        {
            GUI.Label(new Rect(caja.x + 30, caja.y + 45, 460, 45), "Ordena el proceso: elige el siguiente paso.");
            for (int i = 0; i < 4; i++) { int paso = botones[i]; if (!usados[paso] && GUI.Button(new Rect(caja.x + 65, caja.y + 100 + i * 48, 390, 36), pasos[paso]))
            { if (paso != siguiente) { fallo = true; return; } usados[paso] = true; if (++siguiente == 4) Ganar(); } }
        }

        void Memoria(Rect caja)
        {
            if (Time.unscaledTime < mostrarMemoriaHasta) { GUI.Label(new Rect(caja.x + 30, caja.y + 70, 460, 40), "Memoriza: " + colores[memoria[0]] + " - " + colores[memoria[1]] + " - " + colores[memoria[2]] + " - " + colores[memoria[3]]); return; }
            GUI.Label(new Rect(caja.x + 30, caja.y + 45, 460, 35), "Repite la secuencia de colores: " + siguiente + "/4");
            for (int i = 0; i < 4; i++) if (GUI.Button(new Rect(caja.x + 65 + (i % 2) * 200, caja.y + 105 + (i / 2) * 55, 180, 40), colores[i]))
            { if (i != memoria[siguiente]) { fallo = true; return; } if (++siguiente == 4) Ganar(); }
        }

        void Mensaje(Rect caja, string texto)
        {
            GUI.Label(new Rect(caja.x + 80, caja.y + 105, 360, 35), texto);
            if (fallo && GUI.Button(new Rect(caja.x + 175, caja.y + 165, 170, 36), "Reintentar")) Iniciar();
            if (GUI.Button(new Rect(caja.x + 175, caja.y + 225, 170, 36), "Cerrar")) Cerrar();
        }

        void Ganar()
        {
            ganado = true;
            completados[computadora - 1] = true;
            puntos += 5;
            sistemas++;
            pistaHasta = Time.time + 6f;
            Piso7.Core.RecorridoNiveles.Asegurar().RegistrarSistemas();
            if (Piso7.Core.RumboCarrera.Asegurar().Sistemas == 0)
                Piso7.Core.RumboCarrera.Asegurar().SumarSistemas();
            alSumarSistemas.Invoke();
        }
        public static void ReiniciarProgreso()
        {
            puntos = 0;
            sistemas = 0;
            abiertaActual = null;
            System.Array.Clear(completados, 0, completados.Length);
        }

        static void Mezclar(int[] valores) { for (int i = valores.Length - 1; i > 0; i--) { int j = UnityEngine.Random.Range(0, i + 1); int t = valores[i]; valores[i] = valores[j]; valores[j] = t; } }
        class Pregunta { public string texto; public string[] opciones; public Pregunta(string texto, string correcta, string dos, string tres) { this.texto = texto; opciones = new[] { correcta, dos, tres }; } }
    }
}
