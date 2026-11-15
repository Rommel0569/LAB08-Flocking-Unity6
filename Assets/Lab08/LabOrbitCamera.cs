using UnityEngine;
using UnityEngine.SceneManagement;

public class LabOrbitCamera : MonoBehaviour
{
    public Transform target;
    public float distance=24f, yaw=35f, pitch=32f;
    public bool freePan;
    public float panSpeed=8f;
    private Vector3 panOffset;
    void LateUpdate()
    {
        if (!target) return;
        if (Input.GetMouseButton(1)) { yaw += Input.GetAxis("Mouse X")*3f; pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*3f,10f,80f); }
        if (freePan)
        {
            Vector3 forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;
            Vector3 right=Quaternion.Euler(0,yaw,0)*Vector3.right;
            Vector3 move=right*Input.GetAxisRaw("Horizontal")+forward*Input.GetAxisRaw("Vertical");
            if (Input.GetKey(KeyCode.Q)) move+=Vector3.down;
            if (Input.GetKey(KeyCode.E)) move+=Vector3.up;
            panOffset+=move.normalized*panSpeed*(Input.GetKey(KeyCode.LeftShift)?3f:1f)*Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.F)) panOffset=Vector3.zero;
        }
        distance=Mathf.Clamp(distance-Input.mouseScrollDelta.y*2.8f,3.5f,90f);
        transform.rotation=Quaternion.Euler(pitch,yaw,0);
        transform.position=target.position+panOffset-transform.forward*distance;
    }
}
