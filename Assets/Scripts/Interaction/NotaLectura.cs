using UnityEngine;

namespace Piso7.Interaction
{
    public class NotaLectura : MonoBehaviour, IInteractable
    {
        bool abierta;
        bool leida;

        public string Prompt => leida ? "Nota leída" : "Presiona E para leer la nota";
        public bool CanInteract => !abierta && !leida;

        public void Interact(PlayerInteractor interactor)
        {
            abierta = true;
        }

        void OnGUI()
        {
            if (Piso7.Core.GameMenu.BloqueaJuego) return;
            if (abierta)
            {
                var caja = new Rect(Screen.width / 2 - 230, Screen.height / 2 - 105, 460, 210);
                GUI.Box(caja, "NOTA");
                GUI.Label(new Rect(caja.x + 28, caja.y + 52, caja.width - 56, 70),
                    "Si encontraste esto, no intentes salir todavía. Descubre el piso que no existe.");
                if (GUI.Button(new Rect(caja.x + 165, caja.y + 150, 130, 32), "Cerrar"))
                {
                    abierta = false;
                    leida = true;
                }
            }

            if (leida && Camera.main != null)
            {
                Vector3 punto = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.2f);
                GUI.Label(new Rect(punto.x - 110, Screen.height - punto.y, 220, 26), "Busca el laboratorio de sistemas");
            }
        }
    }
}
