using UnityEngine;

// Spawns an exact duplicate of the fish that eats this, right next to it.
// Does NOT lock the eating fish into "has an upgrade" state, and does NOT
// apply the usual color tint - this upgrade has no lasting modifier on the
// fish itself, it just clones it.
public class DoubleFishUpgrade : Upgrade
{
    public Vector2 spawnOffsetRange = new Vector2(0.5f, 0.5f);

    protected override bool RequiresUpgradeSlot => false;

    protected override void ApplyUpgradeEffect(Fish eater)
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnOffsetRange.x, spawnOffsetRange.x),
            Random.Range(-spawnOffsetRange.y, spawnOffsetRange.y),
            0f
        );

        GameObject clone = Instantiate(eater.gameObject, eater.transform.position + randomOffset, eater.transform.rotation);

        Fish cloneFish = clone.GetComponent<Fish>();
        if (cloneFish != null)
        {
            cloneFish.hasUpgrade = false;
        }

        Debug.Log($"[DoubleFishUpgrade] Cloned {eater.gameObject.name} into a new fish.");
    }
}