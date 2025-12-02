using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class DiceManager : MonoBehaviour
{
    [Tooltip("Assign all DiceRoll components to manage multiple dice.")]
    [SerializeField] private DiceRoll[] dice;

    [Tooltip("Optional UI text to display aggregate results (sum and individual results).")]
    [SerializeField] private TextMeshProUGUI resultText;

    // track results as dice stop
    private Dictionary<DiceRoll, int> results = new Dictionary<DiceRoll, int>();
        // prevent duplicate/concurrent roll requests from consuming multiple "rolls"
        private bool isRolling = false;

    [Header("Last Roll / Payout")]
    [Tooltip("Last recorded faces in the same order as the `dice` array.")]
    [SerializeField] private int[] lastFaces;
    [SerializeField] private int lastSum = 0;
    [SerializeField] private int lastPayout = 0;

    [Header("Payout Rules")]
    [Tooltip("If true, payout is calculated per-die using `perFacePayout` mapping. If false, payout = lastSum * sumMultiplier.")]
    [SerializeField] private bool usePerDiePayout = true;

    [Tooltip("Payout for each face value. Index 0 is unused; index 1..6 map to faces 1..6.")]
    [SerializeField] private int[] perFacePayout = new int[] { 0, 1, 2, 3, 4, 5, 6 };

    [Tooltip("Multiplier applied to the sum when `usePerDiePayout` is false.")]
    [SerializeField] private int sumMultiplier = 1;

    [Header("Auto Apply")]
    [Tooltip("If true, the calculated payout will be added to `playerBalance` automatically.")]
    [SerializeField] private bool autoAddToBalance = false;
    [SerializeField] private int playerBalance = 0;

    // Event raised when a payout is calculated (payout, sum)
    public event Action<int, int> OnPayoutCalculated;
    // Event raised when rolls left changes
    public event Action<int> OnRollsLeftChanged;

    [Header("Roll Limits")]
    [Tooltip("How many rolls the player has remaining. Decremented each time RollAll() is called.")]
    [SerializeField] private int rollsLeft = 3;

    [Header("Reset On Payout")]
    [Tooltip("If true, applying a payout will reset the number of rolls to `rollsPerReset`.")]
    [SerializeField] private bool resetRollsOnPayout = false;

    [Tooltip("How many rolls to give when resetting on payout.")]
    [SerializeField] public int rollsPerReset = 3;
    
        [Header("Unlocking Dice")]
        [Tooltip("Base cost for purchasing the first extra die.")]
        [SerializeField] private int baseDiceCost = 10;
    
        [Tooltip("Cost scale factor per already-unlocked die (e.g., 1.5 increases cost each unlock).")]
        [SerializeField] private float costScale = 1.5f;

        [Header("Spawn/Unlock Positions")]
        [Tooltip("Optional explicit spawn points for unlocking dice. If provided, the n'th spawn point will be used for the n'th die in the dice array. If empty, a default offset is used relative to this manager.")]
        [SerializeField] private Transform[] spawnPoints;

        [Tooltip("Fallback horizontal spacing used when no spawnPoints are provided.")]
        [SerializeField] private float spawnSpacing = 1.5f;

        [Tooltip("Default spawn position for dice if no spawnPoints are configured and manager is at origin.")]
        [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(0, 1, 0);

    [Header("Upgrades")]
    [Tooltip("Material to apply when purchasing an upgrade for a die (optional).")]
    [SerializeField] private Material upgradeMaterial;

    [Tooltip("Base cost to upgrade a single die.")]
    [SerializeField] private int baseUpgradeCost = 20;

    [Tooltip("Scaling factor for upgrade costs per already-upgraded die.")]
    [SerializeField] private float upgradeCostScale = 1.5f;

    [Tooltip("Default multiplier applied to an upgraded die's payout.")]
    [SerializeField] private float defaultUpgradeMultiplier = 2f;

    // track last-upgraded flags per index for payout calculation
    private bool[] lastUpgraded;

    private void OnEnable()
    {
        SubscribeAll();
        InitializeActiveDicePositions();
    }

    private void OnDisable()
    {
        UnsubscribeAll();
    }

    // Initialize positions of all active dice so they don't start at origin
    private void InitializeActiveDicePositions()
    {
        if (dice == null) return;
        for (int i = 0; i < dice.Length; i++)
        {
            var d = dice[i];
            if (d == null || !d.gameObject.activeInHierarchy) continue;

            Vector3 spawnPos = GetSpawnPositionForDie(i);
            Quaternion spawnRot = GetSpawnRotationForDie(i);
            try { d.SetStartTransform(spawnPos, spawnRot); } catch { }
            try { d.ResetToStart(); } catch { }
        }
    }

    private void SubscribeAll()
    {
        if (dice == null) return;
        foreach (var d in dice)
        {
            if (d == null) continue;
            if (!d.gameObject.activeInHierarchy) continue;
            d.OnDiceStopped += HandleDiceStopped;
        }
    }

    private void UnsubscribeAll()
    {
        if (dice == null) return;
        foreach (var d in dice)
        {
            if (d == null) continue;
            if (!d.gameObject.activeInHierarchy) continue;
            d.OnDiceStopped -= HandleDiceStopped;
        }
    }

    private void HandleDiceStopped(int face, DiceRoll source)
    {
        results[source] = face;

        // only consider active dice (some dice may be disabled/unlocked later)
        int activeCount = GetActiveDiceCount();
        if (activeCount == 0)
        {
            // nothing active — clear and bail
            results.Clear();
            isRolling = false;
            return;
        }

        if (results.Count == activeCount)
        {
            // all active dice stopped — build ordered results
            if (dice == null || dice.Length == 0)
            {
                results.Clear();
                return;
            }

            // ensure lastFaces and lastUpgraded arrays are initialized
            if (lastFaces == null || lastFaces.Length != dice.Length)
                lastFaces = new int[dice.Length];
            if (lastUpgraded == null || lastUpgraded.Length != dice.Length)
                lastUpgraded = new bool[dice.Length];

            int sum = 0;
            List<string> parts = new List<string>();
            // build results only for active dice in order; also keep lastFaces and upgraded flags per-index
            for (int i = 0; i < dice.Length; i++)
            {
                var d = dice[i];
                if (d == null || !d.gameObject.activeInHierarchy)
                {
                    lastFaces[i] = 0;
                    lastUpgraded[i] = false;
                    continue;
                }

                int v = results.ContainsKey(d) ? results[d] : 0;
                lastFaces[i] = v;
                bool upgraded = d.IsUpgraded;
                lastUpgraded[i] = upgraded;
                float mult = upgraded ? d.UpgradeMultiplier : 1f;
                sum += Mathf.RoundToInt(v * mult);
                // include upgrade mark in parts for clarity
                parts.Add(upgraded ? (v + "x" + mult) : v.ToString());
            }

            lastSum = sum;

            // calculate payout
            int payout = 0;
            if (usePerDiePayout)
            {
                // sum per-face mapped payout (guard array length). If a die was upgraded, apply its multiplier.
                for (int i = 0; i < lastFaces.Length; i++)
                {
                    int faceVal = lastFaces[i];
                    if (perFacePayout == null || perFacePayout.Length <= faceVal || faceVal <= 0) continue;

                    float mult = 1f;
                    if (lastUpgraded != null && lastUpgraded.Length > i && lastUpgraded[i])
                    {
                        var d = (dice != null && dice.Length > i) ? dice[i] : null;
                        mult = (d != null) ? d.UpgradeMultiplier : defaultUpgradeMultiplier;
                    }

                    payout += Mathf.RoundToInt(perFacePayout[faceVal] * mult);
                }
            }
            else
            {
                payout = lastSum * sumMultiplier;
            }

            lastPayout = payout;

            // optionally add to balance
            if (autoAddToBalance)
            {
                playerBalance += payout;
            }

            string outText = $"Sum: {sum}  (" + string.Join(", ", parts) + ")  Payout: " + payout;
            Debug.Log("All dice stopped: " + outText);
            if (resultText != null) resultText.text = outText;

            // notify listeners
            OnPayoutCalculated?.Invoke(payout, sum);

            // clear results to allow next roll
            results.Clear();
            // allow future rolls
            isRolling = false;
        }
    }

    // Call to roll all dice at once
    public void RollAll()
    {
        if (dice == null) return;
        
            // prevent re-entrancy / multiple calls while a roll is already in progress
            if (isRolling)
            {
                Debug.Log("RollAll ignored: already rolling");
                return;
            }
        
            // check rolls left
            if (rollsLeft <= 0)
            {
                Debug.Log("No rolls left");
                return;
            }
        
            // mark rolling and consume one roll
            isRolling = true;
            rollsLeft = Mathf.Max(0, rollsLeft - 1);
            OnRollsLeftChanged?.Invoke(rollsLeft);
        
            results.Clear();
            foreach (var d in dice)
            {
                if (d == null) continue;
                if (!d.gameObject.activeInHierarchy) continue; // only roll active dice
                d.RollDice();
            }
    }

        // Soft-reset all dice to their starting positions and clear in-progress state
        public void SoftReset()
        {
            if (dice == null) return;

            // stop any in-progress roll state
            isRolling = false;
            results.Clear();

            foreach (var d in dice)
            {
                if (d == null) continue;
                d.ResetToStart();
            }

            // notify UI of unchanged rollsLeft (so the shop refreshes buttons/text)
            OnRollsLeftChanged?.Invoke(rollsLeft);
            AddRolls(rollsPerReset-rollsLeft);
        }

    // Rolls-left helpers
    public int GetRollsLeft() => rollsLeft;
    public bool CanRoll() => rollsLeft > 0;
    public void AddRolls(int amount)
    {
        if (amount <= 0) return;
        rollsLeft += amount;
        OnRollsLeftChanged?.Invoke(rollsLeft);
    }

    // Public accessors for other systems
    public int[] GetLastFaces()
    {
        if (lastFaces == null) return new int[0];
        return (int[])lastFaces.Clone();
    }

    public int GetLastSum() => lastSum;
    public int GetLastPayout() => lastPayout;
    public int GetPlayerBalance() => playerBalance;

    // Manually apply the last payout to the player balance (if not auto-applied)
    public void ApplyLastPayout()
    {
        Debug.Log("DiceManager: ApplyLastPayout called");
        playerBalance += lastPayout;

        // optionally reset rolls when a payout is applied
        if (resetRollsOnPayout)
        {
            rollsLeft = Mathf.Max(0, rollsPerReset);
            OnRollsLeftChanged?.Invoke(rollsLeft);
        }
    }

    // number of dice currently active in the scene
    public int GetActiveDiceCount()
    {
        if (dice == null) return 0;
        int c = 0;
        foreach (var d in dice)
            if (d != null && d.gameObject.activeInHierarchy) c++;
        return c;
    }

    // cost to unlock the next die
    public int GetNextDiceCost()
    {
        int active = GetActiveDiceCount();
        float cost = baseDiceCost * Mathf.Pow(costScale, active);
        return Mathf.CeilToInt(cost);
    }

    // try to purchase and unlock the next inactive die; returns true on success
    public bool TryPurchaseUnlockDice()
    {
        if (isRolling) return false;
        int cost = GetNextDiceCost();
        if (playerBalance < cost) return false;

        // find first inactive die
        if (dice == null) return false;
        for (int i = 0; i < dice.Length; i++)
        {
            var d = dice[i];
            if (d == null) continue;
            if (!d.gameObject.activeInHierarchy)
            {
                // deduct cost and unlock
                playerBalance -= cost;
                
                Vector3 spawnPos = GetSpawnPositionForDie(i);
                Quaternion spawnRot = GetSpawnRotationForDie(i);

                // set die's start transform before activating so ResetToStart will restore to this
                try { d.SetStartTransform(spawnPos, spawnRot); } catch { }
                d.gameObject.SetActive(true);
                try { d.ResetToStart(); } catch { }
                // ensure DiceManager listens to this die's stop event
                try { d.OnDiceStopped += HandleDiceStopped; } catch { }
                // notify UI/buyers by firing rolls-left changed (balance changed will be shown elsewhere)
                OnRollsLeftChanged?.Invoke(rollsLeft);
                return true;
            }
        }

        return false;
    }

    // --- Upgrade helpers ---
    // Number of dice currently upgraded
    public int GetUpgradedCount()
    {
        if (dice == null) return 0;
        int c = 0;
        foreach (var d in dice)
            if (d != null && d.IsUpgraded) c++;
        return c;
    }

    // cost to upgrade a specific die (scales with already-upgraded count)
    public int GetUpgradeCost(int index)
    {
        int upgraded = GetUpgradedCount();
        float cost = baseUpgradeCost * Mathf.Pow(upgradeCostScale, upgraded);
        return Mathf.CeilToInt(cost);
    }

    // Try to purchase an upgrade for the die at `index`. Returns true when successful.
    public bool TryPurchaseUpgradeDie(int index)
    {
        if (dice == null) return false;
        if (index < 0 || index >= dice.Length) return false;
        var d = dice[index];
        if (d == null) return false;
        if (d.IsUpgraded) return false;

        int cost = GetUpgradeCost(index);
        if (playerBalance < cost) return false;

        // deduct and apply upgrade
        playerBalance -= cost;
        try { d.Upgrade(upgradeMaterial, defaultUpgradeMultiplier); } catch { }
        return true;
    }

    // Determine spawn position for a die at the given index
    private Vector3 GetSpawnPositionForDie(int index)
    {
        // if spawnPoints provided and index in range, use it
        if (spawnPoints != null && spawnPoints.Length > index && spawnPoints[index] != null)
        {
            return spawnPoints[index].position;
        }
        else if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // if there are spawn points but not one for this index, use one by modulo
            var sp = spawnPoints[index % spawnPoints.Length];
            if (sp != null) return sp.position;
        }

        // fallback: position relative to manager, spaced by index
        return transform.position + defaultSpawnPosition + (Vector3.right * index * spawnSpacing);
    }

    // Determine spawn rotation for a die at the given index
    private Quaternion GetSpawnRotationForDie(int index)
    {
        // if spawnPoints provided and index in range, use it
        if (spawnPoints != null && spawnPoints.Length > index && spawnPoints[index] != null)
        {
            return spawnPoints[index].rotation;
        }
        else if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // if there are spawn points but not one for this index, use one by modulo
            var sp = spawnPoints[index % spawnPoints.Length];
            if (sp != null) return sp.rotation;
        }

        // fallback: identity rotation
        return Quaternion.identity;
    }

    // Button-friendly wrappers
    public void OnButtonRollAll()
    {
        Debug.Log("DiceManager: OnButtonRollAll called");
        RollAll();
    }

    public void OnButtonSoftReset()
    {
        Debug.Log("DiceManager: OnButtonSoftReset called");
        SoftReset();
    }

    public void adjustPlayerBalance(int amount)
    {
        playerBalance += amount;
    }

    // Clear recorded last values and results
    public void ClearLast()
    {
        if (lastFaces != null) Array.Clear(lastFaces, 0, lastFaces.Length);
        lastSum = 0;
        lastPayout = 0;
        results.Clear();
    }
}
