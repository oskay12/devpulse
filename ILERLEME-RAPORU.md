# 📋 DevPulse - İlerleme Raporu ve Mimari Özet

> Bu doküman, şu ana kadar `models.md` ve `doc.md` referans alınarak kurulan .NET Core backend altyapısını ve atılan adımları özetler.
> **Faz:** Backend temelleri (Solution iskeleti, domain modelleri, veritabanı entegrasyonu)

---

## 1. Genel Bakış

DevPulse, geliştirici ve repo analitik platformu (GitHub Insights / SonarQube benzeri) olarak tasarlanan bir sistem. `doc.md` içinde tarif edilen mimariye göre:

- **Backend:** .NET Core (C#) — Webhook API'leri + asenkron işleyen Worker servisleri
- **Veritabanı:** AWS RDS PostgreSQL
- **Mesajlaşma:** RabbitMQ (event-driven işleme)
- **Arama:** OpenSearch (commit/PR/review full-text arama)
- **Medya:** S3 + Lambda (görsel optimizasyonu)

Bu rapor, yukarıdaki mimarinin **kod tarafındaki** ilk iki adımını kapsıyor: (1) solution/proje iskeleti ve domain modelleri, (2) EF Core + PostgreSQL entegrasyonu.

---

## 2. Adım 1 — Solution ve Proje Yapısı

Kök dizinde `DevPulse.sln` oluşturuldu (klasik `.sln` formatında; .NET 10 SDK varsayılan olarak yeni `.slnx` formatını önerdiği için özellikle `-f sln` ile eski formata zorlandı).

`src/` klasörü altında 4 proje oluşturuldu ve solution'a eklendi:

| Proje | Tür | Görevi |
|---|---|---|
| `DevPulse.Core` | Class Library | Domain entity'leri, enum'lar, DTO'lar, mesaj sözleşmeleri, ayar sınıfları — **dışa bağımlılığı yok** |
| `DevPulse.Infrastructure` | Class Library | Veri erişim katmanı (EF Core, DbContext, migration'lar) |
| `DevPulse.Api` | ASP.NET Core Web API | Webhook alıcıları ve REST endpoint'leri |
| `DevPulse.Worker` | Worker Service | RabbitMQ tüketicileri, arka plan metrik hesaplama işleri |

### Proje Referansları (Bağımlılık Yönü)

```
DevPulse.Core  <───────┐
                        │
DevPulse.Infrastructure ──> DevPulse.Core

DevPulse.Api  ──> DevPulse.Core
              └─> DevPulse.Infrastructure

DevPulse.Worker ──> DevPulse.Core
                └─> DevPulse.Infrastructure
```

Yani `Core` hiçbir şeye bağımlı değil (saf domain katmanı), `Infrastructure` sadece `Core`'a bağımlı, `Api` ve `Worker` ise hem `Core` hem `Infrastructure`'a bağımlı — klasik Clean/Onion Architecture ayrımı.

---

## 3. Adım 2 — Domain Modellerinin Core Projesine Taşınması

`models.md` dosyasındaki **33 C# tipi** (entity, enum, DTO, event/job mesajları, ayar sınıfları), `DevPulse.Core` içinde anlamına göre klasörlenerek yerleştirildi:

```
DevPulse.Core/
├── Entities/          (13 tip)  — User, Repository, Commit, PullRequest, DeveloperMetric, vb.
├── Enums/              (10 tip)  — UserRole, RepositoryProvider, PullRequestState, vb.
├── SearchDocuments/    (3 tip)   — OpenSearch indeks dokümanları (Commit/PR/Review)
├── Messages/           (7 tip)   — RabbitMQ webhook event'leri (Push/PullRequest/Review) + CommitPayload
│   └── Jobs/           (3 tip)   — Worker job mesajları (CalculateMetrics, IndexContent, OptimizeImage)
├── Dtos/               (10 tip)  — API request/response modelleri
├── Settings/           (5 tip)   — DatabaseSettings, RabbitMqSettings, QueueSettings, OpenSearchSettings, S3Settings
└── GlobalUsings.cs     — DataAnnotations ve System.Text.Json.Serialization için ortak using'ler
```

**Yapılan küçük düzeltmeler (models.md'deki ham koda göre):**
- `Nullable enable` derleyici ayarına uyum için tüm `string`/`List<T>` gibi non-nullable alanlara `= string.Empty` / `= new()` gibi varsayılan değerler eklendi (aksi halde CS8618 uyarısı verirdi).
- Şablonların oluşturduğu boş `Class1.cs` dosyaları temizlendi.

**Doğrulama:** `dotnet build DevPulse.sln` → **0 hata**.

---

## 4. Adım 3 — EF Core ve AWS RDS PostgreSQL Entegrasyonu

### 4.1 Eklenen NuGet Paketleri

| Proje | Paket |
|---|---|
| `DevPulse.Infrastructure` | `Npgsql.EntityFrameworkCore.PostgreSQL` (PostgreSQL sağlayıcısı) |
| `DevPulse.Infrastructure` | `Microsoft.EntityFrameworkCore.Tools` (migration komutları) |
| `DevPulse.Api` | `Microsoft.EntityFrameworkCore.Design` (migration'ların startup projesi olarak çalışabilmesi için) |

> **Not (versiyon çakışması düzeltmesi):** `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore` paketine esnek bir sürüm aralığı (`[10.0.4, 11.0.0)`) ile bağımlı olduğu için, `DevPulse.Worker` projesi build sırasında farklı bir EF Core sürümü (10.0.4) ile `Infrastructure`'ın derlendiği sürüm (10.0.10) arasında çakışma uyarısı (MSB3277) veriyordu. Çözüm olarak `Microsoft.EntityFrameworkCore` ve `Microsoft.EntityFrameworkCore.Relational` paketleri `DevPulse.Infrastructure.csproj` içinde **açıkça 10.0.10 sürümüne sabitlendi**, böylece sürüm tüm projelere tutarlı şekilde yayılıyor.

### 4.2 `ApplicationDbContext`

Konum: `src/DevPulse.Infrastructure/Data/ApplicationDbContext.cs`

`DevPulse.Core/Entities` altındaki **13 entity** için `DbSet` tanımlandı:

```
Users, ProjectTokens, Repositories, RepositoryContributors,
Commits, CommitFiles, PullRequests, PullRequestReviews,
ReviewComments, DeveloperMetrics, CodeHealthScores,
ArchitecturalPatterns, MediaAssets
```

**`OnModelCreating` içinde yapılan özel konfigürasyonlar:**

- **Composite Primary Key:** `RepositoryContributor` için `(RepositoryId, UserId)` bileşik anahtar tanımlandı — bir kullanıcının bir repoda yalnızca tek bir rolü olabilir.
- **Foreign Key / Delete Davranışları:**
  - Bir varlığın "sahibi" olan ilişkilerde `Cascade` kullanıldı (örn. `CommitFile → Commit`, `PullRequestReview → PullRequest`, `CodeHealthScore/ArchitecturalPattern/ProjectToken → Repository`).
  - `User`'a giden ilişkilerde genelde `Restrict` kullanıldı (bir kullanıcı silindiğinde geçmiş commit/PR/review kayıtlarının kazayla silinmemesi için).
  - Opsiyonel (nullable) ilişkilerde `SetNull` kullanıldı (örn. `PullRequest.MergedById`, `MediaAsset.RepositoryId`).
  - `ReviewComment`, hem `PullRequest`'e hem (opsiyonel olarak) `PullRequestReview`'e bağlı olduğu için — ki `PullRequestReview` zaten `PullRequest`'e bağlı — çift cascade yolunu (multiple cascade paths) önlemek amacıyla `ReviewId` ilişkisi `Restrict` olarak ayarlandı; silme işlemi tek yol üzerinden (`PullRequestId`) yürütülüyor.
- **İndeksler:** Sık sorgulanacak / benzersizliği gereken alanlara indeks eklendi:
  - `User.Username`, `User.Email` → unique
  - `Repository.FullName`, `(Provider, ExternalId)` → unique
  - `Commit (RepositoryId, Sha)` → unique
  - `PullRequest (RepositoryId, PrNumber)` → unique
  - `ProjectToken.TokenHash` → unique
  - Diğer sık kullanılan FK alanlarına (AuthorId, RepositoryId, PullRequestId vb.) normal indeks

### 4.3 Konfigürasyon ve Dependency Injection

**`DevPulse.Api/appsettings.json`** içine `DatabaseSettings` bölümü eklendi:

```json
"DatabaseSettings": {
  "ConnectionString": "Host=devpulse-rds.xxxxxxxxxxxx.us-east-1.rds.amazonaws.com;Port=5432;Database=devpulse;Username=devpulse_user;Password=__SET_VIA_ENV_OR_SECRET__",
  "MaxRetryAttempts": 3,
  "CommandTimeoutSeconds": 30
}
```

> Bağlantı adresi `aws-done.md`'deki gerçek `devpulse-rds` / `devpulse_user` / `us-east-1` bilgileriyle uyumlu bir **placeholder**. Gerçek şifre kesinlikle buraya yazılmadı — `CLAUDE.md`'deki güvenlik kuralı gereği (hardcoded secret yasağı), şifre alanı `__SET_VIA_ENV_OR_SECRET__` olarak bırakıldı ve gerçek değer Environment Variable / K8s Secret / AWS Parameter Store üzerinden enjekte edilecek.

**`DevPulse.Api/Program.cs`** içine DbContext, IoC container'a Npgsql sağlayıcısıyla kaydedildi:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration["DatabaseSettings:ConnectionString"]));
```

### 4.4 İlk Migration

```bash
dotnet ef migrations add InitialCreate --project src/DevPulse.Infrastructure --startup-project src/DevPulse.Api
```

Sonuç: `src/DevPulse.Infrastructure/Migrations/` altında **13 tabloyu** oluşturan `InitialCreate` migration'ı üretildi:

- `20260725154714_InitialCreate.cs`
- `20260725154714_InitialCreate.Designer.cs`
- `ApplicationDbContextModelSnapshot.cs`

> Migration henüz gerçek RDS üzerine **uygulanmadı** (`dotnet ef database update` çalıştırılmadı) — bağlantı dizesindeki şifre placeholder olduğu için bu adım, gerçek RDS erişim bilgileri Secret/Parameter Store üzerinden sağlandığında yapılacak.

---

## 5. Diğer Küçük Düzenlemeler

- `.gitignore` dosyasına .NET build çıktıları eklendi (`bin/`, `obj/`, `*.user`) — daha önce bu klasörler ignore listesinde yoktu.

---

## 6. Şu Anki Durum — Doğrulama

```bash
dotnet build DevPulse.sln
# Build succeeded. 0 Error(s)
```

Tüm 4 proje (`Core`, `Infrastructure`, `Api`, `Worker`) sorunsuz derleniyor. İlk migration dosyaları oluşturuldu ve solution'ın parçası.

---

## 7. Sıradaki Olası Adımlar

- [ ] Gerçek RDS bağlantı bilgilerinin Environment Variable / K8s Secret olarak tanımlanması ve `dotnet ef database update` ile migration'ın uygulanması
- [ ] `DevPulse.Api` içinde Webhook receiver endpoint'lerinin (`WebhookRequestDto` kullanılarak) yazılması
- [ ] `DevPulse.Worker` içinde RabbitMQ tüketici (consumer) servislerinin kurulması (`CalculateMetricsJob`, `IndexContentJob`, `OptimizeImageJob` mesajlarını işleyecek)
- [ ] `DevPulse.Infrastructure` içinde Repository/Unit of Work pattern'lerinin eklenmesi
- [ ] OpenSearch ve S3/Lambda entegrasyonlarının kod tarafında başlatılması
- [ ] K8s manifest'lerinin (`RollingUpdate`, resource limits) yazılması — `doc.md`'de belirtilen altyapı gereksinimleri
