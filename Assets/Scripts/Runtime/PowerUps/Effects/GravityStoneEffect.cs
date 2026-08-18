using PayeGames.AsalYolculuk.Player;

namespace PayeGames.AsalYolculuk.PowerUps.Effects
{
    /// <summary>
    /// Yerçekimi Taşı (ceza): süresi boyunca geminin hareket hızını
    /// <see cref="MineDataSO.Multiplier"/> katına düşürür (0.5) ve gemiyi
    /// <see cref="MineDataSO.Amount"/> birim/saniye aşağı çeker.
    ///
    /// Gemi ekrandan çıkamaz — <see cref="PlayerShipController"/> konumu her karede
    /// ekrana sınırlar; çekim gemiyi en fazla alt kenara yapıştırır. Çekim ayrıca
    /// geminin güncel hızına göre sınırlanır, böylece oyuncu her koşulda yukarı
    /// çıkabilir; aksi halde ceza değil felç olurdu.
    /// </summary>
    public class GravityStoneEffect : IActiveEffect
    {
        private readonly MineDataSO data;
        private readonly PlayerShipController shipController;

        public GravityStoneEffect(MineDataSO data, PlayerShipController shipController)
        {
            this.data = data;
            this.shipController = shipController;
        }

        public EffectType Type => data.EffectType;
        public float Duration => data.Duration;

        public void ApplyEffect()
        {
            if (shipController == null) return;

            shipController.SetMoveSpeedMultiplier(data.Multiplier);
            shipController.SetDownwardPull(data.Amount);
        }

        public void TickEffect(float deltaTime)
        {
        }

        public void RemoveEffect()
        {
            if (shipController == null) return;

            shipController.ResetMoveSpeedMultiplier();
            shipController.ResetDownwardPull();
        }
    }
}
