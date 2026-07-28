# **EĞİTSEL OYUN GDD ŞABLONU** 

|**Proje Adı**|**Asal Yolculuk**|
|---|---|
|**Sürüm**|1.0|
|**Tarih**|24 Haziran|
|**Hazırlayan(lar)**|Filiz Şentürk|
|**Onaylayan(lar)**|______________________________|



### **Kullanım Notu** 

Bu doküman bir şablondur. Boş alanları ve tabloları proje bilgileriyle doldurun. Sol kolon, ekran taslağı/mekanik diyagramı/mockup için; sağ kolon metin alanıdır. İçerik EK-1, EK-2 ve EK-3 başlıkları altında organize edilmiştir. 

### **EK-1: Senarist İş Akışı Şablonu** 

#### **EK-1.1 Proje Brief'i** 

|Mockup / Wireframe / Notlar (görsel<br>ekleyin)|Amaç (1-2 cümle): Asal Yolculuk, oyuncuların asal sayılar ve asal<br>çarpanlara ayırma konularını eğlenceli bir uzay macerası içerisinde<br>öğrenmelerini ve pekiştrmelerini amaçlayan bir eğitsel mobil<br>oyundur. Oyuncular, gezegenler arasında ilerlerken sayı içeren<br>meteorları asal çarpanlarına ayırarak yok eder, problem çözme ve<br>matematksel düşünme becerilerini geliştrir.<br>Öğrenme Alanı / Ders:|
|---|---|
||_____________________________________________<br>Hedef Kitle (yaş/sınıf):|
||__________________________________________<br>Platorm(lar): PC / Mobil / WebGL / VR / AR / Diğer: PC /WebGL<br>Kapsam (neler var): Asal Yolculuk; asal sayılar ve asal çarpanlara<br>ayırma konularını öğreten, uzay temalı bir mobil eğitsel oyundur.<br>Oyunda oyuncu bir uzay gemisini kontrol ederek sayı içeren<br>meteorları parçalar ve asal çarpanlarına ayırarak yok eder. Oyun; 8<br>farklı gezegen, gezegenlere özgü mekanikler, can sistemi, puan<br>sistemi, ödül ve ceza madenleri, gezegen haritası, gezegenler arası<br>yolculuk ekranları, bölüm tamamlama ekranları ve Sonsuz Yolculuk<br>modunu içerir. Oyuncu her gezegende 100 puan toplayarak bir<br>sonraki gezegenin kilidini açar ve sonunda Asal Krallığı'na ulaşır.<br>Oyun boyunca asal sayılar, bileşik sayılar ve asal çarpanlara ayırma<br>becerileri oyunlaştrılmış etkinliklerle desteklenir.<br>Kapsam Dışı (neler yok):|
||●<br>Çok oyunculu (multplayer) oyun modu<br>●<br>Çevrimiçi rekabet sistemi<br>●<br>Oyuncular arası sohbet veya iletşim özellikleri<br>●<br>Hesap oluşturma ve kullanıcı girişi<br>●<br>Bulut kayıt sistemi<br>●<br>Karakter geliştrme veya seviye atlama sistemi<br>●<br>Uzay gemisi özelleştrme sistemi|
||Referanslar:<br>Asal Yolculuk oyununun tasarımında eğitsel matematk oyunları,<br>mobil arcade oyunları ve uzay temalı aksiyon oyunları referans<br>alınmıştr. Oyunun matematksel içeriği ilkokul ve ortaokul<br>düzeyindeki asal sayılar ve asal çarpanlara ayırma kazanımlarına<br>dayanmaktadır.|



|İlham alınan oyun ve tasarım örnekleri:|
|---|



● Angry Birds Space (uzay teması ve görsel atmosfer) ● Jetpack Joyride (basit kontrol yapısı ve sürekli ilerleme hissi) ● Galaxy Attack: Space Shooter (uzay gemisi ve meteor etkileşimleri) ● Space Shooter Galaxy Attack (arcade uzay savaşı mekanikleri) Matematiksel içerik referansları: ● Türkiye Cumhuriyeti Millî Eğitim Bakanlığı Matematik Öğretim Programı ● Ortaokul Matematik Ders Kitapları ● Asal Sayılar ve Asal Çarpanlara Ayırma konuları ● Eğitsel Oyun Tasarımı ve Oyunlaştırma Yaklaşımları Varsayımlar ve Kısıtlar: Oyuncuların temel sayı bilgisine sahip olduğu varsayılmaktadır. Oyuncuların dokunmatik ekran kullanımına aşina olduğu kabul edilmektedir. Oyuncuların internet bağlantısı olmadan da oyunu oynayabileceği varsayılmaktadır. Oyuncuların matematiksel bilgileri oyun oynadıkça geliştireceği kabul edilmektedir. Asal sayılar ve asal çarpanlara ayırma konularının oyun mekanikleri aracılığıyla öğrenilebileceği varsayılmaktadır. Oyuncuların ödül ve ceza madenlerinin işlevlerini deneyimleyerek keşfedeceği varsayılmaktadır. Oyuncuların gezegenler arasında ilerleme motivasyonuna sahip olacağı kabul edilmektedir. 

#### **EK-1.2 Öğrenme Tasarımı ve Ölçme Yaklaşımı (Genel)** 

|Notlar / rubrik taslağı / örnek geri bildirim|Öğrenme Kazanımları (kısa liste):|
|---|---|
|(opsiyonel)|**MAT.6.1.3. Bir doğal sayının asal olma durumunu ve asal**<br>**çarpanlarını çözümleyebilme**<br>**a)**<br>Bir doğal sayının asal olup olmadığını ve asal çarpanlarını<br>belirler.<br>**b)**<br>Asal sayıların özelliklerini ve bir doğal sayı ile asal çarpanları<br>arasındaki ilişkileri belirler.|
||Ön Koşul Bilgi: Oyuncuların oyunu oynayabilmesi için ileri<br>düzey matematk bilgisine sahip olmaları<br>beklenmemektedir. Oyunun hedef kitlesinin temel sayı|



kavramlarını, çarpma işlemini ve basit bölme işlemlerini biliyor olması yeterlidir. 

Oyun sırasında kullanılan matematiksel kavramlar şunlardır: 

- Sayılar 

- ● Çarpan kavramı ● Asal sayı kavramı ● Bileşik sayı kavramı ● Asal çarpanlara ayırma 

- Oyuncuların asal çarpanlara ayırma konusunu önceden biliyor olması zorunlu değildir. Başlangıç Gezegeni ve ilk bölümlerde yer alan basit sayı örnekleri sayesinde oyuncuların oyun oynayarak bu kavramı öğrenmeleri hedeflenmektedir. 

Teknik açıdan oyuncuların: 

- Dokunmatik ekran kullanabilmesi, 

- ● Ekran üzerinde sürükleme hareketi yapabilmesi, 

- ● Temel mobil oyun arayüzlerini kullanabilmesi 

yeterli kabul edilmektedir. 

Oyun herhangi bir internet bağlantısı gerektirmeden çalışacak şekilde tasarlanmıştır ve kullanıcı hesabı oluşturma zorunluluğu bulunmamaktadır. 

Öğretim Stratejisi: Keşfetme / Problem çözme / Senaryo / Diğer: Asal Yolculuk'ta öğrenme süreci doğrudan anlatım yerine oyunlaştırılmış deneyim yoluyla gerçekleştirilecektir. Oyuncular matematiksel kavramları soru çözerek değil, oyun içindeki etkileşimler aracılığıyla keşfederek öğreneceklerdir. Kullanılan öğretim stratejileri aşağıdaki gibidir: 

