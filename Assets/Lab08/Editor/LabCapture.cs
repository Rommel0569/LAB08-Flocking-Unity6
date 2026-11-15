using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Reflection;

public static class LabCapture
{
    [MenuItem("Lab08/Capturar imagenes del informe")]
    public static void Capture()
    {
        string directory=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Capturas");
        Directory.CreateDirectory(directory);
        CaptureFish(Path.Combine(directory,"01_Flocking.png"));
        CaptureSoldiers(Path.Combine(directory,"02_Soldados.png"));
        EditorSceneManager.OpenScene("Assets/Lab08/Scenes/00_Menu.unity");
        Debug.Log("LAB08_CAPTURE_SUCCESS: " + directory);
    }
    static void CaptureFish(string path)
    {
        EditorSceneManager.OpenScene("Assets/Lab08/Scenes/01_Flocking.unity");
        var flock=Object.FindFirstObjectByType<Flock>();
        typeof(Flock).GetMethod("GenerateUnits",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(flock,null);
        var camera=Camera.main;
        camera.transform.position=flock.transform.position+new Vector3(0,22,-26);
        camera.transform.LookAt(flock.transform.position);
        Render(camera,path);
    }
    static void CaptureSoldiers(string path)
    {
        EditorSceneManager.OpenScene("Assets/Lab08/Scenes/02_Soldados.unity");
        var leader=Object.FindFirstObjectByType<PlayerController>();
        var units=Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        Vector3 forward=leader.transform.forward; forward.y=0; forward.Normalize();
        Vector3 right=Vector3.Cross(Vector3.up,forward).normalized;
        System.Array.Sort(units,(a,b)=>a.index.CompareTo(b.index));
        for(int i=0;i<units.Length;i++)
        {
            float t=units.Length<=1?.5f:(float)i/(units.Length-1);
            float angle=Mathf.Lerp(-77.5f,77.5f,t)*Mathf.Deg2Rad;
            Vector3 radial=-forward*Mathf.Cos(angle)+right*Mathf.Sin(angle);
            units[i].transform.position=leader.transform.position+radial*units[i].radius;
            units[i].transform.rotation=Quaternion.LookRotation(-radial);
        }
        var camera=Camera.main;
        camera.transform.position=leader.transform.position+new Vector3(0,17,-17);
        camera.transform.LookAt(leader.transform.position);
        Render(camera,path);
    }
    static void Render(Camera camera,string path)
    {
        var rt=new RenderTexture(1600,900,24);
        var previous=camera.targetTexture;
        var current=RenderTexture.active;
        camera.targetTexture=rt;
        RenderTexture.active=rt;
        camera.Render();
        var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,1600,900),0,0);
        image.Apply();
        File.WriteAllBytes(path,image.EncodeToPNG());
        camera.targetTexture=previous;
        RenderTexture.active=current;
        Object.DestroyImmediate(image);
        Object.DestroyImmediate(rt);
    }
}
