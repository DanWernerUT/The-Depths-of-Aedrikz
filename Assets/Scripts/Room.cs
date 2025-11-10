using UnityEngine;

public class Room : MonoBehaviour
{
    public GameObject northWall, southWall, eastWall, westWall;
    public float detectDistance = 10f;

    void Start()
    {
        UpdateWalls();
    }

    void UpdateWalls()
    {
        Vector3 pos = transform.position;

        // north (+Z)
        if (HasNeighborAt(pos + Vector3.forward * detectDistance))
            northWall.SetActive(false);
        // south (-Z)
        if (HasNeighborAt(pos + Vector3.back * detectDistance))
            southWall.SetActive(false);
        // east (+X)
        if (HasNeighborAt(pos + Vector3.right * detectDistance))
            eastWall.SetActive(false);
        // west (-X)
        if (HasNeighborAt(pos + Vector3.left * detectDistance))
            westWall.SetActive(false);
    }

    bool HasNeighborAt(Vector3 checkPos)
    {
        Collider[] hits = Physics.OverlapSphere(checkPos, 0.5f);
        foreach (var h in hits)
        {
            Room other = h.GetComponentInParent<Room>();
            if (other != null && other != this)
                return true;
        }
        return false;
    }
}
