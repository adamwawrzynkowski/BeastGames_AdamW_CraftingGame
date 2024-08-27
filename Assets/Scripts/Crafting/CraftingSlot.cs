using Equipment;
using Scriptables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Crafting {
    public class CraftingSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        private ItemSO assignedItem;
        
        // UI
        private Image icon;
            
        private int slotID;
        public int GetID() => slotID;
        
        private bool focused;
        
        // Assign ID and icon
        public void Init(int ID) {
            slotID = ID;
            icon = GetComponent<EquipmentSlotReferenceHelper>().icon;
            icon.enabled = false;
        }
        
        // Get currently assigned item in slot
        public ItemSO GetInfo() {
            return assignedItem;
        }
        
        private void Update() {
            // Check if user is currently pointing at this slot
            if (!focused) return;
            
            // Check if user clicked left mouse button, if yes - run the logic
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                // Check if user is currently holding any item in Equipment
                if (Equipment.Equipment.Instance.CheckPickedItem()) {
                    // Check if currently holding item is the same as item in slot, if yes - return function
                    if (Equipment.Equipment.Instance.GetPickedItem() == assignedItem) {
                        return;
                    }
                        
                    var wasItemAssigned = assignedItem;
                    
                    // Assign holding item into this slot
                    Assign(Equipment.Equipment.Instance.GetPickedItem());
                    Equipment.Equipment.Instance.RemovePickedItem();
                        
                    // Check if this slot was taken by another item, if yes - pick it
                    // Items will be swapped
                    if (wasItemAssigned != null) {
                        Equipment.Equipment.Instance.PickItem(wasItemAssigned, slotID);
                    }
                } else {
                    // If user is not holding any item, pick the one in slot
                    if (assignedItem == null) return;
                    Equipment.Equipment.Instance.PickItem(assignedItem, slotID);
                }
            }
        }
        
        // Assign new item into slot
        private void Assign(ItemSO item) {
            assignedItem = item;

            icon.sprite = assignedItem.itemIcon;
            icon.enabled = true;
            
            Crafting.Instance.Refresh();
        }
        
        // Remove current item from slot
        public void Remove() {
            assignedItem = null;
            icon.enabled = false;
            
            Crafting.Instance.Refresh();
        }
        
        // Check if user is pointing at this slot
        public void OnPointerEnter(PointerEventData eventData) {
            if (eventData.pointerEnter) {
                focused = true;
                transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
            
                if (assignedItem == null) return;
                Equipment.Equipment.Instance.ShowTooltip(assignedItem, slotID);
            }
        }
        
        // Check if user stopped pointing this slot
        public void OnPointerExit(PointerEventData eventData) {
            if (eventData.fullyExited) {
                focused = false;
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                
                Equipment.Equipment.Instance.HideTooltip();
            }
        }
    }
}