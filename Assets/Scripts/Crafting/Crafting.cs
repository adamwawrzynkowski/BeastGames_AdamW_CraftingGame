using System.Collections;
using System.Collections.Generic;
using Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crafting {
    public class Crafting : MonoBehaviour {
        public static Crafting Instance;
        private void Awake() {
            Instance = this;
        }

        [Header("Crafting Slots")]
        [SerializeField] private CraftingSlot firstCraftingSlot;
        [SerializeField] private CraftingSlot secondCraftingSlot;

        [Header("Results")]
        [SerializeField] private Image craftingResultIcon;
        [SerializeField] private Button craftButton;
        [SerializeField] private TMP_Text craftingChanceText;

        [Header("Progress Window")]
        [SerializeField] private GameObject progressWindow;
        [SerializeField] private GameObject progressPanel;
        [SerializeField] private GameObject progressResultPanel;
        
        [Space]
        [SerializeField] private Image progressBar;
        [SerializeField] private TMP_Text progressResultText;
        [SerializeField] private Image progressItemIcon;

        [Header("Items")]
        [SerializeField] private List<ItemSO> allItems;

        private void Start() {
            firstCraftingSlot.Init(-1);
            secondCraftingSlot.Init(-2);
            
            Equipment.Equipment.Instance.GetCraftingSlots.Add(firstCraftingSlot);
            Equipment.Equipment.Instance.GetCraftingSlots.Add(secondCraftingSlot);
            
            ClearCraftingResult();
        }

        public void Refresh() {
            if (firstCraftingSlot.GetInfo() == null || secondCraftingSlot.GetInfo() == null) {
                ClearCraftingResult();
                return;
            }
            
            foreach (var item in allItems) {
                var first = false;
                var second = false;
                    
                if (item.firstCraftingItem == firstCraftingSlot.GetInfo() || item.secondCraftingItem == firstCraftingSlot.GetInfo()) {
                    first = true;
                }
                    
                if (item.firstCraftingItem == secondCraftingSlot.GetInfo() || item.secondCraftingItem == secondCraftingSlot.GetInfo()) {
                    second = true;
                }

                if (first && second) {
                    ShowCraftingResult(item);
                    return;
                }
            }
            
            ClearCraftingResult();
        }

        private void ShowCraftingResult(ItemSO item) {
            craftingResultIcon.sprite = item.itemIcon;
            craftingResultIcon.enabled = true;
            craftingChanceText.text = "Chance: " + item.craftingChance + "%";
            
            craftButton.interactable = true;
            craftButton.onClick.AddListener(() => Craft(item));
        }

        private void ClearCraftingResult() {
            craftingResultIcon.sprite = null;
            craftingResultIcon.enabled = false;
            craftingChanceText.text = null;
            
            craftButton.interactable = false;
            craftButton.onClick.RemoveAllListeners();
        }

        private void RemoveCraftingItems() {
            firstCraftingSlot.Remove();
            secondCraftingSlot.Remove();
        }

        private void Craft(ItemSO item) {
            StartCoroutine(DoCraft(item));
            
            RemoveCraftingItems();
            Refresh();
        }

        private IEnumerator DoCraft(ItemSO item) {
            progressWindow.SetActive(true);
            progressPanel.SetActive(true);
            progressResultPanel.SetActive(false);

            progressItemIcon.sprite = item.itemIcon;
            progressBar.fillAmount = 0;
            
            for (var i = 0; i < 100; i++) {
                progressBar.fillAmount += 0.01f;
                yield return new WaitForSeconds(0.01f);
            }
            
            progressPanel.SetActive(false);
            progressResultPanel.SetActive(true);
            
            if (CountSuccessChance(item)) {
                Equipment.Equipment.Instance.AddItem(item);
                progressResultText.text = "<color=green>Success!</color>";
            } else {
                progressResultText.text = "<color=red>Failed!</color>";
            }
            
            yield return new WaitForSeconds(2.0f);
            progressWindow.SetActive(false);
        }

        private bool CountSuccessChance(ItemSO item) {
            return Random.Range(0, 101) <= item.craftingChance;
        }
    }
}