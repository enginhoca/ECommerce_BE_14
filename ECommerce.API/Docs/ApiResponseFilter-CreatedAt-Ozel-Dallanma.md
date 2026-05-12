# ApiResponseFilter: `CreatedAtActionResult` için özel dallanma

Bu doküman, bir `201 Created` yanıtında gövdeyi `ApiResponse` ile sarmalarken **`Location`** başlığının kaybolmaması için filtreye eklenecek özel dalın mantığını ve uygulama sırasını açıklar.

---

## Sorunun özeti

`switch` içinde şu dal:

```csharp
case ObjectResult { Value: IResult result } objectResult:
```

`CreatedAtActionResult` **aynı zamanda** bir `ObjectResult`tır ve `Value` olarak tipik olarak `Result<T>` (`IResult`) taşır. Bu yüzden istek **ilk olarak bu genele düşer**, sonuç düz `ObjectResult` ile değiştirilir ve **`CreatedAtActionResult`’ın özel davranışı ortadan kalkar**.

ASP.NET Core’da `CreatedAtActionResult`, `OnFormatting` içinde URL üretip **`Location`** başlığını yazar. Sonuç **sade** `ObjectResult` olunca bu mekanizma çalışmaz; istemci `Location` ve dolayısıyla `GetCategoryById` rotası bilgisini kaybeder.

---

## Çözümün ilkesi

1. **`CreatedAtActionResult`** için **ayrı bir `case`** yazılır.
2. Bu `case`, **mutlaka** `ObjectResult { Value: IResult }` dalından **önce** değerlendirilir (daha dar / daha özel eşleşme önce gelmeli).
3. Gövdeyi `ApiResponseFactory.FromResult` ile sararız; **`CreatedAtActionResult`** nesnesini ise **yeniden oluştururuz**: `ActionName`, `ControllerName`, `RouteValues` (ve varsa `UrlHelper`) aynı kalır, sadece **`value`** (response gövdesi) `ApiResponse` olur.
4. Böylece yanıt yine `CreatedAtActionResult` olarak işlenir; çalışma zamanında `OnFormatting` tetiklenir ve **`Location`** üretilmeye devam eder.

---

## Dalın yapısı (mantıksal akış)

```
Sonuç == CreatedAtActionResult mı?
  └─ Hayır → mevcut genel dallara devam
  └─ Evet  → Value, IResult mi?
        └─ Evet → Başarılı mı?
              ├─ Evet → ApiResponse sar, YENİ CreatedAtActionResult(aynı action/controller/routeValues, yeni body)
              └─ Hayır → İstenirse düz ObjectResult + uygun 4xx (çoğu API'de CreatedAt başarısızlıkta kullanılmaz)
```

**Not:** Gerçek projede `CategoriesController.Create` gibi aksiyonlar başarısızlıkta zaten `FromResult` döndüğü için `CreatedAtActionResult` yalnızca başarı yolunda oluşur. Yine de filtreyi genel tutmak için başarısız `IResult` için `CreatedAtActionResult` yerine `ObjectResult` + doğru status kodu kullanmak tutarlıdır (aksi halde `Location` yanlışlıkla set edilebilir).

---

## Örnek kod iskeleti

`ApiResponseFilter.OnResultExecuting` içinde, mevcut `if (IApiResponse)` kontrolünden sonra, `switch`’in **en üstüne** yakın bir yere:

```csharp
using Microsoft.AspNetCore.Mvc;

// ...

case CreatedAtActionResult createdAt when createdAt.Value is IResult ir:
{
    if (!ir.IsSuccess)
    {
        var statusCode = MapErrorTypeToStatusCode(ir.ErrorType);
        context.Result = new ObjectResult(ApiResponseFactory.FromResult(ir))
        {
            StatusCode = statusCode
        };
        break;
    }

    var wrapped = ApiResponseFactory.FromResult(ir);
    var newCreated = new CreatedAtActionResult(
        createdAt.ActionName,
        createdAt.ControllerName,
        createdAt.RouteValues,
        wrapped)
    {
        StatusCode = createdAt.StatusCode,
        UrlHelper = createdAt.UrlHelper
    };
    context.Result = newCreated;
    break;
}
```

### Neden yeni `CreatedAtActionResult`?

Framework’te `CreatedAtActionResult`, taban sınıfın formatlamasından sonra **`OnFormatting`** ile çalışır ve `IUrlHelper.Action(...)` ile tam URL üretip **`Location`** atar. Aynı türü kullanmaya devam etmek, bu iş akışını **yeniden kullanır**; elle `Response.Headers["Location"]` yazmaya gerek kalmaz (rotalar veya host değişince tek doğru kaynak `UrlHelper` kalır).

---

## `switch` sırası (kritik)

C# önce **ilk eşleşen** dalı seçer. Aşağıdaki sıra önerilir:

1. `CreatedAtActionResult` (+ `IResult`) — özel dal  
2. `ObjectResult { Value: IResult }` — genel dal  
3. Diğer `ObjectResult` / `StatusCodeResult` dalları  

`CreatedAtActionResult` genel `ObjectResult { Value: IResult }` dalından **önce** gelmezse, özel dal hiç çalışmaz.

---

## İsteğe bağlı genişletmeler

| Durum | Öneri |
|--------|--------|
| `CreatedAtRouteResult` kullanılıyorsa | Aynı mantık: gövdeyi sararken **aynı türde** yeni örnek oluştur; route adı ve route değerlerini kopyala. |
| `AcceptedAtActionResult` | Benzer: özel dal + yeni örnek ile gövdeyi değiştir (genelde `202` + `Location`). |
| Sadece belirli controller’lar | İlk başta gerek yok; genel dal API tutarlılığı için yeterli. |

---

## İlgili dosyalar

- `ECommerce.API/Filters/ApiResponseFilter.cs` — dallanmanın ekleneceği yer  
- `Microsoft.AspNetCore.Mvc.CreatedAtActionResult` — `OnFormatting` ve yapıcı parametreleri  

---

## Kısa kontrol listesi

1. `CreatedAtActionResult` dalı **`ObjectResult { Value: IResult }` önünde** mi?  
2. Başarılı yanıtta gövde `ApiResponse` ile mi sarılı?  
3. `ActionName` / `ControllerName` / `RouteValues` / `UrlHelper` korunuyor mu?  
4. Yanıtta `201` ve **`Location`** header gerçekten geliyor mu (tarayıcı veya Postman ile)?  

Bu liste özellikle **`CategoriesController.Create`** + veritabanında **`SaveChangesAsync`** ile üretilen geçerli `id` birlikte doğrulanmalıdır; `Location` içindeki `id`, kalıcı kayıttan sonra oluşan kimlikle uyumlu olmalıdır.
