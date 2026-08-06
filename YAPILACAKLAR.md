# Yapılacaklar — Asal Yolculuk

Son güncelleme: 2026-08-06 (oyun sonu paneli eklendi)

Bu dosya projenin tamamlanmamış işlerini önceliklendirilmiş şekilde listeler.
Mevcut durum için `SCRIPT_OZETI.md`, hedef tasarım için `AsalYolculuk_Senaryo.md`
ve `CLAUDE.md` referans alınır.

---

## Mevcut Durum Özeti

Çekirdek gameplay döngüsü ve bölümler arası geçiş çalışıyor; ölüm sonrası
yeniden başlatma ve meta katman (ekranlar, kayıt, madenler) henüz yok.

| Sistem | Durum |
|---|---|
| Gemi hareketi, otomatik ateş, object pooling | ✅ Bitti |
| Meteor üretimi, sekme, sorting group, patlama VFX | ✅ Bitti |
| `PrimeMathEngine` (sieve, `IsPrime`, `GetPrimeFactors`) | ✅ Bitti |
| Asal çarpanlara bölünme + `MeteorSession` oturum takibi | ✅ Bitti |
| `LevelNumberPool` (%70/%30, tamamlanan sayı tekrar gelmez) | ✅ Bitti |
| Can sistemi, hasar/ölüm sekansı, kamera sarsıntısı | ✅ Bitti |
| Puan sistemi + HUD göstergesi | ✅ Bitti |
| Bölüm tamamlama tetikleyicisi (`OnTargetReached`) | ✅ Bitti |
| Bölüm sonu sunumu (HUD çekilir, gemi taşınır, sahne temizlenir) | ✅ Bitti |
| Bölüm sonu paneli + bir sonraki bölüme geçiş | ✅ Bitti |
| Oyun sonu paneli (giriş + yolculuk özeti animasyonu) | ✅ Bitti |
| Oyun sonu paneli içeriği (puan, butonlar, kutlama) | ❌ Yok |
| `PlanetDataSO` × 8 | ⚠️ min/max dolu, sprite ve öncelikli sayılar eksik |
| Ölüm sonrası yeniden başlatma | ❌ Yok |
| Maden (PowerUp / Curse) sistemi | ❌ Yok |
| Ekran akışı (Ana Menü, Harita, Yolculuk, Sonuç) | ❌ Yok |
| Kayıt sistemi (`SaveManager`) | ❌ Yok |
| Dokunmatik input | ❌ Yok |
| Ses sistemi | ❌ Yok |

Sahne durumu: tek sahne var — `Assets/Scenes/GamePlay.unity`.

---

## 🔴 Faz 1 — Bölüm Döngüsünü Kapat (Kritik)

Bölüm kazanma ve bir sonraki gezegene geçiş tamamlandı. Kalan: kaybedildikten
sonra devam edememe ve gezegen verilerinin eksikliği.

- [ ] **1.1 — Gemiyi diriltme**
  `PlayerDeathEffect.HideShip()` renderer/collider kapatıyor ve controller'ları devre
  dışı bırakıyor; bunun tersini yapan `RestoreShip()` gerekli.
  *Dosyalar:* `Player/PlayerDeathEffect.cs`

- [ ] **1.2 — `LevelFlowController.RestartLevel()`**
  `GoToNextLevel()` tamamlandı; aynı reset zincirini seviye ilerletmeden kullanan
  `RestartLevel()` eklenecek (`ResetLevelProgress()` zaten ayrı metot) +
  `PlayerDeathEffect.RestoreShip()` çağrısı.
  *Dosyalar:* `Core/LevelFlowController.cs`

- [ ] **1.3 — "Bölüm Başarısız" paneli**
  `LevelCompletePanelView` ile aynı desende, `LevelFailed` durumunda açılan
  "Tekrar Dene" paneli → `RestartLevel()`.
  *Dosyalar:* `UI/LevelFailedPanelView.cs` (yeni)

- [ ] **1.4 — Oyun sonu paneli içeriği**
  Panelin kendisi hazır (`UI/BitisManager.cs`): son gezegende sağdan gelip
  yolculuk özeti animasyonunu (gezegen → çizgi → gezegen) oynatıyor, gemi
  ekrandan çıkıp yok oluyor, HUD panelleri kapanıyor. **Eksik olan içerik:**
  - Toplam puan göstergesi (sayaç animasyonuyla) — `SaveManager` gerektirir (4.1)
  - "Sonsuz Yolculuk açıldı" butonu → mod girişi (5.6)
  - Ana Menü butonu
  - Havai fişek / konfeti (`Prefabs/Effects/FireworkBlue.prefab`,
    `confetti.prefab` hazır bekliyor)
  - Eğitsel özet (parçalanan meteor, bulunan asal çarpan sayısı) — sayaçların
    **şimdiden** tutulmaya başlaması gerekir, yoksa finalde veri olmaz

- [ ] **1.5 — Gezegen sprite'ları**
  `PlanetDataSO.planetSprite` alanı eklendi ama 8 asset'te de boş.
  `Assets/Arts/Asal_0/gezegenler/` altında 6 görsel var, **2 tanesi eksik**.
  Bölüm sonu paneli ve ileride harita ekranı bu alandan okuyor.
  *Dosyalar:* `Assets/Scripts/Runtime/ScriptableObjects/Planet_1..8.asset`