**Yaparak ve Yaşayarak Öğrenme** Oyuncular meteorları parçalayarak asal çarpanlara ayırma işlemini doğrudan deneyimler. Her meteor, oyuncunun etkileşim kurduğu bir matematiksel problem görevi görür. 

**Keşfetmeye Dayalı Öğrenme** Oyuncular asal sayıların ve bileşik sayıların davranışlarını oyun sırasında gözlemleyerek keşfeder. Asal sayıların tek vuruşta yok olması, bileşik sayıların ise parçalanması kavramların doğal olarak öğrenilmesini sağlar. 

|**Oyunlaştrma (Gamifcaton)**|
|---|
|Puan sistemi, gezegen ilerlemesi, bölüm tamamlama|
|ekranları, ödül ve ceza madenleri gibi unsurlar oyuncunun|



|motvasyonunu artrmak için kullanılır.|
|---|
|**Kademeli Öğretm (Basiten Karmaşığa)**<br>Başlangıç Gezegeni'nde küçük ve kolay sayılar kullanılırken<br>ilerleyen gezegenlerde daha büyük ve karmaşık sayılar<br>kullanılmaktadır. Böylece öğrenme süreci aşamalı olarak<br>ilerler.|
|**Tekrar ve Pekiştrme**<br>Benzer sayı türleri ve asal çarpan yapıları farklı bölümlerde<br>tekrar karşılaştrılarak oyuncunun öğrendiklerini pekiştrmesi<br>sağlanır.|
|**Anında Geri Bildirim**<br>Meteorların parçalanması, puan kazanılması, can kaybı ve<br>bölüm tamamlama ekranları sayesinde oyuncu yaptğı<br>işlemlerin sonucunu anında görebilir.|
|**Problem Çözme Temelli Öğrenme**<br>Oyuncu her meteoru yok etmek için sayının yapısını analiz<br>etmek zorundadır. Bu süreç matematksel düşünme ve<br>problem çözme becerilerini destekler.|
|**Motvasyon ve Ödüllendirme**<br>Puanlar, yeni gezegenlerin açılması, havai fşek kutlamaları<br>ve Sonsuz Yolculuk modu oyuncunun oyuna devam etme<br>isteğini artran motvasyon unsurları olarak kullanılır.|
|Bu stratejiler sayesinde oyuncular asal sayılar ve asal<br>çarpanlara ayırma konularını eğlenceli bir oyun deneyimi<br>içerisinde öğrenir ve tekrar ederek kalıcı hale getrir.|
|Ölçme - Değerlendirme: Doğru-yanlış / Rubrik / Süre / Diğer:|
|Asal Yolculuk'ta ölçme ve değerlendirme, oyuncunun oyun<br>içerisindeki performansına dayalı olarak gerçekleştrilecektr.<br>Amaç, oyuncunun asal sayılar ve asal çarpanlara ayırma<br>konularındaki gelişimini oyun içi davranışları üzerinden<br>gözlemlemektr.|
|**Performans Temelli Değerlendirme**|
|Oyuncunun başarısı aşağıdaki kriterler kullanılarak<br>değerlendirilecektr:|
|●<br>Tamamen yok edilen meteor sayısı|
|<br>●<br>Toplanan puan miktarı<br>●<br>Tamamlanan gezegen sayısı<br>●<br>Kalan can sayısı|



- Sonsuz Yolculuk modunda elde edilen skor 

##### **Başarı Göstergeleri** 

Oyuncunun öğrenme sürecindeki ilerlemesi aşağıdaki göstergelerle takip edilir: 

- 100 puana ulaşarak bölüm tamamlama 

- Yeni gezegenlerin kilidini açma 

- Asal Krallığı'na ulaşma 

- Sonsuz Yolculuk modunu açma 

- Yüksek skor elde etme 

##### **Anlık Geri Bildirim** 

Oyuncuya yaptığı işlemler sonucunda anında geri bildirim verilir. 

Örnek geri bildirimler: 

- "+5 Puan" 

- "+10 Puan" 

- "Yeni Gezegen Açıldı" 

- "Tebrikler!" 

Bu geri bildirimler oyuncunun kendi performansını değerlendirmesine yardımcı olur. 

##### **Kazanım Değerlendirmesi** 

Oyuncunun aşağıdaki kazanımlara ulaşması hedeflenmektedir: 

- Asal sayıları tanıyabilme 

- Bileşik sayıları ayırt edebilme 

- Sayıları asal çarpanlarına ayırabilme 

- Sayıların çarpan yapılarını anlayabilme 

- Problem çözme ve strateji geliştirme becerilerini kullanabilme 

##### **Sonuç Değerlendirmesi** 

Oyunun sonunda oyuncunun başarısı; 

- Tamamlanan gezegen sayısı, 

- Toplam puanı, 

- En yüksek skoru 

gibi ölçütler üzerinden değerlendirilir. 

Bu değerlendirme yaklaşımı, oyuncunun matematiksel öğrenme sürecini oyun deneyiminin doğal bir parçası haline getirerek eğitsel hedeflerin gerçekleşmesini destekler. 

Geri Bildirim: Anlık / Sonuç / Açıklamalı / Diğer: Oyuncu 100 puana ulaştığında bölüm tamamlanır ve kutlama ekranı gösterilir. Bu ekranda havai fişek animasyonları, tamamlanan gezegen bilgisi ve açılan yeni gezegen bilgisi yer alır. 

Amaç; oyuncunun başarı hissini güçlendirmek ve bir sonraki bölüme geçme motivasyonunu artırmaktır. 

Adaptasyon (opsiyonel): Zorluk ayarı / Kişiselleştirme / Yok: ______ Öğretmen/Öğrenci Modları (opsiyonel): _____________________________ 

#### **EK-1.3 Senaryo Taslağı ve Anlatı Özeti** 

Akış / sahne listesi / karakter Tema / Bağlam: notları (opsiyonel) Asal Yolculuk, oyuncunun bir uzay gemisiyle galaksideki gezegenler arasında seyahat ettiği eğitsel bir matematik oyunudur. Galaksinin ulaşım yollarını kapatan sayılı meteorlar, asal çarpanlarına ayrıştırılarak temizlenir. Oyuncu her gezegende farklı zorluklarla karşılaşır ve sonunda Asal Krallığı'na ulaşmaya çalışır. Dil Tonu: Keşif odaklı Macera temalı Teşvik edici Pozitif ve motive edici Matematiksel kavramları korkutucu değil eğlenceli şekilde sunan Anlatım Biçimi: Metin / seslendirme / karakter / anlatıcı: 

- UI metinleri 

- ● Bölüm geçiş ekranları ● Başarı mesajları ● Karakter kullanılmaz ● Hikâye çevresel anlatımla ve ekran metinleriyle aktarılır 

Karakterler (varsa): 

##### **Oyuncu Uzay Gemisi** 

- Galaksiyi temizlemekle görevli keşif gemisi 

- ● Meteorları asal çarpanlarına ayırarak yok eder 

**Meteorlar** 

- Üzerlerinde sayılar bulunan engeller 

- Her meteor matematiksel bir problemi temsil eder 

##### **Gezegenler** 

- Oyunun ilerleme aşamalarını temsil eden tematik bölgeler 

Bölüm/Etkinlik Yapısı (özet): 

##### **1. Başlangıç Gezegeni** 

- Temel asal çarpan mantığının öğrenildiği bölüm 

##### **2. Asteroit Kuşağı** 

- Meteor yoğunluğu artar 

##### **3. Lav Gezegeni** 

- Asal sayılar ağırlıklı meteorlar 

##### **4. Buz Gezegeni** 

- Daha büyük sayılar ve daha fazla parçalama adımı 

##### **5. Fırtına Gezegeni** 

- Meteorlar doğrudan asal çarpanlarına ayrılmayabilir 

##### **6. Kara Delik Sınırı** 

