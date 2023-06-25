using MQTTnet;
using SuperCharged.Sparkplug;

internal static class PayloadParser
{

	public static Payload Parse(byte[] payload)
	{
		string json = new MqttApplicationMessageBuilder()
			.WithTopic("any")
			.WithPayload(payload)
			.Build()
			.ConvertPayloadToString();

		return JsonCodec.FromJson<Payload>(json);
	}

	public static Payload Parse(MqttApplicationMessageBuilder builder)
	{
		string json = builder
			.Build()
			.ConvertPayloadToString();

		return JsonCodec.FromJson<Payload>(json);
	}

}