- [ ] **1.6 — `priorityNumberList` doldurma (8 gezegen)**
  Şu an 8 gezegende de liste **boş**; bu yüzden GDD'nin %70 pedagojik sayı kuralı
  fiilen çalışmıyor, tüm sayılar rastgele aralıktan geliyor. Oyunun eğitsel omurgası
  bu — kod tarafı hazır, sadece veri eksik.
  Gezegen bazında zorluk artışına göre (2-3 çarpanlı → 4-5 çarpanlı) liste hazırlanacak.
  *Dosyalar:* `Assets/Scripts/Runtime/ScriptableObjects/Planet_1..8.asset`

---

## 🟠 Faz 2 — Oynanabilirlik (Yüksek Öncelik)

- [ ] **2.1 — Dokunmatik / fare input**
  `PlayerShipController` şu an yalnızca klavye (WASD + ok tuşları) destekliyor.
  GDD birincil kontrolü "parmakla sürükleme" olarak tanımlıyor; hedef platformlar
  WebGL + mobil + akıllı tahta. **Şu an telefonda oyun oynanamıyor.**
  Yeni Input System `Pointer` üzerinden sürükleme eklenecek, klavye fallback kalacak.
  *Dosyalar:* `Player/PlayerShipController.cs`

- [ ] **2.2 — `SmoothDamp` kullanım hatası**
  `PlayerShipController.cs:63` — `Vector2.SmoothDamp(currentVelocity, targetVelocity,
  ref currentVelocity, smoothTime)` çağrısında aynı değişken hem kaynak hem velocity
  buffer olarak geçiliyor. Hareket hissini bozuyor; ayrı bir `smoothVelocity` alanı gerekli.
  *Dosyalar:* `Player/PlayerShipController.cs`

- [ ] **2.3 — Ses sistemi**
  `Assets/Sounds/` dolu ama sahnede hiçbir ses çalmıyor. `SoundManager` (müzik + SFX,
  `PlayerPrefs` ile kalıcı aç/kapa, sahneler arası yaşayan) kurulacak.
  Bağlanacak olaylar: ateş, meteor vurma, parçalanma, yok olma, can kaybı,
  maden toplama, bölüm tamamlama.
  *Not:* Bunun için hazır `unity-ses-sistemi` skill'i mevcut.

- [ ] **2.4 — Duraklatma (Pause)**
  `GameState.Paused` enum'da tanımlı ama hiçbir yerden tetiklenmiyor.
  HUD'a duraklat butonu + panel; `GameManager` zaten `Paused` durumunda
  `Time.timeScale = 0` yapıyor.

---

## 🟡 Faz 3 — Maden Sistemi (PowerUp / Curse)

GDD'nin önemli bir bölümü; görseller `Assets/Arts/Asal_0/Bonus/` altında hazır (5 asset).

- [ ] **3.1 — `IActiveEffect` arayüzü ve `PowerUpManager`**
  Süre bazlı efekt yönetimi (10 sn), aynı anda birden fazla efekt aktif olabilmeli.
  *Dosyalar:* `PowerUps/IActiveEffect.cs`, `PowerUps/PowerUpManager.cs` (yeni)

- [ ] **3.2 — Ödül madenleri**
  - `TimeCrystal` — Zaman Kristali
  - `GoldCrystal` — Altın Kristal (meteor başına +10 puan)
  - `PlasmaFuel` — Plazma Yakıtı
  - `Shield` — Koruma Kalkanı (10 sn hasar almaz, gemi çevresinde turkuaz bariyer)
  - `StarFragment` — Yıldız Parçası (anında +10 puan)

- [ ] **3.3 — Ceza madenleri**
  - `ChaosCore` — Kaos Çekirdeği
  - `GravityStone` — Yerçekimi Taşı
  - `MeteorCaller` — Meteor Çağırıcı

- [ ] **3.4 — Maden spawn + toplama**
  Havuzlanmış maden prefab'ı, drop rate `GameConfigSO`'dan; gemi üzerinden geçince toplanır.

- [ ] **3.5 — Aktif efekt HUD göstergesi**
  Toplanan madenin ikonu + geri sayım (örn. "6s"); süre bitince kaybolur.

- [ ] **3.6 — Kalkan entegrasyonu**
  `ShipCollisionHandler` kalkan aktifken can eksiltmemeli.
  *Dosyalar:* `Player/ShipCollisionHandler.cs`

---

## 🔵 Faz 4 — Ekran Akışı ve Kayıt

GDD 9 ekran tanımlıyor; şu an yalnızca Oyun Ekranı var.

- [ ] **4.1 — `SaveManager` + `SaveData`**
  `maxUnlockedPlanetID`, `completedPlanetIDs`, `totalPointsCollected`,
  `endlessModeHighScore`, ses ayarları. `PlayerPrefs` üzerinde JSON.
  *Dosyalar:* `Core/SaveManager.cs`, `Data/SaveData.cs` (yeni)

