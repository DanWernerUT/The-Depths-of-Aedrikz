using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Item[] inventory;

    public ItemDB itemDatabase;
    public void Start()
    {
        itemDatabase = GameObject.Find("ItemDB").GetComponent<ItemDB>();
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Inventory Contents:");
            foreach (var item in inventory)
            {
                if (item != null)
                {
                    Debug.Log($"- {item.name}");
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            itemDatabase.AddItem(0);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            itemDatabase.RemoveItem(0);
        }
    }
}
