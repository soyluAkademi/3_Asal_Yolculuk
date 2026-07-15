using TMPro;
using UnityEngine;

// GamePlay sahnesindeki puan göstergesi. Puanın kaynağı SaveManager.ToplamPuan'dır;
// PuanManager yalnızca onu ekranda gösterir (artış mantığı SaveManager.SoruCozuldu'dadır,
// çift sayım olmaması için tek yerde kalır). Soru çözülüp effect patladığında
// SelectManager bu manager'ı günceller.
public class PuanManager : MonoBehaviour
{
    public static PuanManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI puanTxt;
    [SerializeField] private TextMeshProUGUI kariyerTxt;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // GamePlay'e özel: kalıcı değil, SaveManager kalıcı puanı tutar.
    }

    private void Start()
    {
        PuanGoster();
    }

    // Mevcut toplam puanı ve puana karşılık gelen kariyer adını ekrana yazar
    // (kaynak SaveManager). Kariyer en üst kademeye ulaşınca orada sabit kalır
    // (UnvanHesapla yüksek puanlarda hep son kariyeri döndürür).
    public void PuanGoster()
    {
        SaveManager sm = SaveManager.Instance;
        int puan = sm != null ? sm.ToplamPuan : 0;

        if (puanTxt != null)    puanTxt.text    = puan.ToString();
        if (kariyerTxt != null) kariyerTxt.text = sm != null ? sm.Unvan : SaveManager.UnvanHesapla(puan);
    }
}
