using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Minimap : MonoBehaviour
{
    public Transform player;
    public float height = 50f;

    [Header("Mini View Settings")]
    public float miniMapSizePixels = 200f;
    public Vector2 miniMapMargin = new Vector2(20, 20);

    private Camera cam;
    private bool mapIsMini = true;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (!player) return;

        Vector3 pos = player.position;
        pos.y += height;
        transform.position = pos;
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);

        if (Input.GetKeyDown(KeyCode.M))
        {
            mapIsMini = !mapIsMini;
            UpdateViewport();
        }
    }

    void UpdateViewport()
    {
        if (mapIsMini)
        {
            float pixelSize = miniMapSizePixels;
            float normalizedSize = pixelSize / Mathf.Min(Screen.width, Screen.height);
            float xNorm = 1f - normalizedSize - (miniMapMargin.x / Screen.width);
            float yNorm = 1f - normalizedSize - (miniMapMargin.y / Screen.height);

            cam.rect = new Rect(xNorm, yNorm, normalizedSize, normalizedSize);
        }
        else
        {
            cam.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }

    void OnEnable()
    {
        cam.SetReplacementShader(Shader.Find("Unlit/Texture"), null);
    }
}
