using System.Collections;
using System.Collections.Generic;
using Audio;
using Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum CurrentState { None, Crafting }

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
        [SerializeField] private TMP_Text craftingResultItemName;

        [Header("Progress Window")]
        [SerializeField] private GameObject progressWindow;
        [SerializeField] private GameObject progressPanel;
        [SerializeField] private GameObject progressResultPanel;
        
        [Space]
        [SerializeField] private Image progressBar;
        [SerializeField] private TMP_Text progressResultText;
        [SerializeField] private Image progressItemIcon;

        [Header("Recipe Book")]
        [SerializeField] private GameObject recipeBookWindow;
        [SerializeField] private GameObject recipeItemPrefab;
        [SerializeField] private Button openRecipeBook;
        [SerializeField] private Button closeRecipeBook;
        [SerializeField] private Transform recipesContainer;
        
        [Header("Items")]
        [SerializeField] private List<ItemSO> allItems;

        [Header("Events")]
        [SerializeField] private List<UnityEvent> onCraftingSucceeded;
        [SerializeField] private List<UnityEvent> onCraftingFailed;

        private CurrentState state = CurrentState.None;
        public CurrentState GetState() => state;

        private void Start() {
            // Initialize crafting slots
            firstCraftingSlot.Init(-1);
            secondCraftingSlot.Init(-2);
            
            Equipment.Equipment.Instance.GetCraftingSlots.Add(firstCraftingSlot);
            Equipment.Equipment.Instance.GetCraftingSlots.Add(secondCraftingSlot);
            
            openRecipeBook.onClick.AddListener(OpenRecipeBook);
            closeRecipeBook.onClick.AddListener(CloseRecipeBook);
            
            ClearCraftingResult();
        }

        // Refresh crafting results
        public void Refresh() {
            // Check if slots are empty
            if (firstCraftingSlot.GetInfo() == null || secondCraftingSlot.GetInfo() == null) {
                ClearCraftingResult();
                return;
            }
            
            // Iterate through recipes to show correct one based on the items in slots
            foreach (var item in allItems) {
                var recipeFound = false;
                
                if (firstCraftingSlot.GetInfo() == item.firstCraftingItem && secondCraftingSlot.GetInfo() == item.secondCraftingItem) {
                    recipeFound = true;
                }
                
                if (firstCraftingSlot.GetInfo() == item.secondCraftingItem && secondCraftingSlot.GetInfo() == item.firstCraftingItem) {
                    recipeFound = true;
                }

                // If recipe has been found, show the result
                if (recipeFound) {
                    ShowCraftingResult(item);
                    return;
                }
            }
            
            ClearCraftingResult();
        }

        // Show crafting result
        private void ShowCraftingResult(ItemSO item) {
            // Assign icon and item name
            craftingResultIcon.sprite = item.itemIcon;
            craftingResultIcon.enabled = true;
            craftingResultItemName.text = item.itemName;
            
            // Show crafting chance and assign color based on the chance
            var textColor = "";
            if (item.craftingChance <= 25) textColor = "<color=red>";
            if (item.craftingChance > 25) textColor = "<color=yellow>";
            if (item.craftingChance >= 75) textColor = "<color=green>";
            craftingChanceText.text = "Chance: " + textColor + item.craftingChance + "%" + "</color>";
            
            craftButton.interactable = true;
            craftButton.onClick.AddListener(() => Craft(item));
        }

        // Clear crafting window
        private void ClearCraftingResult() {
            craftingResultIcon.sprite = null;
            craftingResultIcon.enabled = false;
            craftingChanceText.text = null;
            craftingResultItemName.text = null;
            
            craftButton.interactable = false;
            craftButton.onClick.RemoveAllListeners();
        }

        // Remove items from crafting slots
        private void RemoveCraftingItems() {
            firstCraftingSlot.Remove();
            secondCraftingSlot.Remove();
        }

        // Craft item
        private void Craft(ItemSO item) {
            // Change state to crafting
            state = CurrentState.Crafting;
            StartCoroutine(DoCraft(item));
            
            RemoveCraftingItems();
            Refresh();
            
            AudioController.Instance.PlayAudio(AudioController.Instance.craftingClip, 0.5f);
        }

        private IEnumerator DoCraft(ItemSO item) {
            // Show progress window UI
            progressWindow.SetActive(true);
            progressPanel.SetActive(true);
            progressResultPanel.SetActive(false);

            progressItemIcon.sprite = item.itemIcon;
            progressBar.fillAmount = 0;
            
            // Animate crafting progress bar
            for (var i = 0; i < 100; i++) {
                progressBar.fillAmount += 0.01f;
                yield return new WaitForSeconds(0.01f);
            }
            
            // Hide progress window
            progressPanel.SetActive(false);
            progressResultPanel.SetActive(true);
            
            // Show results
            // Add item to equipment
            if (CountSuccessChance(item)) {
                Equipment.Equipment.Instance.AddItem(item);
                
                // Invoke assigned events
                foreach (var _event in onCraftingSucceeded) {
                    _event.Invoke();
                }
                
                progressResultText.text = "<color=green>Success!</color>";
            } else {
                // Show failed result
                
                // Invoke assigned events
                foreach (var _event in onCraftingFailed) {
                    _event.Invoke();
                }
                
                progressResultText.text = "<color=red>Failed!</color>";
            }
            
            yield return new WaitForSeconds(2.0f);
            progressWindow.SetActive(false);
            
            // Change state back to none
            state = CurrentState.None;
        }

        // Count success rate of the crafting
        private bool CountSuccessChance(ItemSO item) {
            return Random.Range(0, 101) <= item.craftingChance;
        }

        // Show recipes window UI
        private void OpenRecipeBook() {
            if (recipeBookWindow.activeSelf) return;
            recipeBookWindow.SetActive(true);

            // Search for recipes in items list
            foreach (var item in allItems) {
                if (item.firstCraftingItem == null || item.secondCraftingItem == null) {
                    continue;
                }
                
                var newItem = Instantiate(recipeItemPrefab, recipesContainer);
                newItem.transform.localScale = new Vector3(1, 1, 1);

                var newItemRecipeScript = newItem.GetComponent<CraftingRecipeItem>();
                newItemRecipeScript.firstItem.sprite = item.firstCraftingItem.itemIcon;
                newItemRecipeScript.secondItem.sprite = item.secondCraftingItem.itemIcon;
                newItemRecipeScript.resultItem.sprite = item.itemIcon;
            }
        }

        // Close recipes window UI
        public void CloseRecipeBook() {
            for (var i = 0; i < recipesContainer.childCount; i++) {
                Destroy(recipesContainer.GetChild(i).gameObject);
            }
            
            recipeBookWindow.SetActive(false);
        }
    }
}