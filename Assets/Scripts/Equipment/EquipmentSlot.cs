using System;
using Equipment;
using Interfaces;
using Scriptables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour, IEquipmentSlot, IPointerEnterHandler, IPointerExitHandler {
    private ItemSO assignedItem;
    
    // UI
    private Image icon;

    private bool focused;

    public void Init() {
        icon = GetComponent<EquipmentSlotReferenceHelper>().icon;
        icon.enabled = false;
    }

    private void Update() {
        if (!focused) return;
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            if (Equipment.Equipment.Instance.CheckPickedItem()) {
                var wasItemAssigned = assignedItem;
                
                Assign(Equipment.Equipment.Instance.GetPickedItem());
                Equipment.Equipment.Instance.RemovePickedItem();
                
                if (wasItemAssigned != null) {
                    Equipment.Equipment.Instance.PickItem(wasItemAssigned);
                }
            } else {
                if (assignedItem == null) return;
                Equipment.Equipment.Instance.PickItem(assignedItem);
            }
        }
    }

    public ItemSO GetInfo() {
        return assignedItem;
    }

    public void Pick() {
        icon.enabled = false;
    }

    public void Assign(ItemSO item) {
        assignedItem = item;

        icon.sprite = assignedItem.itemIcon;
        icon.enabled = true;
    }

    public void Throw() {
        // Logic
        
        Remove();
    }

    public void Remove() {
        assignedItem = null;
        icon.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (eventData.pointerEnter) focused = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (eventData.fullyExited) focused = false;
    }
}
