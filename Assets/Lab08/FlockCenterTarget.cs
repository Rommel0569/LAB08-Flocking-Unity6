using UnityEngine;
using UnityEngine.SceneManagement;

public class FlockCenterTarget : MonoBehaviour
{
    public Flock flock;
    void LateUpdate()
    {
        if (!flock || flock.allUnits == null || flock.allUnits.Length == 0) return;
        Vector3 sum=Vector3.zero; int count=0;
        foreach (var unit in flock.allUnits)
            if (unit) { sum+=unit.transform.position; count++; }
        if (count>0) transform.position=Vector3.Lerp(transform.position,sum/count,Time.deltaTime*2f);
    }
}
