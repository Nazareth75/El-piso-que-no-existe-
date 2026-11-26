using UnityEngine;

// Compact compass next to the student. It never intercepts mouse input.
public static class GuiaRecorridoUI
{
    static Texture2D flecha;
    static GUIStyle tecla;
    static float angulo,velocidad;
    static string objetivoAnterior;

    static void Preparar()
    {
        if(flecha!=null)return;
        flecha=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        var pixels=new Color[1024];
        for(int y=0;y<32;y++)for(int x=0;x<32;x++){
            bool cabeza=x>=13 && x<=28 && Mathf.Abs(y-15.5f)<=(28-x)*.82f;
            bool cuerpo=x>=5 && x<=18 && y>=12 && y<=19;
            pixels[y*32+x]=cabeza||cuerpo?new Color(1,.70f,.29f,1):Color.clear;
        }
        flecha.SetPixels(pixels);flecha.Apply();
        tecla=new GUIStyle(GUI.skin.label){fontSize=16,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
        tecla.normal.textColor=new Color(.55f,1,.76f);
    }

    public static void Dibujar(DirectorCampus juego,float ancho,float alto)
    {
        if(!juego.TryGuiaRecorrido(out var origen,out var destino,out bool cerca,out string objetivo))return;
        var cam=Camera.main;if(cam==null)return;
        Preparar();
        float escala=Mathf.Max(.5f,Screen.height/720f);
        Vector3 p=cam.WorldToScreenPoint(origen),d=cam.WorldToScreenPoint(destino);
        if(p.z<=0 || d.z<=0)return;
        Vector2 jugador=new Vector2(p.x/escala,(Screen.height-p.y)/escala);
        Vector2 direccion=new Vector2(d.x-p.x,p.y-d.y);
        if(direccion.sqrMagnitude<.01f)direccion=Vector2.down;
        Vector2 centro=jugador+direccion.normalized*68f;
        centro.x=Mathf.Clamp(centro.x,30,ancho-30);
        centro.y=Mathf.Clamp(centro.y,32,alto-32);
        // Existing mission, stamina and inventory widgets keep their full space.
        bool hudIzquierdo=centro.x<360 && centro.y<400;
        bool hudDerecho=centro.x>ancho-540 && centro.y<196;
        if(hudIzquierdo || hudDerecho)centro=new Vector2(ancho*.5f,alto-66);
        // Interaction is anchored to the zone, never to the student's feet.
        Vector2 punto=new Vector2(d.x/escala,(Screen.height-d.y)/escala-30);
        if(cerca && punto.x>18 && punto.x<ancho-18 && punto.y>18 && punto.y<alto-18){
            var zona=new Rect(punto.x-14,punto.y-14,28,28);
            GUI.Label(zona,"E",tecla);
            return;
        }
        float deseado=Mathf.Atan2(direccion.y,direccion.x)*Mathf.Rad2Deg;
        if(objetivoAnterior!=objetivo){angulo=deseado;velocidad=0;objetivoAnterior=objetivo;}
        else if(Event.current.type==EventType.Repaint)angulo=Mathf.SmoothDampAngle(angulo,deseado,ref velocidad,.12f,720,Time.unscaledDeltaTime);
        Matrix4x4 anterior=GUI.matrix;
        // Compose in GUI coordinates so scaled screens cannot separate icon/pivot.
        GUI.matrix=anterior*Matrix4x4.TRS(new Vector3(centro.x,centro.y,0),Quaternion.Euler(0,0,angulo),Vector3.one);
        GUI.DrawTexture(new Rect(-13,-13,26,26),flecha);
        GUI.matrix=anterior;
    }
}
