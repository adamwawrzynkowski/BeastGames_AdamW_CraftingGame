using System.Collections.Generic;
using System.Linq;
using Scriptables;
using UnityEngine;
using UnityEngine.UI;

namespace Equipment {
    public class Equipment : MonoBehaviour {
        public static Equipment Instance;
        private void Awake() {
            Instance = this;
        }

        [Header("Camera")]
        [SerializeField] private Camera camera;
        
        [Header("UI")]
        [SerializeField] private Transform equipmentSlotsContainer;
        [SerializeField] private GameObject equipmentSlot;

        [Space]
        [SerializeField] private Image pickedItemIcon;

        [Header("Settings")]
        [SerializeField] private Vector2 pickedItemOffset;

        private List<EquipmentSlot> slots = new List<EquipmentSlot>();
        
        private ItemSO pickedItem;
        private RectTransform pickedItemRect;
        
        public ItemSO testitemone;
        public ItemSO testitemtwo;

        private void Start() {
            Construct();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                AddItem(testitemone);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                AddItem(testitemtwo);
            }
            
            if (pickedItem == null) return;
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(pickedItemRect, Input.mousePosition,
                null, out pos);
            pickedItemRect.anchoredPosition3D = pos;
        }

        public void AddItem(ItemSO item) {
            var slot = FindAvailableSlot();
            if (slot == null) return;
            
            slot.Assign(item);
        }

        public void RemoveItem(ItemSO item) {
            foreach (var slot in slots.Where(slot => item == slot.GetInfo())) {
                slot.Remove();
                break;
            }
        }

        public void PickItem(ItemSO item) {
            pickedItem = item;
            pickedItemRect = pickedItemIcon.GetComponent<RectTransform>();
            pickedItemIcon.sprite = item.itemIcon;
            pickedItemIcon.enabled = true;
            
            RemoveItem(item);
        }

        public void RemovePickedItem() {
            pickedItem = null;
            pickedItemRect = null;
            pickedItemIcon.sprite = null;
            pickedItemIcon.enabled = false;
        }

        public bool CheckPickedItem() {
            return pickedItem != null;
        }

        public ItemSO GetPickedItem() {
            return pickedItem;
        }

        private void Construct() {
            for (var i = 0; i < 24; i++) {
                slots.Add(CreateSlot());
            }
        }

        private EquipmentSlot CreateSlot() {
            var newSlot = Instantiate(equipmentSlot, equipmentSlotsContainer);
            var slot = newSlot.AddComponent<EquipmentSlot>();
            
            slot.Init();
            return slot;
        }

        private EquipmentSlot FindAvailableSlot() {
            return slots.FirstOrDefault(slot => slot.GetInfo() == null);
        }
    }
}