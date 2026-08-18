using UnityEngine;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.PowerUps;

namespace PayeGames.AsalYolculuk.Player
{
    /// <summary>
    /// Geminin üzerinden geçen madeni toplar. Ayrı bir bileşendir çünkü
    /// <see cref="ShipCollisionHandler"/>'ın sorumluluğu hasar almaktır; maden
    /// toplama dokunulmazlık penceresinden ve can mantığından tamamen bağımsız
    /// çalışmalıdır (kalkanlıyken de maden toplanabilir).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MineCollector : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!GameManager.IsGameplayActive) return;
            if (!other.TryGetComponent(out MineController mine)) return;

            mine.Collect();
        }
    }
}
