# AGENTS.md --- oBiletCase AI Development Instructions

## Genel çalışma kuralları

1.  Kullanıcının açıkça onayladığı mimari ve teknoloji kararlarına uy.
2.  Case gereksinimlerini sessizce değiştirme veya kapsamı genişletme.
3.  API dokümantasyonu/Postman koleksiyonuyla doğrulanmamış endpoint,
    alan, tip, hata kodu, varsayılan veya davranış uydurma. Bilgi
    eksikse eksikliği belirt ve kullanıcıdan gerekli kaynağı iste.
4.  Önce mevcut çözüm yapısını, adlandırmayı ve kullanılan paketleri
    incele; mevcut kodu görmeden varsayımsal yeniden yapılandırma yapma.
5.  Gereksiz teknoloji, abstraction, interface, pattern, katman veya
    servis ekleme. Basit ve anlaşılır çözümü tercih et.
6.  Kullanıcı tasarım renklerini ve standartlarını geliştirme sırasında
    sağlayacaktır. Kullanıcı belirtmeden marka renkleri, logo veya kesin
    tasarım token'ları belirleme.
7.  Birden fazla makul yaklaşım varsa önemli mimari değişiklikleri
    kullanıcıya açıkla ve onay al.

## Mimari sınırlar

-   Çözüm .NET 10 ASP.NET Core MVC'dir.
-   Projeler: `oBiletCase.Web`, `oBiletCase.Application`,
    `oBiletCase.Domain`, `oBiletCase.Infrastructure`,
    `oBiletCase.Tests`.
-   Application feature-based düzenlenir.
-   CQRS ve MediatR kullanılmaz.
-   Web, Application'ı kullanır; Application, Domain'i kullanabilir.
-   Infrastructure, Application sözleşmelerini uygular.
-   Application ve Domain, Web/Infrastructure somut implementasyonlarına
    bağımlı olamaz.
-   Controller'ları ince tut. HTTP orchestration dışında iş kuralı veya
    Obilet HTTP kodu ekleme.
-   Domain'i API DTO'ları veya ViewModel'lerle doldurma.
-   Obilet API ayrıntılarını Infrastructure sınırında tut.

## Kodlama ve ortak bileşenler

-   Mapping için AutoMapper kullan. Profilleri sorumluluk alanlarına
    yakın tut ve önemli mapping'leri test et.
-   Validasyon için FluentValidation kullan. Sunucu tarafı validasyonu
    zorunludur.
-   Helper'ları rastgele `Common/Helpers` alanına yığma. Metodu
    sorumluluğuna ait feature/katmanda tut.
-   Extension metotlarını yalnızca gerçek okunabilirlik veya
    yapılandırma faydası sağladığında kullan.
-   SOLID prensiplerini uygula; her sınıf için interface üretme.
-   `async/await`, uygun CancellationToken, null güvenliği ve anlamlı
    isimlendirme kullan.
