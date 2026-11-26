using System.Collections.Generic;

// El progreso del nivel sobrevive a un reintento, no a una nueva partida.
public static class EstadoPisoSiete
{
    public static readonly HashSet<string> objetos = new HashSet<string>();
    public static readonly HashSet<string> misiones = new HashSet<string>();
    public static bool presentado;
    public static int vida = 100;
    public static int golpes;
    public static bool Tiene(string id) => objetos.Contains(id);
    public static bool Lista(string id) => misiones.Contains(id);
    public static bool PortalListo => Lista("energia") && Lista("clinica") && Lista("archivo") &&
        Lista("antidoto") && Lista("transmision") && Lista("estructura") && Tiene("tarjeta");

    public static void Reiniciar()
    {
        objetos.Clear();
        misiones.Clear();
        presentado = false;
        vida = 100;
        golpes = 0;
    }

    public static string Objetivo()
    {
        if (!Tiene("fusible")) return "Piso 7 · Recoge el fusible en ingeniería (abajo a la derecha).";
        if (!Lista("energia")) return "Piso 7 · Restaura la energía en la sala eléctrica (arriba, al centro).";
        if (!Lista("clinica")) return "Piso 7 · Estabiliza al herido en la clínica (arriba a la izquierda).";
        if (!Lista("archivo")) return "Piso 7 · Recupera la tarjeta y el registro en el archivo (izquierda).";
        if (!Tiene("muestra")) return "Piso 7 · Recoge una muestra junto al núcleo central.";
        if (!Lista("antidoto")) return "Piso 7 · Sintetiza el antídoto en el laboratorio (arriba a la derecha).";
        if (!Lista("transmision")) return "Piso 7 · Emite la evidencia desde streaming (abajo a la izquierda).";
        if (!Lista("estructura")) return "Piso 7 · Refuerza la salida en ingeniería (abajo a la derecha).";
        return "Piso 7 · Todo listo. Activa el núcleo central y escapa.";
    }
}
