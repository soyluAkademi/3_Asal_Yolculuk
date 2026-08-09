using PayeGames.AsalYolculuk.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PayeGames.AsalYolculuk.UI
{
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

        private void Awake()
        {
            sesBtn.onClick.AddListener(OnSesBtnClick);
            musicBtn.onClick.AddListener(OnMusicBtnClick);
        }

        private void Start()
        {
            RefreshSprites();
        }

        private void OnSesBtnClick()
        {
            SoundManager.Instance.ToggleSfx();
            RefreshSprites();
        }

        private void OnMusicBtnClick()
        {
            SoundManager.Instance.ToggleMusic();
            RefreshSprites();
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
}
