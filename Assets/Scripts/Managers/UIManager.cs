using UnityEngine;
using UnityEngine.UI;
using MaskTransitions;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button homeBtn;

    private void Start()
    {
        if (homeBtn != null)
            homeBtn.onClick.AddListener(OnHomeBtnClicked);
    }

    private void OnHomeBtnClicked()
    {
        if (SoundManager.Instance != null && SoundManager.Instance.SfxOn)
        {
            AudioSource src = homeBtn.GetComponent<AudioSource>();
            if (src != null && src.clip != null) src.PlayOneShot(src.clip);
        }
        TransitionManager.Instance.LoadLevel("MainMenu");
    }
}
