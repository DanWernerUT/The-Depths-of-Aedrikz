using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RoomPathGenerator;

public class RoomPathGenerator : MonoBehaviour
{
    [System.Serializable]
    public class RoomPrefab
    {
        public GameObject prefab;
        public Vector2Int size = new Vector2Int(3, 3);
        public int weight = 1;
        public bool guaranteedSpawn = false;
        [Tooltip("If true, this room will only have one connection path (dead-end room)")]
        public bool singleConnectionOnly = false;
        [Tooltip("If true, this room will be connected to a specific edge of the map")]
        public bool connectToEdge = false;
        [Tooltip("Which edge to connect to (North=top, South=bottom, East=right, West=left). If None, chooses randomly.")]
        public EdgeDirection edgeDirection = EdgeDirection.None;
    }

    public enum EdgeDirection
    {
        None,
        North,
        South,
        East,
        West
    }

    [System.Serializable]
    public struct GenerationStats
    {
        public int roomsPlaced;
        public int guaranteedRoomsPlaced;
        public int corridorTilesCreated;
        public int dotsPlaced;
        public float generationTime;

        public override string ToString()
        {
            return $"Rooms: {roomsPlaced} (Guaranteed: {guaranteedRoomsPlaced}), Corridors: {corridorTilesCreated}, Dots: {dotsPlaced}, Time: {generationTime:F3}s";
        }
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
    [SerializeField] private bool showDebugGrid = true;

    [Header("Optimization")]
    [SerializeField] private Transform player;
    [SerializeField] private float activeRadius = 100f;
    [SerializeField] private float updateInterval = 0.5f;

    [Header("Statistics")]
    [SerializeField] private GenerationStats lastGenerationStats;

    private int[,] board;
    private List<PlacedRoom> placedRooms = new List<PlacedRoom>();
    private List<GameObject> spawnedCorridors = new List<GameObject>();
    private List<GameObject> spawnedDots = new List<GameObject>();
    private float updateTimer;
    private int stepCounter;

    // Spatial optimization fields
    private float sqrActiveRadius;
    private Dictionary<Vector2Int, List<GameObject>> spatialGrid;
    private const int GRID_CELL_SIZE = 50;

    // Cache for room containment checks
    private HashSet<Vector2Int> roomTileCache = new HashSet<Vector2Int>();

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
        public bool singleConnectionOnly;
        public int connectionCount;
        public bool connectToEdge;
        public EdgeDirection edgeDirection;

        public PlacedRoom(Vector2Int pos, Vector2Int sz, GameObject inst, bool singleConn = false, bool connToEdge = false, EdgeDirection edgeDir = EdgeDirection.None)
        {
            position = pos;
            size = sz;
            instance = inst;
            center = new Vector2Int(pos.x + sz.x / 2, pos.y + sz.y / 2);
            singleConnectionOnly = singleConn;
            connectionCount = 0;
            connectToEdge = connToEdge;
            edgeDirection = edgeDir;
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

        public bool CanAcceptConnection()
        {
            return !singleConnectionOnly || connectionCount < 1;
        }

        public void AddConnection()
        {
            connectionCount++;
        }
    }

