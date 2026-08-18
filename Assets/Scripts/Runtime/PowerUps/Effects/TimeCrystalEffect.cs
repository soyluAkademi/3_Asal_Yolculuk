using PayeGames.AsalYolculuk.Meteors;

namespace PayeGames.AsalYolculuk.PowerUps.Effects
{
    /// <summary>
    /// Zaman Kristali (ödül): süresi boyunca tüm meteorların hareket hızını
    /// <see cref="MineDataSO.Multiplier"/> katına düşürür (0.5). Oyuncuya sayının
    /// çarpanlarını düşünmek için zaman kazandırır — oyunun eğitsel amacına
    /// doğrudan hizmet eden ödül budur.
    /// Ekranda uçmakta olan meteorlar da anında yavaşlar.
    /// </summary>
    public class TimeCrystalEffect : IActiveEffect
    {
        private readonly MineDataSO data;

        public TimeCrystalEffect(MineDataSO data)
        {
            this.data = data;
        }

        public EffectType Type => data.EffectType;
        public float Duration => data.Duration;

        public void ApplyEffect()
        {
            MeteorController.SetSpeedMultiplier(data.Multiplier);
        }

        public void TickEffect(float deltaTime)
        {
        }

        public void RemoveEffect()
        {
            MeteorController.ResetSpeedMultiplier();
        }
    }
}
