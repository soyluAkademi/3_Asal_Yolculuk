# Asal Yolculuk - Proje Özeti ve Teknik Notlar

## 🚀 Proje Tanımı & Hedefler
- **Proje Adı:** Asal Yolculuk (2D Uzay Temalı Eğitsel Arcade Oyunu)
- **Kazanım:** MAT.6.1.3 - Bir doğal sayının asal olma durumunu ve asal çarpanlarını çözümleme.
- **Hedef Platform:** Unity 6 (Öncelikli WebGL, sonrasında Android/iOS, Akıllı Tahta uyumlu).
- **Hedef Çözünürlük:** 1080 × 1920 (Mobil / Dikey Tasarım).

---

## 🎮 Core Loop & Temel Mekanikler
1. **Hareket & Ateş:** Sürükleme (Touch/Mouse) ile yatay hareket, otomatik ateş etme.
2. **Meteor Parçalama & Asal Çarpanlar:**
   - Vurulan bileşik sayılı meteorlar asal çarpanlarına ayrılır (Maksimum 5 asal çarpan).
   - Asal parçalar tek vuruşta yok olur.
   - Ekrandan kaçan parçalar meteorun tamamlanmadığı anlamına gelir.
3. **Puanlama:**
   - Ana meteor tamamen yok edildiğinde: **+5 Puan**.
   - Altın Kristal aktifken yok edildiğinde: **+10 Puan**.
   - Yıldız Parçası toplandığında: **+10 Puan**.
   - Bölüm tamamlama hedefi: **100 Puan**.
4. **Can Sistemi:**
   - 3 Can ile başlanır. Meteor çarptığında 1 can eksilir.
   - Can 0 olursa: Bölüm puanı sıfırlanır, tamamlanan özel sayılar sıfırlanır, bölüm yeniden başlar.
5. **Madenler (PowerUp / Curse) - (10 saniye etkili):**
   - *Ödüller:* Zaman Kristali, Altın Kristal, Plazma Yakıtı, Koruma Kalkanı (hasarsızlık), Yıldız Parçası.
   - *Cezalar:* Kaos Çekirdeği, Yerçekimi Taşı, Meteor Çağırıcı.
   - Renk & Simge ile ayırt edilir (Ödüller: Yeşil/Mavi/Turkuaz/Altın; Cezalar: Kırmızı/Mor/Koyu Gri).

---

## 🪐 Gezegenler & Sayı Aralıkları (8 Gezegen)
1. **Başlangıç Gezegeni:** $4 - 48$
2. **Asteroit Kuşağı:** $24 - 90$
3. **Lav Gezegeni:** $11 - 89$ (Asal sayı ağırlıklı)
4. **Buz Gezegeni:** $48 - 120$ (Çok adımlı parçalama)
5. **Fırtına Gezegeni:** $60 - 180$ (Elektrikli meteorlar)
6. **Kara Delik Sınırı:** $72 - 200$ (Büyük ara çarpanlar)
7. **Kristal Gezegeni:** $100 - 240$ (Yoğun maden kullanımı)
8. **Asal Krallığı:** $150 - 540$ (Tüm mekaniklerin birleştiği final)
- **Sonsuz Yolculuk Modu:** Asal Krallığı tamamlandıktan sonra açılan rekor modu.

---

## 🎲 Spawn Mantığı (MeteorSpawner)
- **%70:** Gezegene ait özel sayı listesinden.
- **%30:** Gezegenin sayı aralığından rastgele.
- **Kural:** Tamamen yok edilen sayılar o bölüm sonuna kadar tekrar üretilmez.

---

## 🎬 Sahne & UI Yapısı
- **Sahneler:**
  1. `MainMenuScene` (Başlat, Sonsuz Mod, Ses Ayarları)
  2. `MapScene` (8 Gezegen Haritası, İlerle Butonu)
  3. `TravelScene` (Nereden Nereye Geçiş Animasyonu)
  4. `GameplayScene` (Oyun Alanı & HUD)
  5. `PlanetCompleteScene` (Kutlama & Yeni Gezegen Açıldı Mesajı)