- [ ] **4.2 — `GameConfigSO`**
  `targetScorePerPlanet`, `initialPlayerLives`, `scorePerMeteorDestroyed`,
  `powerUpDuration`, drop rate'ler. Şu an bu değerler script'lere dağılmış durumda.
  *Dosyalar:* `Data/GameConfigSO.cs` (yeni)

- [ ] **4.3 — `UIManager` / `ScreenController`**
  Ekranlar arası geçiş yönetimi.

- [ ] **4.4 — Ana Menü ekranı**
  Oyuna Başla / Devam Et / Sonsuz Yolculuk / Ses aç-kapa.

- [ ] **4.5 — Harita ekranı**
  8 gezegen; tamamlananlar işaretli, kilitliler gri, aktif olan parlayan çerçeveli.
  `İLERLE` butonu. `PlanetManager` + `LevelManager.SetLevel` buraya bağlanacak.
  *Görseller:* `Assets/Arts/Asal_0/gezegenler/` (6 adet var, 8 gerekiyor)

- [ ] **4.6 — Yolculuk (geçiş) ekranı**
  Solda mevcut gezegen, sağda hedef gezegen, ortada gemi + yıldız animasyonu.

- [ ] **4.7 — Bölüm Tamamlandı ekranı**
  Havai fişek animasyonu (`Prefabs/Effects/FireworkBlue.prefab`, `confetti.prefab` hazır),
  tamamlanan gezegen adı, toplam puan, açılan yeni gezegen, Devam Et butonu.

- [ ] **4.8 — In-scene panelleri gerçek ekranlarla değiştir**
  `LevelCompletePanelView` ve Faz 1'de eklenecek başarısızlık paneli, tam ekran
  akışı geldiğinde `ScreenController` yönetimine taşınacak. Panelin gezegen
  bilgisi doldurma mantığı (`FillPlanetInfo`) yolculuk ekranında aynen kullanılabilir.

---

## ⚪ Faz 5 — Gezegene Özgü Mekanikler ve Sonsuz Mod

- [ ] **5.1 — Bölünme modu alanı**
  `PlanetDataSO`'ya `SplitMode` enum alanı (`DirectToPrimes` / `ViaDivisionPair`).
  Kara Delik Sınırı'nda "önce iki ara çarpana, sonra asallara" davranışı.
  `PrimeMathEngine.GetDivisionPair` yazılmış ama hiç kullanılmıyor;
  `MeteorSession` çok kademeli bölünmeye zaten hazır.
  *Dosyalar:* `Data/PlanetDataSO.cs`, `Meteors/MeteorController.cs`

- [ ] **5.2 — Lav Gezegeni** — asal sayı ağırlığının artırılması
- [ ] **5.3 — Fırtına Gezegeni** — elektrikli meteorlar
- [ ] **5.4 — Kristal Gezegeni** — yoğun maden kullanımı
- [ ] **5.5 — Asal Krallığı** — tüm mekaniklerin birleşimi

- [ ] **5.6 — Sonsuz Yolculuk modu**
  Asal Krallığı tamamlandıktan sonra açılır. Seçim ekranı (yüksek skor gösterimi),
  oyun ekranında rekor + toplam skor, tüm gezegen mekanikleri aynı anda aktif.

---

## 📋 Faz 6 — Cila ve Teslim

- [ ] **6.1 — `.asmdef` ile modül ayrımı** (derleme süresi + bağımlılık yönü)
- [ ] **6.2 — Meteor bölünme limiti**
  GDD "bir meteor en fazla 5 asal çarpana ayrılabilir" diyor; sayı havuzu
  bunu aşan sayı üretmemeli (veya bölünme kısıtlanmalı). Doğrulanacak.
- [ ] **6.3 — WebGL build optimizasyonu ve test**
- [ ] **6.4 — Responsive kontrol** — 1080×1920 dışı oranlar, akıllı tahta
- [ ] **6.5 — Erişilebilirlik** — renk körlüğü için madenlerde simge desteği,
  yüksek kontrast, büyük dokunmatik butonlar
- [ ] **6.6 — Fonksiyonel test geçişi** (GDD EK-2.9 kontrol listesi)
- [ ] **6.7 — `SilinecekManager`'ı sil**
  Geçici test aracı (`Assets/Scripts/Runtime/SilinecekManager.cs` + sahnedeki
  objesi). Hiçbir sistem referans vermiyor, silmek yeterli.

---

## Önerilen Sıra

**Faz 1 → 2 → 3 → 4 → 5 → 6**

Gerekçe: Faz 1 bitmeden oyun kaybedildikten sonra tekrar başlamıyor ve gezegen
verileri eksik olduğu için eğitsel omurga fiilen çalışmıyor. Faz 2 oyunu hedef
platformda oynanabilir hale getiriyor
(dokunmatik olmadan mobil/tahta testi imkânsız). Faz 3 ve 4 GDD'nin kalan yarısı;
madenler gameplay'i zenginleştirdiği için ekran akışından önce gelmesi playtest
açısından daha verimli. Faz 5 ve 6 içerik derinliği ve teslim cilası.
