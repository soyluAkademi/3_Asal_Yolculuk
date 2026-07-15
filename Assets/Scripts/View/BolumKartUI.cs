using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Bölüm Seç panelindeki tek bir kart. Statik kısımlar (numara, ad) elle dolduruldu;
// bu bileşen yalnızca dinamik ilerlemeyi (x / 100 metni + fill) SaveManager'dan çeker.
public class BolumKartUI : MonoBehaviour
{
    [SerializeField] private int      bolumId;     // bu kart hangi bölüm (1..5)
    [SerializeField] private TMP_Text bolumTxt;    // "25 / 100"
    [SerializeField] private Image    fillImg;     // Image Type: Filled (Horizontal)

    public int BolumId => bolumId;

    private void Start()
    {
        Doldur();
    }

    // İlerleme değişip de sahne yeniden yüklenmediği bir durum olursa dışarıdan da çağrılabilir.
    public void Doldur()
    {
        SaveManager sm = SaveManager.Instance;
        if (sm == null) return;

        BolumData bolum   = sm.GetBolum(bolumId);
        int       toplam  = bolum != null ? bolum.soruSayisi : 100;
        bool      bitti   = sm.BolumTamamlandi(bolumId);

        // Sayı = kaldığın soru (1-based). Çubuk = o sorudan ÖNCE bitenler (kaldığın soru henüz
        // çözülmedi), yani 0-based index oranı; başta boş. Bölüm bittiyse 100/100 + dolu.
        int   kalinan = sm.KalinanSoru(bolumId);
        int   no      = bitti ? toplam : kalinan + 1;
        float oran    = bitti ? 1f : (toplam > 0 ? (float)kalinan / toplam : 0f);

        if (bolumTxt != null) bolumTxt.text     = $"{no} / {toplam}";
        if (fillImg  != null) fillImg.fillAmount = oran;
    }
}
