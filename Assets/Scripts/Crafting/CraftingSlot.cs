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
        
        public void Init(int ID) {
            slotID = ID;
            icon = GetComponent<EquipmentSlotReferenceHelper>().icon;
            icon.enabled = false;
        }
        
        public ItemSO GetInfo() {
            return assignedItem;
        }
        
        private void Update() {
            if (!focused) return;
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                if (Equipment.Equipment.Instance.CheckPickedItem()) {
                    if (Equipment.Equipment.Instance.GetPickedItem() == assignedItem) {
                        return;
                    }
                        
                    var wasItemAssigned = assignedItem;
                        
                    Assign(Equipment.Equipment.Instance.GetPickedItem());
                    Equipment.Equipment.Instance.RemovePickedItem();
                        
                    if (wasItemAssigned != null) {
                        Equipment.Equipment.Instance.PickItem(wasItemAssigned, slotID);
                    }
                } else {
                    if (assignedItem == null) return;
                    Equipment.Equipment.Instance.PickItem(assignedItem, slotID);
                }
            }
        }
        
        private void Assign(ItemSO item) {
            assignedItem = item;

            icon.sprite = assignedItem.itemIcon;
            icon.enabled = true;
            
            Crafting.Instance.Refresh();
        }
        
        public void Remove() {
            assignedItem = null;
            icon.enabled = false;
            
            Crafting.Instance.Refresh();
        }
        
        public void OnPointerEnter(PointerEventData eventData) {
            if (eventData.pointerEnter) {
                focused = true;
            }
        }
        
        public void OnPointerExit(PointerEventData eventData) {
            if (eventData.fullyExited) {
                focused = false;
            }
        }
    }
}