- Meteorlar sıkışarak büyük ara çarpanlara ayrılabilir 

##### **7. Kristal Gezegeni** 

- Bonus ve ödül madenlerinin daha sık görüldüğü bölüm 

##### **8. Asal Krallığı** 

- Tüm mekaniklerin birlikte kullanıldığı final bölümü 

##### **Sonsuz Yolculuk** 

- Tüm gezegen özelliklerinin bir arada bulunduğu yüksek skor modu 

Oyuncu Yolculuğu (özet akış): 

Ana Menü 

↓ 

Gezegen Haritası 

↓ 

İlerle 

↓ 

Yolculuk Ekranı 

↓ 

Oyun Bölümü 

↓ 

Meteorları Asal Çarpanlarına Ayır 

↓ 

100 Puan Topla 

↓ 

Bölüm Tamamlandı 

↓ 

Gezegen Sonu Kutlama Ekranı 

↓ 

Harita Ekranı 

↓ 

Yeni Gezegen 

↓ 

Asal Krallığı 

↓ 

Sonsuz Yolculuk Modu 

Eğitsel Mesaj ve Vurgu Noktaları: 

Her bileşik sayı asal çarpanlarına ayrılabilir. Asal sayılar yalnızca kendisine ve 1'e bölünebilir. Büyük sayılar küçük asal çarpanlardan oluşur. 

Matematik problem çözme becerileri eğlenceli bir oyun deneyimi içinde geliştirilebilir. Oyuncu tekrar eden etkileşimlerle asal çarpanlara ayırma becerisini pekiştirir. Sayıların yapısını analiz etme ve stratejik düşünme desteklenir. 

#### **EK-1.4 Metinler ve Teslim Paketi** 

Ekran metinleri / diyalog örnekleri (opsiyonel) 

Metin Türleri: UI / yönerge / anlatıcı / diyalog / diğer: **Metin Türleri:** ● UI Metinleri ● Başarı Mesajları ● Sonuç Ekranı Metinleri Metin Listesi: ID | Metin | Bağlam | Max. karakter: Seslendirme (varsa): Dil, karakter sayısı, toplam süre: ____________ Lokalizasyon (varsa): Diller / terminoloji notları: ________________ Teslim Edilecek Dosyalar: DOCX / Sheet / CSV / Diğer:   docx Revizyon Notları / Açık Sorular: ___________________________________ 

### **EK-2: Unity Ekibi İş Akışı Şablonu** 

#### **EK-2.1 Core Loop ve State Akışı** 

Core loop diyagramı / flowchart Core Loop (metin): Başlat → Hedef/Kurallar → Etkileşim → Geri Bildirim → Sonuç Oyunu Başlat ↓ 

Gezegen Seç / Haritayı Görüntüle 

↓ 

İlerle 

↓ Yolculuk Ekranı ↓ Bölümü Başlat ↓ Meteorları Vur ↓ Meteorları Asal Çarpanlarına Ayır ↓ Meteorları Tamamen Yok Et ↓ Puan Kazan (+5) ↓ 100 Puana Ulaş ↓ Bölümü Tamamla ↓ Yeni Gezegenin Kilidini Aç ↓ Haritaya Dön ↓ Sonraki Gezegen ↓ Asal Krallığına Ulaş ↓ Sonsuz Yolculuk Modunu Aç ↓ En Yüksek Skoru Topla 

Ana Akış (sahne/state): Ana Menü → Tutorial → Oyun → Sonuç → (Yeniden dene / Sonraki) Ana Menü ↓ Harita Ekranı ↓ Yolculuk Ekranı ↓ Oyun Ekranı ↓ (Başarılı) ↓ Bölüm Tamamlandı Ekranı ↓ Harita Ekranı ↓ Sonraki Gezegen Başarısızlık Akışı: Oyun Ekranı ↓ Meteor Çarpması ↓ Can Azalır ↓ 3 Can Biter 

|↓|
|---|
|Bölüm Başarısız|
|↓|
|Puan Sıfrlanır|
|↓|
|Bölüm Yeniden Başlar|
|Giriş Noktaları:|
|Oyuna Başla<br>Devam Et|
|Sonsuz Yolculuk|
|Bölüm Yeniden Başlat|
|Çıkış Noktaları:<br>Ana Menüye Dön|
|Haritaya Dön<br>Sonraki Bölüme Geç|
|Oyundan Çık|



#### **EK-2.2 Mekanikler ve Etkileşimler (Genel)** 

|Mekanik eskizleri / input şeması|Mekanikler (kısa liste):|
|---|---|
||Uzay gemisini sağa ve sola hareket etrme<br>Ateş etme<br>Meteorları parçalama<br>Meteorları asal çarpanlarına ayırma<br>Meteor parçalarını yok etme<br>Puan toplama<br>Can yönetmi<br>Ödül ve ceza madenlerini toplama<br>Gezegenler arasında ilerleme<br>Sonsuz Yolculuk modunda yüksek skor elde etme|
||Oyuncu Eylemleri: Tıkla / Sürükle-bırak / Seç /<br>Döndür / Onayla / İpucu / Diğer: ____|
||Parmağı sürükleyerek uzay gemisini hareket etrme<br>Ekrana dokunarak veya otomatk ateş sistemi ile<br>meteorlara ateş etme<br>Madenlerin üzerinden geçerek toplama<br>Harita ekranında gezegen seçme<br>İlerle butonuna basma<br>Ses açma/kapatma ikonunu kullanma|



Kurallar ve Sınırlar (özet): **Meteor Kuralları** 

- Meteorların üzerinde sayılar bulunur. 

- ● Sayılar gezegenlere ait sayı havuzlarından seçilir. 

- Özel sayı listesindeki sayılar %70 olasılıkla gelir. 

- Rastgele sayı havuzundan gelen sayılar %30 olasılıkla gelir. 

- ● Tamamen yok edilen sayılar o bölüm boyunca tekrar gelmez. 

- ● Tamamen yok edilemeyen sayılar tekrar gelebilir. 

- ● Bir meteor en fazla 5 asal çarpana ayrılabilir. 

##### **Puan Kuralları** 

- Ana meteor tamamen yok edildiğinde +5 puan verilir. 

- ● Altın Kristal etkisinde +10 puan verilir. 

- ● Yıldız Parçası toplandığında anında +10 puan verilir. 

- ● Bölüm tamamlamak için 100 puan gereklidir. 

##### **Can Kuralları** 

- Oyuncunun 3 canı vardır. 

- ● Meteor gemiye çarptığında 1 can eksilir. 

- ● Canlar sıfırlanırsa: ○ Bölüm yeniden başlar. ○ Bölüm puanı sıfırlanır. 

- ○ Özel sayı listesi başlangıç durumuna döner. 

##### **Maden Kuralları** 

- Ödül ve ceza madenleri 10 saniye boyunca etkilidir. Ödüller: ● Zaman Kristali ● Altın Kristal 

- ● Plazma Yakıtı 

- ● Koruma Kalkanı ● Yıldız Parçası 

Cezalar: 

- Kaos Çekirdeği 

- ● Yerçekimi Taşı ● Meteor Çağırıcı 

Oyuncu ilk karşılaşmalarında madenlerin işlevlerini bilmez. 

Madenler renk ve simgeleriyle ayırt edilir. 

##### **Koruma Kalkanı** 

- Geminin etrafında yarı saydam enerji bariyeri oluşur. 

- ● Meteor çarpmaları can eksiltmez. ● Süre: 10 saniye 

Geri Bildirim: UI / VFX / SFX / Metin: 

