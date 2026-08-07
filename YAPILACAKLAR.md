# Yapılacaklar — Asal Yolculuk

Son güncelleme: 2026-08-07 (ölüm sonrası yeniden başlatma akışı tamamlandı — Faz 1 bitti)

Bu dosya projenin tamamlanmamış işlerini önceliklendirilmiş şekilde listeler.
Mevcut durum için `SCRIPT_OZETI.md`, hedef tasarım için `AsalYolculuk_Senaryo.md`
ve `CLAUDE.md` referans alınır.

---

## Mevcut Durum Özeti

Çekirdek gameplay döngüsü kapandı: bölüm kazanma, kaybetme ve iki durumdan da
devam etme çalışıyor. Meta katman (ekranlar, kayıt, madenler) ve dokunmatik
input henüz yok.

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
| Oyun sonu paneli içeriği (asal/puan sayaçları, ana menü butonu) | ✅ Bitti |
| Toplam puan + keşfedilen asal takibi (`PlayerPrefs`) | ✅ Bitti |
| Oyun sonu kutlaması (konfeti, sırayla rastgele noktalarda) | ✅ Bitti |
| `PlanetDataSO` × 8 (min/max, sprite, öncelikli sayılar) | ✅ Bitti |
| Oyun sonu paneli (`GameOverPanelView`) + gezegen bilgisi | ✅ Bitti |
| Ölüm sonrası yeniden başlatma + ana menüye dönüş | ✅ Bitti |
| Bölüm kaybında puanın toplamdan geri alınması | ✅ Bitti |
| Maden (PowerUp / Curse) sistemi | ❌ Yok |
| Ekran akışı (Ana Menü, Harita, Yolculuk, Sonuç) | ❌ Yok |
| Kayıt sistemi (`SaveManager`) | ❌ Yok |
| Dokunmatik input | ❌ Yok |
| Ses sistemi | ❌ Yok |

Sahne durumu: `Assets/Scenes/GamePlay.unity` (oyun) ve `Assets/Scenes/MainMenu.unity`.
İkisi de Build Settings'te ekli. **MainMenu şu an GamePlay'in kopyası** — bitiş
panelindeki "Başla" butonu buraya dönüyor, içeriği 4.4'te yazılacak.

---

## ✅ Faz 1 — Bölüm Döngüsünü Kapat (Tamamlandı)

Bölüm kazanma, kaybetme ve her ikisinden de devam etme akışı çalışıyor.

- [x] **1.1 — Gemiyi diriltme** ✅
  `PlayerDeathEffect.RestoreShip()` eklendi. Renderer ve collider'lar `Awake`'te
  kaydedilen **başlangıç durumlarına** döner (körlemesine hepsi açılmaz — tasarım
  gereği kapalı duran bir çocuk, ileride kalkan bariyeri gibi, yanlışlıkla görünür
  olmasın). Gemi dönerken `ShipCollisionHandler.GrantInvulnerability` ile
  1.5 sn dokunulmaz olur.
  Ayrıca `OnDeathSequenceCompleted` event'i eklendi: ölüm sekansının süresi
  sahnedeki meteor sayısına göre değiştiği için panel sabit gecikme yerine bunu
  bekler.
  *Dosyalar:* `Player/PlayerDeathEffect.cs`, `Player/ShipCollisionHandler.cs`

- [x] **1.2 — `LevelFlowController.RestartLevel()` + `GoToMainMenu()`** ✅
  `RestartLevel()` sahne değiştirmeden, seviye ilerletmeden bölümü baştan kurar.
  Geçiş **Mask Transitions** paketiyle: ekran kapanır → kapalıyken sıfırlama
  yapılır (oyuncu geminin ışınlanmasını görmez) → ekran açılır.
  `GoToMainMenu()` `TransitionManager.LoadLevel` ile `MainMenu` sahnesini yükler.
  `ResetLevelProgress` artık `revertLevelScore` parametresi alıyor; bölüm
  kaybında `ScoreManager.RevertLevelScoreFromTotal()` ile o bölümün puanı kalıcı
  toplamdan geri alınır (ölüp tekrar oynayarak puan şişirme kapandı). Keşfedilen
  asallar korunur.
  *Dosyalar:* `Core/LevelFlowController.cs`, `Core/ScoreManager.cs`

- [x] **1.3 — Oyun sonu paneli** ✅
  `UI/GameOverPanelView.cs`: `LevelCompletePanelView` ile aynı konumlandırma
  deseni, sağdan sola girer. Kaldığın gezegenin adı ve sprite'ı doldurulur,
  `hideOnGameOver` dizisindeki sahne nesneleri gizlenir, `Gameplay`'e dönülünce
  geri gelir. Butonlar: **Tekrar Dene** → `RestartLevel()`,
  **Ana Menü** → `GoToMainMenu()`.
  *Not:* Panelde "ulaşılan puan" göstergesi yok — gerekirse sonradan eklenir.
  *Dosyalar:* `UI/GameOverPanelView.cs` (yeni)

