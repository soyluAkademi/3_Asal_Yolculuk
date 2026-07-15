using DG.Tweening;
using MaskTransitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Bölüm tamamlandığında açılan geçiş panelini ve içindeki yıldızların sıralı
// açılış animasyonunu yönetir. Tetikleme SelectManager'dan Ac() ile yapılır.
public class BolumGecisManager : MonoBehaviour
{
    public static BolumGecisManager Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject bolumGecisPanel;
    [SerializeField] private GameObject oyunBitisPanel;   // tüm bölümler bitince açılır

    [Header("Geçiş Süresi (sn)")]
    [SerializeField] private float gecisSure = 0.35f;   // tüm geçiş animasyonları için ortak süre

    [Header("Bölüm Geçiş Görseli")]
    [SerializeField] private RectTransform bolumGecisImg; // panel açılınca yukarıdan kayarak gelir
    [SerializeField] private float gecisImgBaslangicY = 750f; // bu Y'den mevcut (0) konumuna gelir
    [SerializeField] private float gecisImgCikisY = 1500f;    // kapanırken bu Y'ye çıkar, sonra 0'a sıfırlanır

    [Header("Bölüm Geçiş Metinleri")]
    [SerializeField] private TMP_Text kacinciBolumTxt;   // "Bölüm 1"
    [SerializeField] private TMP_Text bolumAdiTxt;       // "Hedef 12"
    [SerializeField] private TMP_Text ilerlemeTxt;       // "3 / 5" (tamamlanan / toplam bölüm)

    [Header("Oyun Bitiş Görseli")]
    [SerializeField] private RectTransform oyunBitisImg;  // panel açılınca yukarıdan kayarak gelir

    [Header("Oyun Bitiş Metinleri")]
    [SerializeField] private TMP_Text puanTxt;           // bu oyundan alınan puan
    [SerializeField] private TMP_Text enYuksekPuanTxt;   // tüm oyunlardaki en yüksek puan

    [Header("Yıldızlar")]
    [SerializeField] private Transform yildizlarContainer;
    [SerializeField] private float yildizDuration = 0.35f;
    [SerializeField] private float yildizStagger  = 0.18f;

    [Header("Bölüm Geçiş Butonları")]
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button sonrakiBolumBtn;

    [Header("Oyun Bitiş Butonları")]
    [SerializeField] private Button oyunBitisMainMenuBtn;
    [SerializeField] private Button tekrarOynaBtn;

    [Header("Oyun Alanı")]
    [SerializeField] private RectTransform centerPanel;   // panel açılınca yukarı sahne dışına çıkar
    [SerializeField] private float centerPanelDisariY = 1500f; // sahne dışı (yukarı) Y konumu

    [SerializeField] private string mainMenuSahneAdi = "MainMenu";

    private float _centerPanelOrijinalY;   // centerPanel'in başlangıç (oyun içi) Y konumu

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (centerPanel != null) _centerPanelOrijinalY = centerPanel.anchoredPosition.y;