    private void OnValidate()
    {
        // Validate required references
        if (corridorTilePrefab == null)
            Debug.LogWarning("[RoomPathGenerator] Corridor tile prefab not assigned!");

        if (dotPrefab == null)
            Debug.LogWarning("[RoomPathGenerator] Dot prefab not assigned!");

        // Validate room prefabs
        if (roomPrefabs == null || roomPrefabs.Count == 0)
        {
            Debug.LogWarning("[RoomPathGenerator] No room prefabs assigned!");
        }
        else
        {
            for (int i = 0; i < roomPrefabs.Count; i++)
            {
                var room = roomPrefabs[i];
                if (room.prefab == null)
                    Debug.LogWarning($"[RoomPathGenerator] Room prefab at index {i} is null!");

                if (room.size.x <= 0 || room.size.y <= 0)
                    Debug.LogWarning($"[RoomPathGenerator] Room prefab at index {i} has invalid size: {room.size}");

                if (room.weight <= 0)
                    Debug.LogWarning($"[RoomPathGenerator] Room prefab at index {i} has invalid weight: {room.weight}");
            }
        }

        // Validate generation settings
        if (numRooms > boardSize * boardSize / 100)
            Debug.LogWarning("[RoomPathGenerator] Too many rooms for board size - generation may fail!");

        if (boardSize <= 0)
            Debug.LogError("[RoomPathGenerator] Board size must be positive!");

        if (tileSize <= 0)
            Debug.LogError("[RoomPathGenerator] Tile size must be positive!");

        // Check if guaranteed rooms can fit
        var guaranteedRooms = roomPrefabs.Where(r => r.guaranteedSpawn).ToList();
        int totalGuaranteedArea = guaranteedRooms.Sum(r => r.size.x * r.size.y);
        int boardArea = boardSize * boardSize;

        if (totalGuaranteedArea > boardArea * 0.5f)
            Debug.LogWarning("[RoomPathGenerator] Guaranteed rooms take up more than 50% of board area - placement may fail!");
    }

    private void Start()
    {
        if (seed != 0)
            Random.InitState(seed);

        // Cache squared radius to avoid sqrt calculations
        sqrActiveRadius = activeRadius * activeRadius;

        GenerateWithSeed();
    }