|**UI**|||
|---|---|---|
||●|Puan artşı|
||●|Can göstergesi|
||●|Kalkan göstergesi<br>|
||●|Aktf maden göstergesi|
||●|Gezegen ilerleme bilgisi|
|**VFX**|||
||●|Meteor parçalanma efekt<br>|
||●|Asal çarpan ayrışma efekt|
||●|Patlama efektleri|
||●|Maden toplama efektleri<br>|
||●|Havai fşek kutlama animasyonları|
|**SFX**|||
||●|Ateş etme sesi|
||●|Meteor vurma sesi|
||●|Meteor parçalanma sesi|
||●|Meteor yok olma sesi|
||●|Maden toplama sesi|
||●|Can kaybı sesi|
||●|Bölüm tamamlama sesi|
||●|Gezegen açılma sesi|
|**Met**|**n**||



- "+5 Puan" 

- ● "+10 Puan" ● "Can Kaybettin" 

- ● "Yeni Gezegen Açıldı" 

- "Asal Krallığına Ulaştın" 

- "Yeni Rekor" 

Başarı/Fail Notları ve Edge-case'ler: **Başarı Durumları** 

- 100 puana ulaşılması 

- Bölümün tamamlanması 

- Yeni gezegenin açılması 

- Asal Krallığı'nın tamamlanması 

- Sonsuz Yolculuk modunun açılması 

##### **Başarısızlık Durumları** 

- 3 canın da kaybedilmesi 

- Bölüm puanının sıfırlanması 

- Bölümün yeniden başlaması 

##### **Edge Case'ler** 

- Meteor parçaları ekrandan çıkarsa ilgili meteor tamamlanmamış kabul edilir. 

- Meteorun tüm parçaları yok edilmeden puan verilmez. 

- Aynı anda birden fazla maden etkisi aktif olabilir. 

- Koruma Kalkanı aktifken meteor çarpmaları can eksiltmez. 

- Özel listeden çıkarılan sayılar bölüm sonuna kadar tekrar oluşturulmaz. 

- Sonsuz Yolculuk modunda tüm gezegen mekanikleri aynı anda çalışabilir. 

#### **EK-2.3 Başarı-Başarısızlık, Ödül ve Zorluk (Genel)** 

Ödül ekranı taslağı / ilerleme göstergesi Başarı Koşulları: 

##### **Bölüm Başarısı** 

- Oyuncunun bölüm içerisinde toplam **100 puana ulaşması** gerekir. 

- Her ana meteorun tamamen yok edilmesi **5 puan** kazandırır. 

- Altın Kristal etkisi aktifse aynı meteor **10 puan** kazandırır. 

- Yıldız Parçası toplandığında anında **+10 puan** verilir. 

- ● 100 puana ulaşıldığında bölüm tamamlanır ve sonraki gezegenin kilidi açılır. 

##### **Oyun Başarısı** 

- Oyuncunun sekiz gezegeni tamamlaması gerekir. 

- ● Asal Krallığı tamamlandıktan sonra Sonsuz Yolculuk modu açılır. 

- Sonsuz Yolculuk modunda amaç mümkün olan en yüksek puanı elde etmektir. 

Başarısızlık Koşulları: 

##### **Can Kaybı** 

- Oyuncu oyuna 3 can ile başlar. 

- Meteorun uzay gemisine çarpması durumunda 1 can kaybedilir. 

##### **Bölüm Kaybı** 

- Tüm canlar kaybedildiğinde bölüm başarısız sayılır. 

- ● Bölüm puanı sıfırlanır. 

- Tamamlanmış meteor listesi sıfırlanır. 

- Özel sayı listesi başlangıç durumuna döner. 

- Bölüm baştan başlar. 

Ödül/İlerleme Mantığı (özet): 

Ana meteor tamamen yok edildi     5 puan Altın Kristal etkisinde meteor yok edildi  5 puan Yıldız Parçası toplandı  5 puan 

Gezegen Dünyası 100 Puan 

↓ Bölüm Tamamlandı ↓ Kutlama Ekranı ↓ Yeni Gezegen Açılır ↓ Harita Ekranı ↓ İlerle ↓ Sonraki Gezegen 

##### **Sonsuz Yolculuk** 



<!-- Start of picture text -->
Gezegen Say! Araligi<br>Baslangi¢ Gezegeni 4-48<br>Asteroit Kusagi 24 - 90<br>Lav Gezegeni 11-89<br>Buz Gezegeni 48 - 120<br>Firtina Gezegeni 60 - 180<br>Kara Delik Siniri 72 - 200<br>Kristal Gezegeni 100 - 240<br>Asal Kralligi 150 - 540<br><!-- End of picture text -->

- Meteorlar doğrudan asal çarpanlarına ayrılmayabilir 

- **Kara Delik Sınırı** ● Meteorlar büyük ara çarpanlara ayrılabilir 

- **Kristal Gezegeni** ● Karmaşık sayılar ve yoğun maden kullanımı 

- **Asal Krallığı** ● Tüm mekaniklerin birlikte kullanıldığı final bölgesi 

- **Risk Unsurları** ● Meteor çarpması ● Kaos Çekirdeği ● Yerçekimi Taşı ● Meteor Çağırıcı 

- İpucu Sistemi (opsiyonel): ________________________________________ 

#### **EK-2.4 Bölüm/Etkinlik Şablonu (Opsiyonel - Basit)** 

Bölüm haritası / layout Bölüm/Etkinlik Adı: (opsiyonel) Gezegen Bölümü (Genel Şablon) Amaç: Oyuncunun bulunduğu gezegende karşısına çıkan sayılı meteorları asal çarpanlarına ayırarak tamamen yok etmesi ve toplam **100 puana ulaşarak** bir sonraki gezegene geçiş hakkı kazanması. Kullanılan Mekanikler: 

##### **Temel Mekanikler** 

- Uzay gemisini hareket ettirme 

- ● Ateş etme ● Meteor parçalama ● Asal çarpanlara ayırma ● Meteor parçalarını temizleme ● Puan toplama ● Can yönetimi 

##### **Yardımcı Mekanikler** 

|●<br>Ödül madenleri toplama|
|---|



- Ceza madenlerinden etkilenme 

- Gezegenler arası ilerleme 

- Bölüm sonu kutlama ekranı 

##### **Gezegene Özgü Mekanikler** 

- Lav Gezegeni → Asal sayı ağırlığı 

- Fırtına Gezegeni → Elektrikli meteorlar 

- Kara Delik Sınırı → Sıkışmış meteorlar 

- Kristal Gezegeni → Yoğun bonus kullanımı 

- Asal Krallığı → Tüm mekaniklerin birleşimi 

Başarı Ölçütü (özet): Aşağıdaki koşulların sağlanması gerekir: 

- Toplam 100 puana ulaşılması 

- Meteorların tamamen yok edilmesi 

- ● Oyuncunun en az 1 canla hayatta kalması 

Başarı sonrası: Bölüm Tamamlandı ↓ Kutlama Ekranı ↓ Yeni Gezegen Açıldı ↓ Harita Ekranı 

Tahmini Süre: Sekiz gezegenin tamamlanma süresi 1 saat Sonsuz oyun modu uzun süreli oyun oynama sağlar. Notlar: Her bölüm 100 puan hedefiyle tamamlanır. Her ana meteor normal şartlarda 5 puan kazandırır. Altın Kristal etkisinde meteor başına 10 puan verilir. Yıldız Parçası anında +10 puan kazandırır. Meteorların %70'i bölümün özel sayı listesinden seçilir. Meteorların %30'u gezegen sayı aralığından rastgele oluşturulur. Tamamen yok edilen sayılar bölüm sonuna kadar tekrar oluşturulmaz. Oyuncu ilk karşılaşmada madenlerin etkilerini bilmez. Ödül madenleri pozitif renklerle, ceza madenleri negatif renklerle gösterilir. Bölüm sonunda havai fişek animasyonu oynatılır ve yeni gezegen bilgisi gösterilir. 

