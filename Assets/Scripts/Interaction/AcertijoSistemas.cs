using UnityEngine;

namespace Piso7.Interaction
{
    public class AcertijoSistemas : MonoBehaviour, IInteractable
    {
        bool abierto;
        bool tarjeta;
        string resultado = "";

        public string Prompt => tarjeta ? "Tarjeta obtenida" : "Presiona E para resolver el acertijo";
        public bool CanInteract => !abierto && !tarjeta;

        public void Interact(PlayerInteractor interactor)
        {
            abierto = true;
            resultado = "";
        }

        void OnGUI()
        {
            if (!abierto) return;

            var caja = new Rect(Screen.width / 2 - 230, Screen.height / 2 - 155, 460, 310);
            GUI.Box(caja, "ACERTIJO DE SISTEMAS");
            GUI.Label(new Rect(caja.x + 25, caja.y + 45, 410, 45), "¿Qué convierte un programa en instrucciones para la computadora?");
            if (GUI.Button(new Rect(caja.x + 55, caja.y + 105, 350, 32), "A. Un compilador")) Resolver(true);
            if (GUI.Button(new Rect(caja.x + 55, caja.y + 145, 350, 32), "B. Un monitor")) Resolver(false);
            if (GUI.Button(new Rect(caja.x + 55, caja.y + 185, 350, 32), "C. Un teclado")) Resolver(false);
            GUI.Label(new Rect(caja.x + 55, caja.y + 230, 350, 25), resultado);
            if (GUI.Button(new Rect(caja.x + 165, caja.y + 265, 130, 28), tarjeta ? "Cerrar" : "Cancelar")) abierto = false;
        }

        void Resolver(bool correcta)
        {
            if (correcta)
            {
                tarjeta = true;
                resultado = "Tarjeta obtenida";
            }
            else resultado = "No es la respuesta correcta.";
        }
    }
}
