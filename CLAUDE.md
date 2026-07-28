# CLAUDE.md — Asal Yolculuk (Paye Games)

Bu dosya **Asal Yolculuk** projesine özgü mimari, kodlama standartları, sistem tasarımı ve teknik GDD bilgilerini içerir. Global Paye Games standartları ile proje gereksinimlerini harmanlar.

---

## 1. Proje Genel Bilgileri ve Teknik Özellikler

* **Proje Adı:** Asal Yolculuk (2D Eğitsel Uzay Arcade Oyunu)
* **Oyun Motoru:** Unity 6 (LTS önerilir)
* **Programlama Dili:** C# (.NET 8 Uyumlu)
* **Hedef Platformlar:** WebGL (Öncelikli), Mobil (Android / iOS) ve Etkileşimli Akıllı Tahtalar
* **Ekran Oranı (Aspect Ratio):** 1080 × 1920 (Dikey Mod, Duyarlı/Responsive Canvas)
* **Render Pipeline:** Hafif ve yüksek performanslı 2D ışıklar/VFX için Universal Render Pipeline (URP)

---

## 2. Klasör Yapısı ve Proje Başlatma (Init)

Yeni bir projeyi başlatırken veya klasör yapısını kurarken aşağıdaki komutları/yöntemleri kullanın:

* **Terminalden Başlatma (Init):**
  ```powershell
  powershell -ExecutionPolicy Bypass -File init_project.ps1
  ```
* **Unity Editöründen Başlatma:**
  * Proje Unity'de açıkken üst menüden **`Tools -> Paye Games -> Initialize Project Folders`** yolunu izleyerek klasör yapısını oluşturabilirsiniz.

### Klasör Yapısı Standartları
* `Assets/Art/` — Görsel kaynaklar (Sprites, Animations, Materials, UI)
* `Assets/Audio/` — Ses dosyaları (Music, SFX)
* `Assets/Prefabs/` — Yeniden kullanılabilir prefab şablonları
* `Assets/Scenes/` — Oyun sahneleri
* `Assets/Settings/` — URP/Render pipeline ayarları
* `Assets/Scripts/` — C# kod dosyaları
  * `Assets/Scripts/Runtime/` — Çalışma zamanı kodları
  * `Assets/Scripts/Editor/` — Editör arayüzü eklenti kodları (örn: `ProjectInitializer.cs`)

---

## 3. Sistem Mimarisi & Teknik Altyapı (Model-View-Controller / Event-Driven)

Alt sistemlerin birbirine sıkı sıkıya bağlanmasını (tight coupling) önlemek amacıyla **Veri (Models)**, **Durum Yönetimi/Mantık (Controllers)** ve **Görselleştirme/UI (Views)** katmanları birbirinden ayrılmıştır. Sistemler arası iletişim, tip güvenli (type-safe) C# Actions veya Event Bus aracılığıyla olay tabanlı olarak gerçekleştirilir.

### 3.1 Çekirdek Sistemler & Sınıf Mimarisi
* **`GameManager`:** Oyun döngüsü, durum geçişleri (`GameState` FSM), skor ve can takibi.
* **`PlanetManager`:** Gezegen verilerinin (`PlanetDataSO`) ve kilit açma ilerlemesinin yönetimi.
* **`MeteorSpawner` & `MeteorController`:** Meteor ve alt parçalarının (`MeteorFragmentController`) üretimi, hareketi ve ayrışması.
* **`PlayerShipController`:** Oyuncu gemi hareketi, kalkan/hasar mekaniği ve yetenek etkileri.
* **`PowerUpManager`:** Güçlendiriciler ve cezaların yönetimi (`IActiveEffect`, Strategy/Command Pattern).
* **`SaveManager`:** Oyun verilerinin serileştirilmesi ve `PlayerPrefs` / JSON ortamında saklanması (`SaveData`).
* **`UIManager` / `HUDView` / `ScreenController`:** Arayüz bileşenleri ve ekran geçiş yönetimi.

