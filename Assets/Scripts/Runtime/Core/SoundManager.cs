using UnityEngine;

namespace PayeGames.AsalYolculuk.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public const string MusicVolumePrefKey  = "music_volume";
        public const string EffectVolumePrefKey = "effect_volume";
        public const string BulletVolumePrefKey = "bullet_volume";

        public static SoundManager Instance { get; private set; }

        [Header("SFX Sources")]
        [Tooltip("Oynanış olaylarının (maden toplama, patlama vb.) tek seferlik seslerini çalan ortak kaynak")]
        [SerializeField] private AudioSource gameplaySource;

        private AudioSource source;

        /// <summary>0-1 aralığındaki intro/arka plan müziği seviyesi.</summary>
        public float MusicVolume { get; private set; } = 1f;

        /// <summary>0-1 aralığındaki efekt seviyesi; intro ve mermi dışındaki sesleri kapsar.</summary>
        public float EffectVolume { get; private set; } = 1f;

        /// <summary>0-1 aralığındaki mermi ateş sesi seviyesi.</summary>
        public float BulletVolume { get; private set; } = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Fazladan kopya Play On Awake ile çalmaya başlamış olabilir; yok
                // edilmesi kare sonunda gerçekleştiği için önce susturulur, yoksa
                // ayarlanmış seviyeyi hiçe sayan bir ses sızar.
                if (TryGetComponent(out AudioSource duplicateSource))
                    duplicateSource.Stop();

                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            source = GetComponent<AudioSource>();
            source.loop = true;

            MusicVolume  = ReadStoredVolume(SoundChannel.Music);
            EffectVolume = ReadStoredVolume(SoundChannel.Effect);
            BulletVolume = ReadStoredVolume(SoundChannel.Bullet);
            source.volume = MusicVolume;

            PlayMusic();

            EnsureGameplaySource();
        }

        public float GetVolume(SoundChannel channel) => channel switch
        {
            SoundChannel.Music  => MusicVolume,
            SoundChannel.Effect => EffectVolume,
            SoundChannel.Bullet => BulletVolume,
            _ => 1f
        };

        /// <summary>
        /// Kanalın seviyesini günceller. Slider sürükleme sırasında her karede
        /// çağrıldığı için disk yazımı yapmaz; kalıcılık için sürükleme bitiminde
        /// <see cref="SaveVolume"/> çağrılır.
        /// </summary>
        public void SetVolume(SoundChannel channel, float volume)
        {
            float clamped = Mathf.Clamp01(volume);

            switch (channel)
            {
                case SoundChannel.Music:
                    MusicVolume = clamped;

                    if (source != null)
                        source.volume = MusicVolume;
                    break;

                case SoundChannel.Effect:
                    EffectVolume = clamped;
                    break;

                case SoundChannel.Bullet:
                    BulletVolume = clamped;
                    break;
            }
        }

        public void SaveVolume(SoundChannel channel)
        {
            PlayerPrefs.SetFloat(GetVolumePrefKey(channel), GetVolume(channel));
            PlayerPrefs.Save();
        }

        public static string GetVolumePrefKey(SoundChannel channel) => channel switch
        {
            SoundChannel.Music  => MusicVolumePrefKey,
            SoundChannel.Effect => EffectVolumePrefKey,
            SoundChannel.Bullet => BulletVolumePrefKey,
            _ => MusicVolumePrefKey
        };

        /// <summary>
        /// Kanalın kayıtlı seviyesi. SoundManager henüz sahnede yoksa doğrudan
        /// PlayerPrefs'ten okunur.
        /// </summary>
        public static float ReadStoredVolume(SoundChannel channel)
        {
            if (Instance != null)
                return Instance.GetVolume(channel);

            return Mathf.Clamp01(PlayerPrefs.GetFloat(GetVolumePrefKey(channel), 1f));
        }

        /// <summary>
        /// Müziği başlatır. Akış hiç durdurulmaz; sessizlik artık volume = 0
        /// demektir, böylece slider sıfırdan geri açıldığında parça kaldığı
        /// yerden duyulur.
        /// </summary>
        private void PlayMusic()
        {
            if (source == null || source.clip == null) return;
            if (source.isPlaying) return;

            source.Play();
        }

        /// <summary>
        /// Oynanış olaylarının tek seferlik sesini çalar (maden toplama, patlama,
        /// can kaybı vb.). Klip çağrı bazlı geldiği için ortak bir kaynak kullanılır;
        /// olay başına ayrı AudioSource tutmak sahneyi gereksiz şişirirdi.
        /// </summary>
        /// <param name="volumeScale">Klibe özel ölçek; kanal seviyesiyle çarpılır.</param>
        /// <param name="channel">Sesin bağlı olduğu ayar kanalı.</param>
        public void PlaySfx(AudioClip clip, float volumeScale = 1f, SoundChannel channel = SoundChannel.Effect)
        {
            if (clip == null) return;
            if (gameplaySource == null) return;

            // Ses verisi belleğe alınmamış bir klipte PlayOneShot sessizce hiçbir şey
            // yapar. Doğru çözüm klibin import ayarında Preload Audio Data'yı açmaktır;
            // buradaki yükleme, ayarı unutulmuş yeni kliplere karşı emniyet kemeridir.
            if (clip.loadState == AudioDataLoadState.Unloaded)
            {
                clip.LoadAudioData();
            }

            gameplaySource.PlayOneShot(clip, volumeScale * GetVolume(channel));
        }

        /// <summary>
        /// Oynanış sesleri için müzikten ayrı bir kaynak garantiler. Kaynak müzik
        /// AudioSource'u ile aynı olursa müzik seviyesi efektleri de kısar; bu yüzden
        /// atanmamış ya da yanlışlıkla müzik kaynağına bağlanmış olması durumunda
        /// runtime'da ayrı bir AudioSource eklenir.
        /// </summary>
        private void EnsureGameplaySource()
        {
            if (gameplaySource != null && gameplaySource != source) return;

            if (gameplaySource == source)
            {
                Debug.LogWarning(
                    "[SoundManager] Gameplay Source müzik kaynağına atanmış; " +
                    "efektler için ayrı bir AudioSource oluşturuldu.", this);
            }

            gameplaySource = gameObject.AddComponent<AudioSource>();
            gameplaySource.playOnAwake = false;
            gameplaySource.loop = false;
            gameplaySource.volume = 1f;
        }

        /// <summary>
        /// Kendi AudioSource'u üzerinden çalan sesleri (UI tıkları, meteor patlaması,
        /// gemi hasarı) efekt seviyesine saygı duyarak çalar. SoundManager henüz
        /// sahnede yoksa PlayerPrefs'teki son ayara düşer.
        /// </summary>
        public static void PlaySfxOnSource(AudioSource src, float volumeScale = 1f)
        {
            if (src == null || src.clip == null) return;

            src.PlayOneShot(src.clip, volumeScale * ReadStoredVolume(SoundChannel.Effect));
        }

        /// <summary>UI butonlarının tık sesi için okunabilir kısayol.</summary>
        public static void PlayUiSfx(AudioSource src)
        {
            PlaySfxOnSource(src);
        }
    }
}
