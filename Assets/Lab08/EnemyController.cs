using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform leader;
    public PlayerController leaderController;
    public int index;
    public int count=20;
    public float radius=5f;
    public float speed=8f;
    public float minimumSpacing=1.15f;
    public float followDistance=3.2f;
    public float formationArcDegrees=155f;
    public float stopThreshold=.05f;
    public EnemyController[] companions;
    Animator animator;
    void Awake(){animator=GetComponentInChildren<Animator>();}
    void Update()
    {
        if (!leader) return;
        Vector3 direction=leaderController ? leaderController.MoveDirection : Vector3.zero;
        bool moving=direction.sqrMagnitude>stopThreshold*stopThreshold;
        float speedBlend=leaderController?leaderController.InputMagnitude:(moving?1f:0f);
        Vector3 forward=moving?direction:leader.forward;
        forward.y=0; if(forward.sqrMagnitude<.001f) forward=Vector3.forward; forward.Normalize();
        Vector3 right=Vector3.Cross(Vector3.up,forward).normalized;
        float t=count<=1?.5f:(float)index/(count-1);
        float angle=Mathf.Lerp(-formationArcDegrees*.5f,formationArcDegrees*.5f,t)*Mathf.Deg2Rad;
        Vector3 desired;
        if(moving)
        {
            // File behind the moving leader; alternate lanes create a loose marching group.
            float lane=(index%5-2)*.9f;
            float rank=index/5;
            desired=leader.position-forward*(followDistance+rank*1.35f)+right*lane;
        }
        else
        {
            // On stop, form an open crescent behind the leader, leaving its front clear.
            Vector3 back=-forward;
            Vector3 radial=back*Mathf.Cos(angle)+right*Mathf.Sin(angle);
            desired=leader.position+radial*radius;
        }
        Vector3 separation=Vector3.zero;
        if (companions!=null) foreach(var other in companions)
        {
            if (!other || other==this) continue;
            Vector3 delta=transform.position-other.transform.position;
            float d=delta.magnitude;
            if (d>.001f && d<minimumSpacing) separation+=delta/d*(minimumSpacing-d);
        }
        desired+=separation*1.8f;
        Vector3 movement=desired-transform.position; movement.y=0;
        float distance=movement.magnitude;
        float travelSpeed=speed*Mathf.Clamp(distance/.8f,.12f,1f);
        Vector3 step=Vector3.ClampMagnitude(movement,travelSpeed*Time.deltaTime);
        transform.position+=step;
        Vector3 facing=moving?forward:(step.sqrMagnitude>.001f?step.normalized:forward);
        transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(facing),Time.deltaTime*8f);
        if(animator) animator.SetFloat("Speed",Mathf.Clamp01(step.magnitude/Mathf.Max(Time.deltaTime*speed,.001f)),.12f,Time.deltaTime);
    }
}