### 3.2 Matematik Motoru (`PrimeMathEngine`)
* Statik C# kütüphanesidir.
* Sieve of Eratosthenes algoritması ile önbelleğe alınmış `HashSet<int>` üzerinden $O(1)$ sürede asallık kontrolü yapar.
* `IsPrime(int number)`: Sayının asal olup olmadığını doğrular.
* `GetPrimeFactors(int number)`: Asal çarpanların listesini döner.
* `GetDivisionPair(int compositeNumber)`: Bileşik sayıyı bölünme çiftine (`factorA`, `factorB`) ayırır.

### 3.3 Meteor Bölünme & Oluşturulma Kuralları
1. **Asal Sayı Meteoru Vurulduğunda:** Patlama VFX + SFX tetiklenir, meteor yok edilir, oyuncuya $+5$ puan verilir.
2. **Bileşik Sayı Meteoru Vurulduğunda:** `PrimeMathEngine.GetDivisionPair(N)` çağrılarak $(factorA, factorB)$ çifti türetilir, iki `MeteorFragment` zıt yönlerde yatay hız ve yukarı kuvvet ile fırlatılır, ana meteor yok edilir.
3. **Meteor Değer Dağılımı:**
   * %70 Olasılık: `PlanetDataSO.priorityNumberList` (Pedagojik öncelikli sayılar).
   * %30 Olasılık: `[minNumber, maxNumber]` aralığında rastgele sayılar.

### 3.4 Oyun Döngüsü & Durum Makinesi (`GameState`)
```csharp
public enum GameState
{
    MainMenu,
    MapSelection,
    TravelTransition,
    Gameplay,
    LevelComplete,
    LevelFailed,
    Paused
}
```

### 3.5 Güçlendirici (Power-Up) & Ceza (Curse) Sistemi (`IActiveEffect`)
* **Güçlendiriciler:** `TimeCrystal`, `GoldCrystal`, `PlasmaFuel`, `Shield`, `StarFragment`
* **Cezalar:** `ChaosCore`, `GravityStone`, `MeteorCaller`
* **Arayüz:**
  ```csharp
  public interface IActiveEffect
  {
      EffectType Type { get; }
      void ApplyEffect();
      void TickEffect(float deltaTime);
      void RemoveEffect();
  }
  ```

### 3.6 ScriptableObject ve Kayıt Sistemi
* **`PlanetDataSO`:** Gezegen ID, ismi, sprite'ı, sayı sınırları (`minNumber`, `maxNumber`), `priorityNumberList`, renk teması, hız ve üretim sıklığı çarpanları.
* **`GameConfigSO`:** `targetScorePerPlanet`, `initialPlayerLives`, `scorePerMeteorDestroyed`, `powerUpDuration`, drop rate'ler.
* **`SaveData` & `SaveManager`:** `maxUnlockedPlanetID`, `completedPlanetIDs`, `totalPointsCollected`, `endlessModeHighScore`, ses ayarları. `PlayerPrefs` üzerinde JSON serileştirme ile saklanır.

---

## 4. İletişim ve Kodlama Standartları

### 4.1 İletişim Kuralları
- Yanıtları **Türkçe** ver, dil değiştirme.
- Doğrudan ve somut ol. Gereksiz uzatma; net bir öneri sun ve gerekçesini açıkla.
- Kod yorumları ve commit mesajları Türkçe olabilir; kodun kendisi (tanımlayıcılar, tipler, degişkenler) İngilizce yazılır.

### 4.2 C# İsimlendirme Standartları
> **ÖNEMLİ:** private/protected alanlarda **underscore prefix KULLANILMAZ.**
> `_currentHealth` değil, `currentHealth`. Alan ile parametre/yerel değişken çakışırsa `this.` kullanılır.

