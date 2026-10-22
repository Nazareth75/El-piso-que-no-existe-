using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Piso7.Campus;

[InitializeOnLoad]
public static class ReceptionSetup
{
    static float wait;
    static float approachStarted;
    static string Report => "Library/reception-validation.txt";
    static ReceptionSetup() { EditorApplication.update += Tick; Application.logMessageReceived += Log; }
    static void Log(string s, string stack, LogType type) { if (SessionState.GetInt("ReceptionTest",0)>0 && (type==LogType.Error || type==LogType.Exception)) File.AppendAllText(Report,"ERROR "+s+"\n"+stack+"\n"); }
    static void Check(bool ok,string message) { File.AppendAllText(Report,(ok?"PASS ":"ERROR ")+message+"\n"); }
    static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (File.Exists("Temp/reception-setup.request") && !EditorApplication.isPlaying)
        { try { File.Delete("Temp/reception-setup.request"); } catch (IOException) { return; } Apply(); return; }
        if (File.Exists("Temp/reception-test.request") && !EditorApplication.isPlaying)
        {
            try { File.Delete("Temp/reception-test.request"); } catch (IOException) { return; }
            if (SceneManager.GetActiveScene().isDirty) return;
            File.WriteAllText(Report,"Unity "+Application.unityVersion+"\n");
            EditorSceneManager.OpenScene("Assets/Scenes/Campus/Campus_Recepcion.unity");
            SessionState.SetInt("ReceptionTest",1); wait=0; EditorApplication.isPlaying=true; return;
        }
        int step=SessionState.GetInt("ReceptionTest",0);
        if(step==99 && !EditorApplication.isPlaying)
        {
            SessionState.SetInt("ReceptionTest",0); EditorSceneManager.OpenScene("Assets/Scenes/Campus/Campus_Recepcion.unity");
            var view=EditorWindow.GetWindow<SceneView>(); view.in2DMode=true;
            view.Frame(new Bounds(new Vector3(9,3.2f,0),new Vector3(18,6.4f,1)),true); view.Focus();
            File.AppendAllText(Report,"COMPLETE\n"); return;
        }
        if(step==0 || !EditorApplication.isPlaying || Time.time<wait) return;
        wait=Time.time+0.6f;
        try
        {
            var player=GameObject.Find("Player");
            switch(step)
            {
                case 1:
                    Check(player!=null && player.GetComponent<Piso7.Player.PlayerController2D>()!=null,"Player conserva controlador");
                    Check(player.transform.position.x>11f && !player.GetComponent<Piso7.Player.PlayerController2D>().FacingRight,"Aparece en recepcion mirando izquierda");
                    Check(GameObject.Find("Ascensor")==null,"Sin ascensor activo en recepcion");
                    Check(UnityEngine.Object.FindObjectsByType<CampusHotspot>(FindObjectsSortMode.None).Length==1,"Solo el acceso a clinica esta activo");
                    Check(GameObject.Find("Punto_Clinica").transform.Find("BotonAcceso").GetComponent<SpriteRenderer>().sprite.name=="Ingresar","Boton verde de ingreso asignado");
                    Check(GameObject.Find("Punto_Clinica").GetComponent<CampusHotspot>().TargetScene=="Campus_Clinica","Flecha apunta a clinica");
                    Check(Application.CanStreamedLevelBeLoaded("Campus_Clinica"),"Clinica incluida en Build Settings");
                    Check(GameObject.Find("Fondo").GetComponent<SpriteRenderer>().sprite.name=="recepcion_referencia","Fondo nuevo asignado");
                    Check(GameObject.Find("Suelo").GetComponent<Collider2D>()!=null,"Suelo conserva colision");
                    var body=player.GetComponent<Rigidbody2D>(); body.position=new Vector2(7.3f,1.7f);
                    SessionState.SetInt("ReceptionTest",2); break;
                case 2:
                    Check(player.GetComponent<Rigidbody2D>().position.y>0.5f,"Player apoyado sobre suelo");
                    approachStarted=Time.time;
                    player.GetComponent<Rigidbody2D>().position=GameObject.Find("Punto_Clinica").transform.position;
                    SessionState.SetInt("ReceptionTest",3); break;
                case 3:
                    if (SceneManager.GetActiveScene().name!="Campus_Clinica" && Time.time-approachStarted<4f) return;
                    Check(SceneManager.GetActiveScene().name=="Campus_Clinica","Acercarse y detenerse activa recepcion -> clinica");
                    var back=GameObject.Find("Punto_Recepcion");
                    Check(back!=null,"Clinica conserva salida hacia recepcion");
                    if(back==null) { SessionState.SetInt("ReceptionTest",99); EditorApplication.isPlaying=false; break; }
                    var exitIcon = back.transform.Find("BotonAcceso");
                    var exitRenderer = exitIcon != null ? exitIcon.GetComponent<SpriteRenderer>() : null;
                    Check(exitRenderer != null && exitRenderer.sprite != null && exitRenderer.sprite.name=="Salir","Boton rojo de salida asignado");
                    approachStarted=Time.time;
                    player.GetComponent<Rigidbody2D>().position=back.transform.position;
                    SessionState.SetInt("ReceptionTest",4); break;
                case 4:
                    if(SceneManager.GetActiveScene().name!="Campus_Recepcion" && Time.time-approachStarted<4f) return;
                    Check(SceneManager.GetActiveScene().name=="Campus_Recepcion","Regreso clinica -> recepcion por proximidad");
                    foreach(var name in new[]{"Punto_Entrada","Punto_Hall","Punto_Escaleras","Ascensor"}) Check(SceneManager.GetActiveScene().GetRootGameObjects().Any(x=>x.name==name && !x.activeSelf),"Acceso anterior desactivado "+name);
                    SessionState.SetInt("ReceptionTest",99); EditorApplication.isPlaying=false; break;
            }
        }
        catch(Exception ex) { File.AppendAllText(Report,"ERROR "+ex+"\n"); SessionState.SetInt("ReceptionTest",99); EditorApplication.isPlaying=false; }
    }
    [MenuItem("Tools/Equipo/Aplicar recepcion y acceso clinica")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty) { Debug.LogWarning("Guarda la escena abierta antes de configurar recepcion."); return; }
        const string image="Assets/Art/Backgrounds/Entrada/recepcion_referencia.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(image);
        importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=100; importer.filterMode=FilterMode.Point; importer.mipmapEnabled=false;
        importer.maxTextureSize=2048; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Campus/Campus_Recepcion.unity");
        var roots=scene.GetRootGameObjects();
        var background=roots.First(x=>x.name=="Fondo").GetComponent<SpriteRenderer>(); background.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(image);
        float scale=18f/background.sprite.bounds.size.x;
        background.transform.localScale=Vector3.one*scale;
        background.transform.position=new Vector3(9,0.64f+background.sprite.bounds.size.y*scale/2,0);
        var ground=roots.First(x=>x.name=="Suelo");
        var floor=ground.transform.Find("SueloVisual");
        if(floor==null) { floor=new GameObject("SueloVisual").transform; floor.SetParent(ground.transform,false); }
        var square=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath("311925a002f4447b3a28927169b83ea6")).OfType<Sprite>().First();
        var sr=floor.GetComponent<SpriteRenderer>(); if(sr==null) sr=floor.gameObject.AddComponent<SpriteRenderer>();
        sr.sprite=square; sr.sharedMaterial=background.sharedMaterial; sr.color=new Color(.52f,.52f,.54f); sr.sortingOrder=2;
        floor.localScale=new Vector3(18/square.bounds.size.x,.4f/square.bounds.size.y,1);
        var template=roots.First(x=>x.name=="Punto_Hall");
        var clinic=roots.FirstOrDefault(x=>x.name=="Punto_Clinica");
        if(clinic==null) { clinic=UnityEngine.Object.Instantiate(template); clinic.name="Punto_Clinica"; }
        clinic.transform.position=new Vector3(5.7f,1f,0);
        clinic.GetComponent<CampusHotspot>().Configure("Campus_Clinica","Clínica odontológica");
        roots.First(x=>x.name=="Player").transform.position=new Vector3(9,1.7f,0);
        var camera=roots.First(x=>x.name=="Main Camera").GetComponent<Camera>(); camera.orthographicSize=3.2f; camera.transform.position=new Vector3(9,3.2f,-10);
        var settings=new SerializedObject(camera.GetComponent<CampusPanCamera>());
        settings.FindProperty("cameraSize").floatValue=3.2f; settings.FindProperty("y").floatValue=3.2f; settings.FindProperty("startX").floatValue=9;
        settings.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        var clinicScene=EditorSceneManager.OpenScene("Assets/Scenes/Campus/Campus_Clinica.unity");
        var back=clinicScene.GetRootGameObjects().FirstOrDefault(x=>x.name=="Punto_Recepcion");
        if(back==null)
        {
            var existing=clinicScene.GetRootGameObjects().First(x=>x.GetComponent<CampusHotspot>()!=null);
            back=UnityEngine.Object.Instantiate(existing); back.name="Punto_Recepcion"; back.transform.position=new Vector3(15.6f,1f,0);
        }
        back.GetComponent<CampusHotspot>().Configure("Campus_Recepcion","Recepción");
        EditorSceneManager.MarkSceneDirty(clinicScene); EditorSceneManager.SaveScene(clinicScene);
        EditorSceneManager.OpenScene("Assets/Scenes/Campus/Campus_Recepcion.unity");
        var view=EditorWindow.GetWindow<SceneView>(); view.in2DMode=true;
        view.Frame(new Bounds(new Vector3(9,3.2f,0),new Vector3(18,6.4f,1)),true); view.Focus();
        File.WriteAllText("Library/reception-setup.txt","OK fondo y conexion clinica ida/vuelta.");
    }
}




