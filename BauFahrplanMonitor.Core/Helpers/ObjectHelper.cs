using Newtonsoft.Json;

namespace BauFahrplanMonitor.Core.Helpers;

internal static class ObjectHelper {

    public static string Dump<T>(this T? x) {
        if (x is null)
            return "<null>";

        var settings = new JsonSerializerSettings {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling     = NullValueHandling.Ignore,
            Formatting            = Formatting.Indented
        };

        return JsonConvert.SerializeObject(x, settings);
    }
}