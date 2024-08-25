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
            throw new System.NotImplementedException();
        }
    }
}