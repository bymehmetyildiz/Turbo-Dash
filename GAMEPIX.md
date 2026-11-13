# Turbo Dash — GamePix geçişi

13 Kasım 2026 · Proje: `D:\Github\Turbo-Dash`

GamePix Unity SDK **6.6** kuruldu ve Unity **2022.3.23f1** ile `.gpx` paketi başarıyla üretildi.

## Hazır paket

`LeylGames_Pico_Rush_0.1.0.gpx` — **38.014.701 bayt**, yaklaşık **38,0 MB / 36,3 MiB**.

Dosya adının Pico Rush olması, mevcut Unity Product Name değerinden kaynaklanır. Projenin adı ve markası değiştirilmedi.

Bu dosya GamePix'in Unity yükleme paketidir; normal bir HTML5 ZIP değildir. Paket içindeki semboller ve SDK kitaplığı platformun işleme aşaması içindir. Paket boyutu, oyuncunun indireceği nihai oyun boyutunu göstermez.

SHA-256: `4f1afa3967fbf575a55eff96940f5402c8915537950803c6f9ad71c4eb842827`

## Yapılan değişiklikler

- CrazyGames C# kodu silinmeden `#if TURBODASH_CRAZYGAMES` bloklarına alındı. Bu sembol şu anda tanımlı değil; kod derlenmiyor. CrazyGames JavaScript eklentileri de aynı koşulla devre dışı.
- Varsayılan reklam sağlayıcısı GamePix. Ödüllü reklamda yalnızca başarı geri çağrısı ödül veriyor; iptal, hata ve reklam bulunamaması ödül vermiyor. Yinelenen reklam talepleri engelleniyor.
- Bölüm sonundaki geçiş reklamları doğal oyun molasında çağrılıyor; mevcut 120 saniyelik bekleme aralığı korunuyor.
- GamePix'in duraklatma/ses yönetimine ek olarak arayüz girişleri ve çalışan DOTween animasyonları durduruluyor.
- WebGL kayıtları GamePix'in eşitleyebildiği `localStorage` içindeki `turbo_dash.save.v1` anahtarına taşındı. Editor/native testleri PlayerPrefs kullanır. CrazyGames hesabındaki eski kayıtlar diğer platforma otomatik taşınmaz.
- Yeni rekor ve kaybetme olayları GamePix HappyMoment/SadMoment çağrılarına bağlandı.
- Doku, ses ve atlas optimizasyonları korunuyor. Linear renk uzayı ve WebGL 2 kullanılıyor.

## Yeniden derleme

1. Projeyi Unity 2022.3.23f1 ile aç; hedefi **WebGL** yap ve sahneleri kaydet.
2. **Tools > Turbo Dash > Build GamePix** menüsünü kullan.
3. Paket `Builds\GamePix-<tarih>-<kimlik>` altında oluşur. `Builds\GamePix-latest.txt` son klasörü gösterir.

Bu menü resmi GamePix paketleyicisini, var olan doku/ses ayarlarını değiştirmeyen seçenekle çalıştırır. SDK'nın genel yapılandırma menüsü yerine bu proje menüsünü kullan.

SDK'da iki uyarlama yapıldı: Unity 2022 paketindeki mevcut olmayan Unity 6 assembly referansları çıkarıldı; paketleyici başka bir açık Unity projesinin genel Editor.log dosyası yerine mevcut derleme günlüğünü alacak şekilde düzeltildi.

CrazyGames'e ileride geri dönülecekse `TURBODASH_CRAZYGAMES` sembolüne ek olarak GamePix'e ait **WebGL Emscripten Arguments** temizlenmeli ve CrazyGames derleme ayarları yeniden uygulanmalıdır. Bu çalışmada CrazyGames için yeni paket üretilmedi.

## Kontroller

- Unity WebGL derlemesi başarılı; paket içindeki 12 dosyanın bütünlüğü, ZIP CRC32, `gmpx.v4` yapısı ve WebAssembly geçerliliği doğrulandı.
- Oluşan JavaScript'te GamePix/kayıt bağlantıları var; CrazyGames JavaScript'i yok.
- Reklam adaptöründeki 12 kontrol geçti: tek ödül, başarısızlık, yinelenen istek, geç gelen geri çağrı, bekleme süresi ve duraklatılmış platform.
- Kayıt testleri geçti: Unicode, boş/eksik değer, yeniden yükleme, silme, diğer kayıtları koruyan sıfırlama ve depolama engelinde oturum içi devam.
- Son derleme tarayıcıda açıldı. Oynama, duraklatma/devam, koşu sonu ve menüye dönüş denendi. Reklam yokken 4 coin ikiye katlanmadı; sayfa yenilendiğinde 4 coin korunuyordu.
- Tarayıcıda tekerlek Convex Mesh poligon sınırı uyarıları ve Unity'nin eşzamanlı `FS.syncfs` uyarısı görüldü; test edilen akışı engellemedi.

## GamePix panelindeki son adım

Paket henüz yüklenmedi veya yayımlanmadı. GamePix panelindeki oyununa bu `.gpx` dosyasını yükledikten sonra platformun hazırladığı önizlemede gerçek reklamı, reklam sırasında ses/duraklatmayı ve hesaplar/cihazlar arası kayıt eşitlemesini kontrol et. Yerel tarayıcı testi gerçek reklam sunumunu veya bulut eşitlemesini doğrulamaz. Unity Editor'daki SDK reklamları ise simülasyondur.

Resmi kaynaklar: [GamePix Unity eklentisi](https://partners.gamepix.com/sdk/doc/unity-plugin), [GamePix JavaScript SDK](https://partners.gamepix.com/sdk/doc/javascript).
