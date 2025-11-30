using UnityEngine;

public class AIController : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent agent;
    Animator anim;
    void Start()
    {
        agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        anim = this.GetComponent<Animator>();
    }

    void Update()
    {
        if (agent.remainingDistance < 2)
        {
            anim.SetBool("isMoving", false);
            Debug.Log("false");
        }
        else
        {
            anim.SetBool("isMoving", true);
            Debug.Log("true");

        }
    }
}
