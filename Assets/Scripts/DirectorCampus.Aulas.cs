using UnityEngine;

public partial class DirectorCampus
{
    EntradaAcademica aulaEnCurso;
    GameObject exteriorAcademico;
    GameObject marcoExteriorAcademico;
    Vector2 retornoAcademico;
    float anchoExteriorAcademico;
    float altoExteriorAcademico;
    Texture2D laminaAcademica;

    public bool EnAulaAcademica => aulaEnCurso != null && salaId == "aulaAcademica";
    public string AulaActual => EnAulaAcademica ? aulaEnCurso.id : "";
    public int PisoActual => pisoNumero;
    public Vector2 PosicionJugador => jugador != null ? (Vector2)jugador.transform.position : Vector2.zero;
    public string NombreAulaActual => EnAulaAcademica ? aulaEnCurso.nombre : "";

    void PrepararAulas(int piso)
    {
        // Estas salas pasan de un diálogo en el plano a un interior transitable.
        puntosPiso.RemoveAll(p => p.id == "mac" || p.id == "software" || p.id == "labcomp" ||
            p.id == "informatica" || p.id == "semiologia" || (piso == 2 && p.id == "postgrado"));
        foreach (var sala in SalasAcademicas.Todas)
        {
            if (sala.piso != piso) continue;
            PuntoPiso(sala.id, "Entrar a " + sala.nombre, sala.Posicion, 1.05f);
        }
    }

    PuntoCampus CercaEnAula(Vector2 posicion)
    {
        Vector2 salida = SalasAcademicas.Interior(355, 645);
        if (Vector2.Distance(posicion, salida) < 1.65f)
            return new PuntoCampus { id = "salirAcademica", nombre = "Volver al pasillo del piso " + pisoNumero, posicion = salida, radio = 1.65f };
        Vector2 actividad = SalasAcademicas.Actividad(aulaEnCurso.tipo);
        Vector2 apunte = SalasAcademicas.Apunte(aulaEnCurso.tipo);
        if (!SalasAcademicas.apuntes.Contains(aulaEnCurso.id) && Vector2.Distance(posicion, apunte) < 1.35f)
            return new PuntoCampus { id = "apunteAcademico", nombre = "Recoger material de " + aulaEnCurso.nombre, posicion = apunte, radio = 1.35f };
        if (Vector2.Distance(posicion, actividad) < 1.4f)
            return new PuntoCampus { id = "actividadAcademica", nombre = aulaEnCurso.tipo == 0 ? "Consultar la actividad del aula" : aulaEnCurso.tipo == 1 ? "Usar el equipo del laboratorio" : "Consultar el monitor de práctica", posicion = actividad, radio = 1.4f };
        return null;
    }

    public void EntrarAulaAcademica(string id)
    {
        var sala = SalasAcademicas.Buscar(id);
        if (sala == null || !dentro || salaId != "piso" || sala.piso != pisoNumero) return;
        if (laminaAcademica == null)
            laminaAcademica = RecursosCampus.Textura("Interiores/aulas_laboratorio_clinica.png");
        if (laminaAcademica == null || laminaAcademica.width != 2172 || laminaAcademica.height != 724)
        {
            Decir("INTERIORES", "No se pudo cargar la lámina original de aulas, laboratorio y clínica.", "");
            return;
        }
        SonidoCampus.Emitir("puerta", .55f);
        jugador.Detener();
        retornoAcademico = jugador.transform.position;
        exteriorAcademico = interiorGo;
        marcoExteriorAcademico = marcoGo;
        anchoExteriorAcademico = salaAncho;
        altoExteriorAcademico = salaAlto;
        if (exteriorAcademico != null) exteriorAcademico.SetActive(false);
        if (marcoExteriorAcademico != null) marcoExteriorAcademico.SetActive(false);
        aulaEnCurso = sala;
        salaId = "aulaAcademica";
        salaAncho = 18f;
        salaAlto = 18f;
        puedeSalir = Time.time + 0.6f;

        interiorGo = new GameObject(sala.nombre + " · Piso " + pisoNumero);
        var dibujo = interiorGo.AddComponent<SpriteRenderer>();
        // Recorte del panel original en Unity: se conservan colores, muebles y rótulos de la foto.
        var sprite = Sprite.Create(laminaAcademica, new Rect(sala.tipo * 724, 0, 724, 724), new Vector2(0.5f, 0.5f), 724f / 18f);
        dibujo.sprite = sprite;
        dibujo.sortingOrder = -20;
        interiorGo.AddComponent<ArteSalaAcademica>().sprite = sprite;
        interiorGo.transform.position = new Vector3(9f, 9f, 0f);
        marcoGo = new GameObject("Muebles y paredes · " + sala.nombre);
        MuroAcademico(new Rect(0, 0, 724, 245));
        MuroAcademico(new Rect(0, 85, 48, 550));
        MuroAcademico(new Rect(672, 85, 52, 550));
        MuroAcademico(new Rect(0, 625, 300, 99));
        MuroAcademico(new Rect(410, 625, 314, 99));
        MuroAcademico(new Rect(300, 705, 110, 19));
        foreach (var mueble in SalasAcademicas.Muebles(sala.tipo)) MuroAcademico(mueble);
        jugador.transform.localScale = Vector3.one;
        PonerJugador(SalasAcademicas.Interior(355, 630));
        SalasAcademicas.visitadas.Add(sala.id);
        panel = PanelCampus.Ninguno;
        camara.backgroundColor = new Color(0.12f, 0.06f, 0.22f);
        EnfocarAula();
    }

