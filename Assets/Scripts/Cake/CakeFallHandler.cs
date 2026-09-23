using UnityEngine;

namespace Gaffeurs
{
    /// <summary>
    /// Reazione visiva alla caduta: la torta si stacca dal vassoio e rotola per terra.
    /// Il Rigidbody resta sempre sul modello, cinematico finché la torta è in mano:
    /// così non va mai aggiunto o distrutto a runtime.
    /// </summary>
    public class CakeFallHandler : MonoBehaviour
    {
        [SerializeField] private BalanceSystem balance;
        [SerializeField] private CakeWobble wobble;

        [Tooltip("Il modello della torta. Deve avere un Rigidbody impostato su cinematico.")]
        [SerializeField] private Transform cakeVisual;

        [SerializeField] private float pushForce = 3f;

        private Rigidbody body;
        private Transform originalParent;
        private Vector3 localStartPosition;
        private Quaternion localStartRotation;

        private void Awake()
        {
            if (cakeVisual == null) return;

            body = cakeVisual.GetComponent<Rigidbody>();
            originalParent = cakeVisual.parent;
            localStartPosition = cakeVisual.localPosition;
            localStartRotation = cakeVisual.localRotation;
        }

        private void OnEnable()
        {
            if (balance != null) balance.CakeLost += HandleCakeLost;
        }

        private void OnDisable()
        {
            if (balance != null) balance.CakeLost -= HandleCakeLost;
        }

        private void HandleCakeLost()
        {
            if (cakeVisual == null) return;

            cakeVisual.SetParent(null, true);

            if (body != null)
            {
                body.isKinematic = false;
                body.AddForce((Random.insideUnitSphere + Vector3.up * 2f).normalized * pushForce, ForceMode.VelocityChange);
                body.AddTorque(Random.insideUnitSphere * pushForce, ForceMode.VelocityChange);
            }
        }

        /// <summary>Rimette la torta sul vassoio. Lo chiama il GameManager al restart.</summary>
        public void Restore()
        {
            if (cakeVisual == null) return;

            if (body != null)
            {
                body.isKinematic = true;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            cakeVisual.SetParent(originalParent, false);
            cakeVisual.localPosition = localStartPosition;
            cakeVisual.localRotation = localStartRotation;

            if (wobble != null) wobble.ResetWobble();
        }
    }
}
