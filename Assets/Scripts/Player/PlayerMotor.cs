using UnityEngine;

namespace Gaffeurs
{
    /// <summary>
    /// Movimento di un singolo cameriere. L'input arriva da qualsiasi componente
    /// sullo stesso GameObject che implementi IMoveInput.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float acceleration = 30f;
        [SerializeField] private float rotationSpeed = 900f;
        [SerializeField] private float gravity = -25f;

        [Header("Camera")]
        [Tooltip("Se assegnata, l'input viene ruotato secondo lo yaw della camera: 'su' sul tasto = su sullo schermo. Indispensabile con la vista isometrica.")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private IMoveInput input;
        private Vector3 planarVelocity;
        private float verticalVelocity;

        /// <summary>Velocità orizzontale attuale in unità/secondo. La legge il BalanceSystem.</summary>
        public Vector3 PlanarVelocity => planarVelocity;

        public float MoveSpeed => moveSpeed;

        /// <summary>0 = fermo, 1 = alla massima velocità. Servirà all'Animator in Fase 5.</summary>
        public float SpeedNormalized => moveSpeed > 0f ? planarVelocity.magnitude / moveSpeed : 0f;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<IMoveInput>();

            if (input == null)
                Debug.LogError($"{name}: manca un componente che implementi IMoveInput (es. PlayerInputHandler).", this);
        }

        private void Update()
        {
            Vector3 desired = ToWorldDirection(ReadInput()) * moveSpeed;
            planarVelocity = Vector3.MoveTowards(planarVelocity, desired, acceleration * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = planarVelocity;
            motion.y = verticalVelocity;
            controller.Move(motion * Time.deltaTime);

            if (planarVelocity.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(planarVelocity.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * Time.deltaTime);
            }
        }

        private Vector2 ReadInput()
        {
            if (input == null) return Vector2.zero;
            if (GameManager.Instance != null && !GameManager.Instance.PlayersCanMove) return Vector2.zero;

            return input.ReadMove();
        }

        private Vector3 ToWorldDirection(Vector2 raw)
        {
            Vector3 dir = new Vector3(raw.x, 0f, raw.y);
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            if (cameraTransform == null) return dir;

            return Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f) * dir;
        }

        /// <summary>Sposta il giocatore ignorando il CharacterController. Serve al restart.</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = true;

            planarVelocity = Vector3.zero;
            verticalVelocity = 0f;
        }

        /// <summary>
        /// Spostamento imposto dall'esterno: lo usa il CakeCarrier per tenere
        /// i due camerieri entro la lunghezza del vassoio.
        /// </summary>
        public void ApplyExternalDisplacement(Vector3 delta)
        {
            if (controller != null && controller.enabled) controller.Move(delta);
        }
    }
}
