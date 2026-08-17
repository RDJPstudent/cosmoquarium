using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Lives once in the Aquarium scene. On Start, calculates this night's length and spawn
// budget based on GameManager.currentNight, builds a queue of predators whose combined
// spawnValue fills that budget (this queue is the single source of truth for the budget -
// nothing else can add to it), then spawns them one at a time, evenly spaced across
// the night's duration, each from a randomly chosen Spawner point in the scene.
public class NightManager : MonoBehaviour
{
    [Header("Predator Prefabs")]
    public List<GameObject> predatorPrefabs; // assign Alien, Bug, Mother, Burster prefabs here

    [Header("Night Length")]
    public float baseNightLength = 180f;
    public float nightLengthIncrease = 30f;
    public float maxNightLength = 300f;

    protected float nightLength;
    protected float spawnBudget;
    protected List<GameObject> spawnQueue = new List<GameObject>();
    protected List<Spawner> activeSpawners = new List<Spawner>();

    protected virtual void Start()
    {
        int night = GameManager.currentNight;

        nightLength = CalculateNightLength(night);
        spawnBudget = CalculateSpawnTarget(night);

        FindSpawners();
        BuildSpawnQueue();

        Debug.Log($"[NightManager] Night {night} - Length: {nightLength}s, Spawn Target: {spawnBudget}, Queued: {spawnQueue.Count} predators, Spawners found: {activeSpawners.Count}");

        StartCoroutine(SpawnOverTime());
    }

    protected virtual float CalculateNightLength(int night)
    {
        float length = baseNightLength + (night - 1) * nightLengthIncrease;
        return Mathf.Min(length, maxNightLength);
    }

    protected virtual float CalculateSpawnTarget(int night)
    {
        float[] earlyNightTargets = { 500f, 600f, 750f, 950f, 1350f };

        if (night <= earlyNightTargets.Length)
        {
            return earlyNightTargets[night - 1];
        }

        float value = earlyNightTargets[earlyNightTargets.Length - 1];
        int extraNights = night - earlyNightTargets.Length;

        for (int i = 0; i < extraNights; i++)
        {
            value *= 1.3f;
        }

        return value;
    }

    // Finds every active Spawner point currently in the scene
    protected virtual void FindSpawners()
    {
        activeSpawners.Clear();
        Spawner[] found = FindObjectsByType<Spawner>(FindObjectsSortMode.None);
        activeSpawners.AddRange(found);

        if (activeSpawners.Count == 0)
        {
            Debug.LogWarning("[NightManager] No Spawner objects found in the scene - nothing will be able to spawn this night.");
        }
    }

    // Builds the full night's predator queue ONCE, up front - this is what guarantees
    // the total spawn budget is never exceeded, since nothing else can add to this list
    // or independently decide to spawn something outside of it.
    protected virtual void BuildSpawnQueue()
    {
        spawnQueue.Clear();

        if (predatorPrefabs == null || predatorPrefabs.Count == 0)
        {
            Debug.LogWarning("[NightManager] No predator prefabs assigned - nothing will spawn this night.");
            return;
        }

        float remainingBudget = spawnBudget;
        int safetyLimit = 1000;
        int iterations = 0;

        while (remainingBudget > 0f && iterations < safetyLimit)
        {
            GameObject chosenPrefab = predatorPrefabs[Random.Range(0, predatorPrefabs.Count)];
            Fish fishComponent = chosenPrefab.GetComponent<Fish>();

            float cost = fishComponent != null ? fishComponent.spawnValue : 10f;

            spawnQueue.Add(chosenPrefab);
            remainingBudget -= cost;
            iterations++;
        }
    }

    // Spawns each queued predator one at a time, evenly spaced across the night's duration,
    // each one instantiated at a randomly chosen Spawner's position
    protected virtual IEnumerator SpawnOverTime()
    {
        if (spawnQueue.Count == 0 || activeSpawners.Count == 0)
            yield break; // nothing to spawn, or nowhere to spawn it from

        float interval = nightLength / spawnQueue.Count;

        foreach (GameObject prefab in spawnQueue)
        {
            SpawnPredator(prefab);
            yield return new WaitForSeconds(interval);
        }
    }

    // Instantiates a predator at a randomly chosen Spawner's position
    protected virtual void SpawnPredator(GameObject prefab)
    {
        Spawner chosenSpawner = activeSpawners[Random.Range(0, activeSpawners.Count)];
        Instantiate(prefab, chosenSpawner.transform.position, Quaternion.identity);
    }
}