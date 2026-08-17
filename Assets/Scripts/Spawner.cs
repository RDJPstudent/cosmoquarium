using UnityEngine;

// Marks a point in the scene predators can spawn from. Purely a position marker -
// NightManager finds all active Spawners and picks one at random each time it
// spawns a queued predator, so it stays the single source of truth for the budget.
public class Spawner : MonoBehaviour
{
    // Intentionally minimal - NightManager does all the actual spawning logic.
    // This could later hold per-spawner settings (e.g. only certain predator types
    // allowed from this point) if you want that level of control down the line.
}