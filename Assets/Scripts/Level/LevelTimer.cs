using System;
using UnityEngine;

namespace Gaffeurs
{
    /// <summary>Countdown condiviso del livello.</summary>
    public class LevelTimer : MonoBehaviour
    {
        [Tooltip("Tempo limite del livello, in secondi.")]
        [SerializeField] private float duration = 120f;

        public float Duration => duration;
        public float Remaining { get; private set; }

        /// <summary>1 all'inizio, 0 a tempo scaduto. Comodo per il timer circolare dell'HUD.</summary>
        public float Normalized01 => duration > 0f ? Mathf.Clamp01(Remaining / duration) : 0f;

        /// <summary>Lo pilota il GameManager: il tempo scorre solo mentre si gioca.</summary>
        public bool Running { get; set; } = true;

        public event Action TimeUp;

        private bool fired;

        private void Awake() => ResetTimer();

        private void Update()
        {
            if (!Running || fired) return;

            Remaining -= Time.deltaTime;
            if (Remaining > 0f) return;

            Remaining = 0f;
            fired = true;
            TimeUp?.Invoke();
        }

        public void ResetTimer()
        {
            Remaining = duration;
            fired = false;
        }
    }
}
