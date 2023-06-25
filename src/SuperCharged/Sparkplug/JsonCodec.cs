using System.Text.Json;

namespace SuperCharged.Sparkplug;

public static class JsonCodec
{
	private static readonly JsonSerializerOptions _defaultOpts = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public static string ToJson<T>(this T toSerialize)
	{
		return JsonSerializer.Serialize(toSerialize, _defaultOpts);
	}
}
