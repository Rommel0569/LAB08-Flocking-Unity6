using UnityEngine;
using UnityEngine.SceneManagement;

public class LabMenu : MonoBehaviour
{
    public string title = "LAB 08  |  FLOCKING";
    void OnGUI()
    {
        var w = 430f; var x = (Screen.width-w)/2; var y = Screen.height*.28f;
        GUI.Box(new Rect(x-20,y-30,w+40,250), "");
        GUI.Label(new Rect(x,y,w,45), title);
        if (GUI.Button(new Rect(x,y+58,w,48), "II. ACTIVIDADES  -  100 PECES")) SceneManager.LoadScene("01_Flocking");
        if (GUI.Button(new Rect(x,y+116,w,48), "EJERCICIO 2  -  JEFE Y SOLDADOS")) SceneManager.LoadScene("02_Soldados");
        GUI.Label(new Rect(x,y+185,w,35), "WASD: mover  |  Mouse: camara  |  ESC: menu");
    }
}
