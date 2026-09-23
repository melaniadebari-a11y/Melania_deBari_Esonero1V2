using System;
using UnityEngine;

namespace Gaffeurs
{
    /// <summary>
    /// Il cuore del gioco: un solo numero da 0 (torta stabile) a 1 (torta caduta).
    /// Cresce quando i due camerieri si muovono in modo scoordinato, cala quando
    /// tornano in sincronia. Tutto il resto - HUD, audio, VFX, sconfitta - si limita
    /// ad ascoltare i suoi eventi, non ricalcola niente per conto proprio.
    /// </summary>
    public class BalanceSystem : MonoBehaviour
    {
        [Header("Giocatori")]
        [SerializeField] private PlayerMotor playerA;
        [SerializeField] private PlayerMotor playerB;
        [SerializeField] private CakeCarrier carrier;

        [Header("Quanto pesa ogni causa di sbilanciamento")]
        [Tooltip("Differenza di velocità tra i due: uno accelera, l'altro è fermo.")]
        [Range(0f, 2f)] [SerializeField] private float desyncWeight = 0.55f;

        [Tooltip("Tensione del vassoio: i due si allontanano o si schiacciano addosso.")]
        [Range(0f, 2f)] [SerializeField] private float stretchWeight = 0.45f;

        [Tooltip("Strattoni: cambi di direzione bruschi fanno ballare la torta.")]
        [Range(0f, 2f)] [SerializeField] private float jerkWeight = 0.30f;

        [Header("Ritmo")]
        [Tooltip("Quanto equilibrio si recupera al secondo muovendosi in sincronia.")]
        [SerializeField] private float recoveryPerSecond = 0.40f;

        [Tooltip("Tetto allo sbilanciamento guadagnabile in un secondo: evita le morti istantanee.")]
        [SerializeField] private float maxGainPerSecond = 0.9f;

        [Header("Soglie")]
        [Tooltip("Da qui in su la torta è in pericolo: lo useranno HUD e audio.")]
        [Range(0f, 1f)] [SerializeField] private float warningThreshold = 0.65f;

        /// <summary>0 = torta stabile, 1 = torta caduta.</summary>
        public float Balance01 { get; private set; }

        public bool InWarning => Balance01 >= warningThreshold;
        public float WarningThreshold => warningThreshold;

        public event Action<float> BalanceChanged;
        public event Action EnteredWarning;
        public event Action CakeLost;

        private Vector3 previousVelocityA;
        private Vector3 previousVelocityB;
        private bool lost;
        private bool wasInWarning;

        private void FixedUpdate()
        {
            if (lost || playerA == null || playerB == null) return;

            float dt = Time.fixedDeltaTime;
            float gain = Mathf.Min(ComputeGain(dt), maxGainPerSecond);

            // Guadagno e recupero agiscono insieme: piccole imprecisioni non uccidono.
            SetBalance(Balance01 + (gain - recoveryPerSecond) * dt);

            previousVelocityA = playerA.PlanarVelocity;
            previousVelocityB = playerB.PlanarVelocity;
        }

        private float ComputeGain(float dt)
        {
            float reference = Mathf.Max(playerA.MoveSpeed, 0.01f);

            // 1. Scoordinamento: quanto differiscono i due vettori velocità.
            float desync = Mathf.Clamp01((playerA.PlanarVelocity - playerB.PlanarVelocity).magnitude / (reference * 2f));

            // 2. Tensione: quanto la distanza si discosta da quella comoda del vassoio.
            //    Pesa solo mentre ci si muove. Da fermi la torta si riassesta sempre,
            //    qualunque sia la posa dei due: stare fermi non deve mai uccidere.
            float motion01 = Mathf.Clamp01(
                (playerA.PlanarVelocity.magnitude + playerB.PlanarVelocity.magnitude) / reference);

            float stretch = (carrier != null ? carrier.StretchAmount01 : 0f) * motion01;

            // 3. Strattone: quanto bruscamente è cambiata la velocità in questo frame.
            float jerkA = (playerA.PlanarVelocity - previousVelocityA).magnitude / Mathf.Max(dt, 0.0001f);
            float jerkB = (playerB.PlanarVelocity - previousVelocityB).magnitude / Mathf.Max(dt, 0.0001f);
            float jerk = Mathf.Clamp01((jerkA + jerkB) / (reference * 40f));

            return desync * desyncWeight + stretch * stretchWeight + jerk * jerkWeight;
        }

        /// <summary>
        /// Urto esterno che sbilancia di colpo. Lo useranno gli ostacoli della Fase 3
        /// (carrelli, porte a battente, tavoli).
        /// </summary>
        public void AddImpact(float amount)
        {
            if (lost) return;
            SetBalance(Balance01 + Mathf.Abs(amount));
        }

        public void ResetBalance()
        {
            lost = false;
            wasInWarning = false;
            previousVelocityA = Vector3.zero;
            previousVelocityB = Vector3.zero;
            SetBalance(0f);
        }

        private void SetBalance(float value)
        {
            Balance01 = Mathf.Clamp01(value);
            BalanceChanged?.Invoke(Balance01);

            if (!wasInWarning && InWarning)
            {
                wasInWarning = true;
                EnteredWarning?.Invoke();
            }
            else if (wasInWarning && !InWarning)
            {
                wasInWarning = false;
            }

            if (!lost && Balance01 >= 1f)
            {
                lost = true;
                CakeLost?.Invoke();
            }
        }
    }
}
