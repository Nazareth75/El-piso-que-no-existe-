using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Piso7.Campus;

[InitializeOnLoad]
public static class NavigationButtonsSetup
{
    static NavigationButtonsSetup() { EditorApplication.update += Poll; }
    static void Poll()
    {
        if(!File.Exists("Temp/navigation-buttons.request") || EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        try { File.Delete("Temp/navigation-buttons.request"); } catch(IOException) { return; } Apply();
    }
    [MenuItem("Tools/Equipo/Aplicar botones de ingreso y salida")]
    public static void Apply()
    {
        if(SceneManager.GetActiveScene().isDirty) { Debug.LogWarning("Guarda la escena antes de aplicar los botones."); return; }
        const string path="Assets/Art/Navigation/ingreso_salida.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit=100; importer.maxTextureSize=4096; importer.filterMode=FilterMode.Point;
        importer.mipmapEnabled=false; importer.alphaIsTransparency=true; importer.textureCompression=TextureImporterCompression.Uncompressed;
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        if (provider == null) throw new InvalidOperationException("No se pudo abrir el importador de los botones.");
        provider.InitSpriteEditorDataProvider();
        var existingRects = provider.GetSpriteRects();
        var rects = new[] {
            new SpriteRect { name="Ingresar", rect=new Rect(287,19,799,689), alignment=SpriteAlignment.Custom, pivot=new Vector2(.5f,.5f), spriteID=existingRects.FirstOrDefault(x=>x.name=="Ingresar")?.spriteID ?? GUID.Generate() },
            new SpriteRect { name="Salir", rect=new Rect(1140,20,849,688), alignment=SpriteAlignment.Custom, pivot=new Vector2(.5f,.5f), spriteID=existingRects.FirstOrDefault(x=>x.name=="Salir")?.spriteID ?? GUID.Generate() }
        };
        provider.SetSpriteRects(rects);
        var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (names == null) throw new InvalidOperationException("El importador no permite conservar los identificadores de los botones.");
        names.SetNameFileIdPairs(rects.Select(x=>new SpriteNameFileIdPair(x.name,x.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
        var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        var enter=sprites.First(x=>x.name=="Ingresar"); var exit=sprites.First(x=>x.name=="Salir");
        int count=0;
        foreach(var entry in EditorBuildSettings.scenes.Where(x=>x.enabled && x.path.Contains("/Campus/")))
        {
            var scene=EditorSceneManager.OpenScene(entry.path);
            var roots=scene.GetRootGameObjects();
            bool sceneChanged=false;
            if(scene.name=="Campus_Recepcion")
            {
                foreach(var name in new[]{"Ascensor","Punto_Escaleras","Punto_Hall","Punto_Entrada"})
                { var old=roots.FirstOrDefault(x=>x.name==name); if(old!=null) old.SetActive(false); }
                var player=roots.First(x=>x.name=="Player"); player.transform.position=new Vector3(13.2f,1.7f,0);
                var controller=new SerializedObject(player.GetComponent<Piso7.Player.PlayerController2D>());
                controller.FindProperty("faceLeftOnStart").boolValue=true; controller.ApplyModifiedPropertiesWithoutUndo();
                var visual=player.GetComponentInChildren<SpriteRenderer>(); visual.flipX=true;
                var camera=roots.First(x=>x.name=="Main Camera"); camera.transform.position=new Vector3(12.3f,3.2f,-10);
                var camSettings=new SerializedObject(camera.GetComponent<CampusPanCamera>());
                camSettings.FindProperty("startX").floatValue=13.2f; camSettings.ApplyModifiedPropertiesWithoutUndo();
                var direction=roots.FirstOrDefault(x=>x.name=="Indicacion_Clinica_Izquierda");
                if(direction==null) direction=new GameObject("Indicacion_Clinica_Izquierda");
                var sr=direction.GetComponent<SpriteRenderer>(); if(sr==null) sr=direction.AddComponent<SpriteRenderer>();
                sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Campus/Descubre/Puntos/punto_atras.png");
                sr.sharedMaterial=roots.First(x=>x.name=="Punto_Clinica").GetComponent<SpriteRenderer>().sharedMaterial;
                sr.sortingOrder=9; direction.transform.position=new Vector3(11.1f,1.2f,0);
            }
            foreach(var point in roots.SelectMany(x=>x.GetComponentsInChildren<CampusHotspot>(true)))
            {
                if(!point.gameObject.activeInHierarchy) continue;
                string target=point.TargetScene;
                bool backwards=target=="Campus_EntradaPrincipal" || target=="Campus_Recepcion" || target=="Campus_HallPrincipal";
                bool door=backwards || new[]{"Campus_Clinica","Campus_Admisiones","Campus_Cafeteria","Campus_Auditorio","Campus_LabHistologia"}.Contains(target);
                if(!door) continue;
                // The main entrance leads into Reception; it is an entry rather than an exit.
                bool isExit=backwards && scene.name!="Campus_EntradaPrincipal";
                var source=point.GetComponent<SpriteRenderer>(); source.enabled=false;
                var icon=point.transform.Find("BotonAcceso");
                if(icon==null) {icon=new GameObject("BotonAcceso").transform; icon.SetParent(point.transform,false);}
                var renderer=icon.GetComponent<SpriteRenderer>(); if(renderer==null) renderer=icon.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite=isExit?exit:enter; renderer.sharedMaterial=source.sharedMaterial; renderer.sortingOrder=10;
                icon.localPosition=new Vector3(0,.3f,0); icon.localScale=new Vector3(0.0882f,0.0948f,0.1886f);
                var collider=point.GetComponent<BoxCollider2D>(); if(collider!=null) {collider.size=new Vector2(1.4f,1.4f); collider.offset=new Vector2(0,.3f);}
                EditorUtility.SetDirty(point); count++; sceneChanged=true;
            }
            if(sceneChanged) { EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); }
        }
        File.WriteAllText("Library/navigation-buttons.txt","OK botones asignados: "+count+"; recepcion sin ascensor activo, entrada a clinica a la izquierda.");
        EditorSceneManager.OpenScene("Assets/Scenes/Campus/Campus_Recepcion.unity");
        var view=EditorWindow.GetWindow<SceneView>(); view.in2DMode=true;
        view.Frame(new Bounds(new Vector3(9,3.2f,0),new Vector3(18,6.4f,1)),true); view.Focus();
    }
}