| Öğe | Kural | Örnek |
|---|---|---|
| private/protected alan | `camelCase` (underscore yok) | `private int currentHealth;` |
| public alan (kaçın) | `PascalCase` | `public int MaxHealth;` |
| property | `PascalCase` | `public int Health { get; private set; }` |
| metot | `PascalCase` | `void ApplyDamage(int amount)` |
| yerel değişken / parametre | `camelCase` | `int totalDamage`, `amount` |
| const | `PascalCase` | `const int MaxLevel = 99;` |
| static readonly | `PascalCase` | `static readonly Color WarningColor;` |
| interface | `I` + `PascalCase` | `IActiveEffect`, `IDamageable` |
| enum tipi ve üyeleri | `PascalCase` | `enum GameState { Gameplay, Paused }` |
| boolean | `is`/`has`/`can` öneki | `isInvulnerable`, `hasKey`, `canJump` |
| namespace | `PayeGames.{Modül}` | `PayeGames.PrimeJourney` |
| event | `PascalCase`, fiil/olay | `OnScoreChanged` |

### 4.3 C# Yapı ve Clean Code
- **Guard Clause:** Early return tercih et. Hedef maksimum iç içe geçme derinliği: 2–3.
- Metotlar tek bir iş yapsın (SRP).
- Disambiguation için `this.` kullan (`this.currentHealth = currentHealth`).
- Kısaltmalardan kaçın (`dmg` yerine `damage`, `idx` yerine `index`).

### 4.4 Unity Standartları
- Inspector alanları için `public` yerine **`[SerializeField] private`** kullan.
- Component referanslarını `Awake` içinde cache'le; `Update` içinde `GetComponent` / `Find` çağırma.
- Tek `GetComponent` + null kontrolü yerine `TryGetComponent` tercih et.
- Sık üretilen nesneler (mermi, meteor parçaları vb.) için **Object Pooling** kullan; hot path'te allocation yapma.
- `Update` döngüsünde LINQ, string concatenation veya GC allocation yapma.
- Veri ve mantığı ayırmak için **ScriptableObject** kullan.
- Unity nesnelerinde fake null davranışına dikkat: `?.` ve `??` yerine açık `== null` kontrolü yap.

---

## 5. Genişletilebilirlik Kılavuzu (Matematik Şablonu)

Bu projenin mimarisi başka eğitsel konulara (örneğin *Kesir Sadeleştirme* veya *Ortak Kat Bulma*) uyarlanabilir:
1. **`PrimeMathEngine`:** Yeni matematik konusunun kurallarına göre yenileyin.
2. **`MeteorController.SplitMeteor()`:** Yeni matematik konseptine uygun bölünme mantığını kurgulayın.
3. **`PlanetDataSO`:** Sayı aralıklarını ve öncelikli sayı listesini Unity Inspector üzerinden güncelleyin.

---

## 6. Script Özeti Takibi (`SCRIPT_OZETI.md`)

Proje kök dizininde `SCRIPT_OZETI.md` adında bir özet dosyası tutulur. Bu dosya
script'lerin ne işe yaradığını ve birbirleriyle bağlantısını (hangi script
hangisine referans veriyor / hangi arayüzü uyguluyor) kısaca listeler; amacı
projeye ara verip geri dönüldüğünde script'ler arası ilişkiyi hızla hatırlatmaktır.

- Bir script üzerinde çalışma (yeni script, önemli bir değişiklik/refactor)
  bittiğinde, dosyayı otomatik güncelleme — kullanıcıya kısaca sor:
  **"Bunu `SCRIPT_OZETI.md`'ye ekleyeyim mi?"**
- Kullanıcı onaylarsa script adı, dosya yolu, kısa işlev açıklaması ve varsa
  bağlantılı olduğu diğer script'ler eklenir (mevcut formatı takip et).
- Küçük/kozmetik değişikliklerde (yeniden adlandırma, formatlama) sorma; sadece
  yeni bir script veya sorumluluğu/bağlantıları değiştiren değişikliklerde sor.
