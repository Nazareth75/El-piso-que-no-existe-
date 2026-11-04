using UnityEngine;

namespace Piso7.Campus
{
    /// <summary>
    /// Nombre del lugar que se muestra arriba, al estilo del menú del recorrido.
    /// </summary>
    public class CampusPlaceInfo : MonoBehaviour
    {
        [SerializeField] string piso = "Planta baja";
        [SerializeField] string nombre = "Entrada principal";

        public string Piso => piso;
        public string Nombre => nombre;

        public void Configure(string floorName, string placeName)
        {
            piso = floorName;
            nombre = placeName;
        }
    }
}
