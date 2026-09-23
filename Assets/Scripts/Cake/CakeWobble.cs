using UnityEngine;

namespace Gaffeurs
{
    /// <summary>
    /// Solo estetica: inclina la torta come una molla smorzata, in base all'equilibrio
    /// e alla direzione dello scoordinamento. Non decide niente sul gameplay - quello
    /// è compito del BalanceSystem.
    /// </summary>
    public class CakeWobble : MonoBehaviour
    {
        [SerializeField] private BalanceSystem balance;
        [SerializeField] private PlayerMotor playerA;
        [SerializeField] private PlayerMotor playerB;

        [Tooltip("Il modello della torta da inclinare, figlio del vassoio.")]
        [SerializeField] private Transform cakeVisual;

        [Header("Molla")]
        [SerializeField] private float maxTiltDegrees = 35f;

        [Tooltip("Quanto forte la molla riporta la torta dritta. Alto = si riassesta in fretta.")]
        [SerializeField] private float stiffness = 90f;

        [Tooltip("Quanto l'oscillazione si smorza. Basso = la torta balla a lungo.")]
        [SerializeField] private float damping = 9f;

        [Tooltip("Quanto la torta rolla su accelerazioni e frenate brusche.")]
        [SerializeField] private float impulseScale = 55f;

        private Vector2 tilt;
        private Vector2 tiltVelocity;
        private Vector3 previousAverageVelocity;

        private void LateUpdate()
        {
            if (cakeVisual == null || balance == null) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector3 velA = playerA != null ? playerA.PlanarVelocity : Vector3.zero;
            Vector3 velB = playerB != null ? playerB.PlanarVelocity : Vector3.zero;

            // Inclinazione sostenuta: perpendicolare allo scoordinamento, scalata dall'equilibrio.
            Vector3 desync = ToLocal(velA - velB);
            Vector2 target = new Vector2(desync.z, -desync.x) * balance.Balance01;
            target = Vector2.ClampMagnitude(target, 1f) * maxTiltDegrees;

            // Impulso: partenze e frenate danno un colpo alla torta, che poi si riassesta
            // da sola grazie alla molla. È questo a farla "ballare" invece di piegarsi e basta.
            Vector3 average = (velA + velB) * 0.5f;
            Vector3 change = ToLocal(average - previousAverageVelocity);
            previousAverageVelocity = average;
            tiltVelocity += new Vector2(-change.z, change.x) * impulseScale;

            Vector2 accel = (target - tilt) * stiffness - tiltVelocity * damping;
            tiltVelocity += accel * dt;
            tilt = Vector2.ClampMagnitude(tilt + tiltVelocity * dt, maxTiltDegrees);

            cakeVisual.localRotation = Quaternion.Euler(tilt.x, 0f, tilt.y);
        }

        /// <summary>Porta una direzione dal mondo allo spazio del vassoio, così l'oscillazione
        /// resta coerente comunque siano girati i due camerieri.</summary>
        private Vector3 ToLocal(Vector3 worldDirection)
        {
            Transform reference = cakeVisual != null ? cakeVisual.parent : null;
            return reference != null ? reference.InverseTransformDirection(worldDirection) : worldDirection;
        }

        public void ResetWobble()
        {
            tilt = Vector2.zero;
            tiltVelocity = Vector2.zero;
            previousAverageVelocity = Vector3.zero;
            if (cakeVisual != null) cakeVisual.localRotation = Quaternion.identity;
        }
    }
}
