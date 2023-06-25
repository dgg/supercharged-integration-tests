using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

using System.Net.Mime;

namespace SuperCharged.Sparkplug;

record struct Payload(bool online)
{
	public long timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}

internal class State
{
	public string Topic { get; init; }
	public Payload Payload { get; init; }

	public State(string app)
	{
		Topic = $"{Protocol.NS}/{MessageTypes.State}/{app}";
		Payload = new Payload(false);
	}

	public void Will(ref MqttClientOptionsBuilder partialBuilder)
	{
		partialBuilder
			.WithWillRetain()
			.WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
			.WithWillContentType(MediaTypeNames.Application.Json)
			.WithWillTopic(Topic)
			.WithWillPayload(Payload.ToJson());
	}

	public MqttTopicFilterBuilder ForSubscription()
	{
		var builder = new MqttTopicFilterBuilder()
			.WithTopic(Topic)
			.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce);

		return builder;
	}

	public MqttApplicationMessageBuilder ForPublication(bool online)
	{
		Payload thePayload = online == Payload.online ? Payload : Payload with { online = online };

		var builder = new MqttApplicationMessageBuilder()
			.WithRetainFlag()
			.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
			.WithContentType(MediaTypeNames.Application.Json)
			.WithTopic(Topic)
			.WithPayload(thePayload.ToJson());

		return builder;
	}
}
