public static class EstadoPartida
{
    public static int sistemas;
    public static int medicina;
    public static int tercera;
    public const string NombreTercera = "Por definir";

    public static bool entrada;
    public static bool nota;
    public static bool hall;
    public static bool clinica;
    public static bool ayudo;
    public static bool sistemasListo;
    public static bool huella;
    public static bool huida;
    public static bool huidaActiva;
    public static bool gameRoom;
    public static bool desperto;
    public static bool pista;
    public static bool enPisoX;
    public static bool pisoCruzado;

    public static int personaje;

    public static bool Conoce => entrada && nota && hall;

    public static bool LlegadaLista => Conoce;

    public static bool DecisionesListas => desperto && clinica && sistemasListo;

    public static void Reiniciar()
    {
        EstadoPisoSiete.Reiniciar();
        SalasAcademicas.Reiniciar();
        sistemas = 0;
        medicina = 0;
        tercera = 0;
        entrada = false;
        nota = false;
        hall = false;
        clinica = false;
        ayudo = false;
        sistemasListo = false;
        huella = false;
        huida = false;
        huidaActiva = false;
        gameRoom = false;
        desperto = false;
        pista = false;
        enPisoX = false;
        pisoCruzado = false;
    }

    public static string Objetivo()
    {
        if (pisoCruzado) return "Cruzaste el piso que no existe.";
        if (enPisoX) return EstadoPisoSiete.Objetivo();
        if (!desperto)
        {
            if (!entrada) return "Nivel 1 · Conocer la universidad. Entra por la puerta.";
            if (!nota) return "Nivel 1 · Conocer la universidad. Pide información en recepción.";
            if (!hall) return "Nivel 1 · Conocer la universidad. Cruza el hall.";
            if (!huella) return "Nivel 1 · Conocer la universidad. Valida la huella para ver el ascensor.";
            return "Nivel 1 · Conocer la universidad. Ve el ascensor.";
        }

        if (!pista) return "Nivel 2 · Despiertas. No hay nadie. Lee la nota de recepción.";
        if (!clinica && !sistemasListo) return "Nivel 2 · La universidad está vacía. Revisa la clínica y el ascensor.";
        if (!sistemasListo) return "Nivel 2 · En el ascensor, busca el piso que no está.";
        return "Nivel 3 · Entra a PISO X desde el ascensor.";
    }

    public static string Rumbo()
    {
        if (sistemas > medicina) return "Tu primer día te acerca a Ingeniería de Sistemas.";
        if (medicina > sistemas) return "Tu primer día te acerca a Medicina.";
        return "Ninguna carrera se definió todavía.";
    }
}