- **HUD:**
  - Sol Üst: Can Göstergesi (3 Kalp)
  - Üst Orta: Puan (X / 100)
  - Üst Sağ: Ses İkonu
  - Aktif Maden Sayaçları (Örn: Kalkan 6s)

---

## 🛠️ Unity Kod ve Yöneticiler (Managers)
- `GameManager`: Oyun durumu, puan, can ve kazanma/kaybetme akışı.
- `PlanetManager`: Gezegen verileri (`PlanetData`) ve kilit durumları.
- `MeteorSpawner`: Sayı seçimi (%70/%30) ve meteor türetme.
- `PowerUpManager`: Maden türetme ve 10s etki zamanlayıcıları.
- `SaveManager`: `PlayerPrefs` tabanlı yerel kayıt (kilit açılışları, yüksek skor, ses ayarları).
- `SCORM`: LMS paketleme desteği.

---

## ⚠️ Mevcut Durum (Repo Snapshot)
- **`Assets/Scripts/` şu anda boş.** Bu repo, önceki bir eğitim oyunundan (`2_sayiTamamlamaBakanlik` — Bölüm/Soru/Grid tabanlı bir yapı) devralındı; o oyuna ait tüm scriptler (`GameManager` hariç, hiçbiri yukarıdaki Manager listesiyle uyuşmuyor) silindi ve proje "Asal Yolculuk" olarak sıfırdan kodlanacak. Yukarıdaki Manager/Sistem listesi **hedef mimari**dir, henüz implemente edilmemiştir.
- `Assets/Scenes/GamePlay.unity` üzerinde çalışma devam ediyor (henüz commit edilmedi); sahne script referansları eksik/kırık olabilir.
- Henüz `.asmdef` yok, henüz test yok (paket olarak `com.unity.test-framework` kurulu ama kullanılmıyor).
- `Assets/Resources/question.json` eski oyundan kalma bir veri dosyası; yeni "Asal Yolculuk" mekaniğiyle doğrudan ilgili değil, silinip silinmeyeceği netleşmeden dokunma.

## 🔧 Ortam & Komutlar
- **Unity sürümü:** 6000.3.14f1 (Unity 6), URP (`com.unity.render-pipelines.universal` 17.3.0).
- Bu projede CLI build/init scripti (`init_project.ps1`) yok; global `~/.claude/CLAUDE.md`'de tarif edilen init akışı bu projeye henüz uygulanmamış — klasör yapısı (`Art/`, `Audio/`, `Scripts/Runtime`, `Scripts/Editor`) standarda tam uymuyor (bkz. `Arts/`, `Sounds/` çoğul isimlendirme).
- Build/test için CLI komutu yok; Unity Editor üzerinden çalışılıyor (Play Mode ile test, `File > Build Profiles` ile build).
- Kurulu önemli paketler: Input System (yeni), DOTween (`Assets/Resources/DOTweenSettings.asset`), Epic Toon FX (VFX asset paketi, `Assets/Epic Toon FX/`), 2D Animation/Tilemap/SpriteShape paketleri.
- `.meta` dosyaları repo'da takip ediliyor; yeni asset/script eklerken `.meta` dosyasının da commit'e girdiğinden emin ol (global kural).

## 📁 Kod Yazarken Dikkat
- Yeni Manager'ları oluştururken global `CLAUDE.md`'deki isimlendirme (`underscore yok`, `[SerializeField] private`) ve mimari kurallarına (SO ile veri/davranış ayrımı, event-driven bağlantı, singleton'dan kaçınma) uy.
- `GameManager`, `PlanetManager`, `MeteorSpawner`, `PowerUpManager`, `SaveManager` arasındaki bağlantıyı doğrudan referans yerine ScriptableObject event kanalı veya C# event ile kur; yukarıdaki "Mimari Varsayılanları" bölümü bu proje için de geçerli.
- Gezegen verisi (`PlanetData`) ve maden/power-up tanımları ScriptableObject olarak modellenmeli (`Data/` altında), davranış kodu ayrı `MonoBehaviour` sınıflarında kalmalı.
