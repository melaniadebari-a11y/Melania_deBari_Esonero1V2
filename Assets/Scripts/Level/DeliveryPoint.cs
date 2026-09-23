using System;
using UnityEngine;

namespace Gaffeurs
{
    /// <summary>
    /// Il tavolo di consegna. Scatta quando il vassoio arriva abbastanza vicino.
    /// Usa una distanza invece di un trigger: un collider in meno da sbagliare nel blockout.
    /// </summary>
    public class DeliveryPoint : MonoBehaviour
    {
        [Tooltip("Il vassoio che deve arrivare fin qui.")]
        [SerializeField] private Transform tray;

        [SerializeField] private float radius = 1.5f;

        public event Action Delivered;

        private bool done;

        private void Update()
        {
            if (done || tray == null) return;

            Vector3 here = transform.position;
            Vector3 there = tray.position;
            here.y = 0f;
            there.y = 0f;

            if (Vector3.Distance(here, there) > radius) return;

            done = true;
            Delivered?.Invoke();
        }

        public void ResetPoint() => done = false;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.9f, 0.4f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
