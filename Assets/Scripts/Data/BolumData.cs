using UnityEngine;

public enum BolumModu { SabitHedef, Asal, Rastgele }

// Bir bölümün DEĞİŞMEYEN tanımı. Oyuncu ilerlemesi burada tutulmaz (o SaveData'da).
[CreateAssetMenu(fileName = "Bolum", menuName = "HedefX/Bolum Data", order = 0)]
public class BolumData : ScriptableObject
{
    [Header("Kimlik")]
    public int       bolumId;            // 1..5
    public string    bolumAdi;           // "Hedef 24"
    public BolumModu modu;

    [Header("Soru Aralığı (Resources/question.json içinde)")]
    public int startIndex;               // global dizi indexi: 0, 100, 200, 300, 400
    public int soruSayisi = 100;

    [Header("Kart Görseli (opsiyonel)")]
    public Sprite kartIkonu;
    public Color  kartRengi = Color.white;

    // Bölüm içi göreli index -> question.json'daki global dizi indexi
    public int GlobalIndex(int goreliIndex) => startIndex + Mathf.Clamp(goreliIndex, 0, soruSayisi - 1);
}
