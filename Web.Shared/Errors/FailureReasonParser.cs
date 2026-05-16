using System.Text.Json;
using System.Text.Json.Nodes;

namespace Web.Shared.Errors;

/// <summary>
/// Reads the wire-level <c>failureReasons</c> array off an
/// <c>ApiResultDto&lt;T&gt;</c> JSON body and returns the structured
/// <c>{code, field, message}</c> entries. Necessary because the
/// <c>ResultMessage[]</c> declared type drops the <c>StructuredResultMessage</c>
/// subclass fields during base-type deserialization — round-tripping back to
/// the base type only preserves <c>Message</c>. We re-read the same body as
/// a tree so the structured fields survive.
/// </summary>
public static class FailureReasonParser
{
    public static IReadOnlyList<StructuredFailure> Parse(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return [];

        try
        {
            if (JsonNode.Parse(body) is not JsonObject root) return [];
            if (root["failureReasons"] is not JsonArray reasons) return [];

            var result = new List<StructuredFailure>(reasons.Count);
            foreach (var node in reasons)
            {
                if (node is not JsonObject reason) continue;
                var message = reason["message"]?.GetValue<string>() ?? string.Empty;
                var code = reason["code"]?.GetValue<string>();
                var field = reason["field"]?.GetValue<string>();
                result.Add(new StructuredFailure(message, code, field));
            }
            return result;
        }
        catch (JsonException)
        {
            // Body is not JSON we can parse — caller's fallback handles it.
            return [];
        }
    }
}
