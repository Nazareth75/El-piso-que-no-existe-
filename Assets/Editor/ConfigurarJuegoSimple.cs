using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


public static class ConfigurarJuegoSimple
{


    [MenuItem("Tools/Configurar juego simple")]
    public static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        GameObject player = GameObject.Find("Player");
        GameObject monstruo = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Select(s => s.gameObject).FirstOrDefault(g => g.name.StartsWith("personaje_caminar", StringComparison.OrdinalIgnoreCase));
        if (player == null || monstruo == null)
        {
            Debug.Log("JUEGO_SIMPLE: abre la escena que contiene Player y personaje_caminar; luego Tools > Configurar juego simple.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(player, "Configurar Player");
        Undo.RegisterFullObjectHierarchyUndo(monstruo, "Configurar monstruo");
        if (!player.TryGetComponent<Collider2D>(out _)) Undo.AddComponent<CapsuleCollider2D>(player);
        if (!monstruo.TryGetComponent<Collider2D>(out _))
        {
            var col = Undo.AddComponent<CapsuleCollider2D>(monstruo);
            var sprite = monstruo.GetComponent<SpriteRenderer>().sprite;
            col.size = new Vector2(sprite.bounds.size.x * 0.5f, sprite.bounds.size.y * 0.8f);
            col.offset = sprite.bounds.center;
        }
        PrepararCuerpo(player);
        PrepararCuerpo(monstruo);
        monstruo.transform.rotation = Quaternion.identity;
        var piso = GameObject.Find("Square");
        if (piso != null && !piso.TryGetComponent<Collider2D>(out _)) Undo.AddComponent<BoxCollider2D>(piso);
        if (piso != null && piso.TryGetComponent<Rigidbody2D>(out var cuerpoPiso))
        {
            Undo.RecordObject(cuerpoPiso, "Fijar piso");
            cuerpoPiso.bodyType = RigidbodyType2D.Static;
        }
        var juego = player.GetComponent<JuegoSimple>();
        if (juego == null) juego = Undo.AddComponent<JuegoSimple>(player);
        juego.monstruo = monstruo.transform;
        ConfigurarAnimacion(monstruo);
        EditorUtility.SetDirty(juego);
        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);

        Debug.Log($"JUEGO_SIMPLE_CONFIGURADO Player={player.transform.position} Monstruo={monstruo.transform.position} Piso={(piso != null ? piso.transform.position.ToString() : "sin piso")} Escena={player.scene.path}");
    }

    static void ConfigurarAnimacion(GameObject monstruo)
    {
        var animador = monstruo.GetComponent<Animator>();
        if (animador == null || animador.runtimeAnimatorController == null) return;
        var clip = animador.runtimeAnimatorController.animationClips.FirstOrDefault();
        if (clip == null) return;
        string textura = AssetDatabase.GetAssetPath(monstruo.GetComponent<SpriteRenderer>().sprite);
        var cuadros = AssetDatabase.LoadAllAssetsAtPath(textura).OfType<Sprite>()
            .OrderBy(s => s.rect.x).ToArray();
        if (cuadros.Length == 0) return;
        var claves = cuadros.Select((sprite, i) => new ObjectReferenceKeyframe { time = i / 10f, value = sprite }).ToArray();
        Undo.RecordObject(clip, "Configurar caminar");
        clip.frameRate = 10;
        AnimationUtility.SetObjectReferenceCurve(clip,
            new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, claves);
        var ajustes = AnimationUtility.GetAnimationClipSettings(clip);
        ajustes.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, ajustes);
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
    }

    static void PrepararCuerpo(GameObject objeto)
    {
        var cuerpo = objeto.GetComponent<Rigidbody2D>();
        if (cuerpo == null) cuerpo = Undo.AddComponent<Rigidbody2D>(objeto);
        cuerpo.bodyType = RigidbodyType2D.Dynamic;
        cuerpo.gravityScale = 1;
        cuerpo.constraints = RigidbodyConstraints2D.FreezeRotation;
        cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        cuerpo.interpolation = RigidbodyInterpolation2D.Interpolate;
        objeto.GetComponent<Collider2D>().isTrigger = false;
    }
}

