using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// Drives the Fishing scene's core loop: Shop -> (Cast -> Delay -> Minigame -> Resolve) x5 -> Aquarium.
/// This is the central state machine other Fishing scripts (UI, minigame, upgrades) will hook into.
public class FishingManager : MonoBehaviour
{
    private enum FishingState
    {
        Shop,
        WaitingForCast,
        Delay,
        Minigame,
        Resolve,
        Complete
    }

    [Header("Fish Pool")]
    [Tooltip("All fish species that can be caught. Drag in your FishData assets here.")]
    public List<FishData> fishPool;

    [Header("Timing")]
    [Tooltip("Seconds between casting and the catch minigame starting.")]
    public float delayDuration = 1.5f;

    [Header("Scene Transition")]
    [Tooltip("Name of the scene to load once all casts are used. Must be added to Build Settings.")]
    public string aquariumSceneName = "Aquarium";

    [Header("Minigame")]
    [Tooltip("Drag the GameObject holding FishingMinigameController here.")]
    public FishingMinigameController minigameController;

    private FishingState currentState;
    private int castsRemaining = 5;
    private const int MaxCasts = 5;
    private bool lastMinigameSuccess;

    private void Start()
    {
        castsRemaining = MaxCasts;
        ChangeState(FishingState.Shop);
    }

    private void Update()
    {
        // Only listen for cast input while actually waiting for one.
        if (currentState == FishingState.WaitingForCast
            && Mouse.current != null
            && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ChangeState(FishingState.Delay);
        }
    }

    private void ChangeState(FishingState newState)
    {
        currentState = newState;
        Debug.Log($"[FishingManager] State changed to: {newState}");

        switch (newState)
        {
            case FishingState.Shop:
                HandleShop();
                break;
            case FishingState.WaitingForCast:
                // Nothing to do here yet, just waiting for Update() to catch the click.
                break;
            case FishingState.Delay:
                StartCoroutine(HandleDelay());
                break;
            case FishingState.Minigame:
                HandleMinigame();
                break;
            case FishingState.Resolve:
                HandleResolve();
                break;
            case FishingState.Complete:
                HandleComplete();
                break;
        }
    }

    private void HandleShop()
    {
        // TODO: Show shop UI here once FishingUpgradeManager/UI exist.
        // For now, immediately proceed to the first cast.
        ChangeState(FishingState.WaitingForCast);
    }

    private IEnumerator HandleDelay()
    {
        yield return new WaitForSeconds(delayDuration);
        ChangeState(FishingState.Minigame);
    }

    private void HandleMinigame()
    {
        if (minigameController == null)
        {
            Debug.LogWarning("[FishingManager] No FishingMinigameController assigned. Skipping straight to Resolve.");
            lastMinigameSuccess = false;
            ChangeState(FishingState.Resolve);
            return;
        }

        minigameController.StartMinigame(success =>
        {
            lastMinigameSuccess = success;
            ChangeState(FishingState.Resolve);
        });
    }

    private void HandleResolve()
    {
        FishData caughtFish = GetRandomWeightedFish(lastMinigameSuccess);

        if (caughtFish != null && FishInventory.Instance != null)
        {
            FishInventory.Instance.AddFish(caughtFish);
        }

        castsRemaining--;
        Debug.Log($"[FishingManager] Casts remaining: {castsRemaining}");

        if (castsRemaining > 0)
        {
            ChangeState(FishingState.WaitingForCast);
        }
        else
        {
            ChangeState(FishingState.Complete);
        }
    }

    private void HandleComplete()
    {
        Debug.Log("[FishingManager] All casts used. Loading Aquarium scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(aquariumSceneName);
    }


    /// Picks a random fish from the pool, weighted by each fish's catchWeight.
    /// Higher catchWeight = more likely to be selected under normal odds.
    /// If biasTowardRare is true (minigame succeeded), weighting is inverted so
    /// rarer fish (lower catchWeight) become more likely - the minigame's actual payoff.
    private FishData GetRandomWeightedFish(bool biasTowardRare)
    {
        if (fishPool == null || fishPool.Count == 0)
        {
            Debug.LogWarning("[FishingManager] Fish pool is empty. Assign FishData assets in the Inspector.");
            return null;
        }

        float totalWeight = 0f;
        foreach (FishData fish in fishPool)
        {
            float effectiveWeight = biasTowardRare
                ? 1f / Mathf.Max(fish.catchWeight, 0.01f)
                : fish.catchWeight;
            totalWeight += effectiveWeight;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (FishData fish in fishPool)
        {
            float effectiveWeight = biasTowardRare
                ? 1f / Mathf.Max(fish.catchWeight, 0.01f)
                : fish.catchWeight;
            cumulative += effectiveWeight;
            if (roll <= cumulative)
            {
                return fish;
            }
        }

        // Fallback in case of floating point rounding at the very edge.
        return fishPool[fishPool.Count - 1];
    }
}
