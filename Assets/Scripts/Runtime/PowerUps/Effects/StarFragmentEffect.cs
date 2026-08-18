using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.PowerUps.Effects
{
    /// <summary>
    /// Yıldız Parçası (ödül): toplandığı anda bölüm puanına
    /// <see cref="MineDataSO.Amount"/> kadar puan ekler. Süresi yoktur.
    /// </summary>
    public class StarFragmentEffect : IActiveEffect
    {
        private readonly MineDataSO data;

        public StarFragmentEffect(MineDataSO data)
        {
            this.data = data;
        }

        public EffectType Type => data.EffectType;

        // Anlık etki: veri dosyasına yanlışlıkla süre girilse bile listeye alınmaz.
        public float Duration => 0f;

        public void ApplyEffect()
        {
            if (ScoreManager.Instance == null) return;

            ScoreManager.Instance.AddScore(data.Amount);
        }

        public void TickEffect(float deltaTime)
        {
        }

        public void RemoveEffect()
        {
        }
    }
}
