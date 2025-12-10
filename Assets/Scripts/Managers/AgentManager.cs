using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AgentManager : MonoBehaviour
{
    public float detectionRadius = 10f;
    public Transform player;

    List<NavMeshAgent> agents = new List<NavMeshAgent>();

    void Start()
    {

    }

    void Update()
    {
        foreach (NavMeshAgent agent in agents)
        {
            float dist = Vector3.Distance(agent.transform.position, player.position);

            if (dist <= detectionRadius)
            {
                agent.SetDestination(player.position);
            }
        }
    }

    public void InitializeAgents()
    {
        GameObject[] a = GameObject.FindGameObjectsWithTag("AI");
        foreach (GameObject go in a)
        {
            agents.Add(go.GetComponent<NavMeshAgent>());
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
}