-   Yorumlar kodu tekrar etmek yerine gerekçeyi açıklasın.
-   Magic string/number ve sessiz hata yutma davranışlarından kaçın.
-   Birden fazla yerde birebir aynı yazılması gereken string/sayısal
    anahtarlar (configuration section adı, adlandırılmış HttpClient adı,
    cache/cookie/session/claim key'i, route adı, localization resource
    key'i vb.) elle tekrar yazılmaz; yazım hatası riski taşır. Böyle bir
    değer `public const string`/`const` olarak **tek bir yerde**
    tanımlanır, her kullanan kod o sabite referans verir:
    -   Sabit belirli bir sınıfın kimliğiyle ilgiliyse (ör. bir Options
        sınıfının configuration section adı, bir entegrasyonun
        HttpClient adı), o sınıfın üzerinde tanımlanır (bkz.
        `oBiletAPIOptions.SectionName` / `HttpClientName`).
    -   Birden fazla sınıf/feature'ın paylaştığı ama tek bir sınıfa ait
        olmayan sabitler, ilgili feature/katman içinde dar kapsamlı bir
        `<Alan>Constants` sınıfında tutulur.
    -   Genel/global bir `Constants`, `Common` veya `Shared` klasörüne
        rastgele sabit biriktirilmez; bu, `Common/Helpers` için
        yukarıda tanımlı kuralla aynı mantığı izler.

## API ve güvenlik

-   Tarayıcıdan Obilet Business API'sine doğrudan istek atma. İstemci
    istekleri yalnızca oBiletCase backend'ine yapılır.
-   Token ve diğer secret'ları kaynak koda, repoya, README'ye veya
    loglara yazma.
-   Cookie ile uygulama kullanıcısının tanınmasını Obilet session
    kimliğiyle karıştırma.
-   Obilet session verisini kullanıcılar arasında izole et;
    saklama/yenileme davranışını API dokümanına göre uygula.
-   HTTP iletişiminde `IHttpClientFactory` kullan.
-   Timeout ve retry'ı yapılandır ve API davranışına göre dikkatle seç;
    körlemesine retry ekleme.
-   API yanıt alanlarını veya hata davranışlarını tahmin etme.
-   Global Exception Handling kullan. Kullanıcıya stack trace veya
    hassas teknik ayrıntı gösterme; `ILogger` ile güvenli loglama yap.

## UI kuralları

-   Razor Views + HTML + özel CSS + Vanilla JavaScript kullan.
-   Bootstrap, Tailwind ve jQuery ekleme.
-   CSS component-oriented düzenlenir; ortak değişkenler uygun ayrı
    dosyada tutulabilir.
-   Tasarım kurallarını kullanıcıdan gelen yönergelere göre uygula.
-   TR/EN localization kullan; kullanıcıya görünen metinleri view'lara
    sabit string olarak dağıtma.
-   Responsive düzen, semantik HTML, klavye erişimi, focus görünürlüğü
    ve gerekli ARIA niteliklerini gözet.
-   Loading, empty, validation ve error durumlarını ele al.
-   `localStorage` yalnızca son arama gibi hassas olmayan tercih
    verileri için kullan.
-   Cache eklemeden önce gereksinimi ve API koşullarını doğrula; şu an
    gereksiz cache altyapısı kurma.

## Test ve teslimat

-   Testlerde xUnit + Moq kullan.
-   Unit ve controller testlerini ekle; gerekli entegrasyon testlerini
    değerlendir.
-   `BaseServiceTest`/`BaseControllerTest` gibi ortak sınıflar ancak
    gerçek tekrar varsa kullanılmalı. Kalıtım testlerin okunabilirliğini
    azaltmamalı.
-   Testleri gerçek Obilet API'sine mecbur bırakma; uygun mock/fake
    yanıtlar kullan.
-   Değişiklik sonrası ilgili testleri ve mümkünse build'i çalıştır.
    Çalıştırmadıysan çalıştırmış gibi söyleme.
-   Swagger, Docker ve restore/build/test CI pipeline gereksinimlerini
    koru.
-   README ve rehberler uygulama değiştikçe güncellenmeli.

## Değişiklik yaparken beklenen yaklaşım

1.  İlgili dosyaları ve mevcut davranışı incele.
2.  İstenen değişikliği ve etkilenecek katmanları belirle.
3.  En küçük tutarlı değişikliği uygula.
4.  Gerekli testleri ekle veya güncelle.
5.  Build/test sonuçlarını raporla; başarısızlıkları ve doğrulanmamış
    noktaları açıkça belirt.
6.  Dokümantasyon değişikliği gerekiyorsa güncelle.
7.  İstenen kapsam dışındaki dosyaları gereksiz yere değiştirme.

## Kullanıcıya sorulması gereken durumlar

-   API dokümantasyonu bir davranışı belirlemeye yetmiyorsa.
-   Kullanıcı tarafından henüz belirlenmemiş görsel tasarım değerleri
    gerekiyorsa.
-   Mimari kararlardan birini değiştirmek veya yeni bağımlılık eklemek
    gerekiyorsa.
-   Case kapsamını, veri saklama davranışını veya güvenlik yaklaşımını
    etkileyen bir karar belirsizse.
