using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public const string MusicPrefKey = "music_on";
    public const string SfxPrefKey   = "sfx_on";

    public static SoundManager Instance { get; private set; }

    [Header("SFX Sources")]
    [SerializeField] private AudioSource dogruSource;
    [SerializeField] private AudioSource yanlisSource;

    private AudioSource _source;

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

        _source = GetComponent<AudioSource>();
        _source.loop = true;
        _source.playOnAwake = false;

        MusicOn = PlayerPrefs.GetInt(MusicPrefKey, 1) == 1;
        SfxOn   = PlayerPrefs.GetInt(SfxPrefKey,   1) == 1;

        _source.Stop(); // Play On Awake'i iptal et, ApplyMusicState karar versin
        ApplyMusicState();
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
        if (_source == null) return;

        if (MusicOn)
        {
            if (_source.clip != null && !_source.isPlaying)
            {
                if (_source.time > 0f)
                    _source.UnPause();
                else
                    _source.Play();
            }
        }
        else
        {
            _source.Pause();
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

    private void PlaySfxSource(AudioSource src)
    {
        if (!SfxOn) return;
        if (src == null || src.clip == null) return;
        src.PlayOneShot(src.clip);
    }
}
