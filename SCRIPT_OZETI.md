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
`bulletPool` ve `firePoint` referansları Inspector'dan elle atanır.

- **Bağlantı:** `GameObjectPool` (hangi pool'dan mermi alacağını belirler),
  dolaylı olarak `Bullet.cs` (pool'un ürettiği prefab).

---

## Core

### `PrimeMathEngine.cs`
`Assets/Scripts/Runtime/Core/`

Statik matematik motoru. Başlangıçta (static constructor) Sieve of Eratosthenes
ile 10.000'e kadar asal sayıları `HashSet<int>` içinde önbelleğe alır.
`IsPrime(int)` (cache üstünde O(1), üstünde ise fallback bölme kontrolü),
`GetPrimeFactors(int)` ve `GetDivisionPair(int)` (bileşik sayıyı iki çarpana
ayırır) metotlarını sağlar.

- **Bağlantı:** `MeteorController` sayı atanırken `IsPrime` çağırır. İleride
  meteor bölünme mantığında `GetDivisionPair`/`GetPrimeFactors` kullanılacak.

### `LevelManager.cs`
`Assets/Scripts/Runtime/Core/`

Tek sahne içinde hangi seviyede/gezegende olunduğunu tutan singleton
(`LevelManager.Instance`, dar kapsamlı — DI yerine bilinçli tercih edildi).
`planetDataList` (Inspector'dan atanan `PlanetDataSO[]`) ve `currentLevelIndex`
üzerinden `CurrentPlanetData` döner. `SetLevel(int)` ile seviye ilerletilir.

- **Bağlantı:** `PlanetDataSO` (veri kaynağı), `MeteorSpawner` bu singleton
  üzerinden `CurrentPlanetData`'yı okur.

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
`meteorSpriteRenderer` alanına `SetSprite(Sprite)` ile dışarıdan (spawner'dan)
gövde görseli atanır.

- **Bağlantı:** `GameObjectPool`/`IPoolable`, `Bullet` (trigger çarpışmasında
  ikisini de despawn eder — asal/bileşik bölünme mantığı henüz yok),
  `PrimeMathEngine`.

### `MeteorSpawner.cs`
`Assets/Scripts/Runtime/Meteors/`

`spawnRate` aralığında ekranın sağ kenarından (viewport dışı, `spawnMargin`
kadar tampon) rastgele dikey konumda meteor üretir. `LevelManager.Instance
.CurrentPlanetData`'dan aldığı `PlanetDataSO`'ya göre GDD'deki %70 öncelikli
liste / %30 rastgele aralık kuralıyla (`PriorityNumberChance = 0.7f`) sayı
üretip spawn edilen meteora `SetNumber` ile yazdırır. `meteorSprites[]`
dizisinden (7 görsel, Inspector'da basitten karmaşığa sıralı) seviyeye göre
kilidi açık olan alt kümeden (`baseSpriteCount + LevelManager.Instance
.CurrentLevelIndex`, dizi uzunluğuna clamp'li) rastgele bir görsel seçip
`SetSprite` ile atar.

- **Bağlantı:** `GameObjectPool`, `MeteorController`, `LevelManager`,
  `PlanetDataSO`.

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
`GameManager`, `PowerUpManager`, `SaveManager`, meteor asal çarpanlarına
bölünme mantığı (`GetDivisionPair` ile fragment oluşturma), puan/can sistemi,
gezegenlere özgü öncelikli sayı listelerinin doldurulması.
