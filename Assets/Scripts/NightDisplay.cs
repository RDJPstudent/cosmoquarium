using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Attach to a TextMeshPro UI text object. Displays "Night X" while in the Aquarium
// scene, or "Day X" in any other scene - pulling the number live from GameManager.currentNight.
public class NightDisplay : MonoBehaviour
{
    public string nightPrefix = "Night ";
    public string dayPrefix = "Day ";

    protected TextMeshProUGUI displayText;

    void Awake()
    {
        displayText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (displayText == null) return;

        bool inAquarium = SceneManager.GetActiveScene().name == "Aquarium";
        string prefix = inAquarium ? nightPrefix : dayPrefix;

        displayText.text = prefix + GameManager.currentNight;
    }
}