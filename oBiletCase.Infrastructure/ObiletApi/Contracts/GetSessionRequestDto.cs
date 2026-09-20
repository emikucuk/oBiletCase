using System.Text.Json.Serialization;

namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;

/// <summary>
/// POST /client/getsession istek gövdesi.
/// </summary>
/// <remarks>
/// ÖNEMLİ: docs/obilet.com-API-Integration-Specs.pdf'teki örnek gövde
/// ("type":7, "connection.ip-address", "application.version/equipment-id")
/// canlı API tarafından reddedildi (2026-09-20 tarihinde doğrulandı):
/// "Port can not be null for browsers." / "Browser can not be null for
/// browsers." hatası döndü. Gerçekte kabul edilen şema, Postman
/// koleksiyonundaki (obiletcom_programming_assignment.postman_collection.json)
/// örnekle birebir aynı: "connection.port" ve "browser.name/version"
/// zorunlu, "application" alanı hiç kullanılmıyor. Bu sınıf, PDF değil,
/// canlı API'ye karşı doğrulanmış şemayı temel alır.
/// </remarks>
public sealed class GetSessionRequestDto
{
    [JsonPropertyName("type")]
    public int Type { get; set; } = 1;

    [JsonPropertyName("connection")]
    public ConnectionInfoDto Connection { get; set; } = new();

    [JsonPropertyName("browser")]
    public BrowserInfoDto Browser { get; set; } = new();
}

public sealed class ConnectionInfoDto
{
    [JsonPropertyName("ip-address")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// API bu alanı boş/null string olarak kabul etmiyor ("Port can not be
    /// null for browsers", boş string de aynı hatayı veriyor - 2026-09-20'de
    /// canlı API'ye karşı doğrulandı). BusJourney bir backend istemcisi
    /// olduğundan gerçek bir port anlamı taşımıyor; sabit bir placeholder
    /// gönderilir.
    /// </summary>
    [JsonPropertyName("port")]
    public string Port { get; set; } = "0";
}

/// <summary>
/// API bu alanı "tarayıcı" senaryosu için zorunlu tutuyor; BusJourney bir
/// backend-to-backend istemci olduğundan gerçek bir tarayıcı bilgisi yok.
/// Bu yüzden burada uygulamayı tanımlayan sabit değerler gönderilir
/// (bkz. <see cref="oBiletAPIOptions"/> - ileride yapılandırılabilir hale
/// getirilmesi gerekirse buraya taşınabilir).
/// </summary>
public sealed class BrowserInfoDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "oBiletCase-Backend";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0.0";
}
