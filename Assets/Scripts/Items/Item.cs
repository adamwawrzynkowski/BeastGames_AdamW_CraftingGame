using Console;
using UnityEngine;
using Interfaces;
using Scriptables;

namespace Items {
    public class Item : MonoBehaviour, IInteractable {
        [Header("Settings")]
        [SerializeField] private ItemSO assignedItem;
        [SerializeField] private bool duplicable = true;

        public void ChangeDuplicableMode(bool mode) {
            duplicable = mode;
        }

        // Get this item
        public ItemSO GetInfo() {
            return assignedItem;
        }

        // Set new item
        public void SetItem(ItemSO item) => assignedItem = item;

        // Interact with item
        public void Interact() {
            // Check if inventory is full, and block item interaction if so
            if (Equipment.Equipment.Instance.FindAvailableSlot() == null) {
                ConsoleController.Instance.ShowMessage("Inventory is full!", 2.0f);
                return;
            }
            
            // Add item to the inventory
            Equipment.Equipment.Instance.AddItem(assignedItem, true);
            ConsoleController.Instance.ShowMessage("New item: " + assignedItem.itemName, 2.0f);
            
            if (duplicable) Respawn(); else Destroy(gameObject);
        }

        // Respawn new item on scene if this item is set as duplicable
        private void Respawn() {
            ItemsController.Instance.RespawnItem(transform);
        }
    }
}