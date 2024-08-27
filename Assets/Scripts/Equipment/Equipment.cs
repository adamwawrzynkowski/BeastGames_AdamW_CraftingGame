using System.Collections.Generic;
using System.Linq;
using Audio;
using Console;
using Crafting;
using Player;
using Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryState { Closed, Opened }

namespace Equipment {
    public class Equipment : MonoBehaviour {
        public static Equipment Instance;
        private void Awake() {
            Instance = this;
        }

        [Header("Camera")]
        [SerializeField] private Camera camera;

        [Header("Input")]
        [SerializeField] private KeyCode toggleInventoryKey;

        [Header("UI")]
        [SerializeField] private GameObject inventoryWindow;
        
        [Space]
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

        private InventoryState state = InventoryState.Closed;
        public InventoryState GetState() => state;
        
        private void Start() {
            Construct();
        }

        private void Update() {
            if (Input.GetKeyDown(toggleInventoryKey)) {
                ToggleInventory();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (state == InventoryState.Opened) ToggleInventory(true);
            }
            
            if (pickedItem == null) return;
            pickedItemRect.transform.position = Input.mousePosition;
        }

        private void ToggleInventory(bool forceClose = false) {
            if (Crafting.Crafting.Instance.GetState() == CurrentState.Crafting) return;

            if (!forceClose) {
                if (state == InventoryState.Closed) state = InventoryState.Opened; else state = InventoryState.Closed;
            } else {
                state = InventoryState.Closed;
            }

            if (state == InventoryState.Opened) {
                inventoryWindow.SetActive(true);
            }

            if (state == InventoryState.Closed) {
                if (pickedItem != null) {
                    AddItem(pickedItem);
                    RemovePickedItem();
                }

                foreach (var slot in craftingSlots) {
                    if (slot.GetInfo() != null) {
                        AddItem(slot.GetInfo());
                        slot.Remove();
                    }
                }
                
                HideTooltip();
                Crafting.Crafting.Instance.CloseRecipeBook();
                inventoryWindow.SetActive(false);
            }
            
            AudioController.Instance.PlayAudio(AudioController.Instance.inventoryClip, 0.65f);
        }

        public void AddItem(ItemSO item, bool dontSpawnNew = false) {
            var slot = FindAvailableSlot();
            if (slot == null) {
                if (!dontSpawnNew) ThrowItem(item);
                ConsoleController.Instance.ShowMessage("Inventory is full!", 2.0f);
                return;
            }
            
            slot.Assign(item);
            AudioController.Instance.PlayAudio(AudioController.Instance.collectClip, 0.65f);
        }

        public void ThrowItem(ItemSO item) {
            if (item == null) return;
            
            var newItem = Instantiate(item.itemPrefab);
            newItem.transform.position = PlayerController.Instance.transform.position;

            var itemRigidbody = newItem.GetComponent<Rigidbody>();
            itemRigidbody.AddExplosionForce(20.0f, newItem.transform.position, 1.0f);
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

        public EquipmentSlot FindAvailableSlot() {
            return slots.FirstOrDefault(slot => slot.GetInfo() == null);
        }

        public void ShowTooltip(ItemSO item, int id, bool throwing = false) {
            if (!throwing && pickedItem != null) {
                HideTooltip();
                return;
            }
            
            itemTooltipWindow.gameObject.SetActive(true);

            if (!throwing) {
                var slot = FindSlot(item, id) == null ? FindCraftingSlot(item, id).transform : FindSlot(item, id).transform;

                itemTooltipWindow.transform.position = new Vector3(
                    slot.position.x + tooltipOffset.x * canvas.scaleFactor,
                    slot.position.y + tooltipOffset.y * canvas.scaleFactor);
            } else {
                itemTooltipWindow.transform.position = Input.mousePosition;
            }
            
            var prefix = throwing ? "Throw: " : "";
            itemTooltipItemNameText.text = prefix + item.itemName;
        }

        public void HideTooltip() {
            itemTooltipWindow.gameObject.SetActive(false);
        }
    }
}