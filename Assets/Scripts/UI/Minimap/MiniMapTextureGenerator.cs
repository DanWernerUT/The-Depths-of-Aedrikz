using UnityEngine;

public class MinimapTextureGenerator : MonoBehaviour
{
    [Header("Minimap Settings")]
    [SerializeField] private int textureSize = 2048;
    [SerializeField] private float cameraHeight = 50f;

    [Header("References")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private Material targetMaterial;
    [SerializeField] private RoomPathGenerator pathGenerator;

    private Texture2D mapTexture;
    private RenderTexture renderTexture;

    void Start()
    {
        // Find the path generator if not assigned
        if (pathGenerator == null)
        {
            pathGenerator = FindFirstObjectByType<RoomPathGenerator>();
        }

        // Create or get camera
        if (minimapCamera == null)
        {
            GameObject camObj = new GameObject("MinimapCamera");
            minimapCamera = camObj.AddComponent<Camera>();
        }

        // Don't capture yet - wait for generation to complete
    }

    public void CaptureMap()
    {
        if (pathGenerator == null)
        {
            Debug.LogError("[MinimapTextureGenerator] No RoomPathGenerator found!");
            return;
        }

        // Calculate camera bounds based on the generated map
        int boardSize = pathGenerator.GetBoardSize();
        float tileSize = pathGenerator.GetTileSize();
        float mapWorldSize = boardSize * tileSize;

        // Position camera to see entire map
        Vector3 mapCenter = new Vector3(mapWorldSize / 2f, 0, mapWorldSize / 2f);
        minimapCamera.transform.position = mapCenter + Vector3.up * cameraHeight;
        minimapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = mapWorldSize / 2f;
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = Color.red;

        // Create render texture
        renderTexture = new RenderTexture(textureSize, textureSize, 24);
        minimapCamera.targetTexture = renderTexture;

        // Render
        minimapCamera.Render();

        // Read pixels from render texture
        RenderTexture.active = renderTexture;
        mapTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);
        mapTexture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        mapTexture.Apply();

        // Cleanup
        RenderTexture.active = null;
        minimapCamera.targetTexture = null;

        // Apply to material if assigned
        if (targetMaterial != null)
        {
            targetMaterial.mainTexture = mapTexture;
            Debug.Log("[MinimapTextureGenerator] Texture applied to material!");
        }

        Debug.Log("[MinimapTextureGenerator] Map captured!");
    }

    public Texture2D GetMapTexture()
    {
        // If map hasn't been captured yet, capture it now
        if (mapTexture == null)
        {
            CaptureMap();
        }
        return mapTexture;
    }

    // Call this if you need to update the map
    public void RefreshMap()
    {
        CaptureMap();
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
        }

        if (minimapCamera != null && minimapCamera.gameObject.name == "MinimapCamera")
        {
            Destroy(minimapCamera.gameObject);
        }
    }
}