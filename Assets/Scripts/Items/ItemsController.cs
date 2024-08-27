using UnityEngine;

namespace Items {
    public class ItemsController : MonoBehaviour {
        public static ItemsController Instance;
        private void Awake() {
            Instance = this;
        }

        [Header("Spawn Area")]
        [SerializeField] private BoxCollider area;

        // Search for position inside bounds and set item position
        public void RespawnItem(Transform t) {
            var bounds = GetSpawnBounds();
            t.position = new Vector3(Random.Range(bounds.min.x, bounds.max.x), 1.5f,
                Random.Range(bounds.min.z, bounds.max.z));
        }
        
        private Bounds GetSpawnBounds() {
            return area.bounds;
        }
    }
}