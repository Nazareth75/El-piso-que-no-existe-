using System;

public static class RutaPlantaBaja
{
    // Interaction IDs, in the order of the guided first-day tour.
    public static readonly string[] Destinos={"entrada","recepcion","hall","admisiones","clinica","gestion","huella","servicios","auditorio","cafeteria","game","escaleras","ascensor"};
    public static readonly string[] Titulos={"Entrada: conoce a tu guía","Recepción: solicita información","Hall: ubícate en el campus","Admisiones: conoce el ingreso","Clínica: descubre sus servicios","Gestión financiera: consulta los pagos","Control de huella: valida tu acceso","Servicios estudiantiles: encuentra apoyo","Auditorio: conoce las actividades","Cafetería: ubica dónde comer","Game Room: completa el minijuego","Escaleras y baños: ubica los accesos","Ascensor: termina tu recorrido"};
    public static string TarjetaActual {
        get {
            string id=Completa?"ascensor":Destinos[Paso];
            return id=="gestion"?"gestion_financiera":id=="servicios"?"servicios_estudiantiles":id=="game"?"game_room":id=="escaleras"?"escaleras_banos":id=="huella"?"control_huella":id=="ascensor"?"primera_vez":id;
        }
    }
    public static int Paso {get;private set;}
    public static int Total=>Destinos.Length;
    public static bool Completa=>Paso>=Total;
    public static string Objetivo=>Completa?"Recorrido completo. Continúa la historia.":Titulos[Paso];
    public static void Reiniciar(){Paso=0;}
    public static bool Permite(string id)
    {
        int n=Array.IndexOf(Destinos,id);
        if(id=="gradas")return Completa;
        return n<0 || n<=Paso;
    }
    public static bool Completar(string tarjeta)
    {
        string id=tarjeta=="gestion_financiera"?"gestion":tarjeta=="servicios_estudiantiles"?"servicios":tarjeta=="game_room"?"game":tarjeta=="escaleras_banos"?"escaleras":tarjeta=="control_huella"?"huella":tarjeta=="primera_vez"?"ascensor":tarjeta;
        if(Completa || id!=Destinos[Paso])return false;
        Paso++;return true;
    }
}
