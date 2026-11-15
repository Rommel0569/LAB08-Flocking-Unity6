using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.Animations;
using System.Collections.Generic;

[InitializeOnLoad]
public static class BuildLab
{
    const string root="Assets/Lab08";
    static BuildLab()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                !System.IO.File.Exists(root+"/Scenes/00_Menu.unity")) Build();
        };
    }
    static Material fishMat, groundMat, obstacleMat, redMat, blueMat, sandMat, foliageMat;
    static RuntimeAnimatorController soldierMotion;
    [MenuItem("Lab08/Construir escenas")]
    public static void Build()
    {
        System.IO.Directory.CreateDirectory(root+"/Scenes");
        System.IO.Directory.CreateDirectory(root+"/Materials");
        System.IO.Directory.CreateDirectory(root+"/Animations");
        soldierMotion=BuildSoldierMotion();
        fishMat=Mat("Fish",new Color(.15f,.55f,.75f));
        groundMat=Mat("Ground",new Color(.75f,.84f,.7f));
        obstacleMat=Mat("Obstacles",new Color(.38f,.48f,.46f));
        sandMat=Mat("Sand",new Color(.67f,.84f,.79f));
        foliageMat=Mat("Foliage",new Color(.24f,.51f,.31f));
        var grass=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/External/Textures/Grass001_Color.jpg");
        var sand=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/External/Textures/Ground061_Color.jpg");
        groundMat.mainTexture=grass; groundMat.mainTextureScale=new Vector2(16,16);
        sandMat.mainTexture=sand; sandMat.mainTextureScale=new Vector2(8,8);
        redMat=Mat("Soldier",new Color(.52f,.13f,.12f));
        blueMat=Mat("Leader",new Color(.12f,.34f,.68f));
        PrepareFish();
        Menu(); Flocking(); Soldiers();
        EditorBuildSettings.scenes=new[]{
            new EditorBuildSettingsScene(root+"/Scenes/00_Menu.unity",true),
            new EditorBuildSettingsScene(root+"/Scenes/01_Flocking.unity",true),
            new EditorBuildSettingsScene(root+"/Scenes/02_Soldados.unity",true)};
        EditorSceneManager.OpenScene(root+"/Scenes/00_Menu.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("LAB08_BUILD_SUCCESS: 3 scenes ready");
    }
    static Material Mat(string name,Color color)
    {
        string path=root+"/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) {m=new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(m,path);}
        m.color=color; EditorUtility.SetDirty(m); return m;
    }
    static void PrepareFish()
    {
        string path=root+"/GameDevChef/Prefabs/Fish.prefab";
        var fish=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!fish) throw new System.Exception("Missing GameDevChef Fish prefab");
        var content=PrefabUtility.LoadPrefabContents(path);
        content.transform.localScale=Vector3.one*2f;
        foreach (var r in content.GetComponentsInChildren<Renderer>()) r.sharedMaterial=fishMat;
        PrefabUtility.SaveAsPrefabAsset(content,path); PrefabUtility.UnloadPrefabContents(content);
    }
    static void Base(string name,Vector3 camPos,Vector3 look)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera"); camera.tag="MainCamera";
        var c=camera.AddComponent<Camera>(); c.clearFlags=CameraClearFlags.Skybox; c.fieldOfView=55;
        camera.transform.position=camPos; camera.transform.LookAt(look);
        var light=new GameObject("Directional Light"); var l=light.AddComponent<Light>(); l.type=LightType.Directional; l.intensity=1.25f;
        light.transform.rotation=Quaternion.Euler(50,-30,0);
        if (name!="00_Menu")
        {
            var back=new GameObject("Regresar al menu").AddComponent<LabBackToMenu>();
            back.hint=name=="01_Flocking"?"WASD: explorar   Q/E: altura   Rueda: acercar   Boton derecho: girar   F: centrar":"WASD: mover jefe   Rueda: zoom   Boton derecho: girar";
        }
    }
    static void Save(string name) {EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),root+"/Scenes/"+name+".unity");}
    static void Menu()
    {
        Base("00_Menu",new Vector3(0,8,-18),Vector3.zero);
        new GameObject("Menu principal").AddComponent<LabMenu>();
        for(int i=0;i<8;i++) {var f=GameObject.CreatePrimitive(PrimitiveType.Sphere); f.name="Decoracion - pez"; f.transform.position=new Vector3((i-4)*1.6f,Mathf.Sin(i)*1.5f,0); f.transform.localScale=new Vector3(.4f,.25f,.9f); f.GetComponent<Renderer>().sharedMaterial=fishMat;}
        Save("00_Menu");
    }
    static void Flocking()
    {
        Base("01_Flocking",new Vector3(0,26,-35),new Vector3(0,3,0));
        var tdata=AssetDatabase.LoadAssetAtPath<TerrainData>(root+"/Terrain.asset");
        if (!tdata) { tdata=new TerrainData(); AssetDatabase.CreateAsset(tdata,root+"/Terrain.asset"); }
        tdata.heightmapResolution=129; tdata.size=new Vector3(90,12,90);
        var heights=new float[129,129];
        var centers=new[]{new Vector2(-15,-8),new Vector2(14,10),new Vector2(-7,18),new Vector2(18,-13),new Vector2(-20,18)};
        for(int z=0;z<129;z++) for(int x=0;x<129;x++)
        {
            float wx=x*90f/128f-45f, wz=z*90f/128f-45f;
            float h=.12f+.012f*Mathf.Sin(wx*.8f)*Mathf.Cos(wz*.65f);
            foreach(var c in centers)
            {
                float dx=wx-c.x,dz=wz-c.y;
                h+=.62f*Mathf.Exp(-(dx*dx+dz*dz)/(2f*5.5f*5.5f));
            }
            heights[z,x]=Mathf.Clamp01(h);
        }
        tdata.SetHeights(0,0,heights);
        var groundTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/External/Textures/Ground061_Color.jpg");
        var layer=AssetDatabase.LoadAssetAtPath<TerrainLayer>(root+"/TerrainLayer.terrainlayer");
        if (!layer) {layer=new TerrainLayer(); AssetDatabase.CreateAsset(layer,root+"/TerrainLayer.terrainlayer");}
        layer.diffuseTexture=groundTexture; layer.tileSize=new Vector2(8,8);
        layer.diffuseRemapMax=new Vector4(.62f,.75f,.75f,1f);
        tdata.terrainLayers=new[]{layer}; EditorUtility.SetDirty(tdata); EditorUtility.SetDirty(layer);
        var terrain=Terrain.CreateTerrainGameObject(tdata); terrain.name="Terrain"; terrain.layer=9; terrain.transform.position=new Vector3(-45,-4,-45);
        RenderSettings.fog=true; RenderSettings.fogColor=new Color(.16f,.43f,.48f); RenderSettings.fogDensity=.012f;
        RenderSettings.ambientLight=new Color(.42f,.62f,.65f);
        Camera.main.clearFlags=CameraClearFlags.SolidColor; Camera.main.backgroundColor=new Color(.12f,.38f,.44f);
        var fish=AssetDatabase.LoadAssetAtPath<GameObject>(root+"/GameDevChef/Prefabs/Fish.prefab");
        var flock=new GameObject("Flock"); flock.transform.position=new Vector3(0,5,0);
        var component=flock.AddComponent<Flock>(); var so=new SerializedObject(component);
        so.FindProperty("flockUnitPrefab").objectReferenceValue=fish.GetComponent<FlockUnit>();
        so.FindProperty("flockSize").intValue=100;
        so.FindProperty("spawnBounds").vector3Value=new Vector3(8,3,8);
        Set(so,"_minSpeed",2); Set(so,"_maxSpeed",4); Set(so,"_cohesionDistance",6);
        Set(so,"_avoidanceDistance",2); Set(so,"_aligementDistance",7); Set(so,"_obstacleDistance",5);
        Set(so,"_boundsDistance",26); Set(so,"_cohesionWeight",1); Set(so,"_avoidanceWeight",2.6f);
        Set(so,"_aligementWeight",1.7f); Set(so,"_boundsWeight",2); Set(so,"_obstacleWeight",8);
        so.ApplyModifiedPropertiesWithoutUndo();
        var centerY=terrain.GetComponent<Terrain>().SampleHeight(Vector3.zero);
        var giantRock=Prop("Obstaculo principal - roca gigante", "rock_largeB", new Vector3(0,centerY+2f,0), 12f, obstacleMat);
        giantRock.layer=9;
        var giantCollider=giantRock.AddComponent<SphereCollider>();
        giantCollider.center=new Vector3(0,0,0); giantCollider.radius=4.2f;
        for(int i=0;i<centers.Length;i++)
        {
            var c=centers[i];
            var rock=Prop("Colina - roca "+(i+1),i%2==0?"rock_largeA":"rock_largeB",new Vector3(c.x,0,c.y),7f,obstacleMat);
            rock.transform.position=new Vector3(c.x,terrain.GetComponent<Terrain>().SampleHeight(new Vector3(c.x,0,c.y))+.25f,c.y);
            rock.layer=9; var collider=rock.AddComponent<SphereCollider>(); collider.radius=.5f; collider.center=new Vector3(0,.25f,0);
        }
        for(int i=0;i<22;i++)
        {
            float a=i*2.39996f, r=13f+(i%6)*3f;
            Vector3 p=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
            p.y=terrain.GetComponent<Terrain>().SampleHeight(p);
            Prop("Vegetacion marina "+(i+1),i%3==0?"plant_bushLarge":"grass_large",p,i%3==0?5f:7f,foliageMat);
        }
        var focus=new GameObject("Centro visual del cardumen"); focus.transform.position=flock.transform.position;
        focus.AddComponent<FlockCenterTarget>().flock=component;
        var camera=Camera.main.gameObject.AddComponent<LabOrbitCamera>(); camera.target=focus.transform; camera.distance=22; camera.freePan=true; camera.panSpeed=10;
        Save("01_Flocking");
    }
    static void Set(SerializedObject s,string p,float v){s.FindProperty(p).floatValue=v;}
    static GameObject Prop(string name,string modelName,Vector3 position,float scale,Material fallback)
    {
        var rootObject=new GameObject(name); rootObject.transform.position=position;
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(root+"/External/Models/"+modelName+".fbx");
        if(model)
        {
            var child=(GameObject)PrefabUtility.InstantiatePrefab(model);
            child.transform.SetParent(rootObject.transform,false);
            // Blender FBX imports at 1/100 of the source mesh size in Unity.
            child.transform.localScale=Vector3.one*(scale*100f);
            foreach(var r in child.GetComponentsInChildren<Renderer>())
            {
                var originals=r.sharedMaterials;
                var replacements=new Material[originals.Length];
                for(int i=0;i<originals.Length;i++)
                {
                    string materialName=originals[i] ? originals[i].name.ToLowerInvariant() : "";
                    if(modelName.StartsWith("rock")) replacements[i]=Mat("NatureRock",new Color(.37f,.35f,.32f));
                    else if(materialName.Contains("wood") || materialName.Contains("bark")) replacements[i]=Mat("NatureBark",new Color(.43f,.29f,.18f));
                    else replacements[i]=Mat("NatureLeaves",new Color(.28f,.50f,.32f));
                }
                r.sharedMaterials=replacements;
            }
        }
        return rootObject;
    }
    static GameObject Knight(string name,Vector3 pos,Material material,float scale)
    {
        var rootObject=new GameObject(name); rootObject.transform.position=pos;
        string modelPath=root+"/Jerga99/Toon_RTS_demo/models/ToonRTS_demo_Knight.FBX";
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if(model)
        {
            var child=(GameObject)PrefabUtility.InstantiatePrefab(model); child.transform.SetParent(rootObject.transform,false);
            PrefabUtility.UnpackPrefabInstance(child,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            foreach(var t in child.GetComponentsInChildren<Transform>(true)) GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            var animators=child.GetComponentsInChildren<Animator>(true);
            if(animators.Length==0) animators=new[]{child.AddComponent<Animator>()};
            foreach(var animator in animators)
            {
                animator.runtimeAnimatorController=soldierMotion;
                animator.applyRootMotion=false;
                animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            }
            child.transform.localScale=Vector3.one*scale;
            foreach(var r in child.GetComponentsInChildren<Renderer>()) r.sharedMaterial=material;
        }
        else
        {
            var child=GameObject.CreatePrimitive(PrimitiveType.Capsule);child.transform.SetParent(rootObject.transform,false);
            child.transform.localPosition=Vector3.up;child.GetComponent<Renderer>().sharedMaterial=material;
        }
        return rootObject;
    }
    static RuntimeAnimatorController BuildSoldierMotion()
    {
        const string path=root+"/Animations/SoldierMotion.controller";
        var existing=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
        if(existing) AssetDatabase.DeleteAsset(path);
        var idle=AssetDatabase.LoadAssetAtPath<AnimationClip>(root+"/Jerga99/Toon_RTS_demo/animations/WK_heavy_infantry_05_combat_idle.FBX");
        var walk=AssetDatabase.LoadAssetAtPath<AnimationClip>(root+"/Jerga99/Toon_RTS_demo/animations/WK_heavy_infantry_06_combat_walk.FBX");
        if(!idle || !walk) throw new System.Exception("Missing knight idle/walk clips");
        var motionController=AnimatorController.CreateAnimatorControllerAtPath(path);
        motionController.AddParameter("Speed",AnimatorControllerParameterType.Float);
        var machine=motionController.layers[0].stateMachine;
        var idleState=machine.AddState("Idle"); idleState.motion=idle; machine.defaultState=idleState;
        var walkState=machine.AddState("Walk"); walkState.motion=walk;
        var toWalk=idleState.AddTransition(walkState); toWalk.hasExitTime=false; toWalk.duration=.18f; toWalk.AddCondition(AnimatorConditionMode.Greater,.1f,"Speed");
        var toIdle=walkState.AddTransition(idleState); toIdle.hasExitTime=false; toIdle.duration=.2f; toIdle.AddCondition(AnimatorConditionMode.Less,.08f,"Speed");
        EditorUtility.SetDirty(motionController);
        AssetDatabase.SaveAssets();
        return motionController;
    }
    static void Soldiers()
    {
        Base("02_Soldados",new Vector3(0,24,-23),Vector3.zero);
        RenderSettings.fog=true; RenderSettings.fogColor=new Color(.72f,.83f,.78f); RenderSettings.fogDensity=.004f;
        var ground=GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name="Suelo";
        ground.transform.position=new Vector3(0,-.25f,0); ground.transform.localScale=new Vector3(80,.5f,80);
        ground.GetComponent<Renderer>().sharedMaterial=groundMat;
        var courtyard=GameObject.CreatePrimitive(PrimitiveType.Cylinder); courtyard.name="Claro de reunion";
        courtyard.transform.position=new Vector3(0,-.03f,0); courtyard.transform.localScale=new Vector3(16,.04f,16);
        courtyard.GetComponent<Renderer>().sharedMaterial=sandMat;
        foreach(var col in courtyard.GetComponents<Collider>()) Object.DestroyImmediate(col);
        for(int i=0;i<18;i++)
        {
            float a=i*2f*Mathf.PI/18f, r=13f+(i%3)*3f;
            Vector3 p=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
            Prop("Arbol "+(i+1),i%2==0?"tree_oak":"tree_pineDefaultA",p,7f,foliageMat);
        }
        for(int i=0;i<18;i++)
        {
            float a=i*2.39996f, r=10f+(i%4)*3f;
            Vector3 p=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
            Prop("Roca decorativa "+(i+1),i%2==0?"rock_largeA":"rock_tallA",p,5f,obstacleMat);
        }
        var leader=Knight("Jefe - WASD",Vector3.zero,blueMat,1.1f);
        var player=leader.AddComponent<PlayerController>();
        foreach(var animator in leader.GetComponentsInChildren<Animator>(true)) animator.runtimeAnimatorController=soldierMotion;
        const int n=20; var units=new EnemyController[n];
        for(int i=0;i<n;i++)
        {
            float lane=(i%5-2)*1.5f, rank=i/5;
            var soldier=Knight("Soldado "+(i+1).ToString("00"),new Vector3(lane,0,-5f-rank*1.6f),redMat,.75f);
            units[i]=soldier.AddComponent<EnemyController>(); units[i].leader=leader.transform;
            units[i].leaderController=player; units[i].index=i; units[i].count=n;
        }
        foreach(var unit in units) unit.companions=units;
        var orbit=Camera.main.gameObject.AddComponent<LabOrbitCamera>(); orbit.target=leader.transform; orbit.distance=18; orbit.pitch=65; orbit.yaw=0;
        Save("02_Soldados");
    }
}
