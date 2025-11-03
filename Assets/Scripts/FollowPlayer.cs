using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset = new Vector3(0, 10, -20);
    private float forwardInput;

    void Start()
    {

    }

    void LateUpdate()
    {
        forwardInput = Input.GetAxis("Vertical");
        transform.position = player.transform.position + player.transform.rotation * offset;
        transform.LookAt(player.transform.position + new Vector3(0, 1, 0) * 2f);
    }
}