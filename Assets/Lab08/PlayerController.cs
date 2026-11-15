using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed=7f;
    public Vector3 MoveDirection {get;private set;}
    public Animator Animator {get;private set;}
    public float InputMagnitude {get;private set;}
    void Awake(){Animator=GetComponentInChildren<Animator>();}
    void Update()
    {
        Vector3 input=new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));
        InputMagnitude=Mathf.Clamp01(input.magnitude);
        MoveDirection=input.sqrMagnitude>.01f?input.normalized:Vector3.zero;
        transform.position+=MoveDirection*speed*Time.deltaTime;
        if (MoveDirection.sqrMagnitude>.01f) transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(MoveDirection),Time.deltaTime*10f);
        if(Animator) Animator.SetFloat("Speed",InputMagnitude,0.12f,Time.deltaTime);
    }
}
