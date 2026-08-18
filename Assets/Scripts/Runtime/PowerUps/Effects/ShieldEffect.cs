using PayeGames.AsalYolculuk.Player;

namespace PayeGames.AsalYolculuk.PowerUps.Effects
{
    /// <summary>
    /// Koruma Kalkanı (ödül): süresi boyunca gemi etrafında bariyer açılır ve
    /// meteor çarpmaları can eksiltmez. Çarpan meteor yine patlar; puan verilmez,
    /// yoksa oyuncu ateş etmeyi bırakıp meteorların üzerine sürerek puan toplardı.
    /// </summary>
    public class ShieldEffect : IActiveEffect
    {
        private readonly MineDataSO data;
        private readonly ShipCollisionHandler collisionHandler;

        public ShieldEffect(MineDataSO data, ShipCollisionHandler collisionHandler)
        {
            this.data = data;
            this.collisionHandler = collisionHandler;
        }

        public EffectType Type => data.EffectType;
        public float Duration => data.Duration;

        public void ApplyEffect()
        {
            if (collisionHandler == null) return;

            collisionHandler.SetShieldActive(true);
        }

        public void TickEffect(float deltaTime)
        {
        }

        public void RemoveEffect()
        {
            if (collisionHandler == null) return;

            collisionHandler.SetShieldActive(false);
        }
    }
}
