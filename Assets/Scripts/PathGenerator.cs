using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomPathGenerator : MonoBehaviour
{
    [System.Serializable]
    public class RoomPrefab
    {
        public GameObject prefab;
        public Vector2Int size = new Vector2Int(3, 3);
        public int weight = 1;
        public bool guaranteedSpawn = false;
    }

    [Header("Room Prefabs")]
    [SerializeField] private List<RoomPrefab> roomPrefabs = new List<RoomPrefab>();

    [Header("Corridor Settings")]
    [SerializeField] private GameObject corridorTilePrefab;
    [SerializeField] private float tileSize = 15f;

    [Header("Dot Settings")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private int numDots = 25;

    [Header("Generation Settings")]
    [SerializeField] private int boardSize = 50;
    [SerializeField] private int numRooms = 8;
    [SerializeField] private int seed = 0;
    [SerializeField] private float variation = 0.5f;
    [SerializeField] private int minRoomSpacing = 2;

    [Header("Optimization")]
    [SerializeField] private Transform player;
    [SerializeField] private float activeRadius = 100f;
    [SerializeField] private float updateInterval = 0.5f;

    private int[,] board;
    private List<PlacedRoom> placedRooms = new List<PlacedRoom>();
    private List<GameObject> spawnedCorridors = new List<GameObject>();
    private List<GameObject> spawnedDots = new List<GameObject>();
    private float updateTimer;
    private int stepCounter;

    private static readonly Vector2Int[] Directions = new[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

    private class PlacedRoom
    {
        public Vector2Int position;
        public Vector2Int size;
        public GameObject instance;
        public Vector2Int center;

        public PlacedRoom(Vector2Int pos, Vector2Int sz, GameObject inst)
        {
            position = pos;
            size = sz;
            instance = inst;
            center = new Vector2Int(pos.x + sz.x / 2, pos.y + sz.y / 2);
        }

        public bool Overlaps(Vector2Int otherPos, Vector2Int otherSize, int spacing)
        {
            return !(position.x - spacing >= otherPos.x + otherSize.x ||
                     otherPos.x - spacing >= position.x + size.x ||
                     position.y - spacing >= otherPos.y + otherSize.y ||
                     otherPos.y - spacing >= position.y + size.y);
        }

        public bool ContainsPoint(Vector2Int point)
        {
            return point.x >= position.x && point.x < position.x + size.x &&
                   point.y >= position.y && point.y < position.y + size.y;
        }
    }

    private void Start()
    {
        if (seed != 0)
            Random.InitState(seed);

        GenerateWithSeed();
    }

    private void Update()
    {
        if (player == null) return;

        updateTimer += Time.deltaTime;
        if (updateTimer < updateInterval) return;
        updateTimer = 0f;

        Vector3 playerPos = player.position;

        foreach (var room in placedRooms)
        {
            float dist = Vector3.Distance(playerPos, room.instance.transform.position);
            bool active = dist < activeRadius;
            if (room.instance.activeSelf != active)
                room.instance.SetActive(active);
        }

        foreach (var c in spawnedCorridors)
        {
            if (c == null) continue;
            bool active = Vector3.Distance(playerPos, c.transform.position) < activeRadius;
            if (c.activeSelf != active)
                c.SetActive(active);
        }

        foreach (var d in spawnedDots)
        {
            if (d == null) continue;
            bool active = Vector3.Distance(playerPos, d.transform.position) < activeRadius;
            if (d.activeSelf != active)
                d.SetActive(active);
        }
    }

    private class PriorityQueue
    {
        private SortedSet<PriorityItem> items;
        private int currentIndex = 0;

        public PriorityQueue()
        {
            items = new SortedSet<PriorityItem>(Comparer<PriorityItem>.Create((a, b) =>
            {
                int result = a.Priority.CompareTo(b.Priority);
                return result != 0 ? result : a.Index.CompareTo(b.Index);
            }));
        }

        public void Enqueue(float priority, Vector2Int position)
        {
            items.Add(new PriorityItem(priority, position, currentIndex++));
        }

        public Vector2Int Dequeue()
        {
            var item = items.Min;
            items.Remove(item);
            return item.Position;
        }

        public bool IsEmpty() => items.Count == 0;
    }

    private class PriorityItem
    {
        public float Priority { get; }
        public Vector2Int Position { get; }
        public int Index { get; }

        public PriorityItem(float priority, Vector2Int position, int index)
        {
            Priority = priority;
            Position = position;
            Index = index;
        }
    }

    private RoomPrefab SelectRandomRoomPrefab(HashSet<RoomPrefab> excludeGuaranteed = null)
    {
        var availableRooms = roomPrefabs.Where(r =>
            !r.guaranteedSpawn || (excludeGuaranteed != null && !excludeGuaranteed.Contains(r))
        ).ToList();

        int totalWeight = availableRooms.Sum(r => r.weight);
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var room in availableRooms)
        {
            currentWeight += room.weight;
            if (randomValue < currentWeight)
                return room;
        }

        return availableRooms[0];
    }

    private bool CanPlaceRoom(Vector2Int position, Vector2Int size)
    {
        if (position.x < 0 || position.y < 0 ||
            position.x + size.x > boardSize || position.y + size.y > boardSize)
            return false;

        foreach (var room in placedRooms)
            if (room.Overlaps(position, size, minRoomSpacing))
                return false;

        return true;
    }

    private void PlaceRooms()
    {
        placedRooms.Clear();
        var guaranteedRoomsToPlace = roomPrefabs.Where(r => r.guaranteedSpawn).ToList();
        var guaranteedRoomsPlaced = new HashSet<RoomPrefab>();

        foreach (var roomPrefab in guaranteedRoomsToPlace)
        {
            int attempts = 0;
            while (attempts++ < 100)
            {
                Vector2Int pos = new Vector2Int(
                    Random.Range(0, boardSize - roomPrefab.size.x),
                    Random.Range(0, boardSize - roomPrefab.size.y)
                );
                if (CanPlaceRoom(pos, roomPrefab.size))
                {
                    PlaceRoom(roomPrefab, pos);
                    guaranteedRoomsPlaced.Add(roomPrefab);
                    break;
                }
            }
        }

        int totalAttempts = 0;
        while (placedRooms.Count < numRooms && totalAttempts++ < numRooms * 50)
        {
            var roomPrefab = SelectRandomRoomPrefab(guaranteedRoomsPlaced);
            Vector2Int pos = new Vector2Int(
                Random.Range(0, boardSize - roomPrefab.size.x),
                Random.Range(0, boardSize - roomPrefab.size.y)
            );
            if (CanPlaceRoom(pos, roomPrefab.size))
                PlaceRoom(roomPrefab, pos);
        }
    }

    private void PlaceRoom(RoomPrefab roomPrefab, Vector2Int position)
    {
        for (int x = position.x; x < position.x + roomPrefab.size.x; x++)
            for (int y = position.y; y < position.y + roomPrefab.size.y; y++)
                board[x, y] = ++stepCounter;

        Vector3 worldPos = new Vector3(
            position.x * tileSize + (roomPrefab.size.x * tileSize) / 2f,
            0,
            position.y * tileSize + (roomPrefab.size.y * tileSize) / 2f
        );

        var instance = Instantiate(roomPrefab.prefab, worldPos, Quaternion.identity, transform);
        placedRooms.Add(new PlacedRoom(position, roomPrefab.size, instance));
    }

    private Vector2Int GetClosestEdgePoint(PlacedRoom room, Vector2Int target)
    {
        float minDist = float.MaxValue;
        Vector2Int closest = room.center;
        for (int x = room.position.x; x < room.position.x + room.size.x; x++)
        {
            for (int y = room.position.y; y < room.position.y + room.size.y; y++)
            {
                bool edge = (x == room.position.x || x == room.position.x + room.size.x - 1 ||
                             y == room.position.y || y == room.position.y + room.size.y - 1);
                if (!edge) continue;
                Vector2Int p = new Vector2Int(x, y);
                float dist = ManhattanDistance(p, target);
                if (dist < minDist) { minDist = dist; closest = p; }
            }
        }
        return closest;
    }

    private List<Vector2Int> ConnectRooms(PlacedRoom r1, PlacedRoom r2)
    {
        Vector2Int start = GetClosestEdgePoint(r1, r2.center);
        Vector2Int end = GetClosestEdgePoint(r2, r1.center);
        var frontier = new PriorityQueue();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var cost = new Dictionary<Vector2Int, float>();

        frontier.Enqueue(0, start);
        cost[start] = 0;

        while (!frontier.IsEmpty())
        {
            var cur = frontier.Dequeue();
            if (cur == end) break;

            foreach (var d in Directions)
            {
                var next = cur + d;
                if (!IsInBounds(next)) continue;

                bool inTarget = r1.ContainsPoint(next) || r2.ContainsPoint(next);
                if (!inTarget)
                {
                    bool inOther = placedRooms.Any(r => r != r1 && r != r2 && r.ContainsPoint(next));
                    if (inOther) continue;
                }

                float baseCost = (board[next.x, next.y] > 0) ? 0.1f : 1f;
                float newCost = cost[cur] + baseCost;
                if (!cost.ContainsKey(next) || newCost < cost[next])
                {
                    cost[next] = newCost;
                    float priority = newCost + ManhattanDistance(next, end) + Random.Range(0f, variation);
                    frontier.Enqueue(priority, next);
                    cameFrom[next] = cur;
                }
            }
        }
        return ReconstructPath(start, end, cameFrom);
    }

    private List<Vector2Int> ReconstructPath(Vector2Int start, Vector2Int end, Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        var path = new List<Vector2Int>();
        var current = end;
        while (current != start)
        {
            path.Add(current);
            if (!cameFrom.TryGetValue(current, out current)) break;
        }
        path.Add(start);
        path.Reverse();

        foreach (var p in path)
            if (board[p.x, p.y] == 0)
                board[p.x, p.y] = ++stepCounter;

        return path;
    }

    private void ConnectAllRooms()
    {
        if (placedRooms.Count < 2) return;
        var connected = new HashSet<PlacedRoom> { placedRooms[0] };
        var unconnected = new HashSet<PlacedRoom>(placedRooms.Skip(1));

        while (unconnected.Count > 0)
        {
            float min = float.MaxValue;
            PlacedRoom closest = null;
            PlacedRoom from = null;

            foreach (var c in connected)
                foreach (var u in unconnected)
                {
                    float d = ManhattanDistance(c.center, u.center);
                    if (d < min) { min = d; closest = u; from = c; }
                }

            if (closest == null) break;
            ConnectRooms(from, closest);
            connected.Add(closest);
            unconnected.Remove(closest);
        }
    }

    private void InstantiateCorridors()
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                bool isInRoom = placedRooms.Any(r => r.ContainsPoint(new Vector2Int(x, y)));
                if (board[x, y] > 0 && !isInRoom)
                {
                    var pos = new Vector3(x * tileSize, 0, y * tileSize);
                    var corridor = Instantiate(corridorTilePrefab, pos, Quaternion.identity, transform);
                    spawnedCorridors.Add(corridor);
                }
            }
        }
    }

    private void PlaceAndConnectDots()
    {
        var validTiles = new List<Vector2Int>();
        for (int x = 0; x < boardSize; x++)
            for (int y = 0; y < boardSize; y++)
                if (board[x, y] > 0) validTiles.Add(new Vector2Int(x, y));

        ShuffleList(validTiles);
        int count = Mathf.Min(numDots, validTiles.Count);
        for (int i = 0; i < count; i++)
            InstantiateDot(validTiles[i]);
    }

    private static void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void InstantiateDot(Vector2Int pos)
    {
        var worldPos = new Vector3(pos.x * tileSize, 1f, pos.y * tileSize);
        var dot = Instantiate(dotPrefab, worldPos, Quaternion.identity, transform);
        spawnedDots.Add(dot);
    }

    private static int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < boardSize && pos.y >= 0 && pos.y < boardSize;
    }

    public void GenerateWithSeed(int newSeed = 0)
    {
        ClearExistingObjects();
        if (newSeed != 0)
        {
            seed = newSeed;
            Random.InitState(seed);
        }

        board = new int[boardSize, boardSize];
        stepCounter = 0;
        PlaceRooms();
        ConnectAllRooms();
        InstantiateCorridors();
        PlaceAndConnectDots();
    }

    private void ClearExistingObjects()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        placedRooms.Clear();
        spawnedCorridors.Clear();
        spawnedDots.Clear();
    }

    public void GenerateNew()
    {
        GenerateWithSeed(Random.Range(1, 100000));
    }
}
