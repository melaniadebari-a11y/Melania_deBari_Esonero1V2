using UnityEngine;
using UnityEngine.InputSystem;

namespace Gaffeurs
{
    /// <summary>
    /// Legge la action "Move" da una action map dell'asset GaffeursControls.
    /// Giocatore 1 usa la map "Player1" (WASD), giocatore 2 la map "Player2" (frecce).
    /// Per passare ai joystick del cabinato basta aggiungere i binding dentro l'asset:
    /// questo script non va toccato.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour, IMoveInput
    {
        [SerializeField] private InputActionAsset actions;

        [Tooltip("Nome della action map da usare: Player1 oppure Player2.")]
        [SerializeField] private string actionMapName = "Player1";

        [SerializeField] private string moveActionName = "Move";

        private InputAction moveAction;

        private void Awake()
        {
            if (actions == null)
            {
                Debug.LogError($"{name}: asset GaffeursControls non assegnato.", this);
                return;
            }

            InputActionMap map = actions.FindActionMap(actionMapName, false);
            if (map == null)
            {
                Debug.LogError($"{name}: action map '{actionMapName}' non trovata nell'asset.", this);
                return;
            }

            moveAction = map.FindAction(moveActionName, false);
            if (moveAction == null)
                Debug.LogError($"{name}: action '{moveActionName}' non trovata nella map '{actionMapName}'.", this);
        }

        private void OnEnable()
        {
            if (moveAction != null) moveAction.Enable();
        }

        private void OnDisable()
        {
            if (moveAction != null) moveAction.Disable();
        }

        public Vector2 ReadMove()
        {
            return moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        }
    }
}
