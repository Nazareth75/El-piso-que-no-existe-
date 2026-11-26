using UnityEngine;

public class JugadorCampus : MonoBehaviour
{
    public Sprite[] frames;
    public float caminar = 3.8f;
    public float correr = 6.5f;

    Rigidbody2D cuerpo;
    SpriteRenderer dibujo;
    float anim;
    Vector2 intencion;
    readonly RaycastHit2D[] impactos=new RaycastHit2D[32];
    readonly Collider2D[] solapados=new Collider2D[32];
    ContactFilter2D filtro;
    CapsuleCollider2D pies;
    PhysicsMaterial2D materialPies;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        dibujo = GetComponent<SpriteRenderer>();
        pies=GetComponent<CapsuleCollider2D>();
        materialPies=new PhysicsMaterial2D("Pies sin friccion"){friction=0,bounciness=0};
        if(pies!=null)pies.sharedMaterial=materialPies;
        filtro=new ContactFilter2D();filtro.SetLayerMask(Physics2D.AllLayers);filtro.useTriggers=false;
    }

    void Update()
    {
        var juego = DirectorCampus.Instancia;
        if (juego == null || !juego.jugando || juego.bloqueado)
        {
            intencion=Vector2.zero;
            if (cuerpo != null) cuerpo.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 eje = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (eje.sqrMagnitude > 1f) eje.Normalize();

        bool quiereCorrer = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && eje.sqrMagnitude > 0.01f;
        float velocidad = caminar;
        if (quiereCorrer && juego.estamina > 0f)
        {
            velocidad = correr;
            juego.estamina -= 30f * Time.deltaTime;
        }
        else if (juego.estamina < 100f)
        {
            juego.estamina += (eje.sqrMagnitude > 0.01f ? 8f : 18f) * Time.deltaTime;
        }

        juego.estamina = Mathf.Clamp(juego.estamina, 0f, 100f);
        intencion = eje * velocidad;
        if (eje.sqrMagnitude > .01f && cuerpo != null && cuerpo.linearVelocity.sqrMagnitude > .1f)
            SonidoCampus.Paso(velocidad > caminar + .2f, EstadoPartida.enPisoX);

        if (eje.x < -0.02f) dibujo.flipX = true;
        else if (eje.x > 0.02f) dibujo.flipX = false;

        if (frames != null && frames.Length > 0)
        {
            if (eje.sqrMagnitude > 0.01f) anim += Time.deltaTime * (velocidad > caminar + 0.2f ? 12f : 8f);
            dibujo.sprite = frames[Mathf.FloorToInt(anim) % frames.Length];
        }

        dibujo.sortingOrder = 120 - Mathf.RoundToInt(transform.position.y * 3f);
    }

    void FixedUpdate()
    {
        if(cuerpo==null || pies==null)return;
        AjustarPies();
        var juego=DirectorCampus.Instancia;
        if(juego==null || !juego.jugando || juego.bloqueado){cuerpo.linearVelocity=Vector2.zero;return;}
        // Rescue a foot collider left inside a wall after a scene/scale change.
        for(int paso=0;paso<4;paso++){
            int n=Physics2D.OverlapCapsule(pies.bounds.center,pies.bounds.size,CapsuleDirection2D.Horizontal,0,filtro,solapados);
            Vector2 correccion=Vector2.zero;float penetracion=0;
            for(int i=0;i<n;i++){
                if(solapados[i]==pies || solapados[i].attachedRigidbody==cuerpo)continue;
                var d=Physics2D.Distance(pies,solapados[i]);
                if(d.isOverlapped && -d.distance>penetracion){penetracion=-d.distance;correccion=d.normal*(d.distance-.01f);}
            }
            if(penetracion<.001f)break;
            cuerpo.position+=Vector2.ClampMagnitude(correccion,.7f);Physics2D.SyncTransforms();
        }
        Vector2 restante=intencion*Time.fixedDeltaTime,total=Vector2.zero;
        for(int paso=0;paso<3 && restante.sqrMagnitude>.000001f;paso++){
            float largo=restante.magnitude;Vector2 dir=restante/largo;
            int n=Physics2D.CapsuleCast((Vector2)pies.bounds.center+total,pies.bounds.size,CapsuleDirection2D.Horizontal,0,dir,filtro,impactos,largo+.025f);
            float libre=largo;Vector2 normal=Vector2.zero;
            for(int i=0;i<n;i++){
                var hit=impactos[i];if(hit.collider==pies || hit.rigidbody==cuerpo || Vector2.Dot(dir,hit.normal)>=-.001f)continue;
                float distancia=Mathf.Max(0,hit.distance-.025f);
                if(distancia<libre){libre=distancia;normal=hit.normal;}
            }
            Vector2 tramo=dir*libre;total+=tramo;restante-=tramo;
            if(normal==Vector2.zero)break;
            // Slide along the contacted surface instead of sticking at corners.
            restante-=normal*Mathf.Min(0,Vector2.Dot(restante,normal));
        }
        cuerpo.linearVelocity=total/Time.fixedDeltaTime;
    }

    public Vector2 BuscarLugarLibre(Vector2 destino,Vector2 limites)
    {
        AjustarPies();
        for(int anillo=0;anillo<=22;anillo++){
            int muestras=anillo==0?1:anillo*8;
            for(int i=0;i<muestras;i++){
                float a=i*Mathf.PI*2/muestras;
                Vector2 p=destino+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*anillo*.12f;
                if(p.x<.95f || p.y<1.05f || p.x>limites.x-.95f || p.y>limites.y-.95f)continue;
                int n=Physics2D.OverlapCapsule(p+new Vector2(0,.15f),new Vector2(.51f,.29f),CapsuleDirection2D.Horizontal,0,filtro,solapados);
                bool libre=true;
                for(int j=0;j<n;j++)if(solapados[j]!=pies && solapados[j].attachedRigidbody!=cuerpo){libre=false;break;}
                if(libre)return p;
            }
        }
        Debug.LogWarning("No se encontró un punto libre cerca del destino: "+destino);
        return destino;
    }

    public void AjustarAltura(float altura)
    {
        float alto=dibujo!=null && dibujo.sprite!=null?dibujo.sprite.bounds.size.y:0;
        if(alto>.01f)transform.localScale=Vector3.one*Mathf.Clamp(altura/alto,.5f,2f);
        AjustarPies();
    }
    void LateUpdate(){AjustarPies();}
    public void SincronizarPies(){AjustarPies();}
    void AjustarPies()
    {
        if(pies==null)return;
        float sx=Mathf.Max(.01f,Mathf.Abs(transform.lossyScale.x)),sy=Mathf.Max(.01f,Mathf.Abs(transform.lossyScale.y));
        pies.size=new Vector2(.46f/sx,.24f/sy);pies.offset=new Vector2(0,.15f/sy);
    }
    void OnDestroy(){if(materialPies!=null)Destroy(materialPies);}

    public void Detener()
    {
        intencion=Vector2.zero;
        if (cuerpo != null) cuerpo.linearVelocity = Vector2.zero;
    }
}
