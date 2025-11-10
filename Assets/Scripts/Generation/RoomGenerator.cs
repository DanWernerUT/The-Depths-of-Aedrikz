//using System.Collections.Generic;
//using UnityEngine;

//public class RoomGenerator : MonoBehaviour
//{
//    public GameObject roomPrefab;
//    public int width = 1;
//    public int height = 1;
//    public float spacing = 10f;

//    private Dictionary<Vector2Int, Room> rooms = new Dictionary<Vector2Int, Room>();

//    void Start()
//    {
//        GenerateRooms();
//        UpdateAllWalls();
//    }

//    void GenerateRooms()
//    {
//        for (int x = 0; x < width; x++)
//        {
//            for (int y = 0; y < height; y++)
//            {
//                Vector2Int pos = new Vector2Int(x, y);
//                Vector3 worldPos = new Vector3(x * spacing, 0, y * spacing);
//                GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity, transform);
//                Room room = roomObj.GetComponent<Room>();
//                room.gridPos = pos;
//                room.generator = this;
//                rooms[pos] = room;
//            }
//        }
//    }

//    public bool HasRoomAt(Vector2Int pos)
//    {
//        return rooms.ContainsKey(pos);
//    }

//    void UpdateAllWalls()
//    {
//        foreach (Room r in rooms.Values)
//            r.UpdateWalls();
//    }
//}
