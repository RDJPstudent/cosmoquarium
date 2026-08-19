using UnityEngine;

// Makes the fish that eats this permanently untargetable by predator seeking logic
// (Alien/Bug/Mother/Burster won't pick it as a hunt target, and will immediately
// abandon it if it becomes untargetable mid-chase), but it can still be hit
// directly by burst projectiles or other non-targeting-based damage sources.
public class UntargetableUpgrade : Upgrade
{
    protected override void ApplyUpgradeEffect(Fish eater)
    {
        base.ApplyUpgradeEffect(eater);

        eater.isUntargetable = true;
        Debug.Log($"[UntargetableUpgrade] {eater.gameObject.name} is now untargetable by predators.");
    }
}