using UnityEngine;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Meteors;

namespace PayeGames.AsalYolculuk.Player
{
    /// <summary>
    /// Koruma Kalkanı bariyerinin çarpışma tarafı. Gemi altında ayrı bir obje olarak
    /// durur ve yalnızca kalkan aktifken açıktır — bu yüzden ayrıca "kalkan açık mı"
    /// kontrolü yapmaz, objenin kendisi zaten kalkanın anahtarıdır.
    ///
    /// Bariyerin yarıçapı gemininkinden büyük olduğu için meteor buraya önce değer,
    /// gemi collider'ına ulaşamadan patlar. Gemi collider'ı bilinçli olarak açık
    /// bırakılır: <see cref="MineCollector"/> onu kullanıyor, kapatılsaydı kalkanlıyken
    /// maden toplanamazdı.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShieldBarrier : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!GameManager.IsGameplayActive) return;
            if (!other.TryGetComponent(out MeteorController meteor)) return;

            // Meteor bariyere çarpıp parçalanır: puan verilmez, can eksilmez.
            meteor.DespawnOnShipHit();
        }
    }
}
