namespace PayeGames.AsalYolculuk.PowerUps
{
    /// <summary>
    /// Oyundaki tüm maden türleri. Ödül/ceza ayrımı bilinçli olarak enum sırasına
    /// değil, <see cref="EffectTypeExtensions.IsReward"/> içindeki açık listeye
    /// dayanır; sıra değişince ayrım sessizce bozulmasın.
    /// </summary>
    public enum EffectType
    {
        // Ödüller
        StarFragment,
        GoldCrystal,
        TimeCrystal,
        PlasmaFuel,
        Shield,

        // Cezalar
        ChaosCore,
        GravityStone,
        MeteorCaller,
    }

    public static class EffectTypeExtensions
    {
        /// <summary>
        /// Madenin ödül mü ceza mı olduğunu belirler. Tek doğruluk kaynağı burasıdır;
        /// veri dosyasında ayrı bir bayrak tutulmaz ki yanlış işaretlenemesin.
        /// </summary>
        public static bool IsReward(this EffectType type)
        {
            return type switch
            {
                EffectType.ChaosCore => false,
                EffectType.GravityStone => false,
                EffectType.MeteorCaller => false,
                _ => true,
            };
        }
    }
}
