using UnityEngine;

namespace Gaffeurs
{
    /// <summary>
    /// Sorgente di movimento di un giocatore. Oggi è la tastiera, domani saranno
    /// i joystick del cabinato: il PlayerMotor parla solo con questa interfaccia,
    /// quindi cambiare controllo non significa riscrivere il movimento.
    /// </summary>
    public interface IMoveInput
    {
        /// <summary>Direzione richiesta dal giocatore, in coordinate schermo. Modulo massimo 1.</summary>
        Vector2 ReadMove();
    }
}
