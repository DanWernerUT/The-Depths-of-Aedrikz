using UnityEngine;

public class ToggleUI : MonoBehaviour
{
    // Assign the UI GameObject (like a Panel) in the Inspector
    public GameObject uiElement;

    private void Start()
    {
        // Ensure the UI is invisible at the start
        if (uiElement != null)
            uiElement.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (uiElement != null)
            {
                // Toggle the UI element's active state
                uiElement.SetActive(!uiElement.activeSelf);
            }
        }
    }
}
