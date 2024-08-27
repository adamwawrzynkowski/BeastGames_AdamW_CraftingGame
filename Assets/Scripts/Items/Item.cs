using Console;
using UnityEngine;
using Interfaces;
using Scriptables;

namespace Items {
    public class Item : MonoBehaviour, IInteractable {
        [Header("Settings")]
        [SerializeField] private ItemSO assignedItem;

        public ItemSO GetInfo() {
            return assignedItem;
        }

        public void Interact() {
            if (Equipment.Equipment.Instance.FindAvailableSlot() == null) {
                ConsoleController.Instance.ShowMessage("Inventory is full!", 2.0f);
                return;
            }
            
            Equipment.Equipment.Instance.AddItem(assignedItem, true);
            ConsoleController.Instance.ShowMessage("New item: " + assignedItem.itemName, 2.0f);
            
            Respawn();
        }

        private void Respawn() {
            ItemsController.Instance.RespawnItem(transform);
        }
    }
}