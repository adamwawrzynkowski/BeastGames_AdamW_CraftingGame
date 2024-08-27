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
            firstCraftingSlot.Init(-1);
            secondCraftingSlot.Init(-2);
            
            Equipment.Equipment.Instance.GetCraftingSlots.Add(firstCraftingSlot);
            Equipment.Equipment.Instance.GetCraftingSlots.Add(secondCraftingSlot);
            
            openRecipeBook.onClick.AddListener(OpenRecipeBook);
            closeRecipeBook.onClick.AddListener(CloseRecipeBook);
            
            ClearCraftingResult();
        }

        public void Refresh() {
            if (firstCraftingSlot.GetInfo() == null || secondCraftingSlot.GetInfo() == null) {
                ClearCraftingResult();
                return;
            }
            
            foreach (var item in allItems) {
                var recipeFound = false;
                
                if (firstCraftingSlot.GetInfo() == item.firstCraftingItem && secondCraftingSlot.GetInfo() == item.secondCraftingItem) {
                    recipeFound = true;
                }
                
                if (firstCraftingSlot.GetInfo() == item.secondCraftingItem && secondCraftingSlot.GetInfo() == item.firstCraftingItem) {
                    recipeFound = true;
                }

                if (recipeFound) {
                    ShowCraftingResult(item);
                    return;
                }
            }
            
            ClearCraftingResult();
        }

        private void ShowCraftingResult(ItemSO item) {
            craftingResultIcon.sprite = item.itemIcon;
            craftingResultIcon.enabled = true;
            craftingResultItemName.text = item.itemName;
            
            var textColor = "";
            if (item.craftingChance <= 25) textColor = "<color=red>";
            if (item.craftingChance > 25) textColor = "<color=yellow>";
            if (item.craftingChance >= 75) textColor = "<color=green>";
            craftingChanceText.text = "Chance: " + textColor + item.craftingChance + "%" + "</color>";
            
            craftButton.interactable = true;
            craftButton.onClick.AddListener(() => Craft(item));
        }

        private void ClearCraftingResult() {
            craftingResultIcon.sprite = null;
            craftingResultIcon.enabled = false;
            craftingChanceText.text = null;
            craftingResultItemName.text = null;
            
            craftButton.interactable = false;
            craftButton.onClick.RemoveAllListeners();
        }

        private void RemoveCraftingItems() {
            firstCraftingSlot.Remove();
            secondCraftingSlot.Remove();
        }

        private void Craft(ItemSO item) {
            state = CurrentState.Crafting;
            StartCoroutine(DoCraft(item));
            
            RemoveCraftingItems();
            Refresh();
            
            AudioController.Instance.PlayAudio(AudioController.Instance.craftingClip, 0.5f);
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
                foreach (var _event in onCraftingSucceeded) {
                    _event.Invoke();
                }
                
                progressResultText.text = "<color=green>Success!</color>";
            } else {
                foreach (var _event in onCraftingFailed) {
                    _event.Invoke();
                }
                
                progressResultText.text = "<color=red>Failed!</color>";
            }
            
            yield return new WaitForSeconds(2.0f);
            progressWindow.SetActive(false);
            
            state = CurrentState.None;
        }

        private bool CountSuccessChance(ItemSO item) {
            return Random.Range(0, 101) <= item.craftingChance;
        }

        private void OpenRecipeBook() {
            if (recipeBookWindow.activeSelf) return;
            recipeBookWindow.SetActive(true);

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

        public void CloseRecipeBook() {
            for (var i = 0; i < recipesContainer.childCount; i++) {
                Destroy(recipesContainer.GetChild(i).gameObject);
            }
            
            recipeBookWindow.SetActive(false);
        }
    }
}