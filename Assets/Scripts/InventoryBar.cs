using UnityEngine;
using System.Collections.Generic;

// Attach to a UI Panel under your Canvas. Builds one InventorySlot per owned
// upgrade type/count, refreshing whenever the inventory changes.
public class InventoryBar : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotParent;

    protected Dictionary<string, GameObject> activeSlots = new Dictionary<string, GameObject>();

    void Update()
    {
        RefreshSlots();
    }

    protected virtual void RefreshSlots()
    {
        List<string> toRemove = new List<string>();
        foreach (var kvp in activeSlots)
        {
            if (!GameManager.ownedUpgrades.ContainsKey(kvp.Key))
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (string id in toRemove)
        {
            activeSlots.Remove(id);
        }

        foreach (var kvp in GameManager.ownedUpgrades)
        {
            string upgradeId = kvp.Key;
            int count = kvp.Value;

            if (!activeSlots.ContainsKey(upgradeId))
            {
                GameObject slotObj = Instantiate(slotPrefab, slotParent);
                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                if (slot != null)
                {
                    slot.Setup(upgradeId);
                }
                activeSlots[upgradeId] = slotObj;
            }

            InventorySlot existingSlot = activeSlots[upgradeId].GetComponent<InventorySlot>();
            if (existingSlot != null)
            {
                existingSlot.UpdateCount(count);
            }
        }
    }
}