using UnityEngine;
using UnityEngine.InputSystem;

public class DevToolsSettings : MonoBehaviour
{
    [Header("Log Category Toggles")]
    public bool logCollisions = true;
    public bool logDamage = true;
    public bool logDeath = true;
    public bool logEatAttempts = true;
    public bool logTargeting = true;

    [Header("Test - Add Upgrade")]
    public string testUpgradeId = "your_upgrade_id";

    void Update()
    {
        DevTools.logCollisions = logCollisions;
        DevTools.logDamage = logDamage;
        DevTools.logDeath = logDeath;
        DevTools.logEatAttempts = logEatAttempts;
        DevTools.logTargeting = logTargeting;

        // New Input System equivalent of Input.GetKeyDown(KeyCode.U)
        if (Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame)
        {
            DevTools.TestAddUpgrade(testUpgradeId);
        }
    }
}