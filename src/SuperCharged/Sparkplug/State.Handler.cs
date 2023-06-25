using MQTTnet;
using MQTTnet.Client;

namespace SuperCharged.Sparkplug;

internal class StateHandler
{
	private readonly State _state;
	private readonly IMqttClient _client;
	public StateHandler(State state, IMqttClient client)
	{
		_state = state;
		_client = client;
	}

	public async Task<bool> TryHandle(MqttApplicationMessageReceivedEventArgs args)
	{
		string topic = args.ApplicationMessage.Topic;
		if (topic.StartsWith(Protocol.NS) && topic.Contains(MessageTypes.State))
		{
			await doHandle(args.ApplicationMessage);
			return true;
		}

		return false;
	}

	private async Task doHandle(MqttApplicationMessage message)
	{
		var payload = JsonCodec.FromJson<Payload>(message.ConvertPayloadToString());
		if (!payload.Online)
		{
			await _client.PublishAsync(_state.ForPublication(true).Build());
		}
		return;
	}
}
