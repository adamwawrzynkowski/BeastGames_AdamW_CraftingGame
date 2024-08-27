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
            if (Equipment.Equipment.Instance.GetState() == InventoryState.Opened) {
                return;
            }

            if (state == PlayerState.Idle) {
                animator.SetBool(runStateName, false);
                animator.SetBool(runInvertedStateName, false);
            }

            if (Input.GetKeyDown(KeyCode.E)) {
                CollectItem();
            }

            ScanForItems();
        }

        private void FixedUpdate() {
            Move();
        }
 
        private void Move() {
            if (Equipment.Equipment.Instance.GetState() == InventoryState.Opened) {
                return;
            }
            
            var x = Input.GetAxisRaw("Horizontal");
            var z = Input.GetAxisRaw("Vertical");

            if (x == 0 && z == 0) {
                if (state == PlayerState.Move) state = PlayerState.Idle;
                return;
            }
            
            rb.velocity = transform.forward * (z * (movementSpeed * Time.fixedDeltaTime));
            transform.Rotate(new Vector3(0, x * rotationSpeed * Time.fixedDeltaTime, 0), Space.World);
            cameraHook.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, 0.0f);

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
            var results = new Collider[4];
            interactableInRange = false;
            Physics.OverlapSphereNonAlloc(transform.position, itemsDetectionRange, results);
            
            foreach (var result in results) {
                if (result == null) continue;
                if (!result.TryGetComponent(out IInteractable interactable)) continue;
                
                ConsoleController.Instance.ChangeInteractionMessage("Press [E] to collect: " + interactable.GetInfo().itemName);
                currentInteractable = interactable;
                interactableInRange = true;
            }
            
            if (!interactableInRange) ConsoleController.Instance.ChangeInteractionMessage("");
        }

        private void CollectItem() {
            if (currentInteractable == null || !interactableInRange) return;
            currentInteractable.Interact();
            currentInteractable = null;
        }
    }
}