Asal Krallığı tamamlandıktan sonra Sonsuz Yolculuk modu açılır. 

#### **EK-2.5 UI State'leri ve Ekran Akışı** 

UI ekran akışı / state diyagramı 

Ekran Listesi (kısa): 



<!-- Start of picture text -->
> is > ? y<br>€) PeMASAL,yo.cuLuK  , YOLCULUGAHAZIR MISIN> “_~  (@<br>———— Nereden nereye— gideceginia gésteriyoruz! sitter iment<br>( $U AN BULUNDUGUN >) (__HEDEF GEZEGEN fo)<br>| i.BasLancic |, | 2. ASTEROIT<br>_  GEZEGENi = |, KUSAGI.<br>, ss LO | etme, F<br>fe? : >» |e,<br>a Suni we ih<br>. CED<br>Wa. - = - é S<br>= € — HARITAYA GERI DON<br><!-- End of picture text -->



<!-- Start of picture text -->
ty YO i=& Ce , \ 8. ASALKRALLIG!x<br>TOPLAM PUAN . j<br>ENwe YUKSEK 350  PUAN (aea  7:4S F ; i<br>eco er Ss esa cezecesl a<br>—-<br>i o é  ——<br>a = @ “ — 6. KARADELIKSINIRE<br>2 te<br>So datoJ. (co1  Pre-ee | FIRTINAGEZEGENIj fa’<br>7 fe +<br>+ , 4. BUZ GEZEGENi a<br>< y.& {es 3. LAV GEZEGENi o<br>e\, FF 1<br>= =. as we peo<br>. m MO * 3 x a<br>' é “qe 2. ASTEROIT KUSAGI<br>, ee > we<br>+ [ » GEZEGENi<br>'¢afee. Cee \. 1. BASLANGIC yo<br>(<br>ODULLER @ iLERLE KUTUPHANELg<br><!-- End of picture text -->



<!-- Start of picture text -->
f a - 1 a<br>—<br>CAN: 3/3 457A iy fa ay<br>45| O® ,<br>a ae<br>ae Y @/ y<br>"<br><!-- End of picture text -->



<!-- Start of picture text -->
fo — —<br>ryYwY wy. la PUAN a) (a<br>© can: 3/3 45/100 , tity,<br>_ ae o. ey<br><!-- End of picture text -->

- 3 canın kaybedilmesi 

- **Çıkış** 

   - Aynı bölüm yeniden başlar 

## **7. Bölüm Tamamlandı Ekranı** 

#### **İçerik** 

🚀 Havai fişek animasyonları Örnek: TEBRİKLER! Lav Gezegeni Tamamlandı Yeni Gezegen Açıldı: Buz Gezegeni Buton: [ DEVAM ET ] 

#### **Giriş** 

- 100 puana ulaşılması 

#### **Çıkış** 

- Harita ekranı 

## **8. Sonsuz Yolculuk Seçim Ekranı** 

#### **İçerik** 

- Sonsuz Yolculuk butonu 

- En yüksek skor 

- Başlat butonu 

#### **Giriş** 

- Ana Menü 

#### **Çıkış** 

- Sonsuz Yolculuk oyun ekranı 

## **9. Sonsuz Yolculuk Oyun Ekranı** 

#### **İçerik** 

Normal oyun ekranı ile aynıdır. 

Ek olarak: 

- Rekor puan 

- ● Toplam skor 

Gösterilir. 

Giriş/Çıkış Kuralları (özet): Her gezegen 100 puan ile tamamlanır. 

- Başarı durumunda Harita ekranına dönülür. 

- ● Başarısızlık durumunda bölüm yeniden başlatılır. 

- ● Sonsuz Yolculuk yalnızca Asal Krallığı tamamlandıktan sonra açılır. 

Tutorial Yaklaşımı: Oyun zaten genel oyunların yapısına benzer. Kullanıcının deneme yanılma ile kolayca çözebileceği yapıda. 

Hata Mesajları Yaklaşımı: Kısa ve anlaşılır mesajlar kullanılır. 

##### Örnekler: 

- Bölüm Yeniden Başlıyor 

- ● Mesela üç candan biri kaybedilirse içi boş kalp gösterilir. Saklı duran canlar dolu kalple ifade edilir 

Büyük ve okunabilir yazı tipleri Yüksek kontrastlı arayüz Renk körlüğüne uygun ikon desteği Ses kapatma seçeneği Dokunmatik cihazlara uygun büyük butonlar Ödül ve ceza madenlerinin sadece renkle değil simgelerle de ayırt edilmesi Erişilebilirlik Notları: 

__________________________________________ 

#### **EK-2.6 UI/UX Tasarım Notları (Genel)** 

Mockup / Wireframe / Notlar (görsel ekleyin) Navigasyon Kuralları: 

|Oyuncu mümkün olduğunca az ekran ve az işlem ile|
|---|
|oyuna başlayabilmelidir.|
|Ana akış:|
|Ana Menü<br>|
|↓<br>|
|Harita Ekranı<br>|
|↓|
|İlerle<br>|
|↓|
|Yolculuk Ekranı<br>↓|
|Başlat<br>|
|↓|
|Oyun<br>|
|↓|
|Bölüm Sonu|
|↓|
|Harita|
|Kontrol Şeması: Mouse / Touch / Keyboard / Gamepad:|
|**Hareket**<br>|
|●<br>Oyuncu uzay gemisini parmağını sürükleyerek<br>hareket etrir|
|.<br>●<br>Gemi ekranın alt kısmında bulunur.|
|**Ateş Etme**<br><br>|
|●<br>Otomatk ateş sistemi kullanılır.|
|<br>●<br>Oyuncunun ayrıca ateş tuşuna basmasına gerek|
|yoktur.|
|**Menü Etkileşimleri**<br>|
|●<br>Dokunma (Touch)|
|●<br>Buton seçimi|
|●<br>Ses aç/kapat|
|HUD Elemanları: Skor / süre / ilerleme / ipucu / diğer:|
|**Sol Üst**|
|❤❤❤|



Can göstergesi 

- Oyuncu 3 canla başlar. 

- Her meteor çarpmasında 1 can eksilir. 

**Üst Orta** Puan göstergesi Puan: 65 / 100 

##### **Üst Sağ** 

- 🔊 Ses Aç/Kapat İkonu ● Oyun müziği ● Efekt sesleri 

- açılıp kapatılabilir. 

##### **Oyun Alanı** 

Meteorlar Ödül Madenleri Ceza Madenleri Meteor Parçaları görünür. 

##### **Alt Bölüm** 

🚀 Oyuncu Uzay Gemisi Oyuncunun kontrol ettiği ana nesnedir. 

#### **Aktif Güçlendirme Göstergeleri** 

Maden toplandığında ekranda küçük süre göstergesi çıkar. Örneğin  6s gibi Süre dolduğunda ikon kaybolur. 

**Koruma Kalkanı Tasarımı** Koruma Kalkanı aktif olduğunda: 

|●|Uzay gemisinin etrafnda yarı saydam enerji|
|---|---|
||bariyeri oluşur.|



● Açık mavi/turkuaz renk kullanılır. Bu sayede oyuncu korumanın aktif olduğunu anında fark eder. 

#### **Maden Tasarım Kuralları** 

Oyuncu ilk karşılaşmada madenlerin etkilerini bilmez. Keşfetme duygusunu desteklemek için açıklama gösterilmez. 

|**Ödül Madenleri**|
|---|
|Pozitf renkler:|
|●<br>Yeşil|
|●<br>Mavi|
|●<br>Turkuaz<br>|
|●<br>Altn Sarısı|
|<br>Kullanılan madenlerin isimleri :<br>|
|●<br>Zaman Kristali<br>|
|●<br>Altn Kristal<br>|
|●<br>Plazma Yakıt|
|●<br>Koruma Kalkanı|
|●<br>Yıldız Parçası|
|**Ceza Madenleri**|
|Negatf renkler:|
|●<br>Kırmızı|
|●<br>Mor|
|●<br>Koyu Gri|
|<br>Kullanılan madenler:|
|●<br>Kaos Çekirdeği|
|●<br>Yerçekimi Taşı|
|●<br>Meteor Çağırıcı|
|**Sonuç Ekranı İçeriği**<br>Bölüm tamamlandığında:|
|🚀 Havai fşek animasyonları oynatlır.|
|Gösterilecek bilgiler:<br>|
|●<br>Tamamlanan gezegen|
|●<br>Toplam puan|



- Devam Et butonu 

- **Harita Ekranı Tasarımı** Harita ekranında: 

#### **Harita Ekranı Tasarımı** 

- Tüm gezegenler görünür. 

|●<br>Tamamlanan gezegenler işaretlenir.|
|---|
|●<br>Kilitli gezegenler gri görünür.<br>●<br>Aktf gezegen parlayan çerçeve ile vurgulanır.|
|Alt bölümde:|
|[ İLERLE ] butonu|
|**Yolculuk Ekranı Tasarımı**<br>İlerle butonuna basıldığında açılır.|
|Sol tarafa:|
|●<br>Mevcut gezegen|
|Sağ tarafa:|
|●<br>Hedef gezegen|
|Ortada:|
|🚀 Uzay Gemisi|
|ve yıldız animasyonları bulunur.|
|Bu ekran oyuncuya:|
|"Nereden nereye gidiyorum?"<br>sorusunun cevabını verir.|
|Çözünürlük/Responsive Notları (varsa):**Hedef**<br>**Çözünürlükler**|
|●<br>1080 × 1920 (ana tasarım)|
|**Ekran Uyumluluğu**|
|●<br>Telefon|
|●<br>Akıllı tahta|



#### **EK-2.7 Veri, Kayıt ve Analitik (Opsiyonel)** 

|Rapor ekranı taslağı (opsiyonel)|Kayıt/Devam:|
|---|---|
||Oyuncunun ilerleme durumu cihaz üzerinde otomatk olarak<br>kaydedilir.|
||Kaydedilecek veriler:|
||●<br>Açılan gezegenler<br>●<br>Tamamlanan gezegenler<br>●<br>Toplam puan<br>●<br>En yüksek puan<br>●<br>Sonsuz Yolculuk rekoru<br>●<br>Oyun ayarları (ses açık/kapalı)<br>Oyuncu uygulamayı kapatp tekrar açtğında son ilerleme<br>durumundan devam edebilir.|
||Ölçüm Metrikleri (özet):|
||Oyunun eğitsel ve oynanış başarısını değerlendirmek amacıyla<br>aşağıdaki metrikler takip edilir.|
||**Oyun İlerlemesi**|
||●<br>Tamamlanan gezegen sayısı<br>●<br>Tamamlanan bölüm sayısı|
||**Oyuncu Performansı**|
||●<br>Toplam puan<br>●<br>En yüksek skor<br>●<br>Sonsuz Yolculuk rekoru|
||Raporlama Ekranı (varsa):|
||_________________________________________<br>Dışa Aktarım (varsa):<br>_____________________________________________|
||Analitk Event Listesi (özet):|
||**Oyun Başlangıcı**<br>game_started|





<!-- Start of picture text -->
Oyuncu oyuna başladığında tetiklenir.<br>Bölüm Başlangıcı<br>planet_started<br>Parametreler:<br>● gezegen adı<br>● bölüm numarası<br>Bölüm Tamamlandı<br>planet_completed<br>Parametreler:<br>● gezegen adı<br>● toplam puan<br>Bölüm Başarısız<br>planet_failed<br>Parametreler:<br>● gezegen adı<br>● ulaşılan puan<br>Sonsuz Yolculuk Başlatıldı<br>endless_started<br>Yeni Rekor<br>new_highscore<br>Parametreler:<br>● skor<br><!-- End of picture text -->

#### **EK-2.8 Teknik Kurulum ve Yapı Notları (Özet)** 

Mockup / Wireframe / Notlar (görsel Unity Sürümü: ekleyin) Unity 6 

Hedef Platformlar: Ilk sürüm  : WebGL Sonraki sürümler : Android, IOS 

Giriş Cihazları: **Mobil ve Akıllı Tahta** 

- Dokunmatik ekran (Touch) 

##### **Opsiyonel** 

- Mouse 

- Klavye 

Sahne Yapısı / Prefab Stratejisi (özet): 

##### **Ana Menü Sahnesi** 

MainMenuScene 

İçerik: 

- Logo 

- Oyuna Başla 

- Sonsuz Yolculuk 

- Ses Ayarları 

##### **Harita Sahnesi** 

MapScene 

İçerik: 

- Gezegen Haritası 

- İlerle Butonu 

- Gezegen Kilitleri 

##### **Yolculuk Sahnesi** 

TravelScene 

İçerik: 

- Mevcut Gezegen 

- Hedef Gezegen 

- Yolculuk Animasyonu 

##### **Oyun Sahnesi** 

GameplayScene 

İçerik: 

- Uzay Gemisi 

- ● Meteorlar 

- Meteor Parçaları 

- ● Madenler 

##### **Bölüm Sonu Sahnesi** 

PlanetCompleteScene 

İçerik: 

- Havai Fişekler 

- Tebrikler Mesajı 

- Devam Et Butonu 

#### **Temel Prefab Yapısı** 

##### **PlayerShip Prefab** 

PlayerShip 

Bileşenler: 

- Sprite Renderer 

- Rigidbody2D 

- Collider2D 

- PlayerController 

- ● ShieldController 

##### **Meteor Prefab** 

Meteor 

Bileşenler: 

- Sprite Renderer 

- Number Display 

- Collider2D 

- MeteorController 

Özellikler: 

- Sayı değeri 

- Gezegen tipi 

- Parçalanma bilgisi 

##### **Meteor Fragment Prefab** 

MeteorFragment 

Özellikler: 

- Sayı değeri 

- Asal mı? 

- Kaynak meteor bilgisi 

##### **PowerUp Prefab** 

PowerUp 

Türler: 

- Zaman Kristali 

- Altın Kristal 

- Plazma Yakıtı 

- Koruma Kalkanı 

- Yıldız Parçası 

##### **Curse Prefab** 

Curse 

Türler: 

- Kaos Çekirdeği 

- Yerçekimi Taşı 

- Meteor Çağırıcı 

#### **Oyun Yöneticileri** 

##### **GameManager** 

Görevleri: 

- Oyun akışı 

- Kazanma/kaybetme kontrolü 

- Puan sistemi 

##### **PlanetManager** 

Görevleri: 

- Gezegen verileri 

- Sayı havuzları 

- Bölüm ilerleme sistemi 

##### **MeteorSpawner** 

Görevleri: 

- Meteor oluşturma 

- %70 özel liste 

- %30 rastgele liste mantığı 

##### **PowerUpManager** 

Görevleri: 

- Maden oluşturma 

- Süre yönetimi 

- Aktif etkiler 

##### **SaveManager** 

Görevleri: 

- Kayıt sistemi 

- ● Gezegen ilerlemesi 

- Yüksek skorlar 

#### **Veri Yapısı** 

**Gezegen Verisi** 

PlanetData 

İçerik: 

- Gezegen adı 

- Sayı aralığı 

- Özel sayı listesi 

- Bölüm kilit durumu 

##### **Meteor Verisi** 

MeteorData 

İçerik: 

- Sayı 

- Asal çarpanlar 

- Tamamlandı mı 

**Oyuncu Verisi** PlayerData 

İçerik: 

- Can 

- Puan 

- Aktif güçlendirmeler 

Kaydetme/Veri (varsa): 

##### **Yerel Kayıt** 

Unity:  PlayerPrefs 

Entegrasyonlar: SCORM/LMS/Analitik/Diğer: Scorm 

#### **EK-2.9 Test ve Teslim Akışı (Özet)** 

Mockup / Wireframe / Notlar (görsel ekleyin) 

Test Planı (özet): 

#### **Fonksiyonel Testler** 

Kontrol edilecek başlıklar: 

##### **Meteor Sistemi** 

- Meteorların doğru oluşturulması 

- Sayıların doğru görüntülenmesi 

- Sayıların doğru asal çarpanlara ayrılması 

- Meteor parçalarının doğru oluşması 

- Meteorların tamamen yok edildiğinde puan vermesi 

##### **Puan Sistemi** 

- Ana meteor başına +5 puan verilmesi 

- Altın Kristal etkisinde +10 puan verilmesi 

- Yıldız Parçası ile +10 puan verilmesi 

- 100 puana ulaşıldığında bölümün tamamlanması 

##### **Can Sistemi** 

- Meteor çarpınca can eksilmesi 

- ● Koruma Kalkanı aktifken can eksilmemesi 

- ● 3 can kaybında bölümün sıfırlanması 

##### **Maden Sistemi** 

- Ödül madenlerinin doğru çalışması 

- Ceza madenlerinin doğru çalışması 

- Etkilerin 10 saniye sürmesi 

- ● Süre bitince etkinin sona ermesi 

##### **Bölüm Sistemi** 

- Gezegen geçişlerinin çalışması 

- Harita ekranının güncellenmesi 

- Yeni gezegenlerin açılması 

- Sonsuz Yolculuk modunun açılması 

#### **Kullanılabilirlik Testleri (UX)** 

Aşağıdaki sorular değerlendirilir: 

- Oyuncu ilk dakikada ne yapacağını anlayabiliyor mu? 

- Meteorları vurma mekanizması yeterince açık mı? 

- ● Asal çarpan mantığı anlaşılabiliyor mu? 

- Harita ekranı anlaşılır mı? 

- Madenlerin olumlu ve olumsuz olduğu tahmin edilebiliyor mu? 

- Bölüm geçişleri yeterince açıklayıcı mı? 

Kilometre Taşları: Brief → Prototip → Alpha → Beta → Release: ______ 

Roller ve Sorumlular (özet): 

#### **Oyun Tasarımcısı** 

Sorumluluklar: 

- Oyun kuralları 

- ● Matematik sistemi 

- ● Puan sistemi 

#### **Unity Geliştiricisi** 

Sorumluluklar: 

- Oyun mekanikleri 

- ● UI sistemi ● Veri yönetimi 

- ● Optimizasyon 

#### **2D/UI Tasarımcısı** 

Sorumluluklar: 

- Uzay gemisi 

- ● Meteorlar ● Gezegenler ● Madenler ● Arayüz tasarımları 

Revizyon/Onay Akışı: Alan uzmanı → pedagojik → teknik → final: Oyun Tasarımı ↓ Matematik İçerik Kontrolü ↓ UI/UX Kontrolü ↓ Teknik Kontrol ↓ Test Süreci ↓ Final Onay ↓ Yayın 

Teslimatlar: Build, dokümanlar, varlık paketleri, raporlar: Oyun Senaryosu, Scorm paketi 

### **EK-3: 2D/3D Üretim Ekibi İş Akışı Şablonu** 

#### **EK-3.1 Stil Rehberi ve Referanslar** 

|Moodboard / stl örnekleri|Sanat Stli:|
|---|---|
||**2D Stylized Space Arcade**<br>Asal Yolculuk, eğitsel içeriği eğlenceli ve aksiyon odaklı bir<br>deneyimle birleştren, renkli ve çizgi flm tarzına yakın bir bilim<br>kurgu görsel dili kullanacaktr.<br>Tasarım yaklaşımı:|
||●<br>Renkli ve canlı uzay teması<br>●<br>Çocuklar ve gençler için çekici görseller<br>●<br>Okunabilir ve büyük sayılar<br>●<br>Basit ama etkileyici görsel efektler<br>●<br>Modern mobil oyun estetği<br>●<br>Düşük karmaşıklık, yüksek okunabilirlik|
||Görsel hedef:|
||"Öğretci bir matematk oyunu olduğunu<br>hissetrmeden matematk öğreten eğlenceli bir uzay<br>macerası."|
||Renk Palet / Tipograf:|
||Asal Yolculuk'ta uzayın derinliğini ve keşif duygusunu yansıtmak<br>amacıyla koyu tonlar ana renk olarak kullanılacaktr. Arka<br>planlarda ağırlıklı olarak lacivert, koyu mavi ve siyah tonları tercih<br>edilecektr. Yıldızlar ve uzay efektlerinde beyaz ve açık mavi<br>renkler kullanılacaktr.|
||Oyuncunun kullandığı uzay gemisi mavi ve turkuaz tonlarında<br>tasarlanacaktr. Lazer atşları parlak mavi veya enerji hissi veren<br>açık turkuaz renklerde olacaktr.|
||Ödül madenleri oyuncuda olumlu bir algı oluşturmak amacıyla<br>yeşil, turkuaz, açık mavi ve altn sarısı tonlarında tasarlanacaktr.<br>Ceza madenleri ise tehlike hissi oluşturmak amacıyla kırmızı, koyu<br>kırmızı, mor ve koyu gri renklerde gösterilecektr.<br>Başarı mesajları ve bölüm tamamlama ekranlarında altn sarısı ve<br>parlak sarı tonları kullanılacaktr. Uyarılar için turuncu, hata ve can<br>kaybı durumları için kırmızı renk tercih edilecektr.|



Her gezegen kendine özgü bir renk kimliğine sahip olacaktır: 

- Başlangıç Gezegeni yeşil ve mavi tonlarını kullanacaktır. 

- ● Asteroit Kuşağı gri ve taş renklerinden oluşacaktır. 

- ● Lav Gezegeni kırmızı, turuncu ve sarı tonlarında tasarlanacaktır. 

- ● Buz Gezegeni açık mavi, beyaz ve buz kristali tonlarını kullanacaktır. 

- ● Fırtına Gezegeni sarı, mor ve elektrik efektleriyle desteklenecektir. 

- ● Kara Delik Sınırı siyah, koyu mor ve mor enerji efektleriyle oluşturulacaktır. 

- ● Kristal Gezegeni turkuaz, cam mavisi ve parlak kristal renklerine sahip olacaktır. 

- ● Asal Krallığı altın sarısı, mor ve beyaz ışık efektleriyle diğer gezegenlerden ayrılacaktır. 

Tipografi seçiminde mobil cihazlarda kolay okunabilen, kalın ve modern bir yazı tipi kullanılacaktır. Meteorların üzerindeki sayılar büyük puntolu, yüksek kontrastlı ve oyuncunun hızlı okuyabileceği şekilde tasarlanacaktır. Menü başlıkları ve buton yazıları kısa, net ve dikkat çekici olacaktır. Bu sayede oyuncu hem aksiyon sırasında hem de menülerde bilgileri kolaylıkla takip edebilecektir. 

- UI Stil Prensipleri (varsa): ______________________________________ Referanslar/Moodboard Linkleri: Referans alınabilecek oyunlar: ● Angry Birds Space ● Jetpack Joyride ● Galaxy Attack: Space Shooter 

- ● Space Shooter Galaxy Attack 

- Onay Durumu / Tarih: ______________________________________________ 

#### **EK-3.2 Asset Listesi (Genel)** 

Asset örnekleri / sprite sheet (opsiyonel) 

Asset Listesi (kısa): 

Asal Yolculuk oyunu için üretilecek temel görsel varlıklar aşağıdaki gibidir. 

##### **Uzay Gemisi Assetleri** 

- Oyuncu Uzay Gemisi 

- Uzay Gemisi Ateş Etme Animasyonu 

- ● Uzay Gemisi Hasar Efekti 

- ● Uzay Gemisi Patlama Efekti 

- ● Koruma Kalkanı Efekti 

- ● Koruma Kalkanı Aktivasyon Animasyonu 

##### **Meteor Assetleri** 

- Ana Meteor Gövdesi 

- Küçük Meteor Parçası 

- Asal Sayı Meteoru 

- ● Elektrikli Meteor (Fırtına Gezegeni) 

- ● Kara Delik Meteoru 

- Kristal Meteoru 

- ● Meteor Parçalanma Efekti 

- ● Meteor Patlama Efekti 

##### **Gezegen Assetleri** 

- Başlangıç Gezegeni 

- Asteroit Kuşağı Gezegeni 

- Lav Gezegeni 

- Buz Gezegeni 

- Fırtına Gezegeni 

- Kara Delik Sınırı 

- Kristal Gezegeni 

- Asal Krallığı 

##### **Maden Assetleri** 

Ödül Madenleri: 

- Zaman Kristali 

- Altın Kristal 

- Plazma Yakıtı 

- Koruma Kalkanı Kristali 

- Yıldız Parçası 

Ceza Madenleri: 

- Kaos Çekirdeği 

- Yerçekimi Taşı 

- Meteor Çağırıcı 

##### **UI Assetleri** 

- Oyun Logosu 

- Ana Menü Arka Planı 

- Harita Ekranı 

- Yolculuk Ekranı 

- Bölüm Tamamlandı Ekranı 

- Sonsuz Yolculuk Ekranı 

- ● Buton Seti 

- ● Puan Paneli ● Can İkonları ● Ses Açık İkonu ● Ses Kapalı İkonu ● Kilit İkonu ● Devam Et Butonu ● İlerle Butonu 

##### **HUD Assetleri** 

- Puan Göstergesi 

- Can Göstergesi 

- ● Aktif Güçlendirme Göstergeleri 

- ● Süre Sayaçları 

##### **Arka Plan Assetleri** 

- Uzay Arka Planı 

- ● Yıldız Katmanları 

- Nebula Görselleri 

- Gezegen Atmosfer Efektleri 

- ● Kara Delik Efekti 

- ● Kristal Parıltı Efekti 

Teknik Notlar (format, çözünürlük, ölçü/pivot, animasyon): 

- Tüm 2D grafikler PNG formatında hazırlanacaktır. 

- ● Şeffaf arka plan kullanılacaktır. 

- ● Sprite'lar Unity Sprite Atlas yapısına uygun üretilecektir. 

- ● UI elemanları farklı çözünürlüklere uyumlu tasarlanacaktır. 

- ● Pivot noktaları kullanım amacına göre merkezde veya tabanda konumlandırılacaktır. 

- ● Animasyonlar Unity Animator sistemi ile yönetilecektir. 

- ● Efekt animasyonları Sprite Sheet veya Particle System olarak hazırlanacaktır. 

Önceliklendirme (kritik / nice-to-have): **Kritik Assetler** 

|●<br>Uzay Gemisi|
|---|



- Meteorlar 

- Gezegenler 

- Sayı Gösterim Sistemi 

- ● Can ve Puan Göstergeleri ● Ana Menü ● Harita Ekranı 

##### **Nice-to-Have Assetler** 

- Gelişmiş patlama efektleri 

- Ek uzay dekorları 

- ● Alternatif gemi görünümleri 

- ● Özel başarı animasyonları 

- Koleksiyon ekranı görselleri 

Teslim Formatı: Source + Export (PNG/FBX/PSD/BLEND vb.): Png 

#### **EK-3.3 Optimizasyon Hedefleri** 

Perf notları / LOD şeması Hedef FPS: 60 (opsiyonel) 3D Bütçeleri (poly/tex):   - 2D Bütçeleri (atlas/px): - Dosya Yapısı ve İsimlendirme Standardı: Örnek Assets │ ├── Art │   ── Planets├ │   ── Meteors├ │   ── Spaceship├ │   ── PowerUps├ │   ── UI├ │ ├── Audio │   ── Music├ │   ── SFX├ │ ├── Prefabs │ ├── Scripts │ ├── Animations │ ├── Materials │ └── Scenes 

Test Edilecek Sahne/Örnekler: Her bir gezegen için sahneler ayrı ayrı test edilecektir. 

#### **EK-3.4 Ses ve Müzik (Opsiyonel)** 

|Mockup / Wireframe / Notlar (görsel ekleyin)|Müzik Kullanımı: Yok / döngü / bölümsel: Asal<br>Yolculuk'ta müzik, oyuncunun uzay yolculuğu hissini<br>güçlendirmek amacıyla kullanılacaktr.<br>SFX İhtyacı (özet):|
|---|---|
||______________________________________________<br>Seslendirme (varsa): Dil, karakter sayısı, toplam süre:<br>seslendirme bulunmamaktadır.<br>Altyazı/Capton: Var / Yok: Oyunda sesli anlatm<br>bulunmadığı için altyazı sistemi kullanılmayacaktr.<br>Dosya Format ve Entegrasyon Notları: -|



### **EK-A: İş Akışı Kontrol Listeleri** 

Aşağıdaki kontrol listeleri, EK-1 (Senarist) - EK-2 (Unity) - EK-3 (2D/3D) ekiplerinin üretim adımlarını takip etmek için kullanılır. 

#### **EK-A1: Senarist (Kontrol)** 

|Durum|Madde|
|---|---|
|☐|Proje brief'i (amaç, hedef kitle, platorm, kısıtlar)<br>netleştrildi.|
|☐|Öğrenme kazanımları ve ölçme yaklaşımı<br>tanımlandı.|
|☐|Senaryo taslağı ve oyuncu akışı çıkarıldı.|
|☐|Metn/diyaloglar hedef yaş grubuna uygun yazıldı.|
|☐|Eğitsel vurgu noktaları ve açıklamalar eklendi.|
|☐|UI/asset ihtyaçları listelendi ve ekiplerle paylaşıldı.|
|☐|Geri bildirimler işlendi; fnal teslim paket (metn<br>listesi + versiyon notu) hazırlandı.|



#### **EK-A2: Unity (Kontrol)** 

|Durum|Madde|
|---|---|
|☐|Core loop ve sahne/state akışı dokümante edildi.|
|☐|Mekanikler, kurallar, başarı/başarısızlık koşulları<br>netleştrildi.|
|☐|UI ekranları ve state yönetmi planlandı.|
|☐|Ölçme-değerlendirme / analitk gereksinimleri<br>netleşt (varsa).|
|☐|Performans hedeferi ve teknik kısıtlar belirlendi.|
|☐|Test senaryoları oluşturuldu; build teslim paket<br>hazırlandı.|



#### **EK-A3: 2D/3D (Kontrol)** 

|Durum|Madde|
|---|---|
|☐|Stl rehberi ve referanslar onaylandı.|
|☐|Asset listesi ve teknik gereksinimler netleştrildi.|
|☐|Üretm tamamlandı; revizyonlar işlendi.|
|☐|Optmizasyon hedeferi uygulandı<br>(LOD/atlas/bütçeler).|
|☐|Dosya/klasör yapısı ve isimlendirme standardı<br>uygulandı.|
|☐|Final teslim klasörü oluşturuldu (source + export +<br>sürüm notu).|



