using UnityEngine;
using UnityEngine.UI;

public class ReloadButton : MonoBehaviour
{
    [SerializeField] private RoomPathGenerator generator;
    [SerializeField] private Button reloadButton;
    public int seed = 1;
    [Header("Optional: Key to reload")]
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField] private bool enableKeyboardShortcut = true;

    private void Start()
    {
        // Auto-find generator if not assigned
        if (generator == null)
        {
            generator = FindFirstObjectByType<RoomPathGenerator>();
            if (generator == null)
            {
                Debug.LogError("[ReloadButton] No RoomPathGenerator found in scene!");
                return;
            }
        }

        // Auto-find button if not assigned
        if (reloadButton == null)
        {
            reloadButton = GetComponent<Button>();
        }

        // Set up button click handler
        if (reloadButton != null)
        {
            reloadButton.onClick.AddListener(OnReloadClicked);
        }
        else
        {
            Debug.LogWarning("[ReloadButton] No Button component found! Assign one or attach this script to a Button.");
        }
    }

    private void Update()
    {
        // Keyboard shortcut
        if (enableKeyboardShortcut && Input.GetKeyDown(reloadKey))
        {
            OnReloadClicked();
        }
    }

    private void OnReloadClicked()
    {
        if (generator != null)
        {
            Debug.Log("[ReloadButton] Regenerating dungeon...");
            generator.GenerateWithSeed(seed);
        }
    }
}