    private void Update()
    {
        if (player == null || spatialGrid == null) return;

        updateTimer += Time.deltaTime;
        if (updateTimer < updateInterval) return;
        updateTimer = 0f;

        Vector3 playerPos = player.position;
        Vector2Int playerCell = WorldToGridCell(playerPos);

        // Calculate which cells to check based on active radius
        int cellRadius = Mathf.CeilToInt(activeRadius / GRID_CELL_SIZE);

        // Check objects only in nearby cells
        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int z = -cellRadius; z <= cellRadius; z++)
            {
                Vector2Int cell = new Vector2Int(playerCell.x + x, playerCell.y + z);

                if (!spatialGrid.ContainsKey(cell)) continue;

                foreach (var obj in spatialGrid[cell])
                {
                    if (obj == null) continue;

                    // Use squared distance to avoid expensive sqrt
                    Vector3 offset = obj.transform.position - playerPos;
                    float sqrDist = offset.sqrMagnitude;
                    bool shouldBeActive = sqrDist < sqrActiveRadius;

                    if (obj.activeSelf != shouldBeActive)
                        obj.SetActive(shouldBeActive);
                }
            }
        }
    }

    private Vector2Int WorldToGridCell(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / GRID_CELL_SIZE),
            Mathf.FloorToInt(worldPos.z / GRID_CELL_SIZE)
        );
    }

    private void AddToSpatialGrid(Vector2Int cell, GameObject obj)
    {
        if (!spatialGrid.ContainsKey(cell))
            spatialGrid[cell] = new List<GameObject>();

        spatialGrid[cell].Add(obj);
    }

    private void BuildSpatialGrid()
    {
        spatialGrid = new Dictionary<Vector2Int, List<GameObject>>();

        foreach (var room in placedRooms)
        {
            if (room.instance == null) continue;

            // Calculate world bounds - centered tiles
            Vector3 roomWorldMin = new Vector3(
                room.position.x * tileSize,
                0,
                room.position.y * tileSize
            );
            Vector3 roomWorldMax = new Vector3(
                (room.position.x + room.size.x) * tileSize,
                0,
                (room.position.y + room.size.y) * tileSize
            );

            Vector2Int minCell = WorldToGridCell(roomWorldMin);
            Vector2Int maxCell = WorldToGridCell(roomWorldMax);

            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int z = minCell.y; z <= maxCell.y; z++)
                {
                    AddToSpatialGrid(new Vector2Int(x, z), room.instance);
                }
            }
        }

        foreach (var corridor in spawnedCorridors)
        {
            if (corridor == null) continue;
            Vector2Int cell = WorldToGridCell(corridor.transform.position);
            AddToSpatialGrid(cell, corridor);
        }

        foreach (var dot in spawnedDots)
        {
            if (dot == null) continue;
            Vector2Int cell = WorldToGridCell(dot.transform.position);
            AddToSpatialGrid(cell, dot);
        }

        Debug.Log($"Spatial grid built with {spatialGrid.Count} cells");
    }

    private void BuildRoomTileCache()
    {
        roomTileCache.Clear();
        foreach (var room in placedRooms)
        {
            for (int x = room.position.x; x < room.position.x + room.size.x; x++)
            {
                for (int y = room.position.y; y < room.position.y + room.size.y; y++)
                {
                    roomTileCache.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    private bool IsPointInAnyRoom(Vector2Int point)
    {
        return roomTileCache.Contains(point);
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
            r.prefab != null && // Ensure prefab is not null
            (!r.guaranteedSpawn || (excludeGuaranteed != null && !excludeGuaranteed.Contains(r)))
        ).ToList();

        if (availableRooms.Count == 0)
        {
            Debug.LogError("[RoomPathGenerator] No available room prefabs to select!");
            return null;
        }

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
        var guaranteedRoomsToPlace = roomPrefabs.Where(r => r.guaranteedSpawn && r.prefab != null).ToList();
        var guaranteedRoomsPlaced = new HashSet<RoomPrefab>();

        // Place guaranteed rooms first
        foreach (var roomPrefab in guaranteedRoomsToPlace)
        {
            int attempts = 0;
            bool placed = false;
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
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"[RoomPathGenerator] Failed to place guaranteed room of size {roomPrefab.size} after 100 attempts!");
            }
        }

        // Place remaining rooms
        int totalAttempts = 0;
        while (placedRooms.Count < numRooms && totalAttempts++ < numRooms * 50)
        {
            var roomPrefab = SelectRandomRoomPrefab(guaranteedRoomsPlaced);
            if (roomPrefab == null) continue;

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

        // Calculate world position - align to grid consistently
        // Place at corner, then offset by half the room size
        Vector3 worldPos = new Vector3(
            position.x * tileSize + (roomPrefab.size.x * tileSize) / 2f,
            0,
            position.y * tileSize + (roomPrefab.size.y * tileSize) / 2f
        );

        var instance = Instantiate(roomPrefab.prefab, worldPos, Quaternion.identity, transform);

        // Scale the room to match the grid size
        if (roomPrefab.size.x != 1 || roomPrefab.size.y != 1)
        {
            Vector3 targetScale = new Vector3(
                roomPrefab.size.x * tileSize,
                instance.transform.localScale.y,
                roomPrefab.size.y * tileSize
            );
            instance.transform.localScale = targetScale;
        }
        
        placedRooms.Add(new PlacedRoom(position, roomPrefab.size, instance, 
            roomPrefab.singleConnectionOnly, roomPrefab.connectToEdge, roomPrefab.edgeDirection));
    }

    private Vector2Int GetClosestEdgePoint(PlacedRoom room, Vector2Int target)
    {
        // For edge connections, use center of the wall facing the target
        if (room.connectToEdge && room.singleConnectionOnly)
        {
            return GetWallCenterPoint(room, target);
        }

        // For normal connections, find closest edge point
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

    private Vector2Int GetWallCenterPoint(PlacedRoom room, Vector2Int target)
    {
        // Determine which wall is closest to the target
        Vector2Int roomCenter = room.center;
        Vector2Int diff = target - roomCenter;

        // Find which axis has greater distance
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            // Target is more to the left or right
            if (diff.x > 0)
            {
                // East wall (right)
                return new Vector2Int(room.position.x + room.size.x - 1, room.center.y);
            }
            else
            {
                // West wall (left)
                return new Vector2Int(room.position.x, room.center.y);
            }
        }
        else
        {
            // Target is more above or below
            if (diff.y > 0)
            {
                // North wall (top)
                return new Vector2Int(room.center.x, room.position.y + room.size.y - 1);
            }
            else
            {
                // South wall (bottom)
                return new Vector2Int(room.center.x, room.position.y);
            }
        }
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
                    // Use cached lookup instead of LINQ query
                    bool inOther = IsPointInAnyRoom(next) && !r1.ContainsPoint(next) && !r2.ContainsPoint(next);
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
        {
            bool isInRoom = IsPointInAnyRoom(p);
            if (board[p.x, p.y] == 0 && !isInRoom)
                board[p.x, p.y] = ++stepCounter;
        }

        return path;
    }

    private void ConnectAllRooms()
    {
        if (placedRooms.Count < 2) return;

        // Track rooms that need edge connections and shouldn't be connected normally
        var edgeOnlyRooms = placedRooms.Where(r => r.connectToEdge && r.singleConnectionOnly).ToHashSet();

        // Start with a non-edge-only room if possible
        PlacedRoom startRoom = placedRooms.FirstOrDefault(r => !edgeOnlyRooms.Contains(r));
        if (startRoom == null)
        {
            // All rooms are edge-only, just connect them all to edges
            Debug.Log("[RoomPathGenerator] All rooms are edge-only rooms. Connecting to edges only.");
            ConnectRoomsToEdges();
            return;
        }

        var connected = new HashSet<PlacedRoom> { startRoom };
        var unconnected = new HashSet<PlacedRoom>(placedRooms.Where(r => r != startRoom && !edgeOnlyRooms.Contains(r)));

        while (unconnected.Count > 0)
        {
            float min = float.MaxValue;
            PlacedRoom closest = null;
            PlacedRoom from = null;

            foreach (var c in connected)
            {
                // Skip if this connected room can't accept more connections
                if (!c.CanAcceptConnection()) continue;

                foreach (var u in unconnected)
                {
                    // Skip if the unconnected room can't accept connections
                    if (!u.CanAcceptConnection()) continue;

                    float d = ManhattanDistance(c.center, u.center);
                    if (d < min) { min = d; closest = u; from = c; }
                }
            }

            if (closest == null)
            {
                // If we couldn't find a valid connection, force connect remaining rooms
                // by temporarily ignoring single-connection restrictions
                Debug.LogWarning("[RoomPathGenerator] Some rooms with single-connection restriction may receive multiple connections to ensure all rooms are connected.");

                foreach (var c in connected)
                {
                    foreach (var u in unconnected)
                    {
                        float d = ManhattanDistance(c.center, u.center);
                        if (d < min) { min = d; closest = u; from = c; }
                    }
                }

                if (closest == null) break;
            }

            ConnectRooms(from, closest);
            from.AddConnection();
            closest.AddConnection();
            connected.Add(closest);
            unconnected.Remove(closest);
        }

        // After connecting all non-edge-only rooms, create edge connections
        ConnectRoomsToEdges();
    }

    private void ConnectRoomsToEdges()
    {
        foreach (var room in placedRooms)
        {
            if (!room.connectToEdge) continue;

            // Determine which edge to connect to
            EdgeDirection targetEdge = room.edgeDirection;
            if (targetEdge == EdgeDirection.None)
            {
                // Choose random edge
                EdgeDirection[] options = { EdgeDirection.North, EdgeDirection.South, EdgeDirection.East, EdgeDirection.West };
                targetEdge = options[Random.Range(0, options.Length)];
            }

            // Get the center point of the chosen edge
            Vector2Int edgePoint = GetEdgeCenterPoint(targetEdge);

            // Find the closest edge point on the room
            Vector2Int roomEdgePoint = GetClosestEdgePointToTarget(room, edgePoint);

            // Create path from room to edge
            ConnectRoomToEdge(room, roomEdgePoint, edgePoint);

            // Mark this as a connection (for tracking purposes)
            room.AddConnection();
        }
    }

    private Vector2Int GetEdgeCenterPoint(EdgeDirection edge)
    {
        int centerX = boardSize / 2;
        int centerY = boardSize / 2;

        switch (edge)
        {
            case EdgeDirection.North:
                return new Vector2Int(centerX, boardSize - 1);
            case EdgeDirection.South:
                return new Vector2Int(centerX, 0);
            case EdgeDirection.East:
                return new Vector2Int(boardSize - 1, centerY);
            case EdgeDirection.West:
                return new Vector2Int(0, centerY);
            default:
                return new Vector2Int(centerX, centerY);
        }
    }

    private Vector2Int GetClosestEdgePointToTarget(PlacedRoom room, Vector2Int target)
    {
        // Always connect to the center for edge-only rooms
        return room.center;
    }

    private void ConnectRoomToEdge(PlacedRoom room, Vector2Int roomPoint, Vector2Int edgePoint)
    {
        var frontier = new PriorityQueue();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var cost = new Dictionary<Vector2Int, float>();

        frontier.Enqueue(0, roomPoint);
        cost[roomPoint] = 0;

        while (!frontier.IsEmpty())
        {
            var cur = frontier.Dequeue();
            if (cur == edgePoint) break;

            foreach (var d in Directions)
            {
                var next = cur + d;
                if (!IsInBounds(next)) continue;

                bool inRoom = room.ContainsPoint(next);
                if (!inRoom)
                {
                    // Use cached lookup instead of LINQ query
                    bool inOtherRoom = IsPointInAnyRoom(next) && !room.ContainsPoint(next);
                    if (inOtherRoom) continue;
                }

                float baseCost = (board[next.x, next.y] > 0) ? 0.1f : 1f;
                float newCost = cost[cur] + baseCost;

                if (!cost.ContainsKey(next) || newCost < cost[next])
                {
                    cost[next] = newCost;
                    float priority = newCost + ManhattanDistance(next, edgePoint) + Random.Range(0f, variation);
                    frontier.Enqueue(priority, next);
                    cameFrom[next] = cur;
                }
            }
        }

        // Reconstruct and mark the path
        var current = edgePoint;
        while (current != roomPoint)
        {
            if (!cameFrom.TryGetValue(current, out var prev))
                break;

            bool isInRoom = IsPointInAnyRoom(current);
            if (board[current.x, current.y] == 0 && !isInRoom)
                board[current.x, current.y] = ++stepCounter;

            current = prev;
        }
    }

    private void InstantiateCorridors()
    {
        if (corridorTilePrefab == null)
        {
            Debug.LogError("[RoomPathGenerator] Cannot instantiate corridors - corridor tile prefab is null!");
            return;
        }

        int corridorCount = 0;
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                bool isInRoom = IsPointInAnyRoom(new Vector2Int(x, y));
                if (board[x, y] > 0 && !isInRoom)
                {
                    // Match room positioning: center of tile
                    var pos = new Vector3(
                        x * tileSize + tileSize / 2f, 
                        0, 
                        y * tileSize + tileSize / 2f
                    );
                    var corridor = Instantiate(corridorTilePrefab, pos, Quaternion.identity, transform);
                    spawnedCorridors.Add(corridor);
                    corridorCount++;
                }
            }
        }

        lastGenerationStats.corridorTilesCreated = corridorCount;
    }

    private void PlaceAndConnectDots()
    {
        if (dotPrefab == null)
        {
            Debug.LogWarning("[RoomPathGenerator] Cannot place dots - dot prefab is null!");
            lastGenerationStats.dotsPlaced = 0;
            return;
        }

        var validTiles = new List<Vector2Int>();
        for (int x = 0; x < boardSize; x++)
            for (int y = 0; y < boardSize; y++)
                if (board[x, y] > 0) validTiles.Add(new Vector2Int(x, y));

        ShuffleList(validTiles);
        int count = Mathf.Min(numDots, validTiles.Count);
        for (int i = 0; i < count; i++)
            InstantiateDot(validTiles[i]);

        lastGenerationStats.dotsPlaced = count;
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
        // Match tile positioning
        var worldPos = new Vector3(
            pos.x * tileSize + tileSize / 2f, 
            0.5f, 
            pos.y * tileSize + tileSize / 2f
        );
        float randomYRotation = Random.Range(0f, 360f);
        var rotation = Quaternion.Euler(0, randomYRotation, 0);
        var dot = Instantiate(dotPrefab, worldPos, rotation, transform);
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
        float startTime = Time.realtimeSinceStartup;

        ClearExistingObjects();

        if (newSeed != 0)
        {
            seed = newSeed;
            Random.InitState(seed);
        }

        board = new int[boardSize, boardSize];
        stepCounter = 0;

        PlaceRooms();

        // Count guaranteed rooms by checking if they were in the guaranteed list
        var guaranteedPrefabs = roomPrefabs.Where(p => p.guaranteedSpawn).Select(p => p.prefab).ToHashSet();
        int guaranteedCount = 0;
        foreach (var room in placedRooms)
        {
            string instanceName = room.instance.name.Replace("(Clone)", "").Trim();
            if (guaranteedPrefabs.Any(p => p != null && p.name == instanceName))
                guaranteedCount++;
        }

        lastGenerationStats.roomsPlaced = placedRooms.Count;
        lastGenerationStats.guaranteedRoomsPlaced = guaranteedCount;

        // Build room tile cache for optimized lookups
        BuildRoomTileCache();

        ConnectAllRooms();
        InstantiateCorridors();
        PlaceAndConnectDots();

        // Build spatial grid after all objects are instantiated
        BuildSpatialGrid();
        ClearAllDots();

        lastGenerationStats.generationTime = Time.realtimeSinceStartup - startTime;

        Debug.Log($"[RoomPathGenerator] Generation complete: {lastGenerationStats}");
    }
    public void ClearAllDots()
    {
        foreach (var dot in spawnedDots)
        {
            if (dot != null)
                Destroy(dot);
        }
        spawnedDots.Clear();

        // Update spatial grid to remove dot references
        if (spatialGrid != null)
        {
            foreach (var cell in spatialGrid.Values)
            {
                cell.RemoveAll(obj => obj == null);
            }
        }

        lastGenerationStats.dotsPlaced = 0;
        Debug.Log("[RoomPathGenerator] All dots cleared");
    }
    private void ClearExistingObjects()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        placedRooms.Clear();
        spawnedCorridors.Clear();
        spawnedDots.Clear();
        roomTileCache.Clear();

        // Clear spatial grid
        if (spatialGrid != null)
            spatialGrid.Clear();
    }

    public void GenerateNew()
    {
        GenerateWithSeed(Random.Range(1, 100000));
    }

    public GenerationStats GetLastGenerationStats()
    {
        return lastGenerationStats;
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!showDebugGrid || board == null) return;

        // Draw grid cells
        /*Gizmos.color = Color.gray;
        for (int x = 0; x <= boardSize; x++)
        {
            Vector3 start = new Vector3(x * tileSize, 0, 0);
            Vector3 end = new Vector3(x * tileSize, 0, boardSize * tileSize);
            Gizmos.DrawLine(start, end);
        }
        for (int y = 0; y <= boardSize; y++)
        {
            Vector3 start = new Vector3(0, 0, y * tileSize);
            Vector3 end = new Vector3(boardSize * tileSize, 0, y * tileSize);
            Gizmos.DrawLine(start, end);
        }
        */
        
        // Draw room bounds
        Gizmos.color = Color.green;
        foreach (var room in placedRooms)
        {
            Vector3 min = new Vector3(room.position.x * tileSize - (tileSize / 2), 0, room.position.y * tileSize - (tileSize / 2));
            Vector3 max = new Vector3(((room.position.x + room.size.x) * tileSize) - (tileSize / 2), 0, ((room.position.y + room.size.y) * tileSize) - (tileSize / 2));

            // Draw the boundary box
            Gizmos.DrawLine(new Vector3(min.x, 0, min.z), new Vector3(max.x, 0, min.z));
            Gizmos.DrawLine(new Vector3(max.x, 0, min.z), new Vector3(max.x, 0, max.z));
            Gizmos.DrawLine(new Vector3(max.x, 0, max.z), new Vector3(min.x, 0, max.z));
            Gizmos.DrawLine(new Vector3(min.x, 0, max.z), new Vector3(min.x, 0, min.z));

            // Draw X through the room for visibility
            Gizmos.DrawLine(min, max);
            Gizmos.DrawLine(new Vector3(min.x, 0, max.z), new Vector3(max.x, 0, min.z));
        }
    }
}