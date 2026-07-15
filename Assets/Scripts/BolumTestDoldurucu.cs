using UnityEngine;

// ── GEÇİCİ TEST SCRIPTI ──────────────────────────────────────────────────────
// İki test modu:
//  • Normal: seçilen bölümü "99 soru çözülmüş" işaretler -> o bölüme girip 1 soru
//    çözünce bölüm tamamlanır, bölüm geçiş paneli test edilir.
//  • tumBolumleriDoldur: TÜM bölümleri tamamlar, yalnızca SON bölümün son sorusunu
//    bırakır -> son bölümün son sorusunu çözünce oyun biter, oyun bitiş paneli test edilir.
// Test işi bitince bu scripti ve bağlı olduğu objeyi SİL.
//
// Kullanım: MainMenu sahnesinde boş bir GameObject'e (veya SaveManager objesine) ekle.
public class BolumTestDoldurucu : MonoBehaviour
{
    [Tooltip("HANGİ bölüm test edilecek (1..5) — kaç soru değil! O bölümün soruSayisi-1 kadarı (99) çözülmüş işaretlenir.")]
    [SerializeField] private int bolumId = 1;

    [Tooltip("İşaretliyse TÜM bölümler tamamlanır, sadece son bölümün son sorusu kalır (oyun bitiş panelini test etmek için). Son bölümün kartına girip 1 soru çöz.")]
    [SerializeField] private bool tumBolumleriDoldur = false;

    [Tooltip("Kapatınca test doldurma çalışmaz (kaydı bozmadan devre dışı bırakmak için).")]
    [SerializeField] private bool calistir = true;

    private void Start()
    {
        if (!calistir) return;

        SaveManager sm = SaveManager.Instance;
        if (sm == null) { Debug.LogWarning("[BolumTestDoldurucu] SaveManager bulunamadı."); return; }

        if (tumBolumleriDoldur) TumBolumleriDoldur(sm);
        else                    TekBolumDoldur(sm);

        // Script çalışma sırası ne olursa olsun MainMenu kartları güncel görünsün.
        foreach (BolumKartUI kart in FindObjectsOfType<BolumKartUI>(true))
            kart.Doldur();
    }

    // Tek bölümü 99 soru çözülmüş işaretler (son soru kalır).
    private void TekBolumDoldur(SaveManager sm)
    {
        BolumData bolum = sm.GetBolum(bolumId);
        if (bolum == null) { Debug.LogWarning($"[BolumTestDoldurucu] Bölüm {bolumId} tanımlı değil."); return; }

        Doldur(sm, bolum, bolum.soruSayisi - 1);   // ilk 99 çözüldü
        sm.Kaydet();

        Debug.Log($"[BolumTestDoldurucu] Bölüm {bolumId} -> {bolum.soruSayisi - 1} soru çözülmüş " +
                  $"gibi işaretlendi. Bu bölüme girip 1 soru çözünce bölüm tamamlanır.");
    }

    // Tüm bölümleri tamamlar, en yüksek id'li (son) bölümün yalnızca son sorusunu bırakır.
    private void TumBolumleriDoldur(SaveManager sm)
    {
        int sonId = int.MinValue;
        foreach (BolumData b in sm.TumBolumler)
            if (b != null && b.bolumId > sonId) sonId = b.bolumId;

        foreach (BolumData b in sm.TumBolumler)
        {
            if (b == null) continue;
            bool sonBolum = b.bolumId == sonId;
            // Son bölüm: soruSayisi-1 (son soru kalır). Diğerleri: tamamı çözülü.
            Doldur(sm, b, sonBolum ? b.soruSayisi - 1 : b.soruSayisi);
        }
        sm.Kaydet();

        Debug.Log($"[BolumTestDoldurucu] Tüm bölümler dolduruldu; son bölüm ({sonId}) son soru hariç. " +
                  $"Bölüm {sonId} kartına girip son soruyu çözünce OYUN BİTER.");
    }

    // bolum kaydını sıfırlayıp ilk 'cozulecek' soruyu çözülmüş işaretler.
    private void Doldur(SaveManager sm, BolumData bolum, int cozulecek)
    {
        BolumKaydi kayit = sm.GetKayit(bolum.bolumId);
        kayit.cozulenler.Clear();
        kayit.atlananlar.Clear();

        cozulecek = Mathf.Clamp(cozulecek, 0, bolum.soruSayisi);
        for (int i = 0; i < cozulecek; i++)
            kayit.cozulenler.Add(i);

        kayit.kalinanSoru = bolum.soruSayisi - 1;   // son soru sıradaki
    }
}
