namespace oBiletCase.Infrastructure.oBiletAPI;

/// <summary>
/// Dokümanda verilen "en-EN" dil ayarını garanti altına almak için kullanılır.
/// </summary>
internal static class ObiletLanguage
{
    public static string ToObiletWireValue(string language) => language switch
    {
        "en-US" => "en-EN",
        _ => language,
    };
}
