using UnityEngine;

namespace PayeGames.AsalYolculuk.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public const string MusicPrefKey = "music_on";
        public const string SfxPrefKey   = "sfx_on";

        public static SoundManager Instance { get; private set; }

        [Header("SFX Sources")]
        [SerializeField] private AudioSource dogruSource;
        [SerializeField] private AudioSource yanlisSource;
        [Tooltip("Oynanış olaylarının (maden toplama, patlama vb.) tek seferlik seslerini çalan ortak kaynak")]
        [SerializeField] private AudioSource gameplaySource;

        private AudioSource source;

        public bool MusicOn { get; private set; } = true;
        public bool SfxOn   { get; private set; } = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;

            MusicOn = PlayerPrefs.GetInt(MusicPrefKey, 1) == 1;
            SfxOn   = PlayerPrefs.GetInt(SfxPrefKey,   1) == 1;

            source.Stop(); // Play On Awake'i iptal et, ApplyMusicState karar versin
            ApplyMusicState();

            EnsureGameplaySource();
        }

        private void Start()
        {
            ApplyMusicState();
        }

        public void ToggleMusic()
        {
            SetMusic(!MusicOn);
        }

        public void SetMusic(bool on)
        {
            MusicOn = on;
            PlayerPrefs.SetInt(MusicPrefKey, on ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMusicState();
        }

        public void ToggleSfx()
        {
            SetSfx(!SfxOn);
        }

        public void SetSfx(bool on)
        {
            SfxOn = on;
            PlayerPrefs.SetInt(SfxPrefKey, on ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ApplyMusicState()
        {
            if (source == null) return;

            if (MusicOn)
            {
                if (source.clip != null && !source.isPlaying)
                {
                    if (source.time > 0f)
                        source.UnPause();
                    else
                        source.Play();
                }
            }
            else
            {
                source.Pause();
            }
        }

        public void DogruSesiCikar()
        {
            PlaySfxSource(dogruSource);
        }

        public void YanlisSesiCikar()
        {
            PlaySfxSource(yanlisSource);
        }

        /// <summary>
        /// Oynanış olaylarının tek seferlik sesini çalar (maden toplama, patlama,
        /// can kaybı vb.). Klip çağrı bazlı geldiği için ortak bir kaynak kullanılır;
        /// olay başına ayrı AudioSource tutmak sahneyi gereksiz şişirirdi.
        /// </summary>
        /// <param name="volumeScale">Sık tekrarlanan sesleri (ör. mermi) kısmak için ölçek.</param>
        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (!SfxOn) return;
            if (clip == null) return;
            if (gameplaySource == null) return;

            // Ses verisi belleğe alınmamış bir klipte PlayOneShot sessizce hiçbir şey
            // yapar. Doğru çözüm klibin import ayarında Preload Audio Data'yı açmaktır;
            // buradaki yükleme, ayarı unutulmuş yeni kliplere karşı emniyet kemeridir.
            if (clip.loadState == AudioDataLoadState.Unloaded)
            {
                clip.LoadAudioData();
            }

            gameplaySource.PlayOneShot(clip, volumeScale);
        }

        /// <summary>
        /// Oynanış sesleri için ayrı bir kaynak garantiler. Inspector'da atanmadıysa
        /// runtime'da eklenir; müzik kaynağı loop'lu olduğu için PlayOneShot'ı
        /// oraya yönlendirmek ses ayarlarını birbirine karıştırırdı.
        /// </summary>
        private void EnsureGameplaySource()
        {
            if (gameplaySource != null) return;

            gameplaySource = gameObject.AddComponent<AudioSource>();
            gameplaySource.playOnAwake = false;
            gameplaySource.loop = false;
        }

        private void PlaySfxSource(AudioSource src)
        {
            if (!SfxOn) return;
            if (src == null || src.clip == null) return;
            src.PlayOneShot(src.clip);
        }

        /// <summary>
        /// UI elemanlarının üzerindeki AudioSource'u SFX ayarına saygı duyarak çalar.
        /// SoundManager henüz sahnede yoksa (ör. doğrudan MainMenu'den başlatma)
        /// PlayerPrefs'teki son ayara düşer.
        /// </summary>
        public static void PlayUiSfx(AudioSource src)
        {
            if (src == null || src.clip == null) return;

            bool sfxOn = Instance != null
                ? Instance.SfxOn
                : PlayerPrefs.GetInt(SfxPrefKey, 1) == 1;

            if (!sfxOn) return;

            src.PlayOneShot(src.clip);
        }
    }
}
