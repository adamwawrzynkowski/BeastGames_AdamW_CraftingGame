using Console;
using Interfaces;
using UnityEngine;

public enum PlayerState { Idle, Move }

namespace Player {
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour {
        public static PlayerController Instance;
        
        [Header("Camera")]
        [SerializeField] private Camera camera;
        [SerializeField] private Transform cameraHook;

        [Header("Animations")]
        [SerializeField] private string runStateName;
        [SerializeField] private string runInvertedStateName;
        
        [Header("Settings")]
        [SerializeField] private float movementSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float cameraSpeed;
        [SerializeField] private float itemsDetectionRange;
        
        private PlayerState state = PlayerState.Idle;

        private Animator animator;
        private Rigidbody rb;

        private bool interactableInRange;
        private IInteractable currentInteractable;

        private void Awake() {
            Instance = this;
            
            rb = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
        }

        private void Update() {
            // Block player movement if inventory is currently opened
            if (Equipment.Equipment.Instance.GetState() == InventoryState.Opened) {
                return;
            }

            // If player is not moving, stop running animations
            if (state == PlayerState.Idle) {
                animator.SetBool(runStateName, false);
                animator.SetBool(runInvertedStateName, false);
            }

            // Try to collect item when E key is pressed
            if (Input.GetKeyDown(KeyCode.E)) {
                CollectItem();
            }

            // Scan for items in range
            ScanForItems();
        }

        private void FixedUpdate() {
            Move();
        }
 
        private void Move() {
            // Block player movement if inventory is currently opened
            if (Equipment.Equipment.Instance.GetState() == InventoryState.Opened) {
                return;
            }
            
            // Get input data
            var x = Input.GetAxisRaw("Horizontal");
            var z = Input.GetAxisRaw("Vertical");

            // If input data is equals to 0, change state to idle and block movement script
            if (x == 0 && z == 0) {
                if (state == PlayerState.Move) state = PlayerState.Idle;
                return;
            }
            
            // Move player using physics
            rb.velocity = transform.forward * (z * (movementSpeed * Time.fixedDeltaTime));
            
            // Rotate player based on the input data
            transform.Rotate(new Vector3(0, x * rotationSpeed * Time.fixedDeltaTime, 0), Space.World);
            
            // Rotate camera based on the player rotation
            cameraHook.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, 0.0f);

            // Assign correct animation based on the input data
            if (z != 0) {
                if (z > 0) {
                    animator.SetBool(runStateName, true);
                    animator.SetBool(runInvertedStateName, false);
                }

                if (z < 0) {
                    animator.SetBool(runStateName, false);
                    animator.SetBool(runInvertedStateName, true);
                }
                
                state = PlayerState.Move;
            }
        }

        private void ScanForItems() {
            // Search through colliders inside overlap sphere with allocation size equal 4
            var results = new Collider[4];
            interactableInRange = false;
            Physics.OverlapSphereNonAlloc(transform.position, itemsDetectionRange, results);
            
            // If any of the results was found, check if result is interactable
            foreach (var result in results) {
                if (result == null) continue;
                if (!result.TryGetComponent(out IInteractable interactable)) continue;
                
                // Show UI to let player know that this item is interactable
                ConsoleController.Instance.ChangeInteractionMessage("Press [E] to collect: " + interactable.GetInfo().itemName);
                currentInteractable = interactable;
                interactableInRange = true;
            }
            
            if (!interactableInRange) ConsoleController.Instance.ChangeInteractionMessage("");
        }

        // Collect item if any of the items is currently in range
        private void CollectItem() {
            if (currentInteractable == null || !interactableInRange) return;
            currentInteractable.Interact();
            currentInteractable = null;
        }
    }
}