using UnityEngine;

namespace Gaffeurs
{
    /// <summary>
    /// Tiene il vassoio al punto medio tra i due camerieri, lo orienta lungo l'asse
    /// che li unisce e impedisce che si allontanino più della lunghezza del vassoio.
    /// È questo vincolo a far "sentire" il legame fisico tra i due giocatori.
    /// </summary>
    public class CakeCarrier : MonoBehaviour
    {
        [Header("Aggancio")]
        [SerializeField] private PlayerCarryAnchor anchorA;
        [SerializeField] private PlayerCarryAnchor anchorB;

        [Tooltip("Il transform del vassoio da posizionare. Se vuoto usa questo GameObject.")]
        [SerializeField] private Transform tray;

        [Header("Vincolo di distanza")]
        [Tooltip("Distanza comoda: qui la tensione è zero.")]
        [SerializeField] private float idealDistance = 1.6f;

        [Tooltip("Oltre questa distanza i camerieri vengono richiamati: è la lunghezza fisica del vassoio.")]
        [SerializeField] private float maxDistance = 2.3f;

        [Tooltip("Sotto questa distanza si stanno schiacciando addosso.")]
        [SerializeField] private float minDistance = 0.8f;

        [Header("Chi cede quando il vassoio è teso")]
        [Tooltip("0 = chi si muove viene frenato e il compagno fermo non si sposta di un millimetro. " +
                 "1 = lo strappo si divide sempre a metà e il compagno viene trainato.")]
        [Range(0f, 1f)] [SerializeField] private float dragShare = 0f;

        [Header("Morbidezza")]
        [Tooltip("Quanto rapidamente il vassoio raggiunge la posizione di mezzo. Alto = rigido.")]
        [SerializeField] private float followSharpness = 18f;

        public float CurrentDistance { get; private set; }

        /// <summary>0 = distanza comoda, 1 = vassoio completamente teso o schiacciato.</summary>
        public float StretchAmount01 { get; private set; }

        private Transform Tray => tray != null ? tray : transform;

        private void Start() => SnapToPlayers();

        private void LateUpdate()
        {
            if (anchorA == null || anchorB == null) return;

            EnforceDistance();
            PlaceTray();
        }

        private void EnforceDistance()
        {
            // La distanza si misura tra i CORPI, non tra le mani: gli anchor sporgono in
            // avanti, quindi girarsi sul posto li allontanerebbe anche da fermi, creando
            // una tensione fantasma che non sparisce più. Le mani servono solo al visivo.
            PlayerMotor a = anchorA.Owner;
            PlayerMotor b = anchorB.Owner;
            if (a == null || b == null) return;

            Vector3 axis = b.transform.position - a.transform.position;
            axis.y = 0f;

            CurrentDistance = axis.magnitude;
            if (CurrentDistance < 0.0001f) return;

            Vector3 dir = axis / CurrentDistance;

            float overshoot = CurrentDistance - maxDistance;
            float squeeze = minDistance - CurrentDistance;

            if (overshoot > 0f)
            {
                // Chi si sta allontanando si prende la correzione: viene frenato dal vassoio.
                // Chi è fermo non deve essere trainato, altrimenti uno solo può portarsi
                // dietro l'altro per tutto il livello e la cooperazione non serve più.
                float pullA = Mathf.Max(0f, -Vector3.Dot(a.PlanarVelocity, dir));
                float pullB = Mathf.Max(0f, Vector3.Dot(b.PlanarVelocity, dir));
                SplitCorrection(pullA, pullB, out float shareA, out float shareB);

                a.ApplyExternalDisplacement(dir * (overshoot * shareA));
                b.ApplyExternalDisplacement(-dir * (overshoot * shareB));
                CurrentDistance = maxDistance;
            }
            else if (squeeze > 0f)
            {
                // Stessa logica al contrario: chi sta addosso all'altro viene respinto.
                float pushA = Mathf.Max(0f, Vector3.Dot(a.PlanarVelocity, dir));
                float pushB = Mathf.Max(0f, -Vector3.Dot(b.PlanarVelocity, dir));
                SplitCorrection(pushA, pushB, out float shareA, out float shareB);

                a.ApplyExternalDisplacement(-dir * (squeeze * shareA));
                b.ApplyExternalDisplacement(dir * (squeeze * shareB));
                CurrentDistance = minDistance;
            }

            float slack = Mathf.Max(maxDistance - idealDistance, 0.0001f);
            StretchAmount01 = Mathf.Clamp01(Mathf.Abs(CurrentDistance - idealDistance) / slack);
        }

        /// <summary>
        /// Ripartisce la correzione in proporzione a quanto ciascuno sta forzando il vincolo.
        /// Se nessuno dei due sta spingendo (per esempio dopo un urto) si torna al 50/50.
        /// </summary>
        private void SplitCorrection(float forceA, float forceB, out float shareA, out float shareB)
        {
            float total = forceA + forceB;

            if (total > 0.01f)
            {
                shareA = forceA / total;
                shareB = forceB / total;
            }
            else
            {
                shareA = 0.5f;
                shareB = 0.5f;
            }

            // dragShare riporta gradualmente verso il vecchio comportamento "metà per uno".
            shareA = Mathf.Lerp(shareA, 0.5f, dragShare);
            shareB = Mathf.Lerp(shareB, 0.5f, dragShare);
        }

        private void PlaceTray()
        {
            Vector3 a = anchorA.transform.position;
            Vector3 b = anchorB.transform.position;
            Vector3 mid = (a + b) * 0.5f;

            Vector3 axis = b - a;
            axis.y = 0f;

            Quaternion rot = axis.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(Vector3.Cross(Vector3.up, axis.normalized), Vector3.up)
                : Tray.rotation;

            float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            Tray.position = Vector3.Lerp(Tray.position, mid, t);
            Tray.rotation = Quaternion.Slerp(Tray.rotation, rot, t);
        }

        /// <summary>Riporta il vassoio esattamente tra i due, senza smorzamento. Serve al restart.</summary>
        public void SnapToPlayers()
        {
            if (anchorA == null || anchorB == null) return;

            Tray.position = (anchorA.transform.position + anchorB.transform.position) * 0.5f;
            StretchAmount01 = 0f;
        }

        private void OnDrawGizmosSelected()
        {
            if (anchorA == null || anchorB == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(anchorA.transform.position, anchorB.transform.position);
        }
    }
}
