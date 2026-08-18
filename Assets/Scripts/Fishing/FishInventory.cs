using System.Collections.Generic;
using UnityEngine;

/// Holds the list of fish the player has caught. Persists across scene loads
/// (Fishing -> Aquarium) using the singleton + DontDestroyOnLoad pattern.
public class FishInventory : MonoBehaviour
{
    public static FishInventory Instance { get; private set; }

    // Each entry represents one owned fish instance (not just a species reference),
    // since two fish of the same species can be leveled independently later.
    private readonly List<OwnedFish> ownedFish = new List<OwnedFish>();

    private void Awake()
    {
        // Singleton setup: if one already exists (e.g. we looped back through
        // the Fishing scene again), destroy this duplicate instead of replacing it.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddFish(FishData species)
    {
        ownedFish.Add(new OwnedFish(species));
        Debug.Log($"Caught a {species.speciesName}! Total owned: {ownedFish.Count}");
    }

    public List<OwnedFish> GetAllFish()
    {
        return ownedFish;
    }
}

/// Represents one specific fish the player owns, separate from its species data.
/// This is what lets two Goldfish be leveled independently in the Aquarium.
[System.Serializable]
public class OwnedFish
{
    public FishData species;
    public int level = 1;

    public OwnedFish(FishData species)
    {
        this.species = species;
    }
}
