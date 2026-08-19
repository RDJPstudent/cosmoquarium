using UnityEngine;

// Static logging methods - callable from anywhere without a reference.
// Actual on/off state lives in DevToolsSettings (a MonoBehaviour) so it can show as
// checkboxes in the Inspector; this class just reads those static toggle values.
public static class DevTools
{
    public static bool logCollisions = true;
    public static bool logDamage = true;
    public static bool logDeath = true;
    public static bool logEatAttempts = true;
    public static bool logTargeting = true;

    public static void LogCollision(GameObject self, GameObject other)
    {
        if (!logCollisions) return;
        Debug.Log($"[Collision] {self.name} collided with {other.name}");
    }

    public static void LogDamage(GameObject attacker, GameObject target, int amount, int remainingHP)
    {
        if (!logDamage) return;
        Debug.Log($"[Damage] {attacker.name} dealt {amount} damage to {target.name}. Remaining HP: {remainingHP}");
    }

    public static void LogDeath(GameObject who)
    {
        if (!logDeath) return;
        Debug.Log($"[Death] {who.name} has died.");
    }

    public static void LogEatAttempt(GameObject attacker, GameObject target, float roll, float chance, bool success)
    {
        if (!logEatAttempts) return;
        string result = success ? "SUCCESS" : "failed";
        Debug.Log($"[EatAttempt] {attacker.name} -> {target.name}: rolled {roll:F1} vs {chance}% chance - {result}");
    }

    public static void LogTargetAcquired(GameObject hunter, GameObject target)
    {
        if (!logTargeting) return;
        Debug.Log($"[Targeting] {hunter.name} acquired target: {target.name}");
    }

    public static void LogTargetLost(GameObject hunter, string previousTargetName)
    {
        if (!logTargeting) return;
        Debug.Log($"[Targeting] {hunter.name} lost target: {previousTargetName}");
    }

    // Test helper - adds an upgrade to inventory and logs it, for verifying
    // InventoryBar/InventorySlot behavior before the Shop scene exists.
    public static void TestAddUpgrade(string upgradeId)
    {
        if (string.IsNullOrEmpty(upgradeId))
        {
            Debug.LogWarning("[DevTools] TestAddUpgrade called with an empty upgradeId.");
            return;
        }

        GameManager.AddUpgrade(upgradeId);
        Debug.Log($"[DevTools] Test-added upgrade '{upgradeId}' via debug key.");
    }
}