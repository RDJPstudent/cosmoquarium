using System;
using UnityEngine;

/// Holds the player's Sand Dollar balance. Persists across scene loads
/// (Aquarium -> Fishing) using the singleton + DontDestroyOnLoad pattern,
/// same approach as FishInventory.
public class SandDollarWallet : MonoBehaviour
{
    public static SandDollarWallet Instance { get; private set; }

    [Header("Debug")]
    [Tooltip("Starting balance, mainly useful for testing before the Aquarium reward flow exists.")]
    [SerializeField] private int currentBalance = 0;

    public int CurrentBalance => currentBalance;

    // Fired whenever the balance changes, including once at Awake() with the starting value.
    public event Action<int> OnBalanceChanged;

    private void Awake()
    {
        // Singleton setup: if one already exists (e.g. we looped back through
        // a scene again), destroy this duplicate instead of replacing it.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        OnBalanceChanged?.Invoke(currentBalance);
    }

    /// Adds Sand Dollars to the balance. Called by the Aquarium scene on stage clear.
    public void AddSandDollars(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("[SandDollarWallet] Tried to add a negative amount. Ignored.");
            return;
        }

        currentBalance += amount;
        Debug.Log($"[SandDollarWallet] Added {amount} Sand Dollars. New balance: {currentBalance}");
        OnBalanceChanged?.Invoke(currentBalance);
    }

    /// Attempts to spend Sand Dollars (e.g. buying a fishing upgrade).
    /// Returns true if the purchase succeeded, false if the balance was insufficient.
    public bool TrySpendSandDollars(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("[SandDollarWallet] Tried to spend a negative amount. Ignored.");
            return false;
        }

        if (currentBalance < amount)
        {
            Debug.Log($"[SandDollarWallet] Not enough Sand Dollars. Have {currentBalance}, need {amount}.");
            return false;
        }

        currentBalance -= amount;
        Debug.Log($"[SandDollarWallet] Spent {amount} Sand Dollars. New balance: {currentBalance}");
        OnBalanceChanged?.Invoke(currentBalance);
        return true;
    }
}
