using JetBrains.Annotations;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

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
    [SerializeField] public TextMeshProUGUI errorText;
    [SerializeField] public TextMeshProUGUI confirmationText;


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
    [Tooltip("Dropdown where player selects the die to upgrade (1-6). Displayed values are 1-6, underlying index is 0-based.")]
    public TMP_Dropdown upgradeIndexDropdown;

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
        if (upgradeIndexDropdown != null)
            upgradeIndexDropdown.onValueChanged.RemoveAllListeners();
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
        if (upgradeIndexDropdown != null)
        {
            index = upgradeIndexDropdown.value; // zero-based
        }

        if (upgradeCostText != null)
        {
            int cost = diceManager.GetUpgradeCost(index);
            upgradeCostText.text = $"Upgrade (die {index + 1}): {cost}";
        }

        if (upgradeButton != null)
        {
            int cost = diceManager.GetUpgradeCost(index);
            int facesLen = (diceManager != null ? diceManager.GetLastFaces().Length : 0);
            upgradeButton.interactable = diceManager.GetPlayerBalance() >= cost && index >= 0 && index < facesLen;
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
            showError("No payout available to apply.");
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
            showConfirmation($"Purchased new die for {cost}!");
            UpdateBalanceDisplay();
            UpdateBuyDisplay();
        }
        else
        {
            showError($"Cannot purchase new die for {cost} — insufficient funds or all dice unlocked.");
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

        // Populate the upgrade dropdown (1-6) and wire change listener to refresh UI
        if (upgradeIndexDropdown != null)
        {
            // Ensure options 1..6 are present
            if (upgradeIndexDropdown.options == null || upgradeIndexDropdown.options.Count != 6)
            {
                upgradeIndexDropdown.ClearOptions();
                List<string> opts = new List<string>();
                for (int i = 1; i <= 6; i++) opts.Add(i.ToString());
                upgradeIndexDropdown.AddOptions(opts);
            }

            upgradeIndexDropdown.onValueChanged.RemoveAllListeners();
            upgradeIndexDropdown.onValueChanged.AddListener((int v) => UpdateUpgradeDisplay());
        }
    }

    // Called by UI upgrade button
    public void OnUpgradeButtonPressed()
    {
        Debug.Log("ShopManager: OnUpgradeButtonPressed called");
        if (diceManager == null) return;

        int index = 0;
        if (upgradeIndexDropdown != null)
        {
            index = upgradeIndexDropdown.value; // zero-based
        }

        int cost = diceManager.GetUpgradeCost(index);
        bool ok = diceManager.TryPurchaseUpgradeDie(index);
        if (ok)
        {
            showConfirmation($"Upgraded die {index} for {cost}!");
            UpdateBalanceDisplay();
            UpdateUpgradeDisplay();
            UpdateBuyDisplay();
        }
        else
        {
            showError($"Cannot upgrade die {index} for {cost} — insufficient funds or invalid die.");
        }
    }

    public void toggleShop()
    {
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
        Button1.SetActive(isOpen);
        Button2.SetActive(isOpen);
        Button3.SetActive(isOpen);
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
    public void showError(string message)
    {
        errorText.gameObject.SetActive(true);
        errorText.text = message;
        while (errorText.alpha > 0)
        {
            errorText.alpha -= Time.deltaTime;
        }
        errorText.gameObject.SetActive(false);
        errorText.alpha = 1;

    }

    public void showConfirmation(string message)
    {
        confirmationText.gameObject.SetActive(true);
        confirmationText.text = message;
        while (confirmationText.alpha > 0)
        {
            confirmationText.alpha -= Time.deltaTime;
        }
        confirmationText.gameObject.SetActive(false);
        confirmationText.alpha = 1;

    }
}

