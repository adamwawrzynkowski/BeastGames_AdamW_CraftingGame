using System.Collections.Generic;
using System.Linq;
using Audio;
using Console;
using Crafting;
using Items;
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

        [Header("Null Item")]
        [SerializeField] private ItemSO nullItem;
        
        private List<EquipmentSlot> slots = new List<EquipmentSlot>();
        private List<CraftingSlot> craftingSlots = new List<CraftingSlot>();
        public List<CraftingSlot> GetCraftingSlots => craftingSlots;
        
        private ItemSO pickedItem;
        
        private RectTransform pickedItemRect;

        private InventoryState state = InventoryState.Closed;
        public InventoryState GetState() => state;
        
        private void Start() {
            // Create slots in equipment window
            Construct();
        }

        private void Update() {
            // Check if key responsible for toggling an inventory is clicked
            if (Input.GetKeyDown(toggleInventoryKey)) {
                ToggleInventory();
            }
            
            // Close inventory by clicking ESC key
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (state == InventoryState.Opened) ToggleInventory(true);
            }
            
            // Check if any item is picked, and show its icon on mouse position
            if (pickedItem == null) return;
            pickedItemRect.transform.position = Input.mousePosition;
        }

        // Open / Close inventory
        private void ToggleInventory(bool forceClose = false) {
            // Block inventory toggling if user is currently crafting an item
            if (Crafting.Crafting.Instance.GetState() == CurrentState.Crafting) return;

            if (!forceClose) {
                if (state == InventoryState.Closed) state = InventoryState.Opened; else state = InventoryState.Closed;
            } else {
                state = InventoryState.Closed;
            }

            // Show inventory UI
            if (state == InventoryState.Opened) {
                inventoryWindow.SetActive(true);
            }

            // Close inventory UI
            if (state == InventoryState.Closed) {
                // Check if any item is currently picked, if yes - remove picked item and add it to inventory
                if (pickedItem != null) {
                    AddItem(pickedItem);
                    RemovePickedItem();
                }

                // Check if any item is assigned to crafting slots, if yes - move them back to inventory
                foreach (var slot in craftingSlots) {
                    if (slot.GetInfo() != null) {
                        AddItem(slot.GetInfo());
                        slot.Remove();
                    }
                }
                
                // Hide item tooltip and crafting book
                HideTooltip();
                Crafting.Crafting.Instance.CloseRecipeBook();
                inventoryWindow.SetActive(false);
            }
            
            AudioController.Instance.PlayAudio(AudioController.Instance.inventoryClip, 0.65f);
        }

        // Add item to inventory
        public void AddItem(ItemSO item, bool dontSpawnNew = false) {
            // Check if any free slot is available
            var slot = FindAvailableSlot();
            
            // If slow was not found, throw item
            if (slot == null) {
                if (!dontSpawnNew) ThrowItem(item);
                ConsoleController.Instance.ShowMessage("Inventory is full!", 2.0f);
                return;
            }
            
            // Assign new item to inventory
            slot.Assign(item);
            
            AudioController.Instance.PlayAudio(AudioController.Instance.collectClip, 0.65f);
        }

        // Throw item from inventory
        public void ThrowItem(ItemSO item) {
            if (item == null) return;
            
            // If item prefab is null, assign default item
            if (item.itemPrefab == null) {
                item = nullItem;
            }
            
            // Instantiate new item on scene
            var newItem = Instantiate(item.itemPrefab);
            newItem.transform.position = PlayerController.Instance.transform.position;
            
            var _item = newItem.GetComponent<Item>();
            _item.ChangeDuplicableMode(false);

            var itemRigidbody = newItem.GetComponent<Rigidbody>();
            itemRigidbody.AddExplosionForce(20.0f, newItem.transform.position, 1.0f);
        }

        // Iterate through slots to find free one
        private EquipmentSlot FindSlot(ItemSO item, int id) {
            return slots.FirstOrDefault(slot => item == slot.GetInfo() && slot.GetID() == id);
        }
        
        // Iterate through slots to find free one in crafting slots
        private CraftingSlot FindCraftingSlot(ItemSO item, int id) {
            return craftingSlots.FirstOrDefault(slot => item == slot.GetInfo() && slot.GetID() == id);
        }

        // Remove item from inventory
        private void RemoveItem(ItemSO item, int id) {
            var slot = FindSlot(item, id);
            if (slot != null) slot.Remove();
            else {
                var craftingSlot = FindCraftingSlot(item, id);
                if (craftingSlot != null) craftingSlot.Remove();
            }
        }

        // Pick item to move it to another slot or throw it
        public void PickItem(ItemSO item, int id) {
            pickedItem = item;
            pickedItemRect = pickedItemIcon.GetComponent<RectTransform>();
            pickedItemIcon.sprite = item.itemIcon;
            pickedItemIcon.enabled = true;
            
            RemoveItem(item, id);
            HideTooltip();
        }

        // Remove currently picked item
        public void RemovePickedItem() {
            pickedItem = null;
            pickedItemRect = null;
            pickedItemIcon.sprite = null;
            pickedItemIcon.enabled = false;
        }

        // Check if any item is currently picked
        public bool CheckPickedItem() {
            return pickedItem != null;
        }

        // Get currently picked item
        public ItemSO GetPickedItem() {
            return pickedItem;
        }

        // Create new slots in inventory
        private void Construct() {
            for (var i = 0; i < 24; i++) {
                slots.Add(CreateSlot(i));
            }
        }

        // Instantiate new slots in inventory
        private EquipmentSlot CreateSlot(int i) {
            var newSlot = Instantiate(equipmentSlot, equipmentSlotsContainer);
            newSlot.name += "_" + i;
            
            var slot = newSlot.AddComponent<EquipmentSlot>();
            
            slot.Init(i);
            return slot;
        }

        // Try to find new free slot in inventory
        public EquipmentSlot FindAvailableSlot() {
            return slots.FirstOrDefault(slot => slot.GetInfo() == null);
        }

        // Show item tooltip
        public void ShowTooltip(ItemSO item, int id, bool throwing = false) {
            if (!throwing && pickedItem != null) {
                HideTooltip();
                return;
            }
            
            itemTooltipWindow.gameObject.SetActive(true);

            if (!throwing) {
                // Find slot of currently pointed item and show tooltip based on slot position
                var slot = FindSlot(item, id) == null ? FindCraftingSlot(item, id).transform : FindSlot(item, id).transform;

                itemTooltipWindow.transform.position = new Vector3(
                    slot.position.x + tooltipOffset.x * canvas.scaleFactor,
                    slot.position.y + tooltipOffset.y * canvas.scaleFactor);
            } else {
                // Show tooltip on mouse position while throwing it out of inventory
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