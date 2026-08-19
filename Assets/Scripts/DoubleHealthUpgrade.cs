using UnityEngine;

// Permanently doubles the max HP (and current HP) of the fish that eats this.
public class DoubleHealthUpgrade : Upgrade
{
    protected override void ApplyUpgradeEffect(Fish eater)
    {
        base.ApplyUpgradeEffect(eater); // still applies the fishColorOnConsume tint

        eater.DoubleMaxHP();
        Debug.Log($"[DoubleHealthUpgrade] {eater.gameObject.name}'s HP doubled.");
    }
}