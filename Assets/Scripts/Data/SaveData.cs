using System;
using System.Collections.Generic;

// Tek bir bölümdeki oyuncu ilerlemesi. JsonUtility ile serialize edilir.
[Serializable]
public class BolumKaydi
{
    public int       bolumId;
    public int       kalinanSoru;                    // sıradaki göreli index (0-based)
    public List<int> cozulenler = new List<int>();   // çözülmüş göreli indexler
    public List<int> atlananlar = new List<int>();   // "Değiştir" ile sonraya bırakılmış göreli indexler

    // Ömür boyu puan verilmiş göreli indexler. Bir soru bir kez puan kazandırınca burada kalır;
    // "Baştan Başla" cozulenler'i temizlese de buraya dokunmaz, böylece aynı soru tekrar
    // çözülünce ikinci kez puan vermez (hile engeli). "Tekrar Oyna" tüm kaydı sildiğinden
    // (bolumler.Clear) bu da otomatik sıfırlanır.
    public List<int> puanAlinanlar = new List<int>();

    public int CozulenSayisi => cozulenler.Count;     // kart üzerindeki "63/100" değeri
}

// Tüm oyunun kalıcı verisi. PlayerPrefs'te tek anahtar olarak JSON saklanır.
[Serializable]
public class SaveData
{
    public int             toplamPuan;               // unvan bundan hesaplanır (her 100 puanda bir üst)
    public int             enYuksekPuan;             // ranking için kalıcı rekor (Tekrar Oyna'da sıfırlanmaz)
    public List<BolumKaydi> bolumler = new List<BolumKaydi>();
}
