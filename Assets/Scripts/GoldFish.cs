using UnityEngine;

// Inherits from Fish - behaves exactly like a normal fish (pulse movement, tilt, wall bounce, HP),
// but periodically drops a Gold prefab on a timer.
public class GoldFish : Fish
{
    [Header("Gold Drop")]
    public GameObject goldPrefab;      // assign your Gold prefab here
    public float dropInterval = 5f;    // seconds between each gold drop

    protected float dropTimer = 0f;

    protected override void Update()
    {
        base.Update(); // keeps all normal Fish pulse/movement/rotation behavior

        dropTimer += Time.deltaTime;
        if (dropTimer >= dropInterval)
        {
            dropTimer = 0f;
            DropGold();
        }
    }

    // Spawns a Gold object at this fish's current position
    protected virtual void DropGold()
    {
        if (goldPrefab == null)
            return; // no prefab assigned - skip silently rather than erroring

        Instantiate(goldPrefab, transform.position, Quaternion.identity);
    }
}