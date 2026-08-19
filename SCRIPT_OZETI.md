# Script Özeti

Bu dosya, projedeki script'lerin ne işe yaradığını ve birbirleriyle nasıl
bağlantılı olduğunu takip etmek için tutulur. Yeni bir script tamamlandığında
buraya kısa bir madde eklenir.

---

## Player

### `PlayerShipController.cs`
`Assets/Scripts/Runtime/Player/`

Gemiyi klavyeden (yeni Input System, kod içinde tanımlı WASD + ok tuşları
composite'i) **ve ekrandaki yön butonlarından** hareket ettirir. Transform tabanlı, `SmoothDamp` ile yumuşatılmış
hareket (Rigidbody yok). `Camera.main` ile kamerayı otomatik bulur (Inspector'dan
atama yok). Yatayda `minViewportX` / `maxViewportX` ile hareket alanı sınırlanır
(0 = ekranın solu, 1 = ekranın sağı); dikeyde tüm ekran serbest ama `ClampToScreen`
konumu her karede ekran içinde tutar. `Update` başında
`GameManager.IsGameplayActive` guard'ı vardır; bölüm bitince kontrol kapanır ve
hız alanları sıfırlanır (bölüm sonu tween'i input ile çakışmasın).

Geminin hızı (`currentVelocity`) ile `SmoothDamp`'in iç tamponu (`smoothVelocity`)
**ayrı** alanlardır. Önceden aynı değişken ikisi için de geçiriliyordu; `SmoothDamp`
kendi durumunu geminin hızının üstüne yazdığı için yumuşatma bozuluyordu.

**Yerçekimi Taşı desteği:** `SetMoveSpeedMultiplier` / `ResetMoveSpeedMultiplier`
hareket hızını, `SetDownwardPull` / `ResetDownwardPull` sürekli aşağı çekimi
ayarlar. Çekim iki koruma altındadır:
- `GetClampedPull` çekimi geminin **güncel** hızının `MaxPullRatio` (%70) kadarıyla
  sınırlar; aksi halde çekim hızı geçer, gemi hiç yukarı çıkamaz ve ceza felce döner.
- Çekim `SmoothDamp`'in **dışında** doğrudan konuma uygulanır. Hız vektörüne
  eklenseydi yumuşatma onu her karede geri emer, etki hissedilmezdi.

**Buton girdisi:** `SetButtonMoveInput(Vector2)` ile `TouchMovementPanel` her karede
yön vektörünü iletir. Klavye ve buton girdisi **toplanıp** `ClampMagnitude(…, 1f)`
ile sınırlanır — ikisi aynı anda basılınca gemi hızlanmaz. Oynanış dışına
çıkıldığında buton girdisi de sıfırlanır.

- **Bağlantı:** `GameManager` (guard), `GravityStoneEffect` (hız çarpanı + çekim),
  `TouchMovementPanel` (buton girdisi).

### `TouchMovementPanel.cs`
`Assets/Scripts/Runtime/Player/`

Yön butonlarının bulunduğu panelin **kendi üzerine** eklenir. Inspector'dan
`PlayerShipController` ve dört buton (`leftBtn` / `rightBtn` / `upBtn` / `downBtn`)
bağlanır. `Awake`'te her butona `HoldableButton` bileşenini **otomatik ekler**
(`EnsureHoldable`), böylece sahne kurulumunda elle eklemek unutulmaz. Her karede
basılı butonlardan bir `Vector2` üretip `shipController.SetButtonMoveInput` ile
iletir.

**Görünürlük:** `GameManager.OnGameStateChanged`'e abone olur; `Gameplay`'de panel
`SetActive(true)`, bölüm bitince `SetActive(false)` olur (`Paused` durumunda
dokunulmaz — score/health panelleriyle aynı davranış). Abonelik düz bir C# event'i
olduğu için GameObject kapalıyken de handler çalışır; panel kendini geri açabilir.
`OnDisable` gemiye sıfır girdi gönderir — buton basılıyken bölüm biterse gemi
kaymaya devam etmez.

- **Bağlantı:** `PlayerShipController`, `HoldableButton`, `GameManager`.

### `HoldableButton.cs`
`Assets/Scripts/Runtime/Player/`

Unity `Button`'ı yalnızca "tıklama" verdiği için yön kontrolünde gereken **basılı
tutma** durumunu takip eden küçük bileşen. `IPointerDownHandler` /
`IPointerUpHandler` / `IPointerExitHandler` uygular ve `IsPressed` özelliğini
açar. Parmak butondan kayarsa (`OnPointerExit`) veya nesne kapanırsa
(`OnDisable`) basılı durum düşer.

- **Bağlantı:** `TouchMovementPanel` (okuyan taraf).

### `ShipCollisionHandler.cs`
`Assets/Scripts/Runtime/Player/`

Gemiye çarpan meteoru yakalar. Meteorun collider'ı trigger, geminin dinamik
`Rigidbody2D`'si olduğu için trigger olayı **gemi tarafında** tetiklenir; hasar
sorumluluğu bilinçli olarak burada tutulur (meteor kime çarptığını bilmez).
Çarpışmada `meteor.DespawnOnShipHit()` ve `HealthManager.Instance.LoseLife()`
çağrılır. `invulnerableDuration` (varsayılan 1.5 sn) boyunca sonraki çarpışmalar
yok sayılır — üst üste gelen meteorlar tek karede birden fazla can götürmesin.
`GrantInvulnerability(duration)` aynı pencereyi dışarıdan açar (mevcut pencere
daha uzunsa kısaltmaz); gemi yeniden doğduğunda `PlayerDeathEffect.RestoreShip()`
bunu çağırır.
Oyun `Gameplay` durumunda değilse (`GameManager.IsGameplayActive`) hasar alınmaz.

**Hasar geri bildirimi:** Her hasarda `CameraShake.Instance.Shake(shakeDuration,
shakeStrength)` çağrılır. Ardından **oyuncu hâlâ hayattaysa** (`HealthManager
.IsAlive`) `damageEffectPrefab` geminin **child'ı** olarak oluşturulur — gemi
hareket ederken efekt de onunla gider. Son canda hasar efekti bilinçli olarak
oynatılmaz; patlama sekansını `PlayerDeathEffect` devralır. Yani 1. ve 2.
çarpışmada hasar efekti, 3.'te ölüm sekansı çalışır.

**Koruma Kalkanı:** `SetShieldActive(bool)` `shieldVisual` objesini açıp kapatır
ve `isShieldActive` bayrağını çevirir. Kalkanlıyken çarpan meteor yine patlar
ama **ne can eksilir ne de dokunulmazlık penceresi başlar** — kalkan zaten
korurken 1.5 sn'lik pencereyi harcamanın anlamı yok, kalkan bitince oyuncu tam
korumayla devam eder. Meteorlar normalde geminin dışındaki `ShieldBarrier`'da
patlar; buradaki kontrol, bariyeri tek karede atlayan hızlı meteorlar için
güvenlik ağıdır. Bariyer objesi sahnede **kapalı** başlamalıdır (`PlayerDeathEffect`
başlangıç durumlarını kaydettiği için, açık başlarsa gemi her dirilişte kalkanlı
görünür).

- **Bağlantı:** `MeteorController` (`DespawnOnShipHit`), `HealthManager`
  (can eksiltme + `IsAlive` kontrolü), `GameManager` (durum kontrolü),
  `CameraShake`, dolaylı olarak `PlayerDeathEffect`, `ShieldEffect`
  (`SetShieldActive`), `ShieldBarrier` (açılan obje).

### `PlayerDeathEffect.cs`
`Assets/Scripts/Runtime/Player/`

Gemiye eklenir, ölüm sekansının tamamını yönetir. `HealthManager.OnAllLivesLost`
event'ine `Start`'ta abone olur (manager'ın `Awake`'i garanti çalışmış olsun diye),
`OnDestroy`'da bırakır. `Awake`'te renderer/collider dizilerinin yanı sıra bunların
**başlangıç `enabled` durumları** ve `PlayerShipController` / `PlayerAutoShooter` /
`ShipCollisionHandler` referansları da cache'lenir. Sekans tek bir coroutine zinciridir:

1. **`HideShip()`** — gemi ve alt objelerindeki tüm `Renderer` ve `Collider2D`
   bileşenleri kapatılır, `PlayerShipController` ve `PlayerAutoShooter` devre
   dışı bırakılır. `gameObject.SetActive(false)` **bilinçli olarak kullanılmaz**:
   patlama coroutine'i bu obje üzerinde çalışır, obje kapanırsa sekans ilk
   patlamadan sonra durur.
2. **`PlayExplosionSequence()`** — `explosionCount` (varsayılan 6) kez patlama
   efekti; ilki tam geminin üzerinde, sonrakiler `explosionRadius` yarıçapında
   rastgele yakın noktalarda, `explosionInterval` aralıkla.
3. `meteorClearDelay` beklemesi.
4. **`meteorFieldClearer.ClearRoutine()`** — sahne temizliği. Bu mantık önce bu
   sınıfın içindeydi; bölüm tamamlandığında da gerektiği için `MeteorFieldClearer`
   bileşenine taşındı ve buradan `[SerializeField]` referansla çağrılıyor
   (referans bağlı değilse temizlik atlanır, patlama sekansı yine çalışır).
5. **`OnDeathSequenceCompleted`** event'i yayınlanır.

**`public event Action OnDeathSequenceCompleted`** — sekansın tamamı bittiğinde
tetiklenir; `GameOverPanelView` panelin ne zaman açılacağını buradan öğrenir.
Sabit gecikme kullanılamaz: `ClearRoutine()`'in süresi sahnedeki meteor sayısına
göre değişir, panel ya patlamaların üstüne biner ya da geç kalırdı. Temizleyici
referansı bağlı olmasa bile event tetiklenir — aksi halde panel hiç açılmaz ve
oyun boş ekranda kilitlenirdi.

**`public void RestoreShip()`** — `HideShip()`'in tersi; bölüm yeniden başlatılırken
`LevelFlowController` çağırır. Renderer ve collider'lar `Awake`'te kaydedilen
**başlangıç durumlarına** döner (körlemesine hepsi açılmaz; tasarım gereği kapalı
duran bir çocuk — ileride kalkan bariyeri gibi — yanlışlıkla görünür olmasın).
Controller'lar açılır ve `ShipCollisionHandler.GrantInvulnerability` ile gemi
`respawnInvulnerableDuration` (varsayılan 1.5 sn) boyunca dokunulmaz olur.

- **Bağlantı:** `HealthManager` (`OnAllLivesLost`), `MeteorFieldClearer`
  (sahne temizliği), `PlayerShipController`, `PlayerAutoShooter`,
  `ShipCollisionHandler` (`GrantInvulnerability`), `GameOverPanelView` ve
  `LevelFlowController` (dinleyici / çağıran).

### `MineCollector.cs`
`Assets/Scripts/Runtime/Player/`

Geminin üzerinden geçen madeni toplar: `MineController.Collect()` çağırır.
`ShipCollisionHandler`'dan **ayrı bir bileşen** olması bilinçlidir — o sınıfın
sorumluluğu hasar almaktır ve bir dokunulmazlık penceresi vardır; maden toplama
ondan bağımsız çalışmalıdır (kalkanlıyken ve hasar aldıktan hemen sonra da maden
toplanabilmeli). Geminin mevcut `Collider2D`'sini kullanır, ek alan yoktur.

- **Bağlantı:** `MineController` (`Collect`), `GameManager` (guard).

### `ShieldBarrier.cs`
`Assets/Scripts/Runtime/Player/`

Koruma Kalkanı bariyerinin çarpışma tarafı. Gemi altında ayrı bir obje olarak
durur ve **yalnızca kalkan aktifken açıktır** — bu yüzden "kalkan açık mı"
kontrolü yapmaz; objenin kendisi kalkanın anahtarıdır. Kendi `CircleCollider2D`'
sine giren meteoru `DespawnOnShipHit()` ile patlatır (puan verilmez).

Bariyerin yarıçapı gemininkinden büyük olduğu için meteor buraya önce değer,
gemi collider'ına ulaşamadan yok olur. **Gemi collider'ı bilinçli olarak açık
bırakılır:** `MineCollector` onu kullanıyor, kapatılsaydı kalkanlıyken maden
toplanamazdı.

*Not:* Kullanılan hazır paket 3D için hazırlanmıştır; üzerindeki `SphereCollider`
2D fizikle **hiç** etkileşmez. Bariyer objesinde `CircleCollider2D` (trigger)
olmalıdır.

- **Bağlantı:** `MeteorController` (`DespawnOnShipHit`), `GameManager` (guard),
  `ShipCollisionHandler` (objeyi açıp kapatan).

### `ShieldMatEffect.cs`
`Assets/Scripts/Runtime/Player/`

Kalkan materyalinin `_Offset` vektör özelliğini sürekli kaydırarak akan enerji
görüntüsü üretir. Salt görseldir.

Üç noktaya dikkat edilmiştir:
- **`sharedMaterial` değil `material`** kullanılır; `sharedMaterial` materyal
  **asset'ini** kalıcı değiştirir ve Play'den çıkınca offset kaymış kalırdı.
  Üretilen kopya `OnDestroy`'da yok edilir (sızıntı olmasın).
- Offset `% 1` ile sarmalanır; sürekli artan `float` dakikalar sonra hassasiyet
  kaybedip titremeye başlardı.
- Özellik adı serialize edilir ve `HasProperty` ile denetlenir; yoksa Console'a
  uyarı yazılır. Hologram shader'ında doğru ad **`_Offset`**'tir — bu, doku
  ayarlarındaki tiling/offset **değil**, shader'ın kendi vektör özelliğidir
  (`SetTextureOffset` bu materyalde hiçbir şey yapmaz).

- **Bağlantı:** Yok (bağımsız görsel bileşen).

### `PlayerLevelCompleteEffect.cs`
`Assets/Scripts/Runtime/Player/`

Geminin **kök objesine** eklenir (bir child'a eklenirse oyun sonundaki
`SetActive(false)` yalnızca o child'ı kapatır). `GameManager.OnGameStateChanged`
dinler ve üç duruma ayrılır:

- **`LevelComplete` + ara bölüm** — gemiyi DOTween `DOMove` ile
  `levelCompletePosition` (varsayılan `(0, -5)`) konumuna `moveDelay` gecikmeyle
  yumuşakça taşır. Gemi kontrolü `PlayerShipController` içindeki guard'la zaten
  kapalı olduğu için input tween'i bozmaz.
- **`LevelComplete` + son gezegen** (`LevelManager.IsLastLevel`) —
  `FlyAwayAndDisappear()`: gemi `InBack` ease ile önce hafif geri çekilip
  ekranın sağından dışarı fırlar, aynı anda `exitScaleRatio` kadar küçülerek
  uzaklaşır; tween bitince `gameObject.SetActive(false)` ile tamamen kaybolur.
  Çıkış X'i `Camera.main` viewport'undan hesaplanır (sabit koordinat yok).
- **`Gameplay`** — tween öldürülür, obje tekrar aktif edilir, gemi `Awake`'te
  cache'lenen başlangıç konumuna **ve ölçeğine** döner.

Durum aboneliği düz bir C# event'i olduğu için obje pasifken de canlı kalır;
oyun sonunda kaybolan gemi yeniden başlatmada geri gelebilir.

- **Bağlantı:** `GameManager` (durum event'i), `LevelManager` (`IsLastLevel`),
  `PlayerShipController` (dolaylı — guard sayesinde çakışma olmaz), DOTween.

---

## Pooling

### `IPoolable.cs`
`Assets/Scripts/Runtime/Pooling/`

`GameObjectPool` tarafından yönetilen nesnelerin uyması gereken arayüz:
`OnSpawned(GameObjectPool owner)` ve `OnDespawned()`.

- **Bağlantı:** `GameObjectPool` bu arayüzü implemente eden bileşenleri çağırır.
  Şu an `Bullet.cs` bu arayüzü uyguluyor.

### `GameObjectPool.cs`
`Assets/Scripts/Runtime/Pooling/`

Genel amaçlı, tek bir prefab'ı havuzlayan component (Unity'nin hazır
`UnityEngine.Pool.ObjectPool<T>` altyapısını sarmalar). Prefab, başlangıç
kapasitesi ve maksimum boyut Inspector'dan ayarlanır. Aynı script farklı
sahne nesnelerine eklenip farklı prefab atanarak (mermi, ileride meteor gibi)
tekrar kullanılabilir.

- **Bağlantı:** `IPoolable` — spawn/despawn anında ilgili nesnenin arayüz
  metotlarını çağırır. Şu an `PlayerAutoShooter.cs` bu pool'dan mermi alıyor.

---

## Combat

### `Bullet.cs`
`Assets/Scripts/Runtime/Combat/`

`IPoolable` uygular. Transform tabanlı, `selfTransform.right` yönünde
(soldan sağa) hareket eder. `lifetime` süresi dolunca kendini `ownerPool`
üzerinden pool'a iade eder. Henüz meteor çarpışma/hasar mantığı yok
(meteor script'i geldiğinde eklenecek).

- **Bağlantı:** `GameObjectPool` (spawn eden owner referansı), `IPoolable`.

### `PlayerAutoShooter.cs`
`Assets/Scripts/Runtime/Combat/`

Gemiye eklenen otomatik ateş component'i. `fireRate` (saniyede kaç atış)
aralığında `Fire()` çalışır.

**Çoklu atış:** Tek `firePoint` yerine `firePoints` **dizisi** tutulur.
Normalde yalnızca **Element 0** (merkez) ateş eder; Plazma Yakıtı etkisi
`SetActiveFirePointCount(3)` ile diğerlerini de açar, `ResetFirePointCount()`
tekrar 1'e döner. Dizide o kadar nokta yoksa var olan sayıyla sınırlanır.

Merminin yönü noktanın **kendi rotasyonundan** gelir (`Bullet` kendi
`transform.right` yönünde ilerler), yani yelpaze açısı Inspector'da noktalar
döndürülerek ayarlanır — kodda açı hesabı yoktur. Nokta sayısını artırmak için
diziye eleman eklemek ve `MineDataSO.Amount`'u büyütmek yeterlidir.

`Update` başında `GameManager.IsGameplayActive` guard'ı vardır; canlar bitince
ateş durur.

- **Bağlantı:** `GameObjectPool` (hangi pool'dan mermi alacağını belirler),
  dolaylı olarak `Bullet.cs` (pool'un ürettiği prefab), `GameManager` (guard),
  `PlasmaFuelEffect` (atış noktası sayısı).

---

## Core

### `PrimeMathEngine.cs`
`Assets/Scripts/Runtime/Core/`

Statik matematik motoru. Başlangıçta (static constructor) Sieve of Eratosthenes
ile 10.000'e kadar asal sayıları `HashSet<int>` içinde önbelleğe alır.
`IsPrime(int)` (cache üstünde O(1), üstünde ise fallback bölme kontrolü),
`GetPrimeFactors(int)` ve `GetDivisionPair(int)` (bileşik sayıyı iki çarpana
ayırır) metotlarını sağlar.

- **Bağlantı:** `MeteorController` sayı atanırken `IsPrime`, bileşik meteoru
  parçalarken `GetPrimeFactors` çağırır. `GetDivisionPair` henüz kullanılmıyor;
  ileri gezegenlerdeki "önce iki ara çarpana, sonra asallara" bölünmesi için
  hazır bekliyor.

### `LevelManager.cs`
`Assets/Scripts/Runtime/Core/`

Tek sahne içinde hangi seviyede/gezegende olunduğunu tutan singleton
(`LevelManager.Instance`, dar kapsamlı — DI yerine bilinçli tercih edildi).
`planetDataList` (Inspector'dan atanan `PlanetDataSO[]`) ve `currentLevelIndex`
üzerinden `CurrentPlanetData` döner (aralık dışı index'te clamp'lenir, oynanış
bozulmaz). `SetLevel(int)` ile seviye ilerletilir.

**Kaldığı bölümden devam:** `Awake` içinde `LoadSavedLevel()` çalışır ve
`PlanetProgress.MaxUnlockedIndex` bölüm index'i olarak yüklenir; oyuncu 2.
bölümdeyken oyunu kapatıp açtığında 1. bölüme dönmez. Ayrı bir "kalınan bölüm"
kaydı **bilinçli olarak tutulmaz** — kilitler sırayla açıldığı için açılmış en
yüksek gezegen zaten oyuncunun henüz bitirmediği bölümdür; ikinci bir anahtar
iki kaydın birbirinden ayrı düşme riskini getirirdi. Inspector'daki
`currentLevelIndex` yalnızca editör varsayılanıdır, oyunda üzerine yazılır.

- `IsLastLevel` — son gezegende miyiz. Bölüm sonu paneli ve `LevelFlowController`
  bu bayrakla son bölümde farklı akışa dallanır.
- `NextPlanetData` — bir sonraki gezegen (son gezegende `null`). Bölüm sonu
  panelindeki "hedef gezegen" bilgisi buradan okunur.
- `GetPlanetData(int)` — aralık dışında `null` döner (clamp **yapmaz**), böylece
  "sonraki gezegen var mı" sorusu doğru cevaplanır.

- **Bağlantı:** `PlanetDataSO` (veri kaynağı), `MeteorSpawner`
  (`CurrentPlanetData`), `LevelFlowController` (`SetLevel`),
  `LevelCompletePanelView` (`CurrentPlanetData` + `NextPlanetData`),
  `PlanetProgress` (açılışta kaldığı bölümü okur).

### `ScoreManager.cs`
`Assets/Scripts/Runtime/Core/`

Bölüm puanını tutan singleton (`ScoreManager.Instance`). `targetScore`
(GDD'ye göre 100) ve `scorePerMeteor` (5) Inspector'dan ayarlanır.
`AddMeteorScore()`, `AddScore(int)` ve `ResetScore()` metotlarını sağlar;
her değişimde `OnScoreChanged(currentScore, targetScore)` event'ini yayar.
Puan mantığı UI'dan tamamen bağımsızdır — ekrana yazma işini dinleyen taraf
yapar. `AddScore` negatife düşmeyi engeller (ileride ceza madenleri puan
düşürebilir).

**Bölüm tamamlama tetikleyicisi:** Puan `targetScore`'a ulaşınca `OnTargetReached`
bir kez yayınlanır (`isTargetReached` bayrağı). Bayrak olmadan, bölüm bittikten
sonra havadaki mermilerin vurduğu meteorlardan gelen puanlar event'i tekrar
tetikliyordu. `ResetScore()` bayrağı da sıfırlar — aksi halde yeni bölüm anında
tamamlanmış sayılırdı.

**Toplam puan (kalıcı):** `TotalScore`, tüm oynayışlar boyunca birikir ve
`PlayerPrefs` (`AsalYolculuk.TotalScore`) üzerinde saklanır. `AddScore` içinden
**yalnızca pozitif** miktarlar toplama eklenir — ceza puanı toplamı düşürseydi
bitiş ekranındaki sayaç geri sayardı. `PlayerPrefs.SetInt` yalnızca belleği
günceller; diske yazma `SaveTotalScore()` ile bölüm sonunda
(`LevelFlowController`), bitiş panelinde ana menüye dönerken, `OnDestroy`,
`OnApplicationPause` ve `OnApplicationQuit`'te yapılır (WebGL'de her yazma
IndexedDB'ye flush ettiği için sık çağrılmaz). `ResetTotalScore()` normal akışta
kullanılmaz, ileride "ilerlemeyi sıfırla" için vardır.

**Geçici meteor puanı (Altın Kristal):** `SetMeteorScoreOverride(int)` /
`ClearMeteorScoreOverride()` meteor başına verilen puanı geçici olarak sabitler
(5 → 10). `AddMeteorScore()` bayrağa bakıp hangisini kullanacağına karar verir.
Çarpan değil sabit değer kullanılır; GDD "10 puan" diyor ve rakam
`MineDataSO.Amount`'tan gelir. `ResetScore()` override'ı da temizler — bölüm bir
etki aktifken bitse bile yeni bölüm normal puanla başlasın (`PowerUpManager`
zaten etkileri temizliyor, bu ikinci güvence).

**Bölüm kaybında puan geri alma:** `RevertLevelScoreFromTotal()` o bölümde
kazanılan puanı (`CurrentScore`) toplamdan düşer (0'ın altına inmez) ve ardından
`ResetScore()` çağırır. `AddScore` puanı toplama **anında** eklediği için bu
olmadan ölüp tekrar oynayarak toplam puan şişirilebilirdi. Keşfedilen asallar
bilinçli olarak korunur — öğrenme kaydıdır, bölüm kaybıyla silinmez.
`LevelFlowController` hem "Tekrar Dene" hem "Ana Menü" akışında çağırır.

**Keşfedilen asallar:** `RegisterDiscoveredPrimes(rootNumber)` bir meteor tüm
parçalarıyla yok edildiğinde `MeteorController` tarafından çağrılır; kök sayının
asal çarpanları `SortedSet<int>`'e eklenir, yani aynı asal kaç kez çıkarsa çıksın
bir kez sayılır. Ekrandan kaçan veya gemiye çarpan meteorun çarpanları keşfedilmiş
sayılmaz. `DiscoveredPrimeCount` (bitiş panelinde gösterilir) ve sıralı liste
`DiscoveredPrimes` olarak açılır. `ResetScore()` bunları **sıfırlamaz** — bölüm
puanı sıfırlansa da keşif ve toplam puan tüm yolculuk boyunca birikir.

- **Bağlantı:** `MeteorController` (oturum tamamlanınca `AddMeteorScore` +
  `RegisterDiscoveredPrimes`), `PrimeMathEngine` (`GetPrimeFactors`),
  `ScoreHUDView` (`OnScoreChanged` dinler), `GameManager`
  (`OnTargetReached` dinler), `LevelFlowController` (`SaveTotalScore`,
  `ResetScore`, `RevertLevelScoreFromTotal`), `BitisManager`
  (`DiscoveredPrimeCount`, `TotalScore`).

### `HealthManager.cs`
`Assets/Scripts/Runtime/Core/`

Canları tutar **ve** HUD'daki kalp göstergelerini yönetir (kullanıcı isteğiyle
mantık + görsel tek sınıfta). `hearts` (`RectTransform[]`, soldan sağa sıralı)
Inspector'dan atanır; her kalbin `CanvasGroup`'u ve başlangıç ölçeği `Awake`'te
cache'lenir — hot path'te `GetComponent` çağrılmaz. `MaxLives` dizinin uzunluğudur.

- `LoseLife()` — canı bir azaltır, **en sağdaki** kalbi (index = kalan can)
  DOTween ile söndürür: önce `OutBack` ile hafif büyüme (nabız), ardından
  `DOScale(0)` + `CanvasGroup.DOFade(0)` aynı anda. Sequence `SetUpdate(true)`
  ile unscaled çalışır; oyun duraklatılıp `Time.timeScale = 0` olsa bile
  animasyon yarıda kalmaz.
- `ResetHealth()` — canları ve kalplerin scale/alpha değerlerini ilk hâline döndürür.
- Event'ler: `OnLivesChanged(current, max)`, `OnAllLivesLost`.
- `OnDestroy`'da tüm tween'ler `DOKill` edilir (yok olmuş nesneye tween yazılmasın).

- **Bağlantı:** `ShipCollisionHandler` (`LoseLife` çağırır), `GameManager`
  (`OnAllLivesLost` dinler), DOTween.

### `GameState.cs`
`Assets/Scripts/Runtime/Core/`

`CLAUDE.md`'deki durum makinesi enum'u: `MainMenu`, `MapSelection`,
`TravelTransition`, `Gameplay`, `LevelComplete`, `LevelFailed`, `Paused`.

- **Bağlantı:** `GameManager`.

### `GameManager.cs`
`Assets/Scripts/Runtime/Core/`

Oyun durumunu tutan singleton. `Start`'ta iki event'e abone olur (manager'ların
`Awake`'i garanti çalışmış olsun diye):
- `HealthManager.OnAllLivesLost` → `SetState(GameState.LevelFailed)`
- `ScoreManager.OnTargetReached` → `SetState(GameState.LevelComplete)`

Her durum değişiminde `OnGameStateChanged(GameState)` yayınlanır; bölüm sonu
sunumunun tamamı (HUD panellerinin çekilmesi, geminin taşınması, sahnenin
temizlenmesi, bölüm sonu panelinin gelmesi) bu tek event üzerinden dallanır.

**Zaman yönetimi:** `Time.timeScale` yalnızca `GameState.Paused` durumunda 0
yapılır. `LevelFailed`'de zaman **akmaya devam eder**; aksi halde kayan yıldız
arka planı donuyor ve ölüm patlama sekansı ilerlemiyordu. Bölüm bittiğinde
üretim ve ateş zaten `IsGameplayActive` guard'ıyla durduğu için `timeScale`'i
sıfırlamaya gerek yok.

`public static bool IsGameplayActive` — üretim/ateş sistemlerinin `Update`
başında kullandığı guard. Sahnede `GameManager` yoksa `true` döner, böylece
manager'sız test sahneleri bozulmaz. Sadece `timeScale`'e güvenilmemesinin
sebebi: pause/level complete gibi durumlarda da aynı guard iş görür ve niyet
kodda açıkça durur. `OnDestroy`'da `Time.timeScale` 1'e döndürülür.

- **Bağlantı:** `HealthManager` ve `ScoreManager` (bölüm sonu tetikleyicileri),
  `GameState`, `MeteorSpawner` / `PlayerAutoShooter` / `PlayerShipController` /
  `ShipCollisionHandler` (`IsGameplayActive` guard'ı), `MeteorFieldClearer` /
  `LevelCompletePanelView` / `GameplayHUDPanel` / `PlayerLevelCompleteEffect`
  (`OnGameStateChanged` dinleyicileri), `LevelFlowController` (`SetState`).

### `LevelFlowController.cs`
`Assets/Scripts/Runtime/Core/`

Bölüm sonu geçişlerini tek yerde toplayan sınıf; sahnedeki bir yönetici objesine
eklenir. Singleton **değil** — panel butonu Inspector'dan referansla bağlanır.

`GoToNextLevel()` — bölüm sonu panelindeki "Yolculuğa Başla" butonunun
`OnClick`'ine bağlanır. `isTransitioning` bayrağı çift tıklamada seviyenin iki
atlanmasını engeller, `IsLastLevel` guard'ı son gezegende çalışmasını önler.
Önce `levelCompletePanel.HideToLeft(StartNextLevel)` ile panel ekrandan çıkar,
**çıkış bitince** `StartNextLevel()` sırayla şunları yapar:

1. `LevelManager.SetLevel(CurrentLevelIndex + 1)` — ardından
   `PlanetProgress.Unlock(CurrentLevelIndex)` ile yeni gezegenin kilidi kalıcı
   olarak açılır (ana menüdeki seçim paneli bunu okur). Index `SetLevel` sonrası
   okunur, yani clamp'lenmiş değer kaydedilir.
2. `ScoreManager.SaveTotalScore()` + `ResetScore()` — toplam puan diske yazılır
   (oyun beklenmedik kapanırsa en fazla bir bölümlük puan kaybolur), ardından
   bölüm puanı **ve** `isTargetReached` bayrağı sıfırlanır
3. `HealthManager.ResetHealth()` — canlar ve sönmüş kalp görselleri geri gelir
4. `MeteorSpawner.ResetNumberPool()` — tamamlanan sayı listesi yeni bölüme taşınmaz
5. `GameManager.SetState(GameState.Gameplay)` — üretim, ateş ve gemi kontrolü
   açılır; HUD panelleri ve gemi konumu bu durum değişimini dinleyerek toparlanır

Sıralama bilinçlidir: yeni bölüm ancak panel ekrandan tamamen çıktıktan sonra
başlar, yoksa meteorlar panelin arkasından akmaya başlıyordu.

`ResetLevelProgress(bool revertLevelScore)` iki akış tarafından paylaşılır ve
yalnızca puan davranışında ayrışır: bölüm **tamamlandığında** (`false`) kazanılan
puan toplamda kalır, bölüm **kaybedildiğinde** (`true`) `RevertLevelScoreFromTotal()`
ile geri alınır. Can ve sayı havuzu sıfırlaması her iki durumda aynıdır.

**`RestartLevel()`** — oyun sonu panelindeki "Tekrar Dene" butonuna bağlanır.
Sahne değişmez, seviye **ilerlemez**. Akış bir coroutine'dir:
`TransitionManager.PlayStartHalfTransition` ile ekran kapanır → ekran kapalıyken
`RestartCurrentLevel()` çalışır (panel gizlenir, puan geri alınır + sıfırlanır,
canlar ve sayı havuzu resetlenir, `PlayerDeathEffect.RestoreShip()` gemiyi
diriltir, `GameManager.SetState(Gameplay)`) → `PlayEndHalfTransition` ile ekran
açılır. Sıfırlamanın ekran kapalıyken yapılması bilinçlidir: oyuncu geminin
ışınlanmasını ve panelin kaybolmasını görmez. `SetState(Gameplay)` en sonda
çağrılır çünkü HUD panellerini geri getiren ve gemiyi başlangıç konumuna alan
dinleyiciler (`GameplayHUDPanel`, `PlayerLevelCompleteEffect`) bu durumu dinler.

**`GoToMainMenu()`** — oyun sonu panelindeki **ve** duraklatma panelindeki
"Ana Menü" butonuna bağlanır (`PauseManager` bu metodu çağırır, kendi sahne
geçişini yazmaz). Yarım kalan bölümün puanı toplamdan düşülüp diske yazılır,
`SaveCurrentLevel()` ile kalınan bölüm kaydedilir, `Time.timeScale` 1'e alınır
ve `TransitionManager.LoadLevel(mainMenuSceneName)` ile sahne yüklenir. Puanın
burada da geri alınması bilinçlidir; aksi halde kaybedip menüye çıkarak puan
saklamak mümkün olurdu.

`SaveCurrentLevel()` — `PlanetProgress.Unlock(LevelManager.CurrentLevelIndex)`
çağırarak oyuncunun kaldığı bölümü diske sabitler; oyun yeniden açıldığında
`LevelManager` bu değerden başlar. `Unlock` yalnızca ileri gittiği için bölüm
yarıda bırakıldığında ilerleme artmaz, geri de alınmaz.

`TransitionManager` (Mask Transitions paketi) sahnede yoksa iki akış da geçiş
animasyonu olmadan çalışır — `RestartLevel` doğrudan sıfırlar, `GoToMainMenu`
`SceneManager.LoadScene`'e düşer.

- **Bağlantı:** `LevelCompletePanelView` (`HideToLeft`), `GameOverPanelView`
  (`Hide`), `PlayerDeathEffect` (`RestoreShip`), `LevelManager`, `ScoreManager`,
  `HealthManager`, `MeteorSpawner`, `GameManager`, `PlanetProgress`,
  `MaskTransitions.TransitionManager`, `SceneManager`.

### `CameraShake.cs`
`Assets/Scripts/Runtime/Core/`

Kameraya eklenen sarsıntı bileşeni (`CameraShake.Instance`). DOTween
`DOShakePosition` ile çalışır; `Shake()` varsayılan ayarları, `Shake(duration,
strength)` çağrı bazlı değerleri kullanır. Z ekseni sarsılmaz (2D kamerada
derinlik kayması istenmez). Üst üste gelen sarsıntılarda kamera kaymasın diye
her çağrıda önceki tween `DOKill` edilip pozisyon `Awake`'te cache'lenen
`basePosition`'a çekilir, tween bitiminde de aynı pozisyona sıfırlanır.

- **Bağlantı:** `ShipCollisionHandler` (hasar anında çağırır), DOTween.

### `SoundManager.cs`
`Assets/Scripts/Runtime/Core/`

Ses altyapısının tek giriş noktası. Singleton + `DontDestroyOnLoad` ile sahneler
arası yaşar (`SoundManager.Instance`); `[RequireComponent(typeof(AudioSource))]`
ile gelen kaynak arka plan müziğini, Inspector'dan bağlanan `dogruSource` /
`yanlisSource` ise SFX'leri çalar.

Ayarlar `PlayerPrefs` üzerinde saklanır: `music_on` ve `sfx_on` (1 = açık,
0 = kapalı). `Awake`'te okunur, `SetMusic` / `SetSfx` / `ToggleMusic` /
`ToggleSfx` ile değiştirilip anında diske yazılır. Durum dışarıya `MusicOn` ve
`SfxOn` property'leri olarak açılır.

Müzik kapatılırken `Stop()` değil `Pause()` / `UnPause()` kullanılır — müzik
kaldığı yerden devam eder. `Awake`'teki `source.Stop()` + `playOnAwake = false`,
Inspector'da Play On Awake açık kalsa bile PlayerPrefs kararının uygulanmasını
garanti eder.

`DogruSesiCikar()` / `YanlisSesiCikar()` SFX kapalıysa veya clip bağlı değilse
sessizce çıkar (guard clause).

**`PlaySfx(AudioClip)`** — oynanış olaylarının tek seferlik sesi için genel
giriş. Klip çağrı bazlı geldiği için ortak bir kaynak (`gameplaySource`,
Inspector'dan bağlanır) üzerinden `PlayOneShot` yapılır; olay başına ayrı
`AudioSource` tutmak sahneyi gereksiz şişirirdi. Şu an yalnızca maden toplama
(`MineDataSO.pickupClip`) kullanıyor; ateş, meteor vurma, can kaybı ve bölüm
tamamlama **henüz bağlanmadı** — bkz. `YAPILACAKLAR.md` 2.3.

**`static void PlayUiSfx(AudioSource)`** — UI tık sesleri için ortak giriş
noktası. Butonların **kendi üzerlerindeki** `AudioSource` bileşeni dışarıdan
verilir; metot yalnızca SFX ayarını uygular ve `PlayOneShot` çağırır. Her butonun
kendi kaynağını taşıması, buton başına farklı klip/ses ayarı yapılabilmesini
sağlar (tek merkezi kaynak bunu imkânsız kılardı). Static olmasının sebebi
`Instance` henüz oluşmamış olabilmesidir: MainMenu sahnesi doğrudan başlatılırsa
`PlayerPrefs`'teki (`SfxPrefKey`) son ayara düşer, yani ses ayarı hiçbir koşulda
yok sayılmaz.

- **Bağlantı:** `SettingsManager` ve `MenuManager` (UI tarafı + `PlayUiSfx`),
  `PlayerPrefs`.

### `PlanetProgress.cs`
`Assets/Scripts/Runtime/Core/`

`MonoBehaviour` değil, statik yardımcı sınıf. Gezegen kilit ilerlemesini
`PlayerPrefs` (`AsalYolculuk.MaxUnlockedPlanetIndex`) üzerinde saklar.
**Tek bir değer** tutulur: açılmış en yüksek gezegen index'i (0 tabanlı,
varsayılan 0 — ilk gezegen oyunun başında açıktır). Kilitler sırayla açıldığı
için tek sayı yeterlidir; gezegen başına ayrı bayrak tutmak "sırası gelmeden
açılmama" kuralını sessizce bozabilirdi.

- `MaxUnlockedIndex`, `IsUnlocked(int)`.
- `Unlock(int)` — değer yalnızca **ileri** gider; daha düşük index yok sayılır,
  böylece bölüm tekrar oynandığında ilerleme geri alınmaz.
- `ResetProgress()` — normal akışta kullanılmaz, "ilerlemeyi sıfırla" ve test için.

`SaveManager` geldiğinde bu alan oraya taşınacak; anahtar adı korunursa mevcut
oyuncu ilerlemesi kaybolmaz (bkz. `YAPILACAKLAR.md` 4.1).

Bu değer iki işi birden görür: menüdeki kilitleri açar **ve** oyunun hangi
bölümden başlayacağını belirler (`LevelManager` açılışta okur). Son bölüm
bitirildiğinde ilerleme orada durur; oyuncu tekrar oynadığında son bölümden
başlar (menüde gezegen seçimi olmadığı sürece kasıtlı davranış).

- **Bağlantı:** `LevelFlowController` (`AdvanceLevel` içinde `Unlock`),
  `LevelManager` (`Awake`'te başlangıç bölümü olarak okur),
  `SelectionManager` (kilit sprite'larını okur), `PlayerPrefs`.

---

## Data

### `PlanetDataSO.cs`
`Assets/Scripts/Runtime/Data/`

Gezegen/seviye başına veri tutan ScriptableObject (`Create > PayeGames >
AsalYolculuk > Planet Data`). `planetId`, `planetName`, `planetSprite`,
`minNumber`, `maxNumber`, `priorityNumberList` (pedagojik öncelikli sayılar)
alanlarını içerir. GDD'deki (`AsalYolculuk_Senaryo.md`) gezegen sayı aralıkları
tablosuna göre 8 gezegen için asset üretilir (`Planet_1`...`Planet_8`).

**Zorluk ayarları:** `meteorSpawnRate` bölümün meteor üretim hızıdır (saniyedeki
meteor sayısı; 0.4 = 2.5 sn'de bir meteor). Bölüm ilerledikçe 0.4'ten 1.2'ye
çıkar, ama artış sona doğru bilinçli olarak yavaşlar — ileri gezegenlerde
sayılar da büyüyüp daha çok parçaya ayrıldığı için sıklık aynı tempoda artarsa
final bölüm oynanamaz hale gelir. Değerlerin tek kaynağı `PlanetDataGenerator`.
Maden ayarları (`areRewardMinesEnabled`, `areCurseMinesEnabled`,
`mineRateMultiplier`) da aynı şekilde gezegen başına tutulur.

`DisplayName` property'si arayüzde gösterilen numaralı adı üretir
(`"2. Asteroit Kuşağı"`). Numaralandırma bilinçli olarak veri katmanındadır;
bölüm sonu paneli, harita ekranı vb. aynı biçimi tekrar kurmak zorunda kalmaz.
Sprite gezegen başına tek tek atanır (dizi **değil**) — dizi olsaydı eşleşme
`planetDataList` sırasına bağlı olur, sıra değişince sessizce bozulurdu.

- **Bağlantı:** `LevelManager` (listede tutulur), `MeteorSpawner` (sayı ve
  üretim hızı için okur), `MineSpawner` (maden ayarları),
  `LevelCompletePanelView` (`DisplayName` + `PlanetSprite`),
  `PlanetDataGenerator` (asset'leri üretir).

---

## Meteors

### `MeteorController.cs`
`Assets/Scripts/Runtime/Meteors/`

`IPoolable` uygular. Sağdan sola (`Vector3.left`) hareket eder, spawn anında
`minMoveSpeed`–`maxMoveSpeed` arası rastgele hız ve `minRotationSpeed`–
`maxRotationSpeed` arası rastgele yön/hızda dönüş alır (hepsi aynı yöne
dönmesin diye). Spawn/despawn X sınırları `Camera.main` viewport'undan
otomatik hesaplanır (manuel X girilmez). `hasEnteredScreen` bayrağı sahneye
fiilen girmeden mermiyle çarpışmayı engeller. `meteorTxt` (TMP_Text) alanına
`SetNumber(int)` ile atanan sayı yazdırılır; `PrimeMathEngine.IsPrime` ile
asallık hesaplanıp saklanır (`Number`, `IsPrimeNumber` property'leri).
`meteorSpriteRenderer` alanına `SetVisuals(Sprite, GameObject)` ile dışarıdan
(spawner'dan) gövde görseli **ve o görsele ait patlama efekti prefab'ı** birlikte
atanır; ikisi de saklanıp (`currentSprite`, `explosionEffectPrefab`) parçalara
aktarılır.

**Patlama efekti:** Meteor yok edilirken `PlayExplosionEffect()` çağrılır;
prefab meteorun anlık pozisyonunda `Instantiate` edilir ve meteorun güncel
boyut oranıyla (`localScale / baseScale`, `explosionEffectScale` çarpanıyla)
ölçeklenir — küçük parçalar küçük patlar. Efekt prefab'ı kendini yok ettiği
için ömrü burada takip edilmez.

**Aktif meteor kaydı:** `static readonly List<MeteorController> ActiveMeteorList`
(dışarıya `IReadOnlyList<MeteorController> ActiveMeteors` olarak açılır).
`OnSpawned`'da eklenir, `OnDespawned`'da çıkarılır. `PlayerDeathEffect` ölüm
sekansında sahnedeki meteorları `Find` kullanmadan buradan alır. Static olma
sebebi meteorların havuzdan gelmesi (`OnMeteorCompleted` event'iyle aynı gerekçe).

`public void DespawnWithExplosion()` — meteoru patlatıp havuza iade eder; puan
verilmez, oturum `Cancel()` edilir. Gemi çarpışmasında (`DespawnOnShipHit` buna
delege eder) ve ölüm sekansında sahne temizlenirken kullanılır.

**Asal çarpanlara bölünme:** Mermi çarpınca sayı asalsa meteor doğrudan
despawn olur; bileşikse `PrimeMathEngine.GetPrimeFactors` ile bulunan asal
çarpan sayısı kadar parça meteor aynı havuzdan (`ownerPool`) üretilir, sonra
ana meteor despawn edilir (örn. 48 → `2,2,2,2,3`). Parçalar ana meteorla aynı
sprite'ı kullanır, üzerlerinde kendi asal çarpanları yazar ve ana meteorun
**mevcut** ölçeğinin `fragmentScaleRatio` katı kadar küçülür (mevcut ölçek
üzerinden çarpım, ileride çok kademeli bölünmeye hazır). Parçalar
`fragmentSpawnSpacing` ile dikeyde simetrik dizilir, `fragmentSpreadStrength`
ile sola giderken yukarı/aşağı saçılır ve `fragmentSpeedMultiplier` kadar
hızlanır.

**Parça dokunulmazlığı:** Oluşan parça `fragmentInvulnerableDuration` (varsayılan
1 sn) boyunca `Collider2D`'si kapalı gezer; böylece ana meteoru vuran mermi
aynı karede parçaları da yok edemez. Süre `Update` içindeki sayaçla dolunca
collider tekrar açılır; havuzdan yeniden spawn olan meteorda sayaç sıfırlanır.

**Dikey sekme:** Hareket artık sabit `Vector3.left` yerine `moveDirection`
vektörü üzerinden yapılır. Üst/alt sınıra (`bounceEdgePadding` kadar içeride)
dayanan ve dışarı doğru giden meteorun `moveDirection.y` işareti ters çevrilir —
geliş açısının ayna yansıması kadar geri döner, yatay bileşen korunduğu için
sola akmaya devam eder. `moveDirection.y == 0` olan ana meteorlar bundan
etkilenmez. Despawn kontrolü sol kenara ek olarak dikey sınırları da kapsar
(sekme sayesinde normalde tetiklenmez, güvenlik ağıdır).

**Katman sıralaması (SortingGroup):** Prefab root'unda `SortingGroup` bulunur
(`[RequireComponent]` ile zorunlu, Sorting Layer = `Meteor`). Her `OnSpawned`'da
statik bir sayaçtan benzersiz `sortingOrder` alınır
(`AssignUniqueSortingOrder`, `SortingOrderRange = 500` ile başa dönerek).
Bu olmadan Unity tüm sprite'ları tek grup, tüm text'leri tek grup olarak
çizdiği için üst üste binen meteorlarda alttaki meteorun sayısı üstteki
meteorun gövdesini deliyordu. SortingGroup meteoru tek çizim birimi yapar;
içerideki sprite (order 0) ve text (order 1) değerleri artık yalnızca grup
içinde geçerlidir. Parçalar da aynı prefabtan/havuzdan üretildiği için
otomatik olarak kendi benzersiz order'larını alır.

**Puanlama / oturum takibi:** Her meteor bir `MeteorSession` referansı taşır
(`session`); ana meteor ilk kez bölündüğünde oturum orada oluşturulur ve
parçalara `ApplyFragmentSetup` ile devredilir, havuzdan yeniden spawn olurken
`null`'lanır. Vuruş akışı: bileşikse `TrySplitIntoPrimeFragments()` bölme
yapar ve **puan verilmez** (oturum parçalarla devam eder); asalsa
`CompleteBranch()` çalışır — oturumu yoksa (tek vuruşta ölen asal ana meteor)
doğrudan, varsa yalnızca oturumun son dalıysa `ScoreManager.AddMeteorScore()`
çağrılır. Ekran dışına çıkan meteorun oturumu varsa `Cancel()` edilir.
Vuruş anında collider hemen kapatılır (`SetVulnerable(false)`); havuz
`collectionCheck: false` ile çalıştığı için aynı karede ikinci bir mermi
çarpsaydı meteor iki kez despawn edilip çift puan verirdi.

**Puansız yok olma (gemi çarpışması / sahne temizliği):** `DespawnWithExplosion()`
collider'ı kapatır, patlama efektini oynatır, varsa oturumu `Cancel()` eder ve
meteoru havuza iade eder — puan verilmez. `DespawnOnShipHit()` bunun anlamsal
takma adıdır; çarpışmayı `ShipCollisionHandler` yakalar, meteor kime çarptığını
bilmez. Aynı metot `PlayerDeathEffect` sahneyi temizlerken de kullanılır.

**Patlama efekti:** `PlayExplosionEffect()` meteorun konumunda
`explosionEffectPrefab`'ı üretir (efekt kendini yok eder). Ölçek, meteorun
`baseScale`'e oranıyla çarpılır — küçülen parçalar küçük patlar.

**Tamamlanma duyurusu:** `public static event Action<int, Vector3>
OnMeteorCompleted`. Ana meteor tüm parçalarıyla yok edildiğinde
`CompleteMeteor(rootNumber)` çalışır; önce event yayınlanır, sonra
`ScoreManager.AddMeteorScore()` çağrılır. Root sayı, oturumu olmayan (tek
vuruşta ölen asal) meteorda kendi `number`'ı, bölünmüş meteorda
`session.RootNumber`'dır. İkinci parametre son parçanın yok olduğu **konumdur**;
`MineSpawner` ödül madenini tam o noktada düşürebilsin diye taşınır. Event'in
**static** olmasının sebebi meteorların havuzdan gelmesi — prefab örneklerine
Inspector'dan dinleyici bağlanamaz. Parçası ekrandan kaçan meteorda oturum
`Cancel()` edildiği için event hiç tetiklenmez.

**Ortak hız çarpanı (Zaman Kristali):** `public static float SpeedMultiplier`
tüm meteorların hareket hızına uygulanır; `SetSpeedMultiplier` /
`ResetSpeedMultiplier` ile değiştirilir. Her meteor kendi hızını bu çarpanla
çarptığı için ekranda **uçmakta olan** meteorlar da anında etkilenir, hız
değerlerini tek tek güncellemek gerekmez. `Time.timeScale` bilinçli olarak
kullanılmaz — o gemiyi, mermileri, arka planı ve HUD tween'lerini de
yavaşlatırdı. Static olduğu için sahne değişiminde hayatta kalır; bu yüzden
`MeteorSpawner.Awake` içinde ayrıca sıfırlanır.

- **Bağlantı:** `GameObjectPool`/`IPoolable` (hem kendi despawn'ı hem parça
  spawn'ı için), `Bullet` (trigger çarpışması), `PrimeMathEngine`
  (`IsPrime`, `GetPrimeFactors`), `MeteorSession` (parça tamamlanma takibi),
  `ScoreManager` (puan verme), `MeteorSpawner` (`OnMeteorCompleted` dinleyicisi),
  `MineSpawner` (`OnMeteorCompleted` — ödül düşürme), `TimeCrystalEffect`
  (hız çarpanı), `ShieldBarrier` / `ShipCollisionHandler` (`DespawnOnShipHit`),
  `PlayerDeathEffect`
  (`ActiveMeteors` + `DespawnWithExplosion`), `UnityEngine.Rendering.SortingGroup`.

### `MeteorSession.cs`
`Assets/Scripts/Runtime/Meteors/`

`MonoBehaviour` değil, düz C# sınıfı. Bir ana meteorun parçalanma ağacını
takip eder; ana meteordan doğan tüm parçalar aynı oturum nesnesini paylaşır.
GDD'nin "meteorun tüm parçaları yok edilmeden puan verilmez" ve "parçalar
ekrandan çıkarsa meteor tamamlanmamış sayılır" kurallarını uygular.

Açık dal sayısını (`activeBranchCount`) tutar, kök meteorla `1`'den başlar:
- `RegisterSplit(fragmentCount)` — dal parçalandı, dalın kendisi kapanıp
  yerine `fragmentCount` yeni dal açılır (`+= fragmentCount - 1`). Çok kademeli
  bölünmeye hazır: parça yine bileşikse sayaç doğru şekilde büyür.
- `RegisterBranchDestroyed()` — bir dal tamamen yok edildi; sayaç 0'a inip
  oturum iptal değilse `true` döner (puan zamanı).
- `Cancel()` — bir parça ekrandan çıktı, oturum iptal edilir; kalan parçalar
  vurulsa bile puan verilmez.

- **Bağlantı:** Yalnızca `MeteorController` tarafından oluşturulur ve
  güncellenir.

### `LevelNumberPool.cs`
`Assets/Scripts/Runtime/Meteors/`

`MonoBehaviour` değil, düz C# sınıfı. Bir bölümün sayı havuzunu yönetir; sayı
üretim mantığı `MeteorSpawner`'dan buraya taşınmıştır. GDD'deki iki kuralı
birlikte uygular: %70 öncelikli liste / %30 rastgele aralık
(`PriorityNumberChance = 0.7f`) ve "tamamen yok edilen sayılar o bölüm boyunca
tekrar gelmez".

Kurulurken `PlanetDataSO`'nun `priorityNumberList`'inin **kopyasını** çıkarır —
asset runtime'da değiştirilmez, aksi halde silinen sayılar editörde kalıcı
olarak kaybolurdu.

- `RegisterCompleted(int)` — sayıyı `completedNumbers` (`HashSet<int>`) içine
  ekler ve çalışma kopyasındaki öncelikli listeden çıkarır.
- `GetNumber()` — öncelikli listede sayı kaldıysa %70 olasılıkla oradan,
  aksi halde aralıktan seçer.
- `GetRangeNumber()` — önce `MaxRandomAttempts` (20) kez rastgele dener (normal
  durum), tükenmeye yaklaşıldığında rastgele bir noktadan başlayıp aralığı
  tarar, aralıktaki her sayı tamamlanmışsa kural gevşetilir. Böylece hiçbir
  koşulda sonsuz döngüye girmez veya meteor üretimi durmaz.
- `Reset()` — bölüm baştan başladığında (canlar bitince) havuzu ilk hâline
  döndürür.

- **Bağlantı:** `PlanetDataSO` (veri kaynağı), `MeteorSpawner` (sahibi ve tek
  kullanıcısı).

### `MeteorFieldClearer.cs`
`Assets/Scripts/Runtime/Meteors/`

Sahnedeki meteorları kendi patlama efektleriyle soldan sağa sırayla temizler
(şok dalgası hissi). Mantık önce `PlayerDeathEffect` içindeydi; bölüm
tamamlandığında da gerekince ortak bileşene çıkarıldı. İki giriş noktası:

- **Kendi kendine:** `GameManager.OnGameStateChanged` dinlenir; `LevelComplete`
  olunca `levelCompleteDelay` bekleyip temizliği başlatır.
- **Dışarıdan:** `PlayerDeathEffect` ölüm sekansının sonunda `ClearRoutine()`'i
  kendi coroutine zinciri içinde `yield return` ile çağırır (ölüm sekansının
  zamanlamasını kendisi yönetsin diye durum event'i orada kullanılmaz).

`ClearRoutine()` sahne **tamamen boşalana kadar** tur atar
(`while ActiveMeteors.Count > 0`, `MaxClearPassCount = 12` üst sınırıyla).
Tek tur yeterli değildir: temizlik sürerken havada kalan mermiler ana meteorları
vurup yeni alt parçalar doğuruyor ve bunlar kopyalanmış listede olmadığı için
sahnede kalıyordu. `ClearSinglePass()` o anki listeyi kopyalayıp X'e göre
sıralar ve her meteoru `clearInterval` aralıkla `DespawnWithExplosion()` ile yok
eder — puan verilmez, oturumlar `Cancel()` edilir.

- **Bağlantı:** `MeteorController` (`ActiveMeteors`, `DespawnWithExplosion`),
  `GameManager` (durum event'i), `PlayerDeathEffect` (çağıran).

### `MeteorSpawner.cs`
`Assets/Scripts/Runtime/Meteors/`

Ekranın sağ kenarından (viewport dışı, `spawnMargin` kadar tampon) rastgele
dikey konumda meteor üretir. **Üretim hızı bölüme göre değişir:** `GetSpawnRate()`
aktif gezegenin `PlanetDataSO.MeteorSpawnRate` değerini okur (saniyedeki meteor
sayısı; üretim aralığı `1 / hız`). Inspector'daki `spawnRate` yalnızca yedektir —
gezegen verisi bağlı değilken (test sahneleri) devreye girer. Hız 0 veya altındaysa
üretim durur; `1 / 0` ile sonsuz aralığa düşülmez. Meteorun sayısını
`LevelNumberPool`'dan alıp `SetNumber` ile yazdırır. `GetNumberPool()` aktif
`PlanetDataSO`'yu her çağrıda kontrol eder; gezegen değişince havuzu yeniden
kurar, böylece tamamlanan sayı listesi bir sonraki bölüme taşınmaz.
`MeteorController.OnMeteorCompleted` event'ine `OnEnable`/`OnDisable`'da abone
olur ve tamamlanan sayıyı havuza bildirir. `ResetNumberPool()` public'tir;
canlar bitip bölüm yeniden başladığında çağrılacaktır. `Update` başında
`GameManager.IsGameplayActive` guard'ı vardır; canlar bitince üretim durur.

**Üretim hızı çarpanı (Meteor Çağırıcı):** `SetSpawnRateMultiplier` /
`ResetSpawnRateMultiplier` gezegenin kendi hızının **üzerine** bir çarpan uygular
(`GetSpawnRate()` = `GetBaseSpawnRate()` × çarpan). Çarpan sabit bir değer yerine
oran olduğu için ceza ileri gezegenlerde otomatik olarak sertleşir.

`Awake`'te `MeteorController.ResetSpeedMultiplier()` çağrılır: hız çarpanı static
olduğu için sahne değişiminde (ve Editor'da domain reload kapalıysa) önceki
oturumdan yavaşlamış hâlde kalabilirdi — oyuncu Zaman Kristali aktifken ölüp
menüye çıkıp geri dönseydi meteorlar yavaş başlardı.

**Görsel + efekt eşleşmesi:** `meteorSprites[]` (Inspector'da basitten karmaşığa
sıralı) ve `explosionEffectPrefabs[]` dizileri **aynı sırada** tutulur.
`GetRandomVisualIndex()` seviyeye göre kilidi açık alt kümeden
(`baseSpriteCount + LevelManager.Instance.CurrentLevelIndex`, dizi uzunluğuna
clamp'li) **tek bir index** seçer; sprite ve patlama efekti bu aynı index'ten
alınıp `SetVisuals` ile birlikte atanır. Böylece her meteor kendi görseline ait
particle ile patlar.

- **Bağlantı:** `GameObjectPool`, `MeteorController` (spawn + tamamlanma
  event'i), `LevelNumberPool` (sayı üretimi), `LevelManager`, `PlanetDataSO`,
  `GameManager` (guard).

---

## PowerUps (Maden Sistemi)

Ödül ve ceza madenleri. GDD 8 maden tanımlar ama yalnızca 3'ünün etkisini
yazar (Altın Kristal, Yıldız Parçası, Koruma Kalkanı); kalan 5'inin etkisi bu
projede kararlaştırıldı (bkz. `YAPILACAKLAR.md` 3.2/3.3).

### `EffectType.cs`
`Assets/Scripts/Runtime/PowerUps/`

8 maden türünü tutan enum ve `EffectTypeExtensions.IsReward(this EffectType)`.
Ödül/ceza ayrımı bilinçli olarak **enum sırasına değil** açık bir `switch`
listesine dayanır; enum sırası değişince ayrım sessizce bozulmasın. Ayrımın
veri dosyasında bayrak olarak tutulmamasının sebebi de bu — yanlış
işaretlenemesin.

- **Bağlantı:** `MineDataSO`, `PowerUpManager`, `MineSpawner`, `ActiveEffectHUDView`.

### `IActiveEffect.cs`
`Assets/Scripts/Runtime/PowerUps/`

Bir madenin oyuna uyguladığı etkinin arayüzü: `Type`, `Duration`,
`ApplyEffect()`, `TickEffect(float)`, `RemoveEffect()`. `Duration == 0` anlık
etki demektir; `PowerUpManager` bunları aktif listeye hiç almaz, uygulayıp
bırakır. `CLAUDE.md`'deki tanıma `Duration` eklenmiştir — süreyi etkinin kendisi
bilmeli, yöneticinin tahmin etmesi gerekmemeli.

- **Bağlantı:** `PowerUpManager` (tek kullanıcısı), `Effects/` altındaki sınıflar.

### `MineDataSO.cs`
`Assets/Scripts/Runtime/PowerUps/`

Maden başına veri (`Create > PayeGames > AsalYolculuk > Mine Data`):
`effectType`, `displayName`, `icon`, `duration`, `amount`, `multiplier`,
`pickupEffectPrefab`, `pickupClip`. `IsReward` doğrudan `EffectTypeExtensions`'a
delege eder.

`amount` ve `multiplier` bilinçli olarak **genel amaçlıdır**; her etki kendine
uygun olanı okur: Yıldız Parçası `Amount` (puan), Altın Kristal `Amount` (meteor
başına puan), Plazma Yakıtı `Amount` (atış noktası sayısı), Zaman Kristali
`Multiplier` (hız katsayısı). Etki başına ayrı alan açmak SO'yu şişirirdi.

Sahnedeki maden prefab'ı **tek** ve nötrdür; hangi maden olduğu üretim anında
`MineController.SetData` ile atanır. Bu yüzden ikon prefab'da değil burada
tutulur — aksi halde 8 ayrı prefab ve 8 ayrı havuz gerekirdi.

- **Bağlantı:** `MineController` (görsel + ses), `MineSpawner` (hangi madenler
  düşecek), `PowerUpManager` (etki üretimi), `ActiveEffectHUDView` (ikon).

### `PowerUpManager.cs`
`Assets/Scripts/Runtime/PowerUps/`

Maden sisteminin beyni (`PowerUpManager.Instance`). Toplanan madenin etkisini
üretir, süresi boyunca besler, süre dolunca geri alır.

- **Anlık etki** (`Duration <= 0`) — `ApplyEffect()` çağrılır,
  `OnInstantEffectApplied` yayınlanır, listeye eklenmez.
- **Süreli etki** — `ApplyEffect()` + listeye eklenir + `OnEffectStarted`.
  `Update` içinde `RemainingTime` azalır; sıfırlanınca `RemoveEffect()` +
  `OnEffectEnded`. Liste sondan başa gezilir (eleman çıkarılıyor).
- **Aynı tür ikinci kez toplanırsa** etki **üst üste binmez**, yalnızca süre
  tazelenir. İki Zaman Kristali meteorları %25'e düşürmez, iki Kalkan çift
  bariyer çizmez. GDD "aynı anda birden fazla maden etkisi aktif olabilir"
  derken **farklı** türleri kasteder.
- `GameManager.OnGameStateChanged` dinlenir; `Gameplay` ve `Paused` dışındaki
  her duruma geçişte `ClearAllEffects()` çalışır — kalkan/yavaşlatma bir sonraki
  bölüme sarkmasın.

**Etki hedefleri:** Gemiye/sahneye dokunan etkilerin ihtiyaç duyduğu bileşenler
(`playerShooter`, `shipCollisionHandler`) Inspector'dan buraya bağlanır ve
`CreateEffect` içinde etkilere verilir. Böylece etki sınıfları singleton aramak
veya sahnede `Find` yapmak zorunda kalmaz. `CreateEffect` bu yüzden static
**değildir**. Referans bağlı değilse etki üretilmez ve Console'a hangi alanın
eksik olduğunu söyleyen bir uyarı yazılır.

`CreateEffect` yeni bir maden eklenirken değiştirilecek **tek** yerdir;
yazılmamış türler `null` döner ve uyarı basılır.

- **Bağlantı:** `MineController` (`Collect`), `IActiveEffect` + `Effects/`,
  `GameManager` (durum), `PlayerAutoShooter` / `ShipCollisionHandler` (etki
  hedefleri), `ActiveEffectHUDView` (event dinleyicisi).

### `MineController.cs`
`Assets/Scripts/Runtime/PowerUps/`

`IPoolable` uygular. Sağdan sola akar (meteorlardan **yavaş** — oyuncu yetişip
toplayabilsin), `Mathf.Sin` ile dikeyde salınır, ikonu kendi ekseninde döner.
Sol kenarı geçince havuza döner.

Görselini ve etkisini `SetData(MineDataSO)` ile alır; havuz aynı objeleri
döndürdüğü için her üretimde yeniden atanması **şarttır**.

**Salınım merkezi (`baseY`) ilk `Update`'te okunur, `OnSpawned`'da değil:**
`GameObjectPool.Spawn` konumu `OnSpawned`'dan **sonra** atıyor, `OnSpawned`
sırasında obje hâlâ bir önceki hayatının konumundadır.

`Collect()` — toplama efekti + sesi oynatır, `PowerUpManager.Collect(data)`
çağırır, havuza iade eder. `isCollected` bayrağı ve collider'ın anında
kapatılması aynı karede iki kez toplanmayı engeller.

`static DespawnAll()` — sahnedeki tüm madenleri havuza iade eder; bölüm
bitince/ölünce `MineSpawner` çağırır. Aktif maden listesi static tutulur çünkü
madenler havuzdan gelir, Inspector'dan referans verilemez (meteorlarla aynı
gerekçe).

- **Bağlantı:** `GameObjectPool`/`IPoolable`, `MineDataSO`, `PowerUpManager`,
  `MineCollector` (toplamayı tetikleyen), `SoundManager` (`PlaySfx`),
  `GameManager` (guard).

### `MineSpawner.cs`
`Assets/Scripts/Runtime/PowerUps/`

Madenleri sahneye çıkarır. **Hibrit model** — iki farklı kural bilinçli olarak
birlikte kullanılır:

- **Ödüller:** `MeteorController.OnMeteorCompleted` dinlenir; ana meteor tüm
  parçalarıyla yok edildiğinde **o noktada** `rewardDropChance` olasılığıyla
  düşer. Ödül böylece başarıya bağlanır.
- **Cezalar:** zamanlayıcıyla (`minCurseInterval`–`maxCurseInterval`) ekranın
  sağ kenarından girer. Ceza bilinçli olarak başarıya **bağlanmaz** — meteor
  tamamlamaya bağlansaydı iyi oynamak cezalandırılmış olurdu.

**Gezegen bazlı kademeli açılım:** `PlanetDataSO`'nun `AreRewardMinesEnabled` /
`AreCurseMinesEnabled` / `MineRateMultiplier` alanları okunur. Çarpan hem ödül
şansını hem ceza aralığını etkiler (aralık çarpana bölünür). `PlanetDataSO`
yoksa (test sahnesi) her şey açık kabul edilir.

`GameManager.OnGameStateChanged` dinlenir: oynanış dışına çıkıldığında
`MineController.DespawnAll()` çağrılır ve ceza sayacı sıfırlanır.

Hangi madenin düşeceği `rewardMines` / `curseMines` dizisinden **eşit
olasılıkla** seçilir — ağırlıklandırma henüz yok (`YAPILACAKLAR.md` 3.7).

- **Bağlantı:** `GameObjectPool`, `MineController`, `MineDataSO`,
  `MeteorController` (`OnMeteorCompleted`), `LevelManager` + `PlanetDataSO`,
  `GameManager`.

### `Effects/StarFragmentEffect.cs`
`Assets/Scripts/Runtime/PowerUps/Effects/`

**Yıldız Parçası (ödül) — anlık.** `ScoreManager.AddScore(data.Amount)` ile
anında +10 puan. `Duration` sabit `0` döner; veri dosyasına yanlışlıkla süre
girilse bile listeye alınmaz, HUD'da görünmez.

- **Bağlantı:** `ScoreManager`.

### `Effects/GoldCrystalEffect.cs`
`Assets/Scripts/Runtime/PowerUps/Effects/`

**Altın Kristal (ödül) — 10 sn.** `ScoreManager.SetMeteorScoreOverride(Amount)`
ile meteor puanını 5'ten 10'a sabitler, `RemoveEffect`'te geri alır. Kendisi
puan **vermez**; kazanç oyuncunun o pencerede kaç meteor bitirdiğine bağlıdır.
Çarpan yerine sabit değer kullanılması bilinçlidir — GDD "10 puan" diyor ve
rakam veri dosyasından geliyor.

- **Bağlantı:** `ScoreManager`.

### `Effects/PlasmaFuelEffect.cs`
`Assets/Scripts/Runtime/PowerUps/Effects/`

**Plazma Yakıtı (ödül) — 10 sn.** `PlayerAutoShooter.SetActiveFirePointCount(
data.Amount)` ile gemi tek yerine 3 noktadan ateş eder (çoklu atış). Ateş **hızı
değişmez**; aynı anda daha geniş alan taranır. Yelpaze açısı atış noktalarının
Inspector'daki rotasyonundan gelir, kodda açı hesabı yoktur.

- **Bağlantı:** `PlayerAutoShooter`.

### `Effects/TimeCrystalEffect.cs`
`Assets/Scripts/Runtime/PowerUps/Effects/`

**Zaman Kristali (ödül) — 10 sn.** `MeteorController.SetSpeedMultiplier(
data.Multiplier)` ile tüm meteorları %50 hıza düşürür. Oyunun eğitsel amacına en
doğrudan hizmet eden ödül: çocuğa çarpanları düşünmek için zaman kazandırır.
Ekranda uçmakta olan meteorlar da anında yavaşlar.

- **Bağlantı:** `MeteorController` (static hız çarpanı).

### `Effects/ShieldEffect.cs`
`Assets/Scripts/Runtime/PowerUps/Effects/`

**Koruma Kalkanı (ödül) — 10 sn.** `ShipCollisionHandler.SetShieldActive(bool)`
ile gemi çevresindeki bariyeri açar/kapatır. Çarpan meteor yine patlar ama puan
verilmez — yoksa oyuncu ateş etmeyi bırakıp meteorların üzerine sürerek puan
toplardı, bu da çarpanları çözmeden puan almak demek olurdu.

- **Bağlantı:** `ShipCollisionHandler`.

### `Effects/MeteorCallerEffect.cs`
`Assets/Scripts/Runtime/PowerUps/Effects/`

**Meteor Çağırıcı (ceza) — 10 sn.** `MeteorSpawner.SetSpawnRateMultiplier(
data.Multiplier)` ile meteor üretim sıklığını iki katına çıkarır. Çarpan
gezegenin **kendi** üretim hızının üzerine bindiği için ceza ileri gezegenlerde
otomatik olarak daha sert hissedilir; sabit bir değer yazılsaydı bu ölçekleme
kaybolurdu.

- **Bağlantı:** `MeteorSpawner`.

### `Effects/GravityStoneEffect.cs`
`Assets/Scripts/Runtime/PowerUps/Effects/`

**Yerçekimi Taşı (ceza) — 10 sn.** Geminin hareket hızını
`data.Multiplier` katına düşürür (0.5) ve `data.Amount` birim/saniye aşağı çeker.

Gemi ekrandan **çıkamaz**: `PlayerShipController.ClampToScreen` konumu her karede
dikeyde de sınırlar, çekim gemiyi en fazla alt kenara yapıştırır. Ayrıca çekim
geminin güncel hızının %70'ini geçemez (`MaxPullRatio`) — aksi halde gemi hiç
yukarı çıkamaz ve ceza değil felç olurdu.

- **Bağlantı:** `PlayerShipController`.

### Uygulanmayan: `ChaosCore`
`EffectType` enum'unda tanımlı ama **bilinçli olarak yazılmadı.** Planlanan etki
meteor sayılarını 10 sn gizlemekti; bu, eğitsel oyunun asıl içeriğini geçici
olarak kapattığı için vazgeçildi. Enum'da kalması GDD ile izlenebilirlik
içindir; `MineDataSO`'da seçilirse `CreateEffect` `null` döner ve Console'a uyarı
yazılır. Ayrıntı: `YAPILACAKLAR.md` 3.3.

---

## UI

### `ScoreHUDView.cs`
`Assets/Scripts/Runtime/UI/`

HUD'daki puan göstergesi. `ScoreManager.Instance.OnScoreChanged` event'ini
dinler (abonelik `Start`'ta, böylece manager'ın `Awake`'i garanti çalışmış
olur; `OnDestroy`'da bırakılır) ve `puanTxt` (TMP_Text) alanına
`45 / 100` biçiminde yazar. TMP rich text ile güncel puan `currentScoreColor`
(varsayılan sarı), `" / hedef"` kısmı `targetScoreColor` (beyaz) ile
renklendirilir; hex string'ler `Awake`'de bir kez cache'lenir. Mantık
içermez, sadece state'i gösterir.

- **Bağlantı:** `ScoreManager` (event kaynağı), TextMeshPro.

### `ActiveEffectHUDView.cs`
`Assets/Scripts/Runtime/UI/`

Aktif **süreli** etkileri ikon + geri sayım olarak gösterir (GDD: *"maden
toplandığında ekranda küçük süre göstergesi çıkar, örneğin 6s"*). Mantık
`PowerUpManager`'da; bu sınıf yalnızca durumu çizer.

`PowerUpManager.OnEffectStarted` / `OnEffectEnded` dinlenir (abonelik `Start`'ta,
manager'ın `Awake`'i garanti çalışmış olsun diye). Etkiler toplanma sırasında bir
listede tutulur, `RefreshSlots()` her değişimde tüm diziyi baştan dağıtır —
ortadaki etki bittiğinde boşluk kalmaz, hizalamayı `Horizontal Layout Group`
toparlar. `Update` her karede `GetRemainingTime` okuyup slot'a yazar.

Slot sayısı **süreli maden sayısı kadar (7)** olmalıdır; teorik olarak hepsi
aynı anda aktif olabilir. Anlık etkiler (Yıldız Parçası) burada görünmez —
gösterilecek süreleri yoktur. Ödül/ceza ayrımı geri sayım rengiyle yapılır
(`MineDataSO.IsReward`). `Slots` boşsa veya sahnede `PowerUpManager` yoksa
Console'a uyarı yazılır — gösterge sessizce ölmesin.

- **Bağlantı:** `PowerUpManager` (event kaynağı), `MineDataSO` (ikon + IsReward),
  `ActiveEffectSlotView`.

### `ActiveEffectSlotView.cs`
`Assets/Scripts/Runtime/UI/`

Göstergedeki tek kutucuk. `Image` + `CanvasGroup` zorunlu (`[RequireComponent]`).

**Inspector'da hiç alanı yoktur:** slot düzeni her zaman aynı olduğu için
(`Image` objenin kendisinde, `TMP_Text` altında) referanslar çalışma anında
bulunur. Çözümleme `Awake`'te **ve** `Show()` içinde yapılır; ikincisi gerekli
çünkü slot sahnede kapalı başlarsa `Awake` hiç çalışmaz. Geçiş süreleri de
`const` — kutucuk başına ayarlanacak bir şey yok.

- `Show()` — önce `SetActive(true)`, sonra alpha 0'dan `DOFade(1)`. **Zaten
  görünürse yeniden solmaz**, yalnızca ikon/renk tazelenir; `RefreshSlots` tüm
  diziyi yeniden yazdığı için ekranda duran kutucuk boşuna yanıp sönmesin.
- `Hide()` — `DOFade(0)`, bitince `SetActive(false)`. Obje kapalıysa veya
  `CanvasGroup` yoksa tween çalışmayacağı için doğrudan gizlenir (yarı saydam
  donmuş slot kalmasın).
- `HideImmediate()` — animasyonsuz. Bölüm açılışında kullanılır; oyuncu yedi
  kutucuğun tek tek solmasını görmemeli.
- `SetRemainingTime(float)` — yukarı yuvarlar (5.3 sn → "6s"). Yazılar
  önceden hazırlanmış `SecondLabels` dizisinden okunur ve saniye değişmedikçe
  `TMP_Text`'e atama yapılmaz — `Update`'te string üretilmez (GC baskısı).

Tween'ler `SetUpdate(true)` ile unscaled çalışır (projedeki diğer UI'larla aynı).

- **Bağlantı:** `ActiveEffectHUDView` (tek kullanıcısı), DOTween, TextMeshPro.

### `GameplayHUDPanel.cs`
`Assets/Scripts/Runtime/UI/`

Oynanış sırasında görünen HUD panelleri (`scorePanel`, `healthPanel`) için ortak
görünürlük bileşeni — aynı script iki panelde de kullanılır. `RectTransform` +
`CanvasGroup` zorunlu (`[RequireComponent]`).

`GameManager.OnGameStateChanged` dinlenir: `Gameplay`'de `Show()`, bölüm sonu
durumlarında (`LevelComplete` / `LevelFailed`) `Hide()`. `Paused` bilinçli olarak
atlanır — duraklatmada HUD ekranda kalmalı.

Panelin Inspector'daki yerleşimi `Awake`'te "görünür konum" olarak cache'lenir;
gizlenirken `hideOffset` kadar kayıp `CanvasGroup` alpha'sı 0'a solar (offset
`(0,0)` bırakılırsa sadece solma). Tween'ler `SetUpdate(true)` ile unscaled
çalışır. Gizliyken `interactable` / `blocksRaycasts` kapatılır.

- **Bağlantı:** `GameManager` (durum event'i), DOTween.

### `LevelCompletePanelView.cs`
`Assets/Scripts/Runtime/UI/`

Bölüm sonu paneli (`bolumPanel`). `RectTransform` + `CanvasGroup` zorunlu.

**Konumlandırma:** Panel sahnede **görünür yerinde** bırakılır; `Awake`'te bu
konum `shownAnchoredPosition` olarak cache'lenir ve gizli/çıkış konumları root
canvas genişliğine göre otomatik hesaplanır (`± canvasWidth + hiddenExtraMargin`).
Böylece anchor/pivot ayarı ne olursa olsun panel ekran dışında başlar ve
tasarımcının paneli elle ekran dışına taşımasına gerek kalmaz.

- `Show()` — `GameManager.OnGameStateChanged` → `LevelComplete`'te çağrılır
  (`LevelManager.IsLastLevel` ise **açılmaz**; son gezegen için ayrı panel
  gelecek). `showDelay` sonrası sağdan sola kayarak girer, `CanvasGroup` alpha
  0→1; animasyon bitince `interactable`/`blocksRaycasts` açılır.
- `HideToLeft(onComplete)` — `LevelFlowController` çağırır. Panel **sola** doğru
  çıkar, çıkış biter bitmez sağdaki gizli konuma geri yerleştirilir; böylece
  giriş yönü bozulmaz, bir sonraki bölüm sonunda yine sağdan girer. Geri çağrı
  yeni bölümü başlatıp `Hide()` tetikleyebildiği için biten sequence referansı
  callback'ten **önce** bırakılır.
- `Hide()` — animasyonsuz olarak sağdaki gizli konuma alır; `Gameplay`
  durumunda otomatik çağrılır.
- `FillPlanetInfo()` — panel her açıldığında `kaldiginGezegenTxt` /
  `hedefGezegenTxt` alanlarına `PlanetDataSO.DisplayName`, `kaldiginImg` /
  `hedefImg` alanlarına `PlanetSprite` yazılır. Kaynak: `LevelManager`'ın
  `CurrentPlanetData` ve `NextPlanetData` property'leri. Sprite atanmamış
  gezegende mevcut görsel bozulmadan bırakılır.

- **Bağlantı:** `GameManager` (durum event'i), `LevelManager` (gezegen verisi),
  `PlanetDataSO`, `LevelFlowController` (`HideToLeft`), TextMeshPro, DOTween.

### `GameOverPanelView.cs`
`Assets/Scripts/Runtime/UI/`

Üç can da bittiğinde açılan oyun sonu paneli (`gameOverPanel`).
`RectTransform` + `CanvasGroup` zorunlu.

**Konumlandırma:** `LevelCompletePanelView` ile aynı desen — panel sahnede
**görünür yerinde** bırakılır, `Awake`'te bu konum cache'lenir, gizli konum root
canvas genişliğine göre otomatik hesaplanır. Sağdan sola kayarak girer.

**Zamanlama:** Durum event'i yerine `PlayerDeathEffect.OnDeathSequenceCompleted`
dinlenir. `LevelFailed` durumu ölüm sekansının **başında** geldiği için sabit
gecikme kullanılamaz; sekansın süresi (gemi patlamaları + meteor temizliği)
sahnedeki meteor sayısına göre değişir. `playerDeathEffect` referansı bağlı
değilse güvenlik ağı devreye girer: `LevelFailed`'den `fallbackShowDelay` kadar
sonra panel yine açılır (patlamaların üstüne binebilir).

`Gameplay` durumuna dönüldüğünde `Hide()` çalışır — panel gizli konuma alınır ve
gizlenen nesneler geri getirilir; bölüm yeniden başlatıldığında panel kendiliğinden
toparlanır.

- `hideOnGameOver` (`GameObject[]`) — panel gelirken `SetActive(false)` yapılacak
  sahne nesneleri. **Gemi bu diziye konmaz:** patlama coroutine'i gemi objesi
  üzerinde çalışır, kapatmak sekansı yarıda keser (gemi zaten `HideShip()` ile
  gizlenir). Puan/can HUD panelleri de gerekmez; `GameplayHUDPanel` onları
  `LevelFailed`'de kendisi çeker.
- `FillPlanetInfo()` — panel her açıldığında `kaldiginGezegenTxt` alanına
  `PlanetDataSO.DisplayName`, `gezegenImg` alanına `PlanetSprite` yazılır.
  Kaynak `LevelManager.CurrentPlanetData`; bölüm kaybedildiğinde seviye
  ilerlemediği için bu her zaman kaybedilen gezegendir.

Paneldeki iki buton `LevelFlowController` metotlarına bağlanır:
`tekrarOynaBtn → RestartLevel()`, `anaMenuBtn → GoToMainMenu()`.

- **Bağlantı:** `PlayerDeathEffect` (`OnDeathSequenceCompleted`), `GameManager`
  (durum event'i), `LevelManager` + `PlanetDataSO` (gezegen bilgisi),
  `LevelFlowController` (buton hedefi, `Hide`), TextMeshPro, DOTween.

### `BitisManager.cs`
`Assets/Scripts/Runtime/UI/`

Oyun sonu paneli (`bitisPanel`). Panele `LevelCompletePanelView` **eklenmez** —
o script bölüm sonu paneline aittir ve son gezegende bilinçli olarak açılmaz;
iki panel bu sayede hiç çakışmaz. `RectTransform` + `CanvasGroup` zorunlu.

**Panel girişi:** `GameManager.OnGameStateChanged` → `LevelComplete` **ve**
`LevelManager.IsLastLevel` ise `showDelay` sonrası sağdan sola kayarak gelir.
Konumlandırma `LevelCompletePanelView` ile aynı desende: Inspector'daki yerleşim
"görünür konum" kabul edilir, gizli konum root canvas genişliğine göre otomatik
hesaplanır. Giriş tween'i biter bitmez `PlaySequence()` zincirleme tetiklenir —
sekansın panelin oturmasını beklemesi için elle delay ayarlamak gerekmez.

**Yolculuk özeti sekansı:** `gezegenler` ve `lines` parent'larının çocukları
hiyerarşi sırasına göre eşleştirilir ve tek bir DOTween `Sequence` kurulur:
`startDelay → gezegen[0] (scale + alpha birlikte) → planetToLineDelay →
line[0] doldurma → gezegen[1] → line[1] → ...`
Çizgi sayısı gezegen sayısından bir eksik olduğu için son gezegende çizgi adımı
otomatik atlanır (8 gezegen / 7 çizgi kurulumu doğrudan çalışır).

Gezegenlerin hedef ölçeği `Awake`'te sahnedeki ölçekten cache'lenir; bu yüzden
nesneler sahnede **görünür ve son ölçeklerinde** bırakılır, gizleme runtime'da
yapılır. Sahnede ölçek 0 bırakılmışsa tween 0'dan 0'a gideceği için
`ResolveBaseScale` birim ölçeğe düşer. Çizgilerin `Image`'i **Filled** tipinde
olmalıdır — `DOFillAmount` yalnızca o tipte çalışır.

**Kutlama efekti:** İlk gezegen belirmeye başladığı anda (`startDelay` bittiğinde)
sekansa eklenen `AppendCallback` bir coroutine başlatır: `celebrationEffectPrefab`
(`Prefabs/Effects/confetti.prefab`) `celebrationCount` kez (varsayılan 10),
**aynı anda değil sırayla**, her seferinde kameranın viewport'unda rastgele bir
noktada üretilir (`celebrationEdgePadding` kenarlardan pay bırakır,
`celebrationCameraDistance` 2D düzlemi belirler). Aralık Inspector'dan değil,
gezegen/çizgi fazının gerçek süresinden hesaplanır (`activeSequence.Duration(false)
- startDelay`, sekans kurulurken okunur) ve efekt sayısına bölünür — böylece
efektler süreç boyunca eşit yayılır, gezegenler bitince biter.
Coroutine `WaitForSecondsRealtime` kullanır çünkü bölüm sonunda `Time.timeScale`
0 olabilir (panel tween'leri de `SetUpdate(true)`). Efekt ömrü prefab'ın kendi
Stop Action = Destroy ayarına bırakılır; `celebrationLifetime > 0` verilirse
`Destroy(effect, süre)` ile zorlanır. Kamera boşsa `Awake`'te `Camera.main`
çözülür (runtime'da arama yapılmaz). `ResetToHiddenState` ve `OnDestroy`
coroutine'i durdurur, panel yeniden açılırsa efektler baştan başlar.

**Özet panelleri:** Gezegen/çizgi sekansı bittikten sonra üç `CanvasGroup` sırayla
açılır: asal sayı paneli → puan paneli → başla butonu. Üçü de aynı adımı kullandığı
için tek bir dizide tutulur (`summaryPanels` / `summaryTexts` / `summaryTargetValues`
aynı index ile eşleşir; butonun sayaç metni `null` bırakılır, o adım atlanır).
Boş bırakılan slot sekansta boşluk bırakmaz — gecikme yalnızca gerçekten açılacak
paneller arasında uygulanır.

**Sayaçlar:** Panel yerine oturduğu anda metin 0'dan hedef değere sayar
(`DOTween.To` + `SetTarget(label)`, böylece `label.DOKill()` sayacı da öldürür).
Sayaç sekansa **eklenmez**, `AppendCallback` ile bağımsız başlatılır; aksi halde
sekans sayma bitene kadar bir sonraki panele geçemezdi. Hedef değerler panel
açılırken `ScoreManager`'dan okunur: `DiscoveredPrimeCount` ve `TotalScore`.

**Başla butonu:** Belirdikten sonra `pulseScale` oranında sonsuz yoyo ile nabız
atar; tıklanabilirlik ancak buton tamamen belirince açılır. `GoToMainMenu()`
butona bağlanır — tween'leri öldürür, `ScoreManager.SaveTotalScore()` ile toplam
puanı diske yazar, `Time.timeScale = 1` yapar (panel duraklamışken açılmış
olabilir) ve `mainMenuSceneName` sahnesini yükler. Buton ilk tıklamada kapatılır,
sahne iki kez yüklenmez.

- **Bağlantı:** `GameManager` (durum event'i), `LevelManager` (`IsLastLevel`),
  `ScoreManager` (`DiscoveredPrimeCount`, `TotalScore`, `SaveTotalScore`),
  `SceneManager` (ana menü sahnesi), `Prefabs/Effects/confetti.prefab`
  (kutlama efekti), TextMeshPro,
  DOTween (`DOAnchorPos`, `DOScale`, `DOFade`, `DOFillAmount`, `DOTween.To`).

### `PauseManager.cs`
`Assets/Scripts/Runtime/UI/`

Oynanış sahnesindeki duraklatma akışını yönetir. Sahnedeki Canvas'a eklenir —
**`pausePanel`'in kendisine eklenmemeli**, çünkü panel kapalıyken (`SetActive(false)`)
script de durur ve `pauseBtn` çalışmaz. Tüm buton `onClick` bağlantıları
`Awake`'te koddan kurulur (Inspector'daki OnClick listesine ayrıca eklenmemeli).

`Pause()` — `pauseBtn`'e bağlanır. Yalnızca durum `Gameplay` iken çalışır ve
`GameManager.SetState(GameState.Paused)` çağırır; oyunun durması buradan gelir,
ayrı bir "hepsini durdur" mantığı yoktur — `GameManager` bu durumda
`Time.timeScale`'i 0'lar, meteor üretimi / ateş / gemi hareketi / etki sayaçları
kendiliğinden donar.

`Resume()` — `exitBtn` (panel kapatma) ve `oyunaDonBtn` ikisine de bağlanır;
`SetState(GameState.Gameplay)` ile oyun **kaldığı yerden** akmaya başlar, panel
üzerine kapanış animasyonu oynar. Kapanış başlarken `blocksRaycasts` kapatılır ki
panelin üstündeki tıklama oyuna sızmasın.

**Animasyon:** açılışta `localScale` `hiddenScale` (0.7) → 1 `OutBack` + CanvasGroup
alpha 0 → 1; kapanışta tersi (`InBack`) ve bitince panel `SetActive(false)`.
Tween'ler `SetUpdate(true)` ile çalışır — `timeScale` 0 iken animasyonun donmaması
için zorunludur. `MenuManager`'ın ayarlar paneliyle aynı desendir.

`mainMenuBtn` — kendi sahne geçişini yazmaz, `LevelFlowController.GoToMainMenu()`
çağırır; böylece puan geri alma, kalınan bölüm kaydı ve maske geçişi oyun sonu
paneliyle birebir aynı yoldan gider. Sahne değişeceği için panel kapanış
animasyonu oynatılmaz, yalnızca tıklama kilitlenir.

`OnGameStateChanged` dinlenerek `pauseBtn.interactable` yalnızca `Gameplay`
durumunda açık tutulur; bölüm bitmiş/kaybedilmişken duraklatılamaz.

**Tık sesi:** her butonun üzerindeki `AudioSource` `Awake`'te `TryGetComponent`
ile cache'lenir ve `SoundManager.PlayUiSfx` ile çalınır.

- **Bağlantı:** `GameManager` (`SetState`, `OnGameStateChanged`),
  `LevelFlowController` (`GoToMainMenu`), `SoundManager` (`PlayUiSfx`), DOTween.

### `SettingsManager.cs`
`Assets/Scripts/Runtime/UI/`

Ayarlar panelindeki ses (SFX) ve müzik butonlarını yönetir. Butonların
`onClick` bağlantıları `Awake`'te koddan kurulur (Inspector'daki OnClick
listesine ayrıca eklenmemeli, yoksa çift tetiklenir); tıklamada
`SoundManager.Instance.ToggleSfx()` / `ToggleMusic()` çağrılır ve ikon sprite'ı
açık/kapalı hâline göre değiştirilir.

`RefreshSprites()`, `SoundManager.Instance` henüz yoksa `PlayerPrefs`'ten
(`SoundManager.SfxPrefKey` / `MusicPrefKey`) fallback okur — execution order
sorununa karşı güvenlidir. Sınıf yalnızca UI sorumluluğunu taşır, ses mantığı
tamamen `SoundManager`'dadır.

**Tık sesi:** Her iki butonun üzerindeki `AudioSource` `Awake`'te
`TryGetComponent` ile cache'lenir (tıklama anında `GetComponent` çağrılmaz) ve
`SoundManager.PlayUiSfx` ile çalınır. `musicBtn` sesi toggle'dan **önce**,
`sesBtn` sesi toggle'dan **sonra** çalar: bu sayede ses açıldığında onay sesi
duyulur, kapatıldığında sessiz kalınır. Ters sırada geri bildirim kullanıcının
yaptığı işin tersini anlatırdı (kapatınca ses çıkar, açınca çıkmazdı).

- **Bağlantı:** `SoundManager` (durum + toggle + `PlayUiSfx`), `PlayerPrefs`
  (fallback).

### `MenuManager.cs`
`Assets/Scripts/Runtime/UI/`

Ana menü (`MainMenu` sahnesi) panel akışının tamamını yönetir. Tüm buton
bağlantıları `Awake`'te koddan kurulur — Inspector'daki OnClick listesine
ayrıca eklenmemeli, yoksa çift tetiklenir.

**Buton kaydı ve tık sesi:** Beş buton (`ayarlarBtn`, `exitBtn`, `baslaBtn`,
`backBtn`, `oynaBtn`) tek bir `RegisterButton(Button, UnityAction)` metoduyla
bağlanır. Metot butonun üzerindeki `AudioSource`'u `TryGetComponent` ile
**bir kez** cache'leyip `SoundManager.PlayUiSfx` çağrısını listener olarak ekler,
ardından asıl davranışı bağlar. Ses listener'ının önce eklenmesi bilinçlidir:
`oynaBtn` sahne geçişini başlatmadan önce sesin tetiklenmesi gerekir. Ses
bağlantısı `OpenAyarlarPanel` gibi public metotların **içinde değil**, listener
seviyesindedir — böylece bu metotlar koddan çağrıldığında (ör. geçiş akışları)
ses çalmaz, yalnızca gerçek tıklamada çalar. `AudioSource` bulunmayan butonlar
sessizce çalışmaya devam eder.

**Ayarlar paneli:** `ayarlarBtn` → panel `hiddenScale`'den 1'e (`OutBack`)
büyürken `CanvasGroup` alpha'sı 0→1 solar; **aynı anda** menü buton grubu
(`butonlarPanelCanvasGroup`) alpha 1→0 söner ve etkileşimi kapanır. Panel
içindeki `exitBtn` → animasyon tersine oynar, bitince panel `SetActive(false)`
yapılır ve menü butonları geri açılır.

**Seçim paneli geçişi:** `baslaBtn` → `selectionPanel` sağdaki ekran dışı
konumdan merkeze kayarken, aynı sequence içinde `butonlarPanel` sola doğru
ekrandan çıkar. `backBtn` bu geçişi tersine oynatır ve panel gizli konumuna
dönüp kapanır. Ekran dışı konumlar `Awake`'te root canvas genişliğine göre
hesaplanır (`hiddenExtraMargin` payıyla) — `LevelCompletePanelView` ile aynı
desen, yani paneller sahnede **görünür yerlerinde** bırakılır, elle ekran
dışına taşınmaz.

**Oyuna geçiş:** `oynaBtn` → `StartGameplay()`. `TransitionManager.Instance`
varsa `LoadLevel(gameplaySceneName)` ile maske geçişi yapılır, yoksa
`SceneManager.LoadScene`'e düşülür (`LevelFlowController.GoToMainMenu` ile aynı
desen). `isLoadingScene` bayrağı çift tıklamada sahnenin iki kez yüklenmesini
engeller.

Ayarlar ve seçim geçişleri **ayrı sequence** alanlarında tutulur
(`ayarlarSequence` / `selectionSequence`); biri diğerini kesmez, `OnDestroy`'da
ikisi de `Kill` edilir. Tween'ler `SetUpdate(true)` ile unscaled çalışır.
Bağlanmamış referanslar (panel, CanvasGroup, buton) guard'larla sessizce
atlanır; script eksik referansla da çalışır.

- **Bağlantı:** `SettingsManager` (ayarlar paneli içeriği), `SelectionManager`
  (seçim paneli içeriği, panel `SetActive` üzerinden dolaylı), `SoundManager`
  (`PlayUiSfx`), `MaskTransitions.TransitionManager`, `SceneManager`, DOTween.

### `SelectionManager.cs`
`Assets/Scripts/Runtime/UI/`

Seçim panelindeki 8 gezegen kilidini kayıtlı ilerlemeye göre günceller.
`lockImages` (`Image[]`) dizisindeki sıra gezegen sırasıdır (index 0 = 1.
gezegen); `lockedSprite` / `unlockedSprite` Inspector'dan verilir.

`OnEnable`'da `RefreshLocks()` çalışır — `MenuManager` paneli `SetActive(true)`
ile açtığı için her açılışta ilerleme tazelenir, oyundan menüye dönüldüğünde
yeni açılan gezegen doğru görünür. Kilit durumu `i <= PlanetProgress
.MaxUnlockedIndex` ile belirlenir; sıradaki gezegen bitirilmeden bir sonraki
hiçbir koşulda açık görünmez. Boş slot veya atanmamış sprite sessizce atlanır.

Gezegene tıklayıp o bölümden başlama akışı **henüz yok**; `oynaBtn` her zaman
`LevelManager`'ın mevcut index'iyle başlatır.

- **Bağlantı:** `PlanetProgress` (ilerleme kaynağı), `MenuManager` (paneli açan).

---

## Geçici (Silinecek)

### `SilinecekManager.cs`
`Assets/Scripts/Runtime/`

**GEÇİCİ TEST ARACI — teslimden önce silinecek.** Bitiş akışını tüm bölümleri
oynamadan denemek için yazıldı. Hiçbir sistem bu sınıfa referans vermez, hiçbir
event'e abone olmaz; sahnedeki objesiyle birlikte silindiğinde oyun etkilenmez.

- `useSavedProgress` (varsayılan **açık**) — açıkken `Start` hiçbir şey yapmaz ve
  oyun normal davranışıyla oyuncunun kaldığı bölümden başlar. Bu bayrak şart:
  `Start`, `LevelManager.Awake`'ten sonra çalıştığı için müdahale ederse kayıtlı
  ilerlemeyi her açılışta ezerdi.
- `startLevel` (1–8 enum) — yalnızca `useSavedProgress` **kapalıyken** geçerlidir;
  `Start`'ta `LevelManager.SetLevel(startLevel - 1)` çağrılır (enum 1 tabanlı,
  `SetLevel` 0 tabanlı index bekler). Belirli bir bölümü test etmenin yoludur.
- `completeLevelKey` (varsayılan F9) — eksik puanı `ScoreManager.AddScore` ile
  bir kerede ekler, yani normal akışın aynısı çalışır (`OnTargetReached` →
  `GameManager` → bölüm sonu sunumu). Klavye odağı Game view'da değilse tuş
  algılanmadığı için `[ContextMenu("Bolumu Tamamla")]` yedeği de vardır.
- `failLevelKey` (varsayılan F10) — kalan canları `HealthManager.LoseLife()` ile
  tek tek eksiltir; meteor çarpınca `ShipCollisionHandler`'ın çağırdığı metodun
  aynısıdır, yani ölüm sekansı ve oyun sonu paneli normal akışla gelir. Gerçek
  bir çarpışma olmadığı için kamera sarsıntısı ve hasar efekti oynamaz.
  `[ContextMenu("Canlari Bitir")]` yedeği vardır.

- **Bağlantı:** `LevelManager`, `ScoreManager`, `HealthManager`, `GameManager` —
  hepsi tek yönlü, yalnızca mevcut public API çağrılır.

---

## Editor

### `PlanetDataGenerator.cs`
`Assets/Scripts/Editor/`

Editör menü aracı: **Tools > Paye Games > Asal Yolculuk > Generate Planet
Data**. 8 gezegenin verisini `Assets/Scripts/Runtime/ScriptableObjects/Planets/`
klasöründe `Planet_1`...`Planet_8` asset'leri olarak oluşturur/günceller:
min/max sayı aralığı, `priorityNumberList`, `meteorSpawnRate` (0.4 → 1.2) ve
maden ayarları.

Bu araç bu alanların **tek kaynağıdır** — Inspector'dan yapılan elle düzenlemeler
yeniden üretimde kaybolur; kalıcı değişiklik dosyanın başındaki dizi ve
`Planets` tablosundan yapılır. `ValidateNumbers` her öncelikli sayının kendi
aralığında olduğunu ve en fazla 5 asal çarpanı bulunduğunu (GDD kuralı) denetler;
ihlalleri üretimi durdurmadan tek uyarıda toplar.

`OutputFolder` sabiti asset'lerin gerçek konumuyla aynı kalmalıdır; şaşarsa araç
yeni GUID'li kopyalar üretir ve sahnedeki `LevelManager` eski asset'lere bağlı
kaldığı için değişiklikler oyuna hiç yansımaz.

- **Bağlantı:** `PlanetDataSO` (ürettiği asset), `PrimeMathEngine` (çarpan
  denetimi), `LevelManager` (asset'leri listesinde tutar).

---

## Henüz Yok / Planlanan
Aşağıdakiler `CLAUDE.md`'de hedef mimari olarak tanımlı ama henüz kodlanmadı:
`SaveManager`, `GameConfigSO`, `UIManager`/`ScreenController`.

**Maden sistemi tamamlandı.** 5 ödül (Yıldız Parçası, Altın Kristal, Plazma
Yakıtı, Zaman Kristali, Koruma Kalkanı) + 2 ceza (Meteor Çağırıcı, Yerçekimi
Taşı). `ChaosCore` bilinçli olarak kapsam dışı bırakıldı (yukarıya bakınız).

Maden tarafında kalan iki iş `YAPILACAKLAR.md` 3.7 ve 3.8'de: **ağırlıklı maden
seçimi** (şu an bir maden düştüğünde hangisi olacağı eşit olasılıkla seçiliyor;
Koruma Kalkanı ile Yıldız Parçası aynı sıklıkta çıkıyor) ve **3 eksik görsel**.

Kayıt tarafı notu: toplam puan şu an `ScoreManager` içinde doğrudan `PlayerPrefs`
ile saklanıyor (`AsalYolculuk.TotalScore`). `SaveManager`/`SaveData` geldiğinde bu
alan oraya taşınacak; anahtar adı korunursa mevcut oyuncu verisi kaybolmaz.

Can / oyun sonu tarafında eksik kalanlar:
- **Yeniden başlatma akışı:** ✅ Tamamlandı — `GameOverPanelView` +
  `LevelFlowController.RestartLevel()` / `GoToMainMenu()` +
  `PlayerDeathEffect.RestoreShip()`.
- **Son bölüm bitişi:** `LevelCompletePanelView` son gezegende bilinçli olarak
  açılmıyor (`LevelManager.IsLastLevel`); onun yerine `BitisManager` devreye
  giriyor. Panelin kendisi ve kutlama efekti bitti; eksik kalan yalnızca
  "Sonsuz Yolculuk" girişi (bkz. `YAPILACAKLAR.md` 5.6).
- `GameState.Paused` durumu kodda tanımlı ama henüz hiçbir yerden tetiklenmiyor
  (duraklatma menüsüyle gelecek).

Bölüm sonu akışının mimarisi hakkında not: sunum adımları (`MeteorFieldClearer`,
`GameplayHUDPanel`, `PlayerLevelCompleteEffect`, `LevelCompletePanelView`) ayrı
ayrı `GameManager.OnGameStateChanged` dinliyor ve zamanlamalarını kendi
`delay` alanlarından alıyor. Esnek ama sıralama Inspector'dan elle tutturuluyor;
akış karmaşıklaşırsa tek bir `LevelCompleteSequencer` altında toplanabilir.

Bölünme tarafında eksik kalanlar:
- Bölüme göre bölünme modu: şu an **tüm** gezegenlerde bileşik sayı doğrudan
  asal çarpanlarına ayrılıyor. İleri gezegenlerde istenen "önce iki ara
  çarpana (`GetDivisionPair`), sonra asallara" davranışı için `PlanetDataSO`'ya
  bir bölünme modu alanı eklenip `MeteorController.TrySplitIntoPrimeFragments`
  içinde dallanılacak. `MeteorSession` bu çok kademeli yapıya zaten hazır.

Puanlama tarafında eksik kalanlar:
- Altın Kristal etkisinde meteor başına +10 (×2 çarpan) ve Yıldız Parçası ile
  anında +10 puan (`PowerUpManager` ile birlikte gelecek).
- `AddMeteorScore` içinde oynanış durumu kontrolü yok: ölüm sekansı sürerken
  havada kalan mermilerin vurduğu meteorlar hâlâ puan veriyor. Bölüm puanı
  yeniden başlatmada zaten sıfırlandığı ve toplam puan geri alındığı için
  görünür bir etkisi kalmadı, yine de guard eklenmesi temiz olur.
