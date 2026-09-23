using System;
using UnityEngine;

namespace Gaffeurs
{
    public enum GameState
    {
        Playing,
        Falling, // torta caduta, si aspetta prima di rimettere tutto a posto
        Won,
        Lost
    }

    /// <summary>
    /// Orchestratore della partita. Ascolta torta, timer e punto di consegna,
    /// decide vittoria e sconfitta, e riporta tutto al punto di partenza dopo una caduta.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Sistemi")]
        [SerializeField] private BalanceSystem balance;
        [SerializeField] private LevelTimer timer;
        [SerializeField] private DeliveryPoint deliveryPoint;
        [SerializeField] private CakeCarrier carrier;
        [SerializeField] private CakeFallHandler fallHandler;

        [Header("Giocatori e spawn")]
        [Tooltip("I due camerieri, nello stesso ordine dei punti di spawn qui sotto.")]
        [SerializeField] private PlayerMotor[] players;
        [SerializeField] private Transform[] playerSpawns;

        [Header("Restart")]
        [Tooltip("Pausa dopo la caduta della torta, prima di rimettere tutto a posto.")]
        [SerializeField] private float restartDelay = 1.5f;

        public GameState State { get; private set; } = GameState.Playing;

        /// <summary>I giocatori possono muoversi solo mentre si gioca davvero.</summary>
        public bool PlayersCanMove => State == GameState.Playing;

        public event Action<GameState> StateChanged;

        private float restartAt;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            if (balance != null) balance.CakeLost += HandleCakeLost;
            if (timer != null) timer.TimeUp += HandleTimeUp;
            if (deliveryPoint != null) deliveryPoint.Delivered += HandleDelivered;
        }

        private void OnDisable()
        {
            if (balance != null) balance.CakeLost -= HandleCakeLost;
            if (timer != null) timer.TimeUp -= HandleTimeUp;
            if (deliveryPoint != null) deliveryPoint.Delivered -= HandleDelivered;
        }

        private void Update()
        {
            if (State == GameState.Falling && Time.time >= restartAt)
                RestartFloor();
        }

        private void HandleCakeLost()
        {
            if (State != GameState.Playing) return;

            SetState(GameState.Falling);
            restartAt = Time.time + restartDelay;
        }

        private void HandleTimeUp()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Lost);
        }

        private void HandleDelivered()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Won);
        }

        /// <summary>
        /// Rimette camerieri e torta al punto di partenza del piano corrente.
        /// In Fase 1 il "piano" è tutta la scena; in Fase 2 diventerà il piano vero.
        /// </summary>
        public void RestartFloor()
        {
            if (players != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i] == null) continue;

                    Transform spawn = (playerSpawns != null && i < playerSpawns.Length) ? playerSpawns[i] : null;
                    if (spawn != null) players[i].Teleport(spawn.position, spawn.rotation);
                }
            }

            if (fallHandler != null) fallHandler.Restore();
            if (carrier != null) carrier.SnapToPlayers();
            if (balance != null) balance.ResetBalance();

            SetState(GameState.Playing);
        }

        private void SetState(GameState next)
        {
            if (State == next) return;

            State = next;
            if (timer != null) timer.Running = State == GameState.Playing;

            StateChanged?.Invoke(State);
        }
    }
}
