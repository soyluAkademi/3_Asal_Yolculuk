using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button baslaBtn;
    [SerializeField] private Button ayarlarBtn;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private RectTransform settingsImg;
    [SerializeField] private RectTransform baslikImg;
    [SerializeField] private Button settingsExitBtn;
    [SerializeField] private RectTransform backgroundPanel;
    [SerializeField] private RectTransform bolumSecPanel;
    [SerializeField] private RectTransform soruHaritasiPanel;
    [SerializeField] private Button[] bolumBtns;
    [SerializeField] private string oynaBtnAdi = "oynaBtn"; // kart içindeki Oyna butonunun obje adı
    [SerializeField] private string cerceveImgAdi = "cerceveImg"; // kart içindeki seçim çerçevesinin obje adı
    [SerializeField] private Button bolumSecGeriBtn;
    [SerializeField] private Button bolumSecIleriBtn;
    [SerializeField] private Button soruHaritasiGeriBtn;
    [SerializeField] private SoruHaritasi soruHaritasi;

    private AudioSource _baslaBtnAudio;
    private AudioSource _ayarlarBtnAudio;
    private AudioSource _settingsExitBtnAudio;
    private AudioSource _bolumSecGeriBtnAudio;
    private AudioSource _bolumSecIleriBtnAudio;
    private AudioSource _soruHaritasiGeriBtnAudio;

    private float _settingsImgRestY;
    private float _baslikImgRestY;
    private CanvasGroup _settingsImgCg;
    private CanvasGroup _baslikImgCg;

    private RectTransform _bolumSecIleriBtnRect;
    private float         _bolumSecIleriBtnRestX;
    private Tweener       _bolumSecIleriBtnTween;

    [SerializeField] private float AnimDuration = 0.35f;
    [SerializeField] private float ileriBtnDikkatCekOffsetX = 20f;   // git-gel mesafesi
    [SerializeField] private float ileriBtnDikkatCekSuresi  = 0.5f;  // tek yön süresi

    private const float SettingsImgStartY = 700f;
    private const float BaslikImgUpOffset = 700f;

    private void Start()
    {
        if (baslaBtn != null)
        {
            _baslaBtnAudio = baslaBtn.GetComponent<AudioSource>();
            baslaBtn.onClick.AddListener(OnBaslaBtnClicked);
        }

        if (ayarlarBtn != null)
        {
            _ayarlarBtnAudio = ayarlarBtn.GetComponent<AudioSource>();
            ayarlarBtn.onClick.AddListener(OnAyarlarBtnClicked);
        }

        if (settingsImg != null)
        {
            _settingsImgRestY = settingsImg.anchoredPosition.y;
            _settingsImgCg = settingsImg.GetComponent<CanvasGroup>();
        }

        if (baslikImg != null)
        {
            _baslikImgRestY = baslikImg.anchoredPosition.y;
            _baslikImgCg = baslikImg.GetComponent<CanvasGroup>();
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsExitBtn != null)
        {
            _settingsExitBtnAudio = settingsExitBtn.GetComponent<AudioSource>();
            settingsExitBtn.onClick.AddListener(OnSettingsExitBtnClicked);
        }

        if (bolumSecGeriBtn != null)
        {
            _bolumSecGeriBtnAudio = bolumSecGeriBtn.GetComponent<AudioSource>();
            bolumSecGeriBtn.onClick.AddListener(OnBolumSecGeriBtnClicked);
        }

        if (bolumSecIleriBtn != null)
        {
            _bolumSecIleriBtnAudio = bolumSecIleriBtn.GetComponent<AudioSource>();
            bolumSecIleriBtn.onClick.AddListener(OnBolumSecIleriBtnClicked);
            // Henüz bölüm seçilmediği için ileri pasif; ilk seçimde aktifleşir.
            bolumSecIleriBtn.interactable = false;

            _bolumSecIleriBtnRect = bolumSecIleriBtn.GetComponent<RectTransform>();
            if (_bolumSecIleriBtnRect != null)
                _bolumSecIleriBtnRestX = _bolumSecIleriBtnRect.anchoredPosition.x;
        }

        if (soruHaritasiGeriBtn != null)
        {
            _soruHaritasiGeriBtnAudio = soruHaritasiGeriBtn.GetComponent<AudioSource>();
            soruHaritasiGeriBtn.onClick.AddListener(OnSoruHaritasiGeriBtnClicked);
        }

        if (bolumBtns != null)
        {
            foreach (Button btn in bolumBtns)
            {
                if (btn == null) continue;
                AudioSource btnAudio = btn.GetComponent<AudioSource>();
                Button      captured = btn;
                btn.onClick.AddListener(() => OnBolumBtnClicked(captured, btnAudio));

                // Başta hiçbir kartın seçim çerçevesi görünmesin.
                Image cerceveImg = FindChildImage(captured.transform, cerceveImgAdi);
                if (cerceveImg != null) cerceveImg.gameObject.SetActive(false);

                // Kart içindeki Oyna butonunu adına göre bul, elle atama gerekmesin.
                Button oynaBtn = FindChildButton(captured.transform, oynaBtnAdi);
                if (oynaBtn != null)
                {
                    AudioSource oynaAudio = oynaBtn.GetComponent<AudioSource>();
                    oynaBtn.onClick.AddListener(() => OnOynaBtnClicked(captured, oynaAudio));
                }
            }
        }

        PlaceOffscreenRight(bolumSecPanel);
        PlaceOffscreenRight(soruHaritasiPanel);
    }

    // Stretch anchor'lı panelde anchoredPosition kaydırmak Left/Right/Top/Bottom'u
    // birlikte öteler; panel boyutu bozulmadan ekranın sağına taşınır.
    private void PlaceOffscreenRight(RectTransform panel)
    {
        if (panel == null) return;
        float canvasWidth = ((RectTransform)panel.parent).rect.width;
        panel.anchoredPosition = new Vector2(canvasWidth, 0f);
        panel.gameObject.SetActive(true);
    }

    private void OnBaslaBtnClicked()
    {
        PlayClick(_baslaBtnAudio);
        SwitchPanels(backgroundPanel, bolumSecPanel);
    }

    // Bölüm Seç sol ok: bölüm seç çıkar, ana menü (başlık) girer.
    private void OnBolumSecGeriBtnClicked()
    {
        PlayClick(_bolumSecGeriBtnAudio);
        SwitchPanels(bolumSecPanel, backgroundPanel);
    }

    // Bölüm Seç sağ ok: bölüm seç çıkar, soru haritası girer (ileri).
    private void OnBolumSecIleriBtnClicked()
    {
        PlayClick(_bolumSecIleriBtnAudio);
        SwitchPanels(bolumSecPanel, soruHaritasiPanel);
    }

    // Kart tıklaması: sadece seçim yap. Panel geçişini ileri butonu yapar.
    private void OnBolumBtnClicked(Button btn, AudioSource btnAudio)
    {
        PlayClick(btnAudio);
        SelectBolum(btn);
    }

    // Kart içindeki Oyna butonu: önce o bölümü seç, sonra kaldığın sorudan doğrudan oyuna gir.
    private void OnOynaBtnClicked(Button card, AudioSource oynaAudio)
    {
        PlayClick(oynaAudio);
        SelectBolum(card);
        if (soruHaritasi != null)
            soruHaritasi.DevamEt();   // SelectBolum içindeki Doldur bölümü aktif etti.
    }

    // Basılan kartın çerçevesini aç, diğerlerini kapat; soru haritasını doldur; ileriyi aktifle.
    private void SelectBolum(Button card)
    {
        if (bolumBtns != null)
        {
            foreach (Button other in bolumBtns)
            {
                if (other == null) continue;
                Image cerceveImg = FindChildImage(other.transform, cerceveImgAdi);
                if (cerceveImg != null) cerceveImg.gameObject.SetActive(other == card);
            }
        }

        // Kartın bolumId'sini kök BolumKartUI'dan al, soru haritası panelini doldur.
        BolumKartUI kart = card.GetComponentInParent<BolumKartUI>();
        if (kart != null && soruHaritasi != null)
            soruHaritasi.Doldur(kart.BolumId);

        // Bir bölüm seçildi; ileri butonu artık aktif. İlk aktifleşmede dikkat çekmesi
        // için git-gel animasyonunu başlat (tekrar seçimde tekrar tetiklenmesin).
        if (bolumSecIleriBtn != null && !bolumSecIleriBtn.interactable)
        {
            bolumSecIleriBtn.interactable = true;
            StartIleriBtnDikkatCek();
        }
    }

    private void StartIleriBtnDikkatCek()
    {
        if (_bolumSecIleriBtnRect == null) return;

        _bolumSecIleriBtnTween?.Kill();
        _bolumSecIleriBtnTween = _bolumSecIleriBtnRect
            .DOAnchorPosX(_bolumSecIleriBtnRestX + ileriBtnDikkatCekOffsetX, ileriBtnDikkatCekSuresi)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }


    // Kök altındaki ilk, adı 'ad' olan Button'ı döndürür (Oyna butonunu adına göre bulmak için).
    private static Button FindChildButton(Transform root, string ad)
    {
        Button[] all = root.GetComponentsInChildren<Button>(true);
        foreach (Button b in all)
            if (b.gameObject.name == ad) return b;
        return null;
    }

    // Kök altındaki ilk, adı 'ad' olan Image'ı döndürür (cerceveImg'i adına göre bulmak için).
    private static Image FindChildImage(Transform root, string ad)
    {
        Image[] all = root.GetComponentsInChildren<Image>(true);
        foreach (Image img in all)
            if (img.gameObject.name == ad) return img;
        return null;
    }

    private void OnSoruHaritasiGeriBtnClicked()
    {
        PlayClick(_soruHaritasiGeriBtnAudio);
        SwitchPanels(soruHaritasiPanel, bolumSecPanel);
    }

    // Çıkan panel sola kayar, ekran dışına çıkınca sağdaki bekleme noktasına ışınlanır.
    // Giren panel sağdan (0,0)'a gelir; böylece her geçiş hep sağdan sola akar.
    private void SwitchPanels(RectTransform outgoing, RectTransform incoming)
    {
        if (outgoing == null || incoming == null) return;

        float canvasWidth = ((RectTransform)outgoing.parent).rect.width;

        outgoing.DOAnchorPosX(-canvasWidth, AnimDuration).SetEase(Ease.InOutSine)
            .OnComplete(() => outgoing.anchoredPosition = new Vector2(canvasWidth, 0f));

        incoming.DOAnchorPos(Vector2.zero, AnimDuration).SetEase(Ease.InOutSine);
    }

    private void OnAyarlarBtnClicked()
    {
        PlayClick(_ayarlarBtnAudio);
        OpenSettingsPanel();
    }

    private void OnSettingsExitBtnClicked()
    {
        PlayClick(_settingsExitBtnAudio);
        CloseSettingsPanel();
    }

    private void OpenSettingsPanel()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(true);

        // settingsImg başta gizli ve aşağıda beklesin.
        Vector2 startPos = settingsImg.anchoredPosition;
        startPos.y = SettingsImgStartY;
        settingsImg.anchoredPosition = startPos;
        if (_settingsImgCg != null)
            _settingsImgCg.alpha = 0f;

        // Önce baslikImg yukarı çıkıp kaybolsun; tamamlanınca settingsImg girsin.
        if (baslikImg != null)
            baslikImg.DOAnchorPosY(_baslikImgRestY + BaslikImgUpOffset, AnimDuration)
                .OnComplete(ShowSettingsImg);
        else
            ShowSettingsImg();
        if (_baslikImgCg != null)
            _baslikImgCg.DOFade(0f, AnimDuration);
    }

    private void ShowSettingsImg()
    {
        settingsImg.DOAnchorPosY(_settingsImgRestY, AnimDuration);
        if (_settingsImgCg != null)
            _settingsImgCg.DOFade(1f, AnimDuration);
    }

    private void CloseSettingsPanel()
    {
        if (settingsPanel == null) return;

        // Önce settingsImg çıkıp kaybolsun; tamamlanınca baslikImg geri gelsin.
        settingsImg.DOAnchorPosY(SettingsImgStartY, AnimDuration)
            .OnComplete(HideSettingsAndShowBaslik);
        if (_settingsImgCg != null)
            _settingsImgCg.DOFade(0f, AnimDuration);
    }

    private void HideSettingsAndShowBaslik()
    {
        settingsPanel.SetActive(false);
        // settingsImg'i dinlenme konumuna geri al; sonraki açılış temiz başlasın.
        Vector2 pos = settingsImg.anchoredPosition;
        pos.y = _settingsImgRestY;
        settingsImg.anchoredPosition = pos;

        // baslikImg eski yerine geri iner ve görünür olur.
        if (baslikImg != null)
            baslikImg.DOAnchorPosY(_baslikImgRestY, AnimDuration);
        if (_baslikImgCg != null)
            _baslikImgCg.DOFade(1f, AnimDuration);
    }

    private void PlayClick(AudioSource src)
    {
        if (SoundManager.Instance == null || !SoundManager.Instance.SfxOn) return;
        if (src == null || src.clip == null) return;
        src.PlayOneShot(src.clip);
    }
}
