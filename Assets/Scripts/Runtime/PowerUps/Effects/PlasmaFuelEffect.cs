using PayeGames.AsalYolculuk.Combat;

namespace PayeGames.AsalYolculuk.PowerUps.Effects
{
    /// <summary>
    /// Plazma Yakıtı (ödül): süresi boyunca gemi tek yerine
    /// <see cref="MineDataSO.Amount"/> noktadan (3) ateş eder. Ateş hızı değişmez;
    /// aynı anda daha geniş bir alan taranır. Puanı doğrudan artırmaz.
    /// </summary>
    public class PlasmaFuelEffect : IActiveEffect
    {
        private readonly MineDataSO data;
        private readonly PlayerAutoShooter shooter;

        public PlasmaFuelEffect(MineDataSO data, PlayerAutoShooter shooter)
        {
            this.data = data;
            this.shooter = shooter;
        }

        public EffectType Type => data.EffectType;
        public float Duration => data.Duration;

        public void ApplyEffect()
        {
            if (shooter == null) return;

            shooter.SetActiveFirePointCount(data.Amount);
        }

        public void TickEffect(float deltaTime)
        {
        }

        public void RemoveEffect()
        {
            if (shooter == null) return;

            shooter.ResetFirePointCount();
        }
    }
}
