using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Console {
    public class ConsoleController : MonoBehaviour {
        public static ConsoleController Instance;
        private void Awake() {
            Instance = this;
        }

        [Header("UI")]
        [SerializeField] private TMP_Text consoleText;
        [SerializeField] private TMP_Text interactionText;
        
        public void ChangeInteractionMessage(string msg) {
            interactionText.text = msg;
        }
        
        public void ShowMessage(string msg, float t) {
            StopCoroutine(HideMessage(t));
            
            consoleText.text = msg;
            StartCoroutine(HideMessage(t));
        }

        private IEnumerator HideMessage(float t) {
            yield return new WaitForSeconds(t);
            consoleText.text = "";
        }
    }
}