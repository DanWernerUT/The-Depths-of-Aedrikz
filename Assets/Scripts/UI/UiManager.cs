using UnityEngine;

public class ToggleUI : MonoBehaviour
{
    public GameObject uiElement;

    private void Start()
    {
        uiElement.SetActive(false);
        GameState.paused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool newState = !uiElement.activeSelf;
            uiElement.SetActive(newState);

            GameState.paused = newState;

            if (newState)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
