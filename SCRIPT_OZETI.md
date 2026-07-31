# Script Özeti

Bu dosya, projedeki script'lerin ne işe yaradığını ve birbirleriyle nasıl
bağlantılı olduğunu takip etmek için tutulur. Yeni bir script tamamlandığında
buraya kısa bir madde eklenir.

---

## Player

### `PlayerShipController.cs`
`Assets/Scripts/Runtime/Player/`

Gemiyi klavyeden (yeni Input System, kod içinde tanımlı WASD + ok tuşları
composite'i) hareket ettirir. Transform tabanlı, `SmoothDamp` ile yumuşatılmış
hareket (Rigidbody yok). `Camera.main` ile kamerayı otomatik bulur (Inspector'dan
atama yok). Yatayda `minViewportX` / `maxViewportX` ile hareket alanı sınırlanır
(0 = ekranın solu, 1 = ekranın sağı); dikeyde tüm ekran serbest.

- **Bağlantı:** Yok (bağımsız çalışır).

### `ShipCollisionHandler.cs`
`Assets/Scripts/Runtime/Player/`

Gemiye çarpan meteoru yakalar. Meteorun collider'ı trigger, geminin dinamik
`Rigidbody2D`'si olduğu için trigger olayı **gemi tarafında** tetiklenir; hasar
sorumluluğu bilinçli olarak burada tutulur (meteor kime çarptığını bilmez).
Çarpışmada `meteor.DespawnOnShipHit()` ve `HealthManager.Instance.LoseLife()`
çağrılır. `invulnerableDuration` (varsayılan 1.5 sn) boyunca sonraki çarpışmalar
yok sayılır — üst üste gelen meteorlar tek karede birden fazla can götürmesin.
Oyun `Gameplay` durumunda değilse (`GameManager.IsGameplayActive`) hasar alınmaz.

**Hasar geri bildirimi:** Her hasarda `CameraShake.Instance.Shake(shakeDuration,
shakeStrength)` çağrılır. Ardından **oyuncu hâlâ hayattaysa** (`HealthManager
.IsAlive`) `damageEffectPrefab` geminin **child'ı** olarak oluşturulur — gemi
hareket ederken efekt de onunla gider. Son canda hasar efekti bilinçli olarak
oynatılmaz; patlama sekansını `PlayerDeathEffect` devralır. Yani 1. ve 2.
çarpışmada hasar efekti, 3.'te ölüm sekansı çalışır.

- **Bağlantı:** `MeteorController` (`DespawnOnShipHit`), `HealthManager`
  (can eksiltme + `IsAlive` kontrolü), `GameManager` (durum kontrolü),
  `CameraShake`, dolaylı olarak `PlayerDeathEffect`.

### `PlayerDeathEffect.cs`
`Assets/Scripts/Runtime/Player/`

Gemiye eklenir, ölüm sekansının tamamını yönetir. `HealthManager.OnAllLivesLost`
event'ine `Start`'ta abone olur (manager'ın `Awake`'i garanti çalışmış olsun diye),
`OnDestroy`'da bırakır. Sekans tek bir coroutine zinciridir:

1. **`HideShip()`** — gemi ve alt objelerindeki tüm `Renderer` ve `Collider2D`
   bileşenleri kapatılır, `PlayerShipController` ve `PlayerAutoShooter` devre
   dışı bırakılır. `gameObject.SetActive(false)` **bilinçli olarak kullanılmaz**:
   patlama coroutine'i bu obje üzerinde çalışır, obje kapanırsa sekans ilk
   patlamadan sonra durur.
2. **`PlayExplosionSequence()`** — `explosionCount` (varsayılan 6) kez patlama
   efekti; ilki tam geminin üzerinde, sonrakiler `explosionRadius` yarıçapında
   rastgele yakın noktalarda, `explosionInterval` aralıkla.
3. `meteorClearDelay` beklemesi.
4. **`ClearMeteorsSequentially()`** — `MeteorController.ActiveMeteors` listesinin
   kopyası alınıp X pozisyonuna göre sıralanır (soldan sağa şok dalgası hissi),
   her meteor `meteorClearInterval` aralıkla `DespawnWithExplosion()` ile kendi
   patlama efektiyle yok edilir. Liste kopyalanır çünkü meteorlar patladıkça
   aktif listeden çıkar.

- **Bağlantı:** `HealthManager` (`OnAllLivesLost`), `MeteorController`
  (`ActiveMeteors`, `DespawnWithExplosion`), `PlayerShipController`,
  `PlayerAutoShooter`.

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

Gemiye eklenen otomatik ateş component'i. `fireRate` (saniyede kaç mermi)
aralığında `bulletPool.Spawn(firePoint.position, firePoint.rotation)` çağırır.
`bulletPool` ve `firePoint` referansları Inspector'dan elle atanır. `Update`
başında `GameManager.IsGameplayActive` guard'ı vardır; canlar bitince ateş durur.

- **Bağlantı:** `GameObjectPool` (hangi pool'dan mermi alacağını belirler),
  dolaylı olarak `Bullet.cs` (pool'un ürettiği prefab), `GameManager` (guard).

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
üzerinden `CurrentPlanetData` döner. `SetLevel(int)` ile seviye ilerletilir.

- **Bağlantı:** `PlanetDataSO` (veri kaynağı), `MeteorSpawner` bu singleton
  üzerinden `CurrentPlanetData`'yı okur.

### `ScoreManager.cs`
`Assets/Scripts/Runtime/Core/`

Bölüm puanını tutan singleton (`ScoreManager.Instance`). `targetScore`
(GDD'ye göre 100) ve `scorePerMeteor` (5) Inspector'dan ayarlanır.
`AddMeteorScore()`, `AddScore(int)` ve `ResetScore()` metotlarını sağlar;
her değişimde `OnScoreChanged(currentScore, targetScore)` event'ini yayar.
Puan mantığı UI'dan tamamen bağımsızdır — ekrana yazma işini dinleyen taraf
yapar. `AddScore` negatife düşmeyi engeller (ileride ceza madenleri puan
düşürebilir).

- **Bağlantı:** `MeteorController` (oturum tamamlanınca `AddMeteorScore`
  çağırır), `ScoreHUDView` (event'i dinler).

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

Oyun durumunu tutan singleton. `HealthManager.OnAllLivesLost`'a `Start`'ta abone
olur (manager'ın `Awake`'i garanti çalışmış olsun diye) ve canlar bitince
`SetState(GameState.LevelFailed)` ile durumu değiştirir. Her durum değişiminde
`OnGameStateChanged(GameState)` yayınlanır.

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

- **Bağlantı:** `HealthManager` (oyun sonu tetikleyicisi), `GameState`,
  `MeteorSpawner` ve `PlayerAutoShooter` (`IsGameplayActive` guard'ı),
  `ShipCollisionHandler`.

### `CameraShake.cs`
`Assets/Scripts/Runtime/Core/`

Kameraya eklenen sarsıntı bileşeni (`CameraShake.Instance`). DOTween
`DOShakePosition` ile çalışır; `Shake()` varsayılan ayarları, `Shake(duration,
strength)` çağrı bazlı değerleri kullanır. Z ekseni sarsılmaz (2D kamerada
derinlik kayması istenmez). Üst üste gelen sarsıntılarda kamera kaymasın diye
her çağrıda önceki tween `DOKill` edilip pozisyon `Awake`'te cache'lenen
`basePosition`'a çekilir, tween bitiminde de aynı pozisyona sıfırlanır.

- **Bağlantı:** `ShipCollisionHandler` (hasar anında çağırır), DOTween.

---

## Data

### `PlanetDataSO.cs`
`Assets/Scripts/Runtime/Data/`

Gezegen/seviye başına veri tutan ScriptableObject (`Create > PayeGames >
AsalYolculuk > Planet Data`). `planetId`, `planetName`, `minNumber`,
`maxNumber`, `priorityNumberList` (pedagojik öncelikli sayılar) alanlarını
içerir. GDD'deki (`AsalYolculuk_Senaryo.md`) gezegen sayı aralıkları tablosuna
göre 8 gezegen için asset üretilir (`Planet_1`...`Planet_8`).

- **Bağlantı:** `LevelManager` (listede tutulur), `MeteorSpawner` (sayı
  üretirken okur).

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

**Tamamlanma duyurusu:** `public static event Action<int> OnMeteorCompleted`.
Ana meteor tüm parçalarıyla yok edildiğinde `CompleteMeteor(rootNumber)`
çalışır; önce event yayınlanır, sonra `ScoreManager.AddMeteorScore()` çağrılır.
Root sayı, oturumu olmayan (tek vuruşta ölen asal) meteorda kendi `number`'ı,
bölünmüş meteorda `session.RootNumber`'dır. Event'in **static** olmasının
sebebi meteorların havuzdan gelmesi — prefab örneklerine Inspector'dan
dinleyici bağlanamaz. Parçası ekrandan kaçan meteorda oturum `Cancel()`
edildiği için event hiç tetiklenmez.

- **Bağlantı:** `GameObjectPool`/`IPoolable` (hem kendi despawn'ı hem parça
  spawn'ı için), `Bullet` (trigger çarpışması), `PrimeMathEngine`
  (`IsPrime`, `GetPrimeFactors`), `MeteorSession` (parça tamamlanma takibi),
  `ScoreManager` (puan verme), `MeteorSpawner` (`OnMeteorCompleted` dinleyicisi),
  `ShipCollisionHandler` (`DespawnOnShipHit`), `PlayerDeathEffect`
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

### `MeteorSpawner.cs`
`Assets/Scripts/Runtime/Meteors/`

`spawnRate` aralığında ekranın sağ kenarından (viewport dışı, `spawnMargin`
kadar tampon) rastgele dikey konumda meteor üretir. Meteorun sayısını
`LevelNumberPool`'dan alıp `SetNumber` ile yazdırır. `GetNumberPool()` aktif
`PlanetDataSO`'yu her çağrıda kontrol eder; gezegen değişince havuzu yeniden
kurar, böylece tamamlanan sayı listesi bir sonraki bölüme taşınmaz.
`MeteorController.OnMeteorCompleted` event'ine `OnEnable`/`OnDisable`'da abone
olur ve tamamlanan sayıyı havuza bildirir. `ResetNumberPool()` public'tir;
canlar bitip bölüm yeniden başladığında çağrılacaktır. `Update` başında
`GameManager.IsGameplayActive` guard'ı vardır; canlar bitince üretim durur.

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

---

## Editor

### `PlanetDataGenerator.cs`
`Assets/Scripts/Editor/`

Editör menü aracı: **Tools > Paye Games > Asal Yolculuk > Generate Planet
Data**. GDD'deki (`AsalYolculuk_Senaryo.md`) 8 gezegenin min/max sayı
aralığını `Assets/Scripts/Runtime/ScriptableObjects/` klasöründe
`Planet_1`...`Planet_8` asset'leri olarak oluşturur/günceller. `priorityNumberList`'e
dokunmaz (GDD'de somut sayı verilmediği için Inspector'dan elle doldurulur).

- **Bağlantı:** `PlanetDataSO`.

---

## Henüz Yok / Planlanan
Aşağıdakiler `CLAUDE.md`'de hedef mimari olarak tanımlı ama henüz kodlanmadı:
`PowerUpManager`, `SaveManager`, `UIManager`/`ScreenController`, gezegenlere
özgü öncelikli sayı listelerinin doldurulması.

Can / oyun sonu tarafında eksik kalanlar:
- Ölüm sekansı bittikten sonrası: "tekrar dene" ekranı, `HealthManager
  .ResetHealth()` + `ScoreManager.ResetScore()` + `MeteorSpawner
  .ResetNumberPool()` çağrılarını yapan yeniden başlatma akışı.
- `GameState.Paused` durumu kodda tanımlı ama henüz hiçbir yerden tetiklenmiyor
  (duraklatma menüsüyle gelecek).

Bölünme tarafında eksik kalanlar:
- Bölüme göre bölünme modu: şu an **tüm** gezegenlerde bileşik sayı doğrudan
  asal çarpanlarına ayrılıyor. İleri gezegenlerde istenen "önce iki ara
  çarpana (`GetDivisionPair`), sonra asallara" davranışı için `PlanetDataSO`'ya
  bir bölünme modu alanı eklenip `MeteorController.TrySplitIntoPrimeFragments`
  içinde dallanılacak. `MeteorSession` bu çok kademeli yapıya zaten hazır.

Puanlama tarafında eksik kalanlar:
- Altın Kristal etkisinde meteor başına +10 (×2 çarpan) ve Yıldız Parçası ile
  anında +10 puan (`PowerUpManager` ile birlikte gelecek).
- 100 puana ulaşınca bölüm tamamlama akışı (`GameManager.SetState(GameState
  .LevelComplete)` çağrılacak yer henüz yok).
- Canlar bitince `ScoreManager.ResetScore()` çağrısı (yeniden başlatma akışıyla).
