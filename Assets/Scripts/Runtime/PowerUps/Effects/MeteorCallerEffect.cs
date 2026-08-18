using PayeGames.AsalYolculuk.Meteors;

namespace PayeGames.AsalYolculuk.PowerUps.Effects
{
    /// <summary>
    /// Meteor Çağırıcı (ceza): süresi boyunca meteor üretim sıklığını
    /// <see cref="MineDataSO.Multiplier"/> katına çıkarır (×2). Çarpan gezegenin
    /// kendi üretim hızının üzerine bindiği için ceza ileri gezegenlerde
    /// otomatik olarak daha sert hissedilir.
    /// </summary>
    public class MeteorCallerEffect : IActiveEffect
    {
        private readonly MineDataSO data;
        private readonly MeteorSpawner spawner;

        public MeteorCallerEffect(MineDataSO data, MeteorSpawner spawner)
        {
            this.data = data;
            this.spawner = spawner;
        }

        public EffectType Type => data.EffectType;
        public float Duration => data.Duration;

        public void ApplyEffect()
        {
            if (spawner == null) return;

            spawner.SetSpawnRateMultiplier(data.Multiplier);
        }

        public void TickEffect(float deltaTime)
        {
        }

        public void RemoveEffect()
        {
            if (spawner == null) return;

            spawner.ResetSpawnRateMultiplier();
        }
    }
}
