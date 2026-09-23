using UnityEngine;

namespace Gaffeurs
{
    /// <summary>
    /// Camera isometrica che segue il punto medio tra i due camerieri e si allontana
    /// quanto basta perché restino entrambi inquadrati. In un couch co-op a schermo
    /// condiviso questo script pesa sulla giocabilità quanto il movimento.
    /// </summary>
    public class IsometricCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform targetA;
        [SerializeField] private Transform targetB;

        [Header("Inquadratura")]
        [Tooltip("Angolo di discesa: 35-40° è il look tipo Overcooked.")]
        [SerializeField] private float pitch = 40f;

        [Tooltip("Rotazione attorno all'asse verticale: 45° dà la classica vista isometrica.")]
        [SerializeField] private float yaw = 45f;

        [SerializeField] private float minDistance = 9f;
        [SerializeField] private float maxDistance = 16f;

        [Tooltip("Margine attorno ai due giocatori prima che la camera si allontani.")]
        [SerializeField] private float padding = 3f;

        [SerializeField] private float followSharpness = 6f;

        /// <summary>Al primo frame la camera è già in posizione, senza volata iniziale.</summary>
        private void Start() => SnapToTargets();

        private void LateUpdate()
        {
            if (targetA == null || targetB == null) return;

            float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, ComputeDesiredPosition(), t);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        public void SnapToTargets()
        {
            if (targetA == null || targetB == null) return;

            transform.position = ComputeDesiredPosition();
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private Vector3 ComputeDesiredPosition()
        {
            Vector3 mid = (targetA.position + targetB.position) * 0.5f;
            float spread = Vector3.Distance(targetA.position, targetB.position);
            float distance = Mathf.Clamp(minDistance + spread + padding, minDistance, maxDistance);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            return mid - rotation * Vector3.forward * distance;
        }
    }
}
