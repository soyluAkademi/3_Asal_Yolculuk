namespace PayeGames.AsalYolculuk.Core
{
    /// <summary>
    /// Ayarlar ekranındaki ayrı ayrı kısılabilen ses kanalları.
    /// </summary>
    public enum SoundChannel
    {
        /// <summary>Intro / arka plan müziği.</summary>
        Music,

        /// <summary>Mermi dışındaki tüm efektler: patlama, maden, UI tıkları.</summary>
        Effect,

        /// <summary>Geminin ateş sesi; saniyede birkaç kez tekrarlandığı için ayrı kısılır.</summary>
        Bullet
    }
}
