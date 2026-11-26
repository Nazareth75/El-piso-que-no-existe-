using UnityEngine;

public static class ColisionesCampus
{
    // Continuous strips aligned to the 1024x508 campus illustration.
    // Sensor gates and the entrance door keep their existing dynamic colliders.
    public static readonly Rect[] Estructura={
        new Rect(0,84,12,386),new Rect(1010,18,14,480),new Rect(66,16,946,10),
        new Rect(13,84,100,10),new Rect(142,103,12,200),new Rect(8,302,145,12),
        new Rect(184,88,194,10),new Rect(184,176,194,10),
        new Rect(184,95,10,81),new Rect(368,96,10,80),
        new Rect(514,150,499,14),
        new Rect(542,219,282,12),new Rect(542,348,282,12),
        new Rect(542,229,12,119),new Rect(812,229,12,119),
        new Rect(11,450,491,14),new Rect(245,336,176,10),
        new Rect(12,336,223,10),new Rect(232,342,11,109),new Rect(493,384,272,12)
    };
    public static void AgregarEstructura(Transform padre)
    {
        foreach(var r in Estructura){
            var go=new GameObject("Pared continua");go.transform.SetParent(padre,false);
            go.transform.localPosition=new Vector3(r.center.x/1024f*40f,(508-r.center.y)/508f*20f,0);
            var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(r.width/1024f*40f,r.height/508f*20f);
        }
    }

    public static void UnirParedesDePiso(Transform padre)
    {
        // Upper floors contain only static walls here; elevator gates live elsewhere.
        var body=padre.GetComponent<Rigidbody2D>();
        if(body==null)body=padre.gameObject.AddComponent<Rigidbody2D>();
        body.bodyType=RigidbodyType2D.Static;
        var composite=padre.GetComponent<CompositeCollider2D>();
        if(composite==null)composite=padre.gameObject.AddComponent<CompositeCollider2D>();
        composite.generationType=CompositeCollider2D.GenerationType.Manual;
        composite.geometryType=CompositeCollider2D.GeometryType.Polygons;
        composite.vertexDistance=.005f;
        foreach(var box in padre.GetComponentsInChildren<BoxCollider2D>())
            box.compositeOperation=Collider2D.CompositeOperation.Merge;
        composite.GenerateGeometry();
    }
}
