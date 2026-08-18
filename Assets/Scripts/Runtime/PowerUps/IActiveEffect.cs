namespace PayeGames.AsalYolculuk.PowerUps
{
    /// <summary>
    /// Toplanan bir madenin oyuna uyguladığı etki. Süreli etkiler
    /// <see cref="Duration"/> saniye boyunca <see cref="TickEffect"/> ile beslenir,
    /// süre dolunca <see cref="RemoveEffect"/> ile geri alınır.
    /// Anlık etkiler (ör. Yıldız Parçası) <see cref="Duration"/> olarak 0 döner;
    /// <see cref="PowerUpManager"/> bunları listeye hiç almaz.
    /// </summary>
    public interface IActiveEffect
    {
        EffectType Type { get; }

        /// <summary>Etkinin saniye cinsinden süresi. 0 = anlık etki.</summary>
        float Duration { get; }

        void ApplyEffect();
        void TickEffect(float deltaTime);
        void RemoveEffect();
    }
}
