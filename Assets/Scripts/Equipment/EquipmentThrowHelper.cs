using UnityEngine;
using UnityEngine.EventSystems;

namespace Equipment {
    public class EquipmentThrowHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        private bool focused;
        
        private void Update() {
            // When area outside inventory is focused and user clicked left mouse button - throw an item
            if (focused && Input.GetKeyDown(KeyCode.Mouse0)) {
                Equipment.Instance.ThrowItem(Equipment.Instance.GetPickedItem());
                Equipment.Instance.RemovePickedItem();
                Equipment.Instance.HideTooltip();
            }
        }

        // Check if user stopped pointing this area
        public void OnPointerEnter(PointerEventData eventData) {
            if (eventData.pointerEnter && Equipment.Instance.GetPickedItem() != null) {
                focused = true;
                Equipment.Instance.ShowTooltip(Equipment.Instance.GetPickedItem(), -1, true);
            }
        }

        // Check if user stopped pointing this area
        public void OnPointerExit(PointerEventData eventData) {
            if (eventData.fullyExited) {
                focused = false;
                Equipment.Instance.HideTooltip();
            }
        }
    }
}