    void MuroAcademico(Rect rect)
    {
        Vector2 centro = SalasAcademicas.Interior(rect.center.x, rect.center.y);
        Borde(centro.x, centro.y, rect.width / 724f * 18f, rect.height / 724f * 18f);
    }

    void EnfocarAula()
    {
        if (camara == null) return;
        camara.transform.position = new Vector3(9f, 9f, -10f);
        camara.orthographicSize = Mathf.Max(9.5f, 9.5f / camara.aspect);
    }

    public void SalirAulaAcademica()
    {
        if (!EnAulaAcademica) return;
        SonidoCampus.Emitir("puerta", .55f);
        jugador.Detener();
        if (interiorGo != null){interiorGo.SetActive(false);Destroy(interiorGo);}
        if (marcoGo != null){marcoGo.SetActive(false);Destroy(marcoGo);}
        interiorGo = exteriorAcademico;
        marcoGo = marcoExteriorAcademico;
        exteriorAcademico = null;
        marcoExteriorAcademico = null;
        aulaEnCurso = null;
        salaId = "piso";
        salaAncho = anchoExteriorAcademico;
        salaAlto = altoExteriorAcademico;
        if (interiorGo != null) interiorGo.SetActive(true);
        if (marcoGo != null) marcoGo.SetActive(true);
        panel = PanelCampus.Ninguno;
        puedeSalir = Time.time + 0.6f;
        PonerJugador(retornoAcademico);
    }

    void DescartarExteriorAcademico()
    {
        if (exteriorAcademico != null) Destroy(exteriorAcademico);
        if (marcoExteriorAcademico != null) Destroy(marcoExteriorAcademico);
        exteriorAcademico = null;
        marcoExteriorAcademico = null;
        aulaEnCurso = null;
    }

    void UsarAulaAcademica(string id)
    {
        if (id == "salirAcademica") { SalirAulaAcademica(); return; }
        if (id == "apunteAcademico")
        {
            if (!SalasAcademicas.apuntes.Add(aulaEnCurso.id)) return;
            SonidoCampus.Emitir("recoger", .7f);
            Decir("MATERIAL RECOGIDO", "Guardas el material de " + aulaEnCurso.nombre + ". Puedes seguir explorando o volver al pasillo por la puerta inferior.", "");
        }
        else if (id == "actividadAcademica")
        {
            string mensaje = aulaEnCurso.tipo == 0
                ? "El proyector muestra la actividad de la clase. Hay material para recoger junto a las mesas."
                : aulaEnCurso.tipo == 1
                    ? "El equipo del laboratorio está disponible. Revisa el material junto a la fila de computadoras."
                    : "El monitor muestra un caso de simulación de semiología. Puedes recoger la ficha de práctica junto a los asientos.";
            Decir(aulaEnCurso.nombre.ToUpperInvariant(), mensaje, "");
        }
    }

    public void DibujarAulaAcademica(float ancho, float alto)
    {
        if (!EnAulaAcademica) return;
        GUI.Box(new Rect(12f, 12f, 285f, 54f), "Piso " + pisoNumero + " · " + aulaEnCurso.nombre + "\nE: interactuar · Puerta inferior: volver");
        if (SalasAcademicas.apuntes.Contains(aulaEnCurso.id))
            GUI.Box(new Rect(12f, alto - 54f, 240f, 38f), "Material de esta sala recogido");
    }

    public void ProbarPisoAcademico(int piso)
    {
        if (piso < 2 || piso > 5) return;
        if (dentro) SalirSala();
        jugando = true;
        panel = PanelCampus.Ninguno;
        EntrarPisoNumero(piso);
    }

    public void ProbarAulaAcademica(string id)
    {
        var sala = SalasAcademicas.Buscar(id);
        if (sala == null) return;
        ProbarPisoAcademico(sala.piso);
        PonerJugador(sala.Posicion);
        EntrarAulaAcademica(id);
    }

    void OnDestroy()
    {
        if (laminaAcademica != null) Destroy(laminaAcademica);
    }
}
