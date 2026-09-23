using UnityEngine;

namespace Gaffeurs
{
    /// <summary>
    /// Punto-mano a cui si aggancia il vassoio. Va messo su un figlio del cameriere,
    /// all'altezza delle mani e davanti al corpo.
    /// </summary>
    public class PlayerCarryAnchor : MonoBehaviour
    {
        [Tooltip("Il cameriere proprietario. Se vuoto viene cercato tra i genitori.")]
        [SerializeField] private PlayerMotor owner;

        public PlayerMotor Owner
        {
            get
            {
                if (owner == null) owner = GetComponentInParent<PlayerMotor>();
                return owner;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.55f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, 0.12f);
        }
    }
}
