using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Attach to a UI slot prefab (needs an Image for the icon, a TextMeshProUGUI for the
// count, and a Canvas Group). Handles the drag-and-drop: dragging off the UI and
// releasing over the game world spawns the upgrade at that exact release position.
[RequireComponent(typeof(CanvasGroup))]
public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image iconImage;
    public TextMeshProUGUI countText;

    protected string upgradeId;
    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;
    protected Canvas rootCanvas;
    protected Vector2 dragStartPosition;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public virtual void Setup(string id)
    {
        upgradeId = id;

        if (UpgradeDatabase.Instance != null)
        {
            var entry = UpgradeDatabase.Instance.GetEntry(id);
            if (entry != null && iconImage != null)
            {
                iconImage.sprite = entry.icon;
            }
        }
    }

    public virtual void UpdateCount(int count)
    {
        if (countText != null)
        {
            countText.text = count.ToString();
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        bool droppedOverUI = EventSystem.current.IsPointerOverGameObject(eventData.pointerId);

        if (!droppedOverUI)
        {
            SpawnUpgradeAtScreenPosition(eventData.position);
        }

        rectTransform.anchoredPosition = dragStartPosition;
    }

    // Spawns the upgrade exactly where the mouse was released, converted to world space
    protected virtual void SpawnUpgradeAtScreenPosition(Vector2 screenPosition)
    {
        if (UpgradeDatabase.Instance == null) return;

        var entry = UpgradeDatabase.Instance.GetEntry(upgradeId);
        if (entry == null || entry.prefab == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -cam.transform.position.z));
        Vector3 spawnPosition = new Vector3(worldPoint.x, worldPoint.y, 0f);

        GameObject spawned = Object.Instantiate(entry.prefab, spawnPosition, Quaternion.identity);

        GameManager.RemoveUpgrade(upgradeId);
    }
}