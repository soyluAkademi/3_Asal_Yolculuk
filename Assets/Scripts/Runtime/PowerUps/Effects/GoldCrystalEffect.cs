using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.PowerUps.Effects
{
    /// <summary>
    /// Altın Kristal (ödül): süresi boyunca tamamen yok edilen her meteor 5 yerine
    /// <see cref="MineDataSO.Amount"/> (10) puan kazandırır. Kendisi puan vermez —
    /// kazanç oyuncunun o pencerede kaç meteor bitirdiğine bağlıdır.
    /// </summary>
    public class GoldCrystalEffect : IActiveEffect
    {
        private readonly MineDataSO data;

        public GoldCrystalEffect(MineDataSO data)
        {
            this.data = data;
        }

        public EffectType Type => data.EffectType;
        public float Duration => data.Duration;

        public void ApplyEffect()
        {
            if (ScoreManager.Instance == null) return;

            ScoreManager.Instance.SetMeteorScoreOverride(data.Amount);
        }

        public void TickEffect(float deltaTime)
        {
        }

        public void RemoveEffect()
        {
            if (ScoreManager.Instance == null) return;

            ScoreManager.Instance.ClearMeteorScoreOverride();
        }
    }
}
