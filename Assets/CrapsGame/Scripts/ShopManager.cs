using JetBrains.Annotations;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ShopManager : MonoBehaviour
{
    public GameObject shopPanel;
    public GameObject Button1;
    public GameObject Button2;
    public GameObject Button3;
    private bool isOpen = false;

    [Header("References")]
    [Tooltip("Assign the DiceManager so the shop can receive payouts and show balance.")]
    public DiceManager diceManager;

    [Tooltip("Text element inside the shop panel to display player's money.")]
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI dicePrice;

    [Tooltip("Text element (on a button or label) to display rolls left.")]
    public TextMeshProUGUI rollsLeftText;
    [Tooltip("Text element to display cost for unlocking the next die.")]
    public TextMeshProUGUI buyCostText;

    [Header("Buttons (optional)")]
    [Tooltip("Optional reference to the Roll UI Button so ShopManager can auto-wire the click and enable/disable it.")]
    public Button rollButton;

    [Tooltip("Optional reference to the Payout UI Button so ShopManager can auto-wire the click and enable/disable it.")]
    public Button payoutButton;
    public Button purchaseUpgradeButton;

    [Tooltip("If true, the shop will automatically apply last payout to balance when a payout event arrives.")]
    public bool autoApplyPayouts = true;
    [Tooltip("Optional Buy button to purchase/unlock another die.")]
    public Button buyButton;

    [Header("Upgrade UI (optional)")]
    [Tooltip("Input field where designer/player can enter the die index to upgrade (0-based).")]
    public TMP_InputField upgradeIndexInput;

    [Tooltip("Text element to display the cost for upgrading the chosen die.")]
    public TextMeshProUGUI upgradeCostText;

    [Tooltip("Button to purchase an upgrade for the entered die index.")]
    public Button upgradeButton;

    private void OnEnable()
    {
        if (diceManager != null)
            diceManager.OnPayoutCalculated += OnPayoutCalculated;

        if (diceManager != null)
            diceManager.OnRollsLeftChanged += OnRollsLeftChanged;
        // auto-update UI immediately when enabled
        UpdateBalanceDisplay();
        if (diceManager != null) UpdateRollsLeftDisplay(diceManager.GetRollsLeft());
        UpdateBuyDisplay();
        UpdateUpgradeDisplay();
    }

    private void OnDisable()
    {
        if (diceManager != null)
            diceManager.OnPayoutCalculated -= OnPayoutCalculated;

        if (diceManager != null)
            diceManager.OnRollsLeftChanged -= OnRollsLeftChanged;
        // remove auto-wired listeners
        if (rollButton != null)
            rollButton.onClick.RemoveListener(OnRollButtonPressed);
        if (payoutButton != null)
            payoutButton.onClick.RemoveListener(OnPayoutButtonPressed);
    }

    private void OnPayoutCalculated(int payout, int sum)
    {
        if (diceManager == null) return;
        
        UpdateBalanceDisplay();
        // re-enable roll button if player still has rolls
        if (rollButton != null && diceManager != null)
            rollButton.interactable = diceManager.CanRoll();

        // update payout button state: enable if there's an unapplied payout and autoApply is off
        if (payoutButton != null && diceManager != null)
        {
            bool hasUnapplied = diceManager.GetLastPayout() > 0 && !autoApplyPayouts;
            payoutButton.interactable = hasUnapplied;
        }

        UpdateBuyDisplay();
    }

    private void UpdateBalanceDisplay()
    {
        if (balanceText == null || diceManager == null) return;
        balanceText.text = "Money: " + diceManager.GetPlayerBalance();
    }

    private void OnRollsLeftChanged(int newLeft)
    {
        UpdateRollsLeftDisplay(newLeft);
    }

    private void UpdateRollsLeftDisplay(int newLeft)
    {
        if (rollsLeftText == null) return;
        rollsLeftText.text = $"Rolls: {newLeft}";
        if (rollButton != null)
            rollButton.interactable = newLeft > 0;
        UpdateBuyDisplay();
    }

    private void UpdateBuyDisplay()
    {
        if (buyCostText == null || diceManager == null) return;
        int cost = diceManager.GetNextDiceCost();
        buyCostText.text = $"Buy Die: {cost}";

        if (buyButton != null)
            buyButton.interactable = diceManager.GetPlayerBalance() >= cost;
        UpdateUpgradeDisplay();
    }

    private void UpdateUpgradeDisplay()
    {
        if (diceManager == null) return;

        int index = 0;
        if (upgradeIndexInput != null)
        {
            int.TryParse(upgradeIndexInput.text, out index);
        }

        if (upgradeCostText != null)
        {
            int cost = diceManager.GetUpgradeCost(index);
            upgradeCostText.text = $"Upgrade (die {index}): {cost}";
        }

        if (upgradeButton != null)
        {
            int cost = diceManager.GetUpgradeCost(index);
            upgradeButton.interactable = diceManager.GetPlayerBalance() >= cost && (diceManager != null && index >= 0 && index < (diceManager != null ? (diceManager.GetLastFaces().Length) : 0));
        }
    }

    // Called by UI roll button
    public void OnRollButtonPressed()
    {
        Debug.Log("ShopManager: OnRollButtonPressed called");
        if (diceManager == null) return;
        diceManager.RollAll();
        // disable roll button right away to avoid duplicate clicks
        if (rollButton != null) rollButton.interactable = false;
        // refresh display immediately
        UpdateRollsLeftDisplay(diceManager.GetRollsLeft());
    }

    // Called by UI payout button
    public void OnPayoutButtonPressed()
    {
        Debug.Log("ShopManager: OnPayoutButtonPressed called");
        if (diceManager == null) return;
        int payout = diceManager.GetLastPayout();
        if (payout <= 0)
        {
            Debug.Log("No payout available to apply");
            // ensure button disabled to reflect no payout
            if (payoutButton != null) payoutButton.interactable = false;
            return;
        }

        diceManager.ApplyLastPayout();
        UpdateBalanceDisplay();
        // after applying, disable payout button
        if (payoutButton != null) payoutButton.interactable = false;
        UpdateBuyDisplay();
    }

    // Called by UI buy button
    public void OnBuyButtonPressed()
    {
        if (diceManager == null) return;
        int cost = diceManager.GetNextDiceCost();
        bool ok = diceManager.TryPurchaseUnlockDice();
        if (ok)
        {
            Debug.Log($"Purchased die for {cost}");
            UpdateBalanceDisplay();
            UpdateBuyDisplay();
        }
        else
        {
            Debug.Log($"Cannot purchase die for {cost} — insufficient funds or none available");
        }
    }

    private void Start()
    {
        // auto-wire button listeners if buttons were assigned but not hooked in the inspector
        if (rollButton != null)
        {
            // ensure listener is not double-added
            rollButton.onClick.RemoveListener(OnRollButtonPressed);
            rollButton.onClick.AddListener(OnRollButtonPressed);
            // initial interactable state
            if (diceManager != null) rollButton.interactable = diceManager.CanRoll();
        }

        if (payoutButton != null)
        {
            payoutButton.onClick.RemoveListener(OnPayoutButtonPressed);
            payoutButton.onClick.AddListener(OnPayoutButtonPressed);
            // initial state: enable if there's a payout available
            if (diceManager != null) payoutButton.interactable = (diceManager.GetLastPayout() > 0 && !autoApplyPayouts);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeButtonPressed);
            upgradeButton.onClick.AddListener(OnUpgradeButtonPressed);
        }
    }

    // Called by UI upgrade button
    public void OnUpgradeButtonPressed()
    {
        Debug.Log("ShopManager: OnUpgradeButtonPressed called");
        if (diceManager == null) return;

        int index = 0;
        if (upgradeIndexInput != null)
        {
            if (!int.TryParse(upgradeIndexInput.text, out index))
            {
                Debug.Log("Invalid upgrade index");
                return;
            }
        }

        int cost = diceManager.GetUpgradeCost(index);
        bool ok = diceManager.TryPurchaseUpgradeDie(index);
        if (ok)
        {
            Debug.Log($"Upgraded die {index} for {cost}");
            UpdateBalanceDisplay();
            UpdateUpgradeDisplay();
            UpdateBuyDisplay();
        }
        else
        {
            Debug.Log($"Cannot upgrade die {index} for {cost} — insufficient funds or invalid index/already upgraded");
        }
    }

    public void toggleShop()
    {
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
        Button1.SetActive(isOpen);
        Button2.SetActive(isOpen);
        Button3.SetActive(isOpen);
        balanceText.gameObject.SetActive(isOpen);
        dicePrice.gameObject.SetActive(isOpen);

        if (isOpen)
        {
            // refresh display when opening
            UpdateBalanceDisplay();
        }
    }

    public void purchaseRolls(int cost) 
    {
        if (diceManager == null) return;
        int playerBalance = diceManager.GetPlayerBalance();
        if (playerBalance >= cost) {
            diceManager.adjustPlayerBalance(-cost);
            diceManager.rollsPerReset += 1;
            UpdateBalanceDisplay();
        }

    }
}
