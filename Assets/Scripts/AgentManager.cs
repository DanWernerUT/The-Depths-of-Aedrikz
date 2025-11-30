using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
public class AgentManager : MonoBehaviour
{
    List<NavMeshAgent> agents = new List<NavMeshAgent>();
    void Start()
    {
        GameObject[] a = GameObject.FindGameObjectsWithTag("AI");
        foreach (GameObject go in a)
        {
            agents.Add(go.GetComponent<NavMeshAgent>());
        }
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                foreach(NavMeshAgent a in agents)
                {
                    a.SetDestination(hit.point);
                }
            }
        }
    }
}
