using TMPro;
using UnityEngine;

// Soru Haritası sağ ızgarasındaki tek bir soru karesi.
// Üç durum görselini barındırır: çözüldü / sıradaki / çözülmedi. sayiTxt hep açık kalır.
public class SoruKareUI : MonoBehaviour
{
    public enum Durum { Cozulmedi, Siradaki, Cozuldu }

    // Üç durum görseli ve sayı metni Awake'te çocuk objelerden isimle bulunur.
    private GameObject cozulduImg;
    private GameObject siradakiImg;
    private GameObject cozulmediImg;
    private TMP_Text   sayiTxt;

    // Sıradaki durumda sayı siyah, diğer durumlarda beyaz.
    private static readonly Color SiradakiSayiRengi = Color.black;
    private static readonly Color DigerSayiRengi    = Color.white;

    private void Awake()
    {
        // Referansları çocuk objelerden isimle bul (100 kopya için tek tek atama yapmamak adına).
        cozulduImg   = Bul("cozulduImg", "cozuldulmImg");
        siradakiImg  = Bul("siradakiImg");
        cozulmediImg = Bul("cozulmediImg", "cozulmadiImg");
        sayiTxt      = GetComponentInChildren<TMP_Text>(true);
    }

    private GameObject Bul(params string[] adlar)
    {
        foreach (string ad in adlar)
        {
            Transform t = transform.Find(ad);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    public void SetDurum(Durum durum)
    {
        if (cozulduImg   != null) cozulduImg.SetActive(durum == Durum.Cozuldu);
        if (siradakiImg  != null) siradakiImg.SetActive(durum == Durum.Siradaki);
        if (cozulmediImg != null) cozulmediImg.SetActive(durum == Durum.Cozulmedi);

        // Sıradaki durumda sayı siyah, çözüldü/çözülmedi durumunda beyaz.
        if (sayiTxt != null)
            sayiTxt.color = durum == Durum.Siradaki ? SiradakiSayiRengi : DigerSayiRengi;
    }

    public void SetSayi(int sayi)
    {
        if (sayiTxt != null) sayiTxt.text = sayi.ToString();
    }
}
