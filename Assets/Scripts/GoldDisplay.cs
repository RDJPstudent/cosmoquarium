using UnityEngine;
using TMPro; // TextMeshPro - Unity's standard modern UI text component

// Attach to a TextMeshPro UI text object. Displays the player's current gold count,
// pulling live from GameManager's static totalGold value.
public class GoldDisplay : MonoBehaviour
{
    public string prefix = "Gold: $"; // text shown before the number, e.g. "Gold: 5"

    protected TextMeshProUGUI goldText;

    void Awake()
    {
        goldText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (goldText != null)
        {
            goldText.text = prefix + GameManager.totalGold;
        }
    }
}