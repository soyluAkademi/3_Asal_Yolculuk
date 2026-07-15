using MaskTransitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Soru Haritası panelinin SOL tarafını doldurur: bölüm no, unvan, mod, ilerleme (x / 100 + fill)
// ve "Devam Et" buton metni. Sağdaki 100'lük soru ızgarasını da doldurur ve Devam Et ile oyunu açar.
public class SoruHaritasi : MonoBehaviour
{
    [Header("Sol panel")]
    [SerializeField] private TMP_Text bolumNoTxt;     // sarı kutudaki "3"
    [SerializeField] private TMP_Text unvanTxt;       // "Hazırlık Oyuncusu"
    [SerializeField] private TMP_Text modTxt;         // "Rastgele" / "Hedef 24" ...
    [SerializeField] private TMP_Text ilerlemeTxt;    // "98 / 100"
    [SerializeField] private Image    ilerlemeFill;   // Image Type: Filled (Horizontal)
    [SerializeField] private TMP_Text devamBtnTxt;    // "Soru 48 ... Devam Et"
    [SerializeField] private Button   devamBtn;       // boşsa devamBtnTxt'in parent'ından bulunur
    [SerializeField] private Button   bastanBaslaBtn; // bölümü baştan başlatır (ilerlemeyi sıfırlar)

    [Header("Sağ panel (soru ızgarası)")]
    [SerializeField] private Transform sorularPanel;  // 100 soru karesinin parent'ı

    [Header("Geçiş")]
    [SerializeField] private string gameplaySahneAdi = "GamePlay";

    private SoruKareUI[] _kareler;

    public int AktifBolumId { get; private set; } = -1;

    private void Start()
    {
        if (devamBtn == null && devamBtnTxt != null)
            devamBtn = devamBtnTxt.GetComponentInParent<Button>();

        if (devamBtn != null)
            devamBtn.onClick.AddListener(OnDevamClicked);

        if (bastanBaslaBtn != null)
            bastanBaslaBtn.onClick.AddListener(OnBastanBaslaClicked);
    }

    // Baştan Başla: bu bölümün ilerlemesini sıfırlar (puan korunur, hile yok), bölümü
    // ilk sorudan aktif eder ve doğrudan GamePlay sahnesine geçer.
    private void OnBastanBaslaClicked()
    {
        SaveManager sm = SaveManager.Instance;
        if (sm == null || AktifBolumId < 0) return;

        PlayClick(bastanBaslaBtn);
        sm.BolumSifirla(AktifBolumId);
        sm.BolumBaslat(AktifBolumId, 0);   // ilk sorudan başlat
        TransitionManager.Instance.LoadLevel(gameplaySahneAdi);
    }

    // Devam Et: aktif bölüm + kalınan soruyu SaveManager'a yaz, GamePlay sahnesini aç.
    private void OnDevamClicked()
    {
        PlayClick(devamBtn);
        DevamEt();
    }

    // Aktif bölümde kalınan sorudan oyunu açar. Bölüm Seç panelindeki kart "Oyna"
    // butonu da (önce Doldur ile bölümü aktif edip) bunu çağırır.
    public void DevamEt()
    {
        SaveManager sm = SaveManager.Instance;
        if (sm == null || AktifBolumId < 0) return;

        sm.BolumBaslat(AktifBolumId, sm.KalinanSoru(AktifBolumId));
        TransitionManager.Instance.LoadLevel(gameplaySahneAdi);
    }

    private void PlayClick(Button btn)
    {
        if (SoundManager.Instance == null || !SoundManager.Instance.SfxOn || btn == null) return;
        AudioSource src = btn.GetComponent<AudioSource>();
        if (src != null && src.clip != null) src.PlayOneShot(src.clip);
    }

    // Bölüm Seç panelinde bir kartın Oyna'sına basıldığında o bölümün id'si ile çağrılır.
    public void Doldur(int bolumId)
    {
        AktifBolumId = bolumId;

        SaveManager sm = SaveManager.Instance;
        if (sm == null) return;

        BolumData bolum   = sm.GetBolum(bolumId);
        int       toplam  = bolum != null ? bolum.soruSayisi : 100;
        int       kalinan = sm.KalinanSoru(bolumId);   // göreli index (0-based)
        bool      bitti   = sm.BolumTamamlandi(bolumId);

        // Sayı = kaldığın soru (1-based). Çubuk = o sorudan ÖNCE bitenler (kaldığın soru henüz
        // çözülmedi), yani 0-based index oranı; başta boş. Bölüm bittiyse 100/100 + dolu.
        int   no   = bitti ? toplam : kalinan + 1;
        float oran = bitti ? 1f : (toplam > 0 ? (float)kalinan / toplam : 0f);

        if (bolumNoTxt   != null) bolumNoTxt.text         = bolumId.ToString();
        if (unvanTxt     != null) unvanTxt.text           = sm.Unvan;
        if (modTxt       != null) modTxt.text             = bolum != null ? bolum.bolumAdi : string.Empty;
        if (ilerlemeTxt  != null) ilerlemeTxt.text        = $"{no} / {toplam} soru";
        if (ilerlemeFill != null) ilerlemeFill.fillAmount = oran;
        if (devamBtnTxt  != null) devamBtnTxt.text        = $"Soru {kalinan + 1}";

        DolduIzgara(bolumId, kalinan);
    }

    // Sağ ızgara: her kare numarasını yazar ve durumunu (çözüldü/sıradaki/çözülmedi) ayarlar.
    private void DolduIzgara(int bolumId, int kalinan)
    {
        if (sorularPanel == null) return;
        if (_kareler == null || _kareler.Length == 0)
            _kareler = sorularPanel.GetComponentsInChildren<SoruKareUI>(true);

        SaveManager sm = SaveManager.Instance;

        for (int i = 0; i < _kareler.Length; i++)
        {
            SoruKareUI kare = _kareler[i];
            if (kare == null) continue;

            kare.SetSayi(i + 1);   // 1..100

            SoruKareUI.Durum durum;
            if (sm != null && sm.Cozuldu(bolumId, i)) durum = SoruKareUI.Durum.Cozuldu;
            else if (i == kalinan)                    durum = SoruKareUI.Durum.Siradaki;
            else                                       durum = SoruKareUI.Durum.Cozulmedi;

            kare.SetDurum(durum);
        }
    }
}
