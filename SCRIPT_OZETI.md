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

## Henüz Yok / Planlanan
Aşağıdakiler `CLAUDE.md`'de hedef mimari olarak tanımlı ama henüz kodlanmadı:
`GameManager`, `PlanetManager`, `MeteorSpawner`, `PowerUpManager`, `SaveManager`,
Meteor/asal çarpanlama mantığı, mermi-meteor çarpışma/hasar sistemi.
