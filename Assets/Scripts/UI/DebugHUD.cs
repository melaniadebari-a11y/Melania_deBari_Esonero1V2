using UnityEngine;
using UnityEngine.InputSystem;

namespace Gaffeurs
{
    /// <summary>
    /// HUD provvisorio della Fase 1, disegnato con OnGUI: zero setup, serve solo a
    /// leggere i numeri mentre si tarano i parametri. In Fase 2 lo sostituisce
    /// l'HUD vero (barra curva, floor counter, timer circolare).
    /// </summary>
    public class DebugHUD : MonoBehaviour
    {
        [SerializeField] private BalanceSystem balance;
        [SerializeField] private LevelTimer timer;
        [SerializeField] private CakeCarrier carrier;

        private GUIStyle style;

        private void Update()
        {
            // R rimette tutto al punto di partenza: comodissimo mentre si tara.
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame && GameManager.Instance != null)
                GameManager.Instance.RestartFloor();
        }

        private void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold
                };
                style.normal.textColor = Color.white;
            }

            const float x = 24f;
            const float y = 24f;
            const float w = 360f;
            const float h = 28f;

            float value = balance != null ? balance.Balance01 : 0f;

            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

            GUI.color = Color.Lerp(new Color(0.35f, 0.8f, 0.4f), new Color(0.92f, 0.25f, 0.25f), value);
            GUI.DrawTexture(new Rect(x + 2f, y + 2f, (w - 4f) * value, h - 4f), Texture2D.whiteTexture);

            GUI.color = Color.white;

            float line = y + h + 8f;
            GUI.Label(new Rect(x, line, 700f, 24f), $"EQUILIBRIO  {value:P0}", style);

            line += 24f;
            if (timer != null) GUI.Label(new Rect(x, line, 700f, 24f), $"TEMPO  {timer.Remaining:0.0} s", style);

            line += 24f;
            if (carrier != null)
                GUI.Label(new Rect(x, line, 700f, 24f),
                    $"DISTANZA  {carrier.CurrentDistance:0.00} m   ·   TENSIONE  {carrier.StretchAmount01:P0}", style);

            line += 24f;
            string state = GameManager.Instance != null ? GameManager.Instance.State.ToString() : "-";
            GUI.Label(new Rect(x, line, 700f, 24f), $"STATO  {state}", style);

            line += 32f;
            GUI.Label(new Rect(x, line, 900f, 24f), "P1: WASD    ·    P2: frecce    ·    R: restart", style);
        }
    }
}
