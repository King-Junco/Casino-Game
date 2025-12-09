using JetBrains.Annotations;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
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
    [SerializeField] public TextMeshProUGUI errorText;
    [SerializeField] public TextMeshProUGUI confirmationText;

    [Header("Betting System")]
    [Tooltip("Bet amount input field for custom bet multiplier.")]
    public TMP_InputField betAmountInput;

    [Tooltip("Current bet multiplier.")]
    private float currentBetMultiplier = 1.5f;

    [Tooltip("Current bet amount (stored when rolling).")]
    private float currentBetAmount = 0f;

    [Tooltip("Toggle for tier 2-4 (low).")]
    public Toggle tierLowToggle;

    [Tooltip("Toggle for tier 5-9 (mid).")]
    public Toggle tierMidToggle;

    [Tooltip("Toggle for tier 10-12 (high).")]
    public Toggle tierHighToggle;

    [Tooltip("Current selected tier (0=none, 1=low 2-4, 2=mid 5-9, 3=high 10-12).")]
    private int currentTier = 0;

    [Header("Buttons (optional)")]
    [Tooltip("Optional reference to the Roll UI Button so ShopManager can auto-wire the click and enable/disable it.")]
    public Button rollButton;

    [Tooltip("Optional reference to the Payout UI Button so ShopManager can auto-wire the click and enable/disable it.")]
    public Button payoutButton;

    [Header("Upgrade UI (optional)")]
    [Tooltip("Dropdown where player selects the die to upgrade (1-6). Displayed values are 1-6, underlying index is 0-based.")]
    public TMP_Dropdown upgradeIndexDropdown;

    [Tooltip("Text element to display the cost for upgrading the chosen die.")]
    public TextMeshProUGUI upgradeCostText;

    [Tooltip("Button to purchase an upgrade for the entered die index.")]
    public Button upgradeButton;

    [Header("Purchase Rolls")]
    [Tooltip("Button to purchase additional rolls.")]
    public Button purchaseRollsButton;

    [Tooltip("Text to display cost of next roll purchase.")]
    public TextMeshProUGUI purchaseRollsCostText;

    [Header("Upgrade System")]
    [Tooltip("Dictionary tracking rolls remaining for each upgraded die.")]
    private Dictionary<int, int> upgradeRollsRemaining = new Dictionary<int, int>();

    [Tooltip("Base cost to purchase a roll.")]
    [SerializeField] private int baseRollCost = 5;
    [Tooltip("Scaling factor for roll purchase costs.")]
    [SerializeField] private float rollCostScale = 1.5f;
    private int rollsPurchased = 0;


    private void OnEnable()
    {
        if (diceManager != null)
            diceManager.OnPayoutCalculated += OnPayoutCalculated;

        if (diceManager != null)
            diceManager.OnRollsLeftChanged += OnRollsLeftChanged;
        // auto-update UI immediately when enabled
        UpdateBalanceDisplay();
        if (diceManager != null) UpdateRollsLeftDisplay(diceManager.GetRollsLeft());
        UpdateBetDisplay();
        UpdatePurchaseRollsDisplay();
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
        if (tierLowToggle != null)
            tierLowToggle.onValueChanged.RemoveListener(OnTierLowToggled);
        if (tierMidToggle != null)
            tierMidToggle.onValueChanged.RemoveListener(OnTierMidToggled);
        if (tierHighToggle != null)
            tierHighToggle.onValueChanged.RemoveListener(OnTierHighToggled);
    }

    private void OnPayoutCalculated(int payout, int sum)
    {
        if (diceManager == null) return;
        
        UpdateBalanceDisplay();
        // re-enable roll button if player still has rolls
        if (rollButton != null && diceManager != null)
            rollButton.interactable = diceManager.CanRoll();

        if (payoutButton != null && diceManager != null)
        {
            payoutButton.interactable = true;
        }

        UpdatePurchaseRollsDisplay();
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
        UpdatePurchaseRollsDisplay();
    }

    private void UpdateBetDisplay()
    {
        // Update bet multiplier input field
        if (betAmountInput != null)
        {
            betAmountInput.text = currentBetMultiplier.ToString("F2");
        }
    }

    private void UpdatePurchaseRollsDisplay()
    {
        if (purchaseRollsCostText == null || diceManager == null) return;
        int cost = GetRollPurchaseCost();
        purchaseRollsCostText.text = $"Buy Rolls: {cost}";

        if (purchaseRollsButton != null)
            purchaseRollsButton.interactable = diceManager.GetPlayerBalance() >= cost;
    }

    private void UpdateUpgradeDisplay()
    {
        if (diceManager == null) return;
        
        // Find the next die to upgrade using CURRENT upgrade status (not from last roll)
        int indexToUpgrade = -1;
        bool[] currentUpgraded = diceManager.GetCurrentUpgraded();
        
        // Check first 2 dice (or however many exist)
        for (int i = 0; i < currentUpgraded.Length && i < 2; i++)
        {
            if (currentUpgraded[i] == false)
            {
                indexToUpgrade = i;
                break;
            }
        }

        if (upgradeCostText != null)
        {
            if (indexToUpgrade == -1)
            {
                upgradeCostText.text = "All dice upgraded!";
            }
            else
            {
                int cost = diceManager.GetUpgradeCost(indexToUpgrade);
                upgradeCostText.text = $"Upgrade die {indexToUpgrade + 1}: {cost}";
            }
        }

        if (upgradeButton != null)
        {
            if (indexToUpgrade == -1)
            {
                upgradeButton.interactable = false;
            }
            else
            {
                int cost = diceManager.GetUpgradeCost(indexToUpgrade);
                bool canAfford = diceManager.GetPlayerBalance() >= cost;
                Debug.Log($"UpdateUpgradeDisplay: indexToUpgrade={indexToUpgrade}, cost={cost}, balance={diceManager.GetPlayerBalance()}, canAfford={canAfford}");
                upgradeButton.interactable = canAfford;
            }
        }
    }

    // Called by UI roll button
    public void OnRollButtonPressed()
    {
        Debug.Log("ShopManager: OnRollButtonPressed called");
        if (diceManager == null) return;
        
        // Check if a tier is selected
        if (currentTier == 0)
        {
            showError("Please select a betting tier before rolling!");
            return;
        }
        
        // Check if player has enough balance to make the bet
        int betAmount = Mathf.RoundToInt(currentBetMultiplier);
        if (diceManager.GetPlayerBalance() < betAmount)
        {
            showError($"Insufficient balance to place bet of {betAmount}! You have {diceManager.GetPlayerBalance()}.");
            return;
        }
        
        // Store the bet amount (don't deduct yet - deduct on payout)
        currentBetAmount = currentBetMultiplier;
        Debug.Log($"OnRollButtonPressed: Bet amount stored: {currentBetAmount}. currentTier={currentTier}");
        
        diceManager.RollAll();
        // Hide betting UI but keep balance visible
        Button1.SetActive(false);
        Button2.SetActive(false);
        Button3.SetActive(false);
        purchaseRollsButton.gameObject.SetActive(false);
        dicePrice.gameObject.SetActive(false);
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

        // Deduct the bet from balance (now, when payout is applied)
        int betAmount = Mathf.RoundToInt(currentBetAmount);
        diceManager.DeductBalance(betAmount);
        Debug.Log($"OnPayoutButtonPressed: Bet {betAmount} deducted. Balance after deduction: {diceManager.GetPlayerBalance()}");
        
        // Apply the payout
        int payoutToApply = diceManager.GetLastPayout();
        Debug.Log($"OnPayoutButtonPressed: About to apply payout of {payoutToApply}");
        diceManager.ApplyLastPayout();
        Debug.Log($"OnPayoutButtonPressed: After ApplyLastPayout, balance is {diceManager.GetPlayerBalance()}");
        UpdateBalanceDisplay();
        if (payoutButton != null) payoutButton.interactable = false;
        shopPanel.SetActive(true);
        Button1.SetActive(true);
        Button2.SetActive(true);
        Button3.SetActive(true);
        purchaseRollsButton.gameObject.SetActive(true);
        dicePrice.gameObject.SetActive(true);
        UpdatePurchaseRollsDisplay();
    }

    // Called by UI buy button - REMOVED (no longer purchasing dice)
    // Betting tier toggle handlers
    private void OnTierLowToggled(bool isOn)
    {
        if (isOn)
        {
            currentTier = 1; // tier 2-4
            if (tierMidToggle != null) tierMidToggle.SetIsOnWithoutNotify(false);
            if (tierHighToggle != null) tierHighToggle.SetIsOnWithoutNotify(false);
        }
        else if (currentTier == 1)
        {
            currentTier = 0;
        }
    }

    private void OnTierMidToggled(bool isOn)
    {
        if (isOn)
        {
            currentTier = 2; // tier 5-9
            if (tierLowToggle != null) tierLowToggle.SetIsOnWithoutNotify(false);
            if (tierHighToggle != null) tierHighToggle.SetIsOnWithoutNotify(false);
        }
        else if (currentTier == 2)
        {
            currentTier = 0;
        }
    }

    private void OnTierHighToggled(bool isOn)
    {
        if (isOn)
        {
            currentTier = 3; // tier 10-12
            if (tierLowToggle != null) tierLowToggle.SetIsOnWithoutNotify(false);
            if (tierMidToggle != null) tierMidToggle.SetIsOnWithoutNotify(false);
        }
        else if (currentTier == 3)
        {
            currentTier = 0;
        }
    }

    private void Start()
    {
        // auto-wire button listeners if buttons were assigned but not hooked in the inspector
        if (rollButton != null)
        {
            rollButton.onClick.RemoveListener(OnRollButtonPressed);
            rollButton.onClick.AddListener(OnRollButtonPressed);
            if (diceManager != null) rollButton.interactable = diceManager.CanRoll();
        }

        if (payoutButton != null)
        {
            payoutButton.onClick.RemoveListener(OnPayoutButtonPressed);
            payoutButton.onClick.AddListener(OnPayoutButtonPressed);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeButtonPressed);
            upgradeButton.onClick.AddListener(OnUpgradeButtonPressed);
        }

        // Wire up tier betting toggles
        if (tierLowToggle != null)
        {
            tierLowToggle.onValueChanged.RemoveListener(OnTierLowToggled);
            tierLowToggle.onValueChanged.AddListener(OnTierLowToggled);
        }

        if (tierMidToggle != null)
        {
            tierMidToggle.onValueChanged.RemoveListener(OnTierMidToggled);
            tierMidToggle.onValueChanged.AddListener(OnTierMidToggled);
        }

        if (tierHighToggle != null)
        {
            tierHighToggle.onValueChanged.RemoveListener(OnTierHighToggled);
            tierHighToggle.onValueChanged.AddListener(OnTierHighToggled);
        }

        // Wire purchase rolls button
        if (purchaseRollsButton != null)
        {
            purchaseRollsButton.onClick.RemoveListener(OnPurchaseRollsPressed);
            purchaseRollsButton.onClick.AddListener(OnPurchaseRollsPressed);
        }

        // Wire bet input field
        if (betAmountInput != null)
        {
            betAmountInput.onEndEdit.RemoveListener(OnBetMultiplierChanged);
            betAmountInput.onEndEdit.AddListener(OnBetMultiplierChanged);
        }

        UpdateBetDisplay();
        UpdatePurchaseRollsDisplay();
    }

    // Called by UI upgrade button
    public void OnUpgradeButtonPressed()
    {
        Debug.Log("ShopManager: OnUpgradeButtonPressed called");
        if (diceManager == null) return;

        // Find the first unupgraded die and upgrade it
        int indexToUpgrade = -1;
        bool[] currentUpgraded = diceManager.GetCurrentUpgraded();
        Debug.Log($"CurrentUpgraded array length: {currentUpgraded.Length}");
        for (int i = 0; i < currentUpgraded.Length && i < 2; i++)
        {
            Debug.Log($"Die {i}: upgraded = {currentUpgraded[i]}");
            if (currentUpgraded[i] == false)
            {
                indexToUpgrade = i;
                break;
            }
        }

        if (indexToUpgrade == -1)
        {
            showError("Both dice are already upgraded!");
            return;
        }

        int cost = diceManager.GetUpgradeCost(indexToUpgrade);
        Debug.Log($"Attempting to upgrade die {indexToUpgrade} (die {indexToUpgrade + 1}), cost: {cost}");
        bool ok = diceManager.TryPurchaseUpgradeDie(indexToUpgrade);
        if (ok)
        {
            // Mark the upgraded die with 3 rolls remaining
            upgradeRollsRemaining[indexToUpgrade] = 3;
            
            showConfirmation($"Upgraded die {indexToUpgrade + 1} for {cost}! (3 rolls)");
            UpdateBalanceDisplay();
            UpdateUpgradeDisplay();
            UpdatePurchaseRollsDisplay();
        }
        else
        {
            showError($"Cannot upgrade die {indexToUpgrade + 1} for {cost}!");
        }
    }

    // Purchase rolls button handler
    public void OnPurchaseRollsPressed()
    {
        if (diceManager == null) return;
        int cost = GetRollPurchaseCost();
        if (diceManager.GetPlayerBalance() < cost)
        {
            showError($"Insufficient funds to purchase rolls. Need {cost}");
            return;
        }

        diceManager.adjustPlayerBalance(-cost);
        diceManager.AddRolls(1);
        rollsPurchased++;
        
        showConfirmation($"Purchased 1 roll for {cost}!");
        UpdateBalanceDisplay();
        UpdatePurchaseRollsDisplay();
    }

    // Bet multiplier input field handler
    public void OnBetMultiplierChanged(string value)
    {
        if (float.TryParse(value, out float multiplier) && multiplier > 0)
        {
            currentBetMultiplier = multiplier;
        }
        else
        {
            // Reset to previous valid multiplier
            UpdateBetDisplay();
        }
    }

    // Check if a tier is currently selected (for validation)
    public bool IsTierSelected()
    {
        Debug.Log("IsTierSelected: " + (currentTier != 0));
        return currentTier != 0;
    }

    // Calculate payout based on dice sum, current tier selection, and current bet
    public int CalculatePayout(int sum)
    {
        Debug.Log($"=== CalculatePayout START ===");
        Debug.Log($"  currentBetAmount={currentBetAmount}, currentTier={currentTier}, sum={sum}");
        
        // Determine tier multiplier based on sum
        float tierMultiplier = 0f;
        
        if (currentTier == 1 && sum >= 2 && sum <= 4)
        {
            tierMultiplier = 2f; // Low tier: 2x bet
            showConfirmation($"TIER 1 WIN!");
        }
        else if (currentTier == 2 && sum >= 5 && sum <= 9)
        {
            tierMultiplier = 1.5f; // Mid tier: 1.5x bet
            showConfirmation($"TIER 2 WIN!");
        }
        else if (currentTier == 3 && sum >= 10 && sum <= 12)
        {
            tierMultiplier = 2f; // High tier: 2x bet
            showConfirmation($"TIER 3 WIN!");
        }
        else
        {
            showError($"LOSS.");
        }
        // If tier not selected or sum doesn't match, tierMultiplier stays 0

        // Payout = bet amount * tier multiplier
        int payout = Mathf.RoundToInt(currentBetAmount * tierMultiplier);
        
        // Apply upgrade bonuses (2x per upgraded die that still has rolls remaining)
        if (diceManager != null && tierMultiplier > 0)
        {
            bool[] lastUpgraded = diceManager.GetLastUpgraded();
            float upgradeMultiplier = 1f;
            for (int i = 0; i < lastUpgraded.Length; i++)
            {
                if (lastUpgraded[i] && upgradeRollsRemaining.ContainsKey(i) && upgradeRollsRemaining[i] > 0)
                {
                    upgradeMultiplier *= 2f;
                }
            }
            payout = Mathf.RoundToInt(payout * upgradeMultiplier);
            Debug.Log($"  Upgrade multiplier: {upgradeMultiplier}, payout after upgrades: {payout}");
        }
        
        Debug.Log($"  Final payout: {currentBetAmount} * {tierMultiplier} = {payout}");
        Debug.Log($"=== CalculatePayout END ===");
        
        // Decrement upgrade roll counters after this roll
        DecrementUpgradeRolls();
        
        return payout;
    }

    // Get cost for next roll purchase
    private int GetRollPurchaseCost()
    {
        float cost = baseRollCost * Mathf.Pow(rollCostScale, rollsPurchased);
        return Mathf.CeilToInt(cost);
    }

    // Decrement upgrade roll counters after each roll
    private void DecrementUpgradeRolls()
    {
        List<int> keysToRemove = new List<int>();
        List<int> keysToUpdate = new List<int>(upgradeRollsRemaining.Keys);
        
        foreach (var key in keysToUpdate)
        {
            upgradeRollsRemaining[key]--;
            if (upgradeRollsRemaining[key] <= 0)
            {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            upgradeRollsRemaining.Remove(key);
        }
    }

    public void toggleShop()
    {
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
        Button1.SetActive(isOpen);
        Button2.SetActive(isOpen);
        Button3.SetActive(isOpen);
        purchaseRollsButton.gameObject.SetActive(isOpen);
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
            diceManager.AddRolls(1);
            UpdateBalanceDisplay();
        }
    }
    public void showError(string message)
    {
        StartCoroutine(FadeOutError(message));
    }

    private IEnumerator FadeOutError(string message)
    {
        errorText.gameObject.SetActive(true);
        errorText.text = message;
        errorText.alpha = 1f;
        float duration = 3f; // fade over 3 seconds
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            errorText.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        errorText.alpha = 0f;
        errorText.gameObject.SetActive(false);
    }

    public void showConfirmation(string message)
    {
        StartCoroutine(FadeOutConfirmation(message));
    }

    private IEnumerator FadeOutConfirmation(string message)
    {
        confirmationText.gameObject.SetActive(true);
        confirmationText.text = message;
        confirmationText.alpha = 1f;
        float duration = 3f; // fade over 3 seconds
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            confirmationText.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        confirmationText.alpha = 0f;
        confirmationText.gameObject.SetActive(false);
    }
}

