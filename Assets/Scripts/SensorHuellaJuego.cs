using System;

// Deterministic timing challenge: three clean readings unlock the turnstiles.
public sealed class SensorHuellaJuego
{
    float tiempo;
    float espera;
    public int Aciertos {get;private set;}
    public float Posicion {get {float t=tiempo%2f;return t<=1?t:2-t;}}
    public float Centro=>Aciertos==0?.35f:Aciertos==1?.65f:.5f;
    public bool EnZona=>Math.Abs(Posicion-Centro)<=.14f;
    public string Mensaje {get;private set;}="Pulsa Espacio o el botón cuando la línea esté en la zona verde.";
    public void Reiniciar(){tiempo=0;espera=.3f;Aciertos=0;Mensaje="Pulsa Espacio o el botón cuando la línea esté en la zona verde.";}
    public void Avanzar(float dt){if(Aciertos>=3)return;tiempo+=Math.Max(0,dt)*.65f;espera=Math.Max(0,espera-dt);}
    public bool Leer()
    {
        if(Aciertos>=3 || espera>0)return false;
        espera=.25f;
        if(!EnZona){Mensaje="Lectura inestable. Espera a la zona verde y vuelve a intentarlo.";return false;}
        Aciertos++;tiempo=0;
        Mensaje=Aciertos==3?"Huella validada. Acceso permitido.":"Lectura correcta. Continúa con la siguiente validación.";
        return Aciertos==3;
    }
}
