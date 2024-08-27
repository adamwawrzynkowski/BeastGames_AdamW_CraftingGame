using System;
using UnityEngine;

namespace Audio {
    public class AudioController : MonoBehaviour {
        public static AudioController Instance;
        private void Awake() {
            Instance = this;
        }

        [Header("Audio Source")]
        [SerializeField] private AudioSource source;
        
        [Header("Audio Clips")]
        public AudioClip inventoryClip;
        public AudioClip collectClip;
        public AudioClip craftingClip;

        public void PlayAudio(AudioClip clip, float volume) {
            source.PlayOneShot(clip, volume);
        }
    }
}