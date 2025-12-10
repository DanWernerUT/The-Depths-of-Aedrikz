using System.Collections.Generic;
using UnityEngine;

public class ItemDB : MonoBehaviour
{
    public List<Item> itemDatabase = new();

    public void AddItem(int id)
    {
        foreach(var item in itemDatabase)
        {
            if(item.id == id)
            {
                Debug.Log("Item " + id + " is present in the database");
                return;
            }
        }
        Debug.Log("Item " + id + " is present in the database");

    }

    public void RemoveItem(int id)
    {

    }
}
