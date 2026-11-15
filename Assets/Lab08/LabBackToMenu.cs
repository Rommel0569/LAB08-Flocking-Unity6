using UnityEngine;
using UnityEngine.SceneManagement;

public class LabBackToMenu : MonoBehaviour
{
    public string hint="";
    void Update() { if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("00_Menu"); }
    void OnGUI()
    {
        if (GUI.Button(new Rect(12,12,130,34), "MENU (ESC)")) SceneManager.LoadScene("00_Menu");
        if (!string.IsNullOrEmpty(hint)) GUI.Box(new Rect(155,12,Mathf.Min(590,Screen.width-170),34),hint);
    }
}