- [x] **1.4 — Oyun sonu paneli içeriği** ✅
  `UI/BitisManager.cs` tamamlandı: son gezegende sağdan geliyor, yolculuk özeti
  animasyonunu (gezegen → çizgi → gezegen) oynatıyor, ardından keşfedilen asal
  sayısı ve toplam puan panelleri sayaç animasyonuyla açılıyor, son olarak nabız
  atan "Başla" butonu geliyor ve ana menü sahnesine dönüyor.
  Eğitsel sayaç `ScoreManager.RegisterDiscoveredPrimes` ile **oyun boyunca**
  toplanıyor; toplam puan `PlayerPrefs` üzerinde kalıcı.
  Kutlama efekti de eklendi: gezegenler açılmaya başlarken 10 konfeti efekti
  sırayla, ekranın rastgele noktalarında, gezegen fazına yayılmış aralıklarla
  çıkıyor (`celebration*` alanları).
  *Kalan başka faza taşındı:* "Sonsuz Yolculuk açıldı" butonu → 5.6.

- [x] **1.5 — Gezegen sprite'ları** ✅
  8 asset'in de `planetSprite` alanı dolu.

- [x] **1.6 — `priorityNumberList` doldurma (8 gezegen)** ✅
  Listeler `PlanetDataGenerator.cs` içine veri olarak girildi; **Tools > Paye Games >
  Asal Yolculuk > Generate Planet Data** menüsü 8 asset'e birden yazıyor.
  Sayılar gezegen temasına göre kurgulandı (Lav asal ağırlıklı, Kara Delik yarı-asal,
  Buz/Fırtına/Kristal çok kademeli bölünme). Araç üretim sırasında her sayıyı
  aralık ve "en fazla 5 asal çarpan" kuralına karşı doğrulayıp uyarı basıyor.
  **Not:** Liste tamamen değiştirilir — Inspector'dan elle düzenleme yeniden
  üretimde kaybolur, kalıcı değişiklik generator'daki dizilerden yapılır.

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
  *Not:* Toplam puan şimdilik `ScoreManager` içinde doğrudan `PlayerPrefs`
  (`AsalYolculuk.TotalScore`) ile tutuluyor; buraya taşınacak.
  *Dosyalar:* `Core/SaveManager.cs`, `Data/SaveData.cs` (yeni)

- [ ] **4.2 — `GameConfigSO`**
  `targetScorePerPlanet`, `initialPlayerLives`, `scorePerMeteorDestroyed`,
  `powerUpDuration`, drop rate'ler. Şu an bu değerler script'lere dağılmış durumda.
  *Dosyalar:* `Data/GameConfigSO.cs` (yeni)

- [ ] **4.3 — `UIManager` / `ScreenController`**
  Ekranlar arası geçiş yönetimi.

- [ ] **4.4 — Ana Menü ekranı**
  Oyuna Başla / Devam Et / Sonsuz Yolculuk / Ses aç-kapa.
  `Assets/Scenes/MainMenu.unity` oluşturuldu ve Build Settings'e eklendi ama
  içeriği hâlâ GamePlay kopyası; temizlenip menü arayüzü kurulacak.
  Bitiş panelindeki "Başla" butonu (`BitisManager.mainMenuSceneName`) buraya
  yönleniyor.

- [ ] **4.5 — Harita ekranı**
  8 gezegen; tamamlananlar işaretli, kilitliler gri, aktif olan parlayan çerçeveli.
  `İLERLE` butonu. `PlanetManager` + `LevelManager.SetLevel` buraya bağlanacak.
  *Görseller:* `Assets/Arts/Asal_0/gezegenler/` (6 adet var, 8 gerekiyor)

- [ ] **4.6 — Yolculuk (geçiş) ekranı**
  Solda mevcut gezegen, sağda hedef gezegen, ortada gemi + yıldız animasyonu.

- [ ] **4.7 — Bölüm Tamamlandı ekranı + kutlama efekti**
  Havai fişek animasyonu (`Prefabs/Effects/FireworkBlue.prefab`, `confetti.prefab` hazır),
  tamamlanan gezegen adı, toplam puan, açılan yeni gezegen, Devam Et butonu.
  *Not:* Oyun sonu paneli (`BitisManager`) tarafı bitti — konfeti orada çalışıyor;
  burada kalan iş yalnızca **bölüm sonu** ekranının kendisi ve efektin ona bağlanması.

- [ ] **4.8 — In-scene panelleri gerçek ekranlarla değiştir**
  `LevelCompletePanelView`, `GameOverPanelView` ve `BitisManager`, tam ekran akışı
  geldiğinde `ScreenController` yönetimine taşınacak. Panellerin gezegen bilgisi
  doldurma mantığı (`FillPlanetInfo`) yolculuk ekranında aynen kullanılabilir.
  `GameOverPanelView.GoToMainMenu` şu an doğrudan sahne yüklüyor; ekran akışı
  gelince buraya bağlanacak.

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
  Bitiş paneline "Sonsuz Yolculuk açıldı" butonu bu modla birlikte eklenecek
  (1.4'ten taşındı); `BitisManager`'daki özet panel dizisine yeni bir slot olarak
  girer.

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
  objesi). Başlangıç bölümü seçimi, **F9** bölümü tamamlar, **F10** canları
  bitirir. Hiçbir sistem referans vermiyor, silmek yeterli.

---

## Önerilen Sıra

**Faz 2 → 3 → 4 → 5 → 6** (Faz 1 tamamlandı)

Gerekçe: Faz 1 bittiği için oyun baştan sona oynanabiliyor; sıradaki darboğaz
hedef platform. Faz 2 oyunu oynanabilir hale getiriyor
(dokunmatik olmadan mobil/tahta testi imkânsız). Faz 3 ve 4 GDD'nin kalan yarısı;
madenler gameplay'i zenginleştirdiği için ekran akışından önce gelmesi playtest
açısından daha verimli. Faz 5 ve 6 içerik derinliği ve teslim cilası.
