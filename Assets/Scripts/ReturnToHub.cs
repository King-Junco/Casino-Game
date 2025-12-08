using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReturnToHub : MonoBehaviour
{
    [Header("Hub Scene Settings")]
    [Tooltip("Name of the hub world scene")]
    [SerializeField]
    private string hubSceneName = "HubWorld";

    [Header("UI Button")]
    [SerializeField]
    private Button returnButton;


    void Start()
    {
        // Make sure cursor is visible for UI games
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hook up button if assigned
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToHubWorld);
        }
    }

    void Update()
    {
        
    }

    public void ReturnToHubWorld()
    {
        if (!string.IsNullOrEmpty(hubSceneName))
        {
            SceneManager.LoadScene(hubSceneName);
        }
        else
        {
            Debug.LogError("Hub scene name is not set!");
        }
    }
}