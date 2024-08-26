using System.Collections.Generic;
using System.Linq;
using Crafting;
using Scriptables;
using TMPro;
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
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasScaler scaler;
        
        [Space]
        [SerializeField] private Transform equipmentSlotsContainer;
        [SerializeField] private GameObject equipmentSlot;

        [Space]
        [SerializeField] private Image pickedItemIcon;

        [Space]
        [SerializeField] private RectTransform itemTooltipWindow;
        [SerializeField] private TMP_Text itemTooltipItemNameText;

        [Header("Settings")]
        [SerializeField] private Vector2 tooltipOffset;
        
        private List<EquipmentSlot> slots = new List<EquipmentSlot>();
        private List<CraftingSlot> craftingSlots = new List<CraftingSlot>();
        public List<CraftingSlot> GetCraftingSlots => craftingSlots;
        
        private ItemSO pickedItem;
        private RectTransform pickedItemRect;
        
        public ItemSO testitemone;
        public ItemSO testitemtwo;
        public ItemSO testitemthree;
        
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
            
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                AddItem(testitemthree);
            }
            
            if (pickedItem == null) return;
            pickedItemRect.transform.position = Input.mousePosition;
        }

        public void AddItem(ItemSO item) {
            var slot = FindAvailableSlot();
            if (slot == null) return;
            
            slot.Assign(item);
        }

        private EquipmentSlot FindSlot(ItemSO item, int id) {
            return slots.FirstOrDefault(slot => item == slot.GetInfo() && slot.GetID() == id);
        }
        
        private CraftingSlot FindCraftingSlot(ItemSO item, int id) {
            return craftingSlots.FirstOrDefault(slot => item == slot.GetInfo() && slot.GetID() == id);
        }

        private void RemoveItem(ItemSO item, int id) {
            var slot = FindSlot(item, id);
            if (slot != null) slot.Remove();
            else {
                var craftingSlot = FindCraftingSlot(item, id);
                if (craftingSlot != null) craftingSlot.Remove();
            }
        }

        public void PickItem(ItemSO item, int id) {
            pickedItem = item;
            pickedItemRect = pickedItemIcon.GetComponent<RectTransform>();
            pickedItemIcon.sprite = item.itemIcon;
            pickedItemIcon.enabled = true;
            
            RemoveItem(item, id);
            HideTooltip();
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
                slots.Add(CreateSlot(i));
            }
        }

        private EquipmentSlot CreateSlot(int i) {
            var newSlot = Instantiate(equipmentSlot, equipmentSlotsContainer);
            newSlot.name += "_" + i;
            
            var slot = newSlot.AddComponent<EquipmentSlot>();
            
            slot.Init(i);
            return slot;
        }

        private EquipmentSlot FindAvailableSlot() {
            return slots.FirstOrDefault(slot => slot.GetInfo() == null);
        }

        public void ShowTooltip(ItemSO item, int id) {
            if (pickedItem != null) {
                HideTooltip();
                return;
            }
            
            itemTooltipWindow.gameObject.SetActive(true);
            Transform slot;
            slot = FindSlot(item, id) == null ? FindCraftingSlot(item, id).transform : FindSlot(item, id).transform;

            itemTooltipWindow.transform.position = new Vector3(
                slot.position.x + tooltipOffset.x * canvas.scaleFactor,
                slot.position.y + tooltipOffset.y * canvas.scaleFactor);

            itemTooltipItemNameText.text = item.itemName;
        }

        public void HideTooltip() {
            itemTooltipWindow.gameObject.SetActive(false);
        }
    }
}