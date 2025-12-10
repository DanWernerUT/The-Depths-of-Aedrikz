using UnityEngine;

public class AIController : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent agent;
    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead) return;

        if (agent.remainingDistance < 2)
            anim.SetBool("isMoving", false);
        else
            anim.SetBool("isMoving", true);
    }

    public void Die()
    {
        isDead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (anim != null)
            anim.SetBool("isDead", true);
    }
}
