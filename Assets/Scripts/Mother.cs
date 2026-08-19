using UnityEngine;

// Mother is functionally identical to Alien - same movement, seeking, eating,
// targeting, and bug-spawn-on-death logic, all inherited directly.
// Kept as its own class (rather than just using Alien.cs on the Mother GameObject)
// so it can have its own distinct component name/identity in the Inspector,
// and so any Mother-specific behavior can be added here later without
// affecting Alien, Bug, or Burster.
public class Mother : Alien
{
}