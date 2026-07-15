using System.Collections.Generic;
using UnityEngine;

// Oyunun veri katmanı. Singleton + DontDestroyOnLoad: MainMenu'de doğar,
// GamePlay'e geçişte yaşamaya devam eder. Bölüm tanımlarını (BolumData SO'ları)
// tutar, oyuncu ilerlemesini PlayerPrefs'e JSON olarak kalıcı kaydeder.
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private BolumData[] bolumler;   // 5 SO, Inspector'dan atanır

    private const string SaveKey = "hedefx_save";

    private SaveData _data;

    // ── Aktif oyun durumu (MainMenu -> GamePlay aktarımı) ──────────────────────
    public int AktifBolumId     { get; private set; } = -1;
    public int AktifGoreliIndex { get; private set; }

    public BolumData AktifBolum      => GetBolum(AktifBolumId);
    public int       AktifGlobalIndex => AktifBolum != null ? AktifBolum.GlobalIndex(AktifGoreliIndex) : 0;

    public int    ToplamPuan  => _data.toplamPuan;
    public int    EnYuksekPuan => _data.enYuksekPuan;
    public string Unvan       => UnvanHesapla(_data.toplamPuan);

    public IReadOnlyList<BolumData> TumBolumler => bolumler;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // FPS sınırı: basit bir 2D oyun olduğundan GPU sınırsız kare üretip
        // boş ekranda bile %100'e gidiyordu (fan aşırı çalışması). vSync'i kapatıp
        // 60 FPS'e sabitliyoruz (vSync açıkken targetFrameRate yok sayılır).
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Yukle();
    }

    // ── Bölüm tanımı (statik) ──────────────────────────────────────────────────

    public BolumData GetBolum(int bolumId)
    {
        if (bolumler == null) return null;
        foreach (BolumData b in bolumler)
            if (b != null && b.bolumId == bolumId) return b;
        return null;
    }

    // ── Kayıt erişimi (oyuncu ilerlemesi) ──────────────────────────────────────

    public BolumKaydi GetKayit(int bolumId)
    {
        foreach (BolumKaydi k in _data.bolumler)
            if (k.bolumId == bolumId) return k;

        BolumKaydi yeni = new BolumKaydi { bolumId = bolumId };
        _data.bolumler.Add(yeni);
        return yeni;
    }

    public int  KalinanSoru(int bolumId)              => GetKayit(bolumId).kalinanSoru;
    public int  CozulenSayisi(int bolumId)            => GetKayit(bolumId).CozulenSayisi;
    public bool Cozuldu(int bolumId, int goreliIndex) => GetKayit(bolumId).cozulenler.Contains(goreliIndex);
    public bool Atlandi(int bolumId, int goreliIndex) => GetKayit(bolumId).atlananlar.Contains(goreliIndex);

    public bool BolumTamamlandi(int bolumId)
    {
        BolumData b = GetBolum(bolumId);
        return b != null && CozulenSayisi(bolumId) >= b.soruSayisi;
    }

    // Toplam bölüm sayısı ve bunlardan kaçının tamamlandığı (cozulenler'den türetilir,
    // ayrıca saklanmaz). "3 / 5" gibi ilerleme göstergeleri için.
    public int ToplamBolumSayisi => bolumler?.Length ?? 0;

    public int TamamlananBolumSayisi
    {
        get
        {
            if (bolumler == null) return 0;
            int sayac = 0;
            foreach (BolumData b in bolumler)
                if (b != null && BolumTamamlandi(b.bolumId)) sayac++;
            return sayac;
        }
    }

    // ── MainMenu: bölüm + başlangıç sorusu seç, sonra GamePlay sahnesi yüklenir ─

    // goreliBaslangic: "Devam Et" -> KalinanSoru, kareye tıklama -> o index, "Baştan Başla" -> 0
    public void BolumBaslat(int bolumId, int goreliBaslangic)
    {
        AktifBolumId     = bolumId;
        AktifGoreliIndex = Mathf.Max(0, goreliBaslangic);
    }

    // GamePlay içinde oynanan soru değiştiğinde (örn. atlanan kareye atlama) çağrılır
    public void AktifGoreliIndexAyarla(int goreliIndex) => AktifGoreliIndex = goreliIndex;

    // ── Bölüm bitince: sonraki oynanmamış bölüme geç ───────────────────────────

    // Aktif bölümden SONRAKİ ilk tamamlanmamış bölüm; sonrası yoksa baştan dönüp
    // en küçük id'li tamamlanmamış bölüm (5. bölüm bitince başa sarması için).
    // Tüm bölümler tamamlandıysa null.
    public BolumData SonrakiOynanmamisBolum()
    {
        if (bolumler == null) return null;

        BolumData sonraki = null;   // current'tan büyük en küçük id'li tamamlanmamış
        BolumData genel   = null;   // genel en küçük id'li tamamlanmamış (wrap)

        foreach (BolumData b in bolumler)
        {
            if (b == null || BolumTamamlandi(b.bolumId)) continue;

            if (genel == null || b.bolumId < genel.bolumId) genel = b;

            if (b.bolumId > AktifBolumId && (sonraki == null || b.bolumId < sonraki.bolumId))
                sonraki = b;
        }

        return sonraki != null ? sonraki : genel;
    }

    // Sonraki oynanmamış bölümü, kalınan sorusundan başlayacak şekilde aktif eder.
    // Hiç tamamlanmamış bölüm kalmadıysa false döner.
    public bool SonrakiBolumeGec()
    {
        BolumData b = SonrakiOynanmamisBolum();
        if (b == null) return false;

        BolumBaslat(b.bolumId, KalinanSoru(b.bolumId));
        return true;
    }

    // ── GamePlay: doğru cevap ──────────────────────────────────────────────────

    public void SoruCozuldu()
    {
        if (AktifBolumId < 0) return;

        BolumKaydi kayit = GetKayit(AktifBolumId);
        int idx = AktifGoreliIndex;

        if (!kayit.cozulenler.Contains(idx))
            kayit.cozulenler.Add(idx);

        // Puan yalnızca bu soru ömür boyu ilk kez çözüldüğünde verilir. "Baştan Başla"
        // cozulenler'i temizler ama puanAlinanlar kalır; bu sayede aynı soru tekrar
        // çözülünce ikinci puan gelmez (puan şişirme engeli).
        if (!kayit.puanAlinanlar.Contains(idx))
        {
            kayit.puanAlinanlar.Add(idx);
            _data.toplamPuan += 1;
        }

        kayit.atlananlar.Remove(idx);       // çözüldüyse artık atlanmış sayılmaz

        kayit.kalinanSoru = SonrakiCozulmemis(kayit);
        AktifGoreliIndex  = kayit.kalinanSoru;
        Kaydet();
    }

    // ── GamePlay: "Değiştir" ile soruyu sonraya bırak ──────────────────────────

    public void SoruAtla()
    {
        if (AktifBolumId < 0) return;

        BolumKaydi kayit = GetKayit(AktifBolumId);
        int idx = AktifGoreliIndex;

        if (!kayit.cozulenler.Contains(idx) && !kayit.atlananlar.Contains(idx))
            kayit.atlananlar.Add(idx);

        kayit.kalinanSoru = SonrakiCozulmemis(kayit);
        AktifGoreliIndex  = kayit.kalinanSoru;
        Kaydet();
    }

    // Bölüm içindeki bir sonraki çözülmemiş göreli index:
    // önce mevcut konumdan ileri, sona gelinince baştan (atlananları toplamak için) tarar.
    private int SonrakiCozulmemis(BolumKaydi kayit)
    {
        BolumData b = GetBolum(kayit.bolumId);
        int n = b != null ? b.soruSayisi : 100;

        for (int i = AktifGoreliIndex + 1; i < n; i++)
            if (!kayit.cozulenler.Contains(i)) return i;

        for (int i = 0; i < n; i++)
            if (!kayit.cozulenler.Contains(i)) return i;

        return n - 1;   // bölümün tamamı çözüldü
    }

    // ── "Baştan Başla": bölüm ilerlemesini sıfırla ─────────────────────────────
    // Yalnızca oynanış ilerlemesi (kalınan soru, çözülen/atlanan kareler) temizlenir.
    // puanAlinanlar ve toplamPuan'a DOKUNULMAZ: kazanılmış puan kalır, bölümü tekrar
    // oynayıp çözmek ikinci kez puan vermez (hile engeli). Tam yeni oyun için TekrarOyna kullanılır.
    public void BolumSifirla(int bolumId)
    {
        BolumKaydi kayit = GetKayit(bolumId);

        kayit.cozulenler.Clear();
        kayit.atlananlar.Clear();
        kayit.kalinanSoru = 0;
        Kaydet();
    }

    // ── Oyun bitti: rekoru sakla / tekrar oyna ─────────────────────────────────

    // Oyun bitiş anında çağrılır. Mevcut toplam puan rekoru geçtiyse rekoru günceller.
    // (Ranking için kalıcı; Tekrar Oyna toplamPuan'ı sıfırlasa da bu kalır.)
    public void RekoruGuncelle()
    {
        if (_data.toplamPuan > _data.enYuksekPuan)
        {
            _data.enYuksekPuan = _data.toplamPuan;
            Kaydet();
        }
    }

    // Tüm ilerlemeyi ve toplam puanı sıfırlar (rekor korunur), ilk bölümü aktif eder.
    public void TekrarOyna()
    {
        _data.bolumler.Clear();
        _data.toplamPuan = 0;

        int ilkId = -1;
        if (bolumler != null)
            foreach (BolumData b in bolumler)
                if (b != null && (ilkId < 0 || b.bolumId < ilkId)) ilkId = b.bolumId;

        if (ilkId >= 0) BolumBaslat(ilkId, 0);
        Kaydet();
    }

    // ── Unvan (toplam puana göre, her 100 puanda bir üst) ──────────────────────

    public static string UnvanHesapla(int puan) => puan switch
    {
        < 100 => "Hazırlık Oyuncusu",
        < 200 => "İşlem Yolcusu",
        < 300 => "Hesap Uzmanı",
        < 400 => "İleri Düzey Uzman",
        _     => "Süper Kahraman"
    };

    // ── Kalıcı kayıt (PlayerPrefs + JSON) ──────────────────────────────────────

    public void Yukle()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        _data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);

        if (_data == null)          _data = new SaveData();
        if (_data.bolumler == null) _data.bolumler = new List<BolumKaydi>();
    }

    public void Kaydet()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(_data));
        PlayerPrefs.Save();
    }

    // Test/geliştirme için: tüm kaydı siler
    public void TumKaydiSil()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        _data = new SaveData();
    }
}
