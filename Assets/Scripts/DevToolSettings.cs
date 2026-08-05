using UnityEngine;

// Attach this to any single GameObject in your scene (e.g. an empty called "DevTools").
// Exposes checkboxes in the Inspector that control which DevTools log categories fire,
// without needing to edit code to toggle them on/off during testing.
public class DevToolsSettings : MonoBehaviour
{
    [Header("Log Category Toggles")]
    public bool logCollisions = true;
    public bool logDamage = true;
    public bool logDeath = true;
    public bool logEatAttempts = true;
    public bool logTargeting = true;

    // Pushes the Inspector checkbox values into the static DevTools class every frame,
    // so changes made in the Inspector during Play mode take effect immediately
    void Update()
    {
        DevTools.logCollisions = logCollisions;
        DevTools.logDamage = logDamage;
        DevTools.logDeath = logDeath;
        DevTools.logEatAttempts = logEatAttempts;
        DevTools.logTargeting = logTargeting;
    }
}