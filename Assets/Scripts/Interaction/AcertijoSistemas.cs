using UnityEngine;
using UnityEngine.Events;

namespace Piso7.Interaction
{
    public class AcertijoSistemas : MonoBehaviour, IInteractable
    {
        public UnityEvent alAcertar = new UnityEvent();
        bool abierto;
        bool completado;
        bool pistaObtenida;
        float pistaHasta;

        public string Prompt => completado ? "Acertijo completado" : "Presiona E para resolver el acertijo";
        public bool CanInteract => !abierto && !completado;

        public void Interact(PlayerInteractor interactor)
        {
            abierto = true;
        }

        void OnGUI()
        {
            if (Time.time < pistaHasta)
                GUI.Box(new Rect(Screen.width - 300, 42, 270, 42), "PISTA: la biblioteca está a la derecha");
            if (!abierto) return;
            var caja = new Rect(Screen.width / 2 - 260, Screen.height / 2 - 180, 520, 360);
            GUI.Box(caja, "ACERTIJO DE SISTEMAS");
            if (pistaObtenida)
            {
                GUI.Label(new Rect(caja.x + 150, caja.y + 105, 240, 32), "Pista obtenida");
                if (GUI.Button(new Rect(caja.x + 175, caja.y + 190, 170, 36), "Cerrar")) abierto = false;
                return;
            }
            GUI.Label(new Rect(caja.x + 35, caja.y + 55, 450, 45), "¿Qué convierte un programa en instrucciones para la computadora?");
            if (GUI.Button(new Rect(caja.x + 65, caja.y + 120, 390, 36), "A. Un compilador")) Acertar();
            if (GUI.Button(new Rect(caja.x + 65, caja.y + 168, 390, 36), "B. Un monitor")) Incorrecta();
            if (GUI.Button(new Rect(caja.x + 65, caja.y + 216, 390, 36), "C. Un teclado")) Incorrecta();
            if (GUI.Button(new Rect(caja.x + 175, caja.y + 285, 170, 32), "Cerrar")) abierto = false;
        }

        void Incorrecta() { abierto = false; }
        void Acertar()
        {
            completado = true;
            pistaObtenida = true;
            pistaHasta = Time.time + 6f;
            // TODO: cambiar la siguiente línea por RumboCarrera.Instance.SumarSistemas() al juntar la rama de Kathlen.
            alAcertar.Invoke();
            if (Piso7.Core.GameSession.Instance != null)
                Piso7.Core.GameSession.Instance.SetObjective("Pista obtenida: busca la biblioteca.");
        }
    }
}
