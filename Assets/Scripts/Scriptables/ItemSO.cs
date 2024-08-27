using UnityEngine;

namespace Scriptables {
    [CreateAssetMenu(fileName = "Item", menuName = "CraftingGame/New Item")]
    public class ItemSO : ScriptableObject {
        [Header("Settings")]
        public string itemName = "New Item";
        public Sprite itemIcon;
        public GameObject itemPrefab;

        [Header("Crafting")]
        public ItemSO firstCraftingItem;
        public ItemSO secondCraftingItem;

        [Space] [Range(0, 100)]
        public int craftingChance = 100;
    }
}