        if (mainMenuBtn != null)          mainMenuBtn.onClick.AddListener(OnMainMenuClicked);
        if (sonrakiBolumBtn != null)      sonrakiBolumBtn.onClick.AddListener(OnSonrakiBolumClicked);
        if (oyunBitisMainMenuBtn != null) oyunBitisMainMenuBtn.onClick.AddListener(OnOyunBitisMainMenuClicked);
        if (tekrarOynaBtn != null)        tekrarOynaBtn.onClick.AddListener(OnTekrarOynaClicked);
    }

    // Bölüm bitince çağrılır. Tüm bölümler tamamlandıysa oyun bitiş panelini,
    // değilse bölüm geçiş panelini açar. Açılacak panel atanmamışsa false döner
    // (çağıran eski davranışa düşebilsin).
    public bool Ac()
    {
        SaveManager sm = SaveManager.Instance;
        bool oyunBitti = sm != null && sm.ToplamBolumSayisi > 0
                         && sm.TamamlananBolumSayisi >= sm.ToplamBolumSayisi;

        if (oyunBitti)
        {
            if (oyunBitisPanel == null) return false;
            sm.RekoruGuncelle();   // skoru kalıcı rekora yaz (Tekrar Oyna sıfırlasa da kalır)
            OyunBitisMetinleriDoldur();
            OyunBitisPanelAc();
            return true;
        }

        if (bolumGecisPanel == null) return false;

        BolumGecisPanelAc();
        MetinleriDoldur();

        // Yıldızları önce gizle, panel açılınca sırayla scale + alpha ile aç.
        ResetStars();
        DOVirtual.DelayedCall(gecisSure, AnimateStars);

        return true;
    }

    // Bölüm geçiş panelini açar: panel aktif edilir, içindeki görsel mevcut (0) konumuna
    // y=gecisImgBaslangicY (750) yukarıdan DOTween From() ile kayarak gelir.
    private void BolumGecisPanelAc()
    {
        bolumGecisPanel.SetActive(true);
        CenterPaneliDisariAl();

        if (bolumGecisImg != null)
        {
            bolumGecisImg.DOKill();
            bolumGecisImg.DOAnchorPosY(gecisImgBaslangicY, gecisSure)
                         .From()
                         .SetEase(Ease.OutBack);
        }
    }

    // Oyun bitiş panelini açar: panel aktif edilir, içindeki görsel mevcut (0) konumuna
    // y=gecisImgBaslangicY (750) yukarıdan DOTween From() ile kayarak gelir.
    private void OyunBitisPanelAc()
    {
        oyunBitisPanel.SetActive(true);
        CenterPaneliDisariAl();

        if (oyunBitisImg != null)
        {
            oyunBitisImg.DOKill();
            oyunBitisImg.DOAnchorPosY(gecisImgBaslangicY, gecisSure)
                        .From()
                        .SetEase(Ease.OutBack);
        }
    }

    // Geçiş/bitiş paneli açılınca oyun alanını yukarı sahne dışına kaydırır.
    private void CenterPaneliDisariAl()
    {
        if (centerPanel == null) return;
        centerPanel.DOKill();
        centerPanel.DOAnchorPosY(centerPanelDisariY, gecisSure).SetEase(Ease.InBack);
    }

    // Yeni bölüme geçince oyun alanını eski (oyun içi) konumuna geri getirir.
    private void CenterPaneliGeriGetir()
    {
        if (centerPanel == null) return;
        centerPanel.DOKill();
        centerPanel.DOAnchorPosY(_centerPanelOrijinalY, gecisSure).SetEase(Ease.OutBack);
    }

    // Tamamlanan bölümün numarasını ("Bölüm 1") ve adını ("Hedef 12") yazar.
    private void MetinleriDoldur()
    {
        SaveManager sm = SaveManager.Instance;
        if (sm == null) return;

        BolumData bolum = sm.AktifBolum;
        if (bolum != null)
        {
            if (kacinciBolumTxt != null) kacinciBolumTxt.text = $"Bölüm {bolum.bolumId}";
            if (bolumAdiTxt     != null) bolumAdiTxt.text     = bolum.bolumAdi;
        }

        // Tamamlanan bölümler / toplam bölüm (ör. "3 / 5").
        if (ilerlemeTxt != null)
            ilerlemeTxt.text = $"{sm.TamamlananBolumSayisi} / {sm.ToplamBolumSayisi}";
    }

    // Oyun bitiş paneli: bu oyunun puanı + tüm oyunların en yüksek puanı.
    private void OyunBitisMetinleriDoldur()
    {
        SaveManager sm = SaveManager.Instance;
        if (sm == null) return;

        if (puanTxt         != null) puanTxt.text         = sm.ToplamPuan.ToString();
        if (enYuksekPuanTxt != null) enYuksekPuanTxt.text = sm.EnYuksekPuan.ToString();
    }

    private void ResetStars()
    {
        if (yildizlarContainer == null) return;

        foreach (Transform yildiz in yildizlarContainer)
        {
            yildiz.localScale = Vector3.zero;
            CanvasGroup cg = yildiz.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;
        }
    }

    private void AnimateStars()
    {
        if (yildizlarContainer == null) return;

        int i = 0;
        foreach (Transform yildiz in yildizlarContainer)
        {
            float delay = i * yildizStagger;

            yildiz.DOScale(Vector3.one, yildizDuration).SetDelay(delay).SetEase(Ease.OutBack);

            CanvasGroup cg = yildiz.GetComponent<CanvasGroup>();
            if (cg != null) cg.DOFade(1f, yildizDuration).SetDelay(delay);

            i++;
        }
    }

    // ── Butonlar ─────────────────────────────────────────────────────────────

    // Ana Menü: doğrudan MainMenu sahnesine geç.
    private void OnMainMenuClicked()
    {
        PlayClick(mainMenuBtn);
        TransitionManager.Instance.LoadLevel(mainMenuSahneAdi);
    }

    // Sonraki Bölüm: önce paneli kapat, sonra sonraki oynanmamış bölümün sorusunu aç.
    // Tamamlanmamış bölüm kalmadıysa ana menüye dön.
    private void OnSonrakiBolumClicked()
    {
        PlayClick(sonrakiBolumBtn);
        if (sonrakiBolumBtn != null) sonrakiBolumBtn.interactable = false;

        SaveManager sm = SaveManager.Instance;
        if (sm != null && sm.SonrakiBolumeGec() && SelectManager.Instance != null)
        {
            // centerPanel geri gelirken görsel yukarı (1500) çıkar; çıkış bitince panel kapanır.
            CenterPaneliGeriGetir();
            SelectManager.Instance.YeniBolumeBaslat();
            Kapat(bolumGecisPanel, bolumGecisImg);
            if (sonrakiBolumBtn != null) sonrakiBolumBtn.interactable = true;
        }
        else
        {
            // Tüm bölümler tamamlandı (ya da gerekli referans yok) -> ana menü.
            TransitionManager.Instance.LoadLevel(mainMenuSahneAdi);
        }
    }

    // Oyun Bitiş -> Ana Menü: doğrudan MainMenu sahnesine geç (rekor zaten yazıldı).
    private void OnOyunBitisMainMenuClicked()
    {
        PlayClick(oyunBitisMainMenuBtn);
        TransitionManager.Instance.LoadLevel(mainMenuSahneAdi);
    }

    // Tekrar Oyna: ilerleme + puanı sıfırla (rekor korunur), paneli kapat, ilk bölümün
    // ilk sorusunu aç.
    private void OnTekrarOynaClicked()
    {
        PlayClick(tekrarOynaBtn);
        if (tekrarOynaBtn != null) tekrarOynaBtn.interactable = false;

        SaveManager sm = SaveManager.Instance;
        if (sm != null && SelectManager.Instance != null)
        {
            sm.TekrarOyna();
            // centerPanel geri gelirken görsel yukarı (1500) çıkar; çıkış bitince panel kapanır.
            CenterPaneliGeriGetir();
            SelectManager.Instance.YeniBolumeBaslat();
            if (PuanManager.Instance != null) PuanManager.Instance.PuanGoster();
            Kapat(oyunBitisPanel, oyunBitisImg);
            if (tekrarOynaBtn != null) tekrarOynaBtn.interactable = true;
        }
        else
        {
            TransitionManager.Instance.LoadLevel(mainMenuSahneAdi);
        }
    }

    // Paneli kapatır: içindeki görsel y=gecisImgCikisY (1500) yukarı çıkar, çıkış bitince
    // panel SetActive(false) olur ve görsel tekrar y=0'a sıfırlanır (sonraki açılış için).
    private void Kapat(GameObject panel, RectTransform img)
    {
        if (panel == null) return;

        if (img != null)
        {
            img.DOKill();
            img.DOAnchorPosY(gecisImgCikisY, gecisSure).SetEase(Ease.InBack)
               .OnComplete(() =>
               {
                   panel.SetActive(false);
                   Vector2 p = img.anchoredPosition;
                   p.y = 0f;
                   img.anchoredPosition = p;
               });
        }
        else
        {
            panel.SetActive(false);
        }
    }

    private void PlayClick(Button btn)
    {
        if (SoundManager.Instance == null || !SoundManager.Instance.SfxOn || btn == null) return;
        AudioSource src = btn.GetComponent<AudioSource>();
        if (src != null && src.clip != null) src.PlayOneShot(src.clip);
    }
}
