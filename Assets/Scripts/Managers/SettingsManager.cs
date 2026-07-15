using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Ses (SFX) Butonu")]
    [SerializeField] private Button sesBtn;
    [SerializeField] private Image  sesBtnImage;
    [SerializeField] private Sprite sesOnSprite;
    [SerializeField] private Sprite sesOffSprite;

    [Header("Müzik Butonu")]
    [SerializeField] private Button musicBtn;
    [SerializeField] private Image  musicBtnImage;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    private AudioSource _sesBtnAudio;
    private AudioSource _musicBtnAudio;

    private void Awake()
    {
        _sesBtnAudio   = sesBtn.GetComponent<AudioSource>();
        _musicBtnAudio = musicBtn.GetComponent<AudioSource>();

        sesBtn.onClick.AddListener(OnSesBtnClick);
        musicBtn.onClick.AddListener(OnMusicBtnClick);
    }

    private void Start()
    {
        RefreshSprites();
    }

    private void OnSesBtnClick()
    {
        bool wasOn = SoundManager.Instance != null && SoundManager.Instance.SfxOn;
        SoundManager.Instance.ToggleSfx();
        if (wasOn) PlayClick(_sesBtnAudio);
        RefreshSprites();
    }

    private void OnMusicBtnClick()
    {
        bool sfxOn = SoundManager.Instance != null && SoundManager.Instance.SfxOn;
        if (sfxOn) PlayClick(_musicBtnAudio);
        SoundManager.Instance.ToggleMusic();
        RefreshSprites();
    }

    private void PlayClick(AudioSource src)
    {
        if (src == null || src.clip == null) return;
        src.PlayOneShot(src.clip);
    }

    private void RefreshSprites()
    {
        bool sfxOn   = SoundManager.Instance != null
            ? SoundManager.Instance.SfxOn
            : PlayerPrefs.GetInt(SoundManager.SfxPrefKey, 1) == 1;

        bool musicOn = SoundManager.Instance != null
            ? SoundManager.Instance.MusicOn
            : PlayerPrefs.GetInt(SoundManager.MusicPrefKey, 1) == 1;

        sesBtnImage.sprite   = sfxOn   ? sesOnSprite   : sesOffSprite;
        musicBtnImage.sprite = musicOn ? musicOnSprite : musicOffSprite;
    }
}
