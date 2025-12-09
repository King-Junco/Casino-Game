using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReturnToHub : MonoBehaviour
{
    [SerializeField] private ExternalFileManager UniversalCurrency;
    [SerializeField] private DiceManager diceManager;
    [SerializeField] private BlackjackBettingSystem blackjackBettingSystem;
    [SerializeField] private BettingSystem bettingSystem;
    [SerializeField] private PachinkoMachine pachinkoMachine;

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
            // writing dicemanager money to external file
            if (SceneManager.GetActiveScene().name == "Craps"){
                UniversalCurrency.WriteToExternalFile(diceManager.getMoney());
            } else if (SceneManager.GetActiveScene().name == "Blackjack") {
                UniversalCurrency.WriteToExternalFile(blackjackBettingSystem.getMoney());
            } else if (SceneManager.GetActiveScene().name == "HorseRace"){
                UniversalCurrency.WriteToExternalFile(bettingSystem.getMoney());
            } else if (SceneManager.GetActiveScene().name == "Pachinko"){
                UniversalCurrency.WriteToExternalFile(pachinkoMachine.getMoney());
            }
            SceneManager.LoadScene(hubSceneName);
        }
        else
        {
            Debug.LogError("Hub scene name is not set!");
        }
    }
}
