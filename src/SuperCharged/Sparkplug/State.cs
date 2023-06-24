using System.Net.Mime;
using System.Text.Json;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace Sparkplug;


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
}
