using MQTTnet;
using MQTTnet.Client;

namespace SuperCharged.Sparkplug;

public class PrimaryHostApplication : IDisposable
{
	private bool _disposedValue;
	private IMqttClient _client;
	private readonly string _name;
	private readonly State _state;
	private readonly string? _sharedGroup;

	public PrimaryHostApplication(string name, string? sharedGroup = null)
	{
		_name = name;
		_sharedGroup = sharedGroup;

		_client = new MqttFactory().CreateMqttClient();
		_state = new State(name);
	}

	public async Task ConnectAsync(MqttClientOptionsBuilder partialBuilder, CancellationToken token = default(CancellationToken))
	{
		await connectAsPrimaryHostApp(partialBuilder, token);

		await subscribeToState(token);

		await publishBirth(token);

		return;
	}

	private Task<MqttClientConnectResult> connectAsPrimaryHostApp(MqttClientOptionsBuilder partialBuilder, CancellationToken token)
	{
		partialBuilder.WithClientId(ClientId.Build(_name))
			.WithCleanSession()
			.WithSessionExpiryInterval(0)
			.WithKeepAlivePeriod(TimeSpan.FromSeconds(5))
			.WithTimeout(TimeSpan.FromSeconds(5))
			// "special" properties from State
			.WithStateOptions(_state);

		return _client.ConnectAsync(partialBuilder.Build(), token);
	}

	private Task<MqttClientSubscribeResult> subscribeToState(CancellationToken token)
	{
		MqttTopicFilterBuilder subToState = _state.ForSubscription();
		if (_sharedGroup != null)
		{
			string sharedTopic = $"$share/{subToState.Build().Topic}";
			subToState.WithTopic(sharedTopic);
		}
		return _client.SubscribeAsync(subToState.Build(), token);
	}

	private Task<MqttClientPublishResult> publishBirth(CancellationToken token)
	{
		return _client.PublishAsync(_state.ForPublication(true).Build(), token);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_client.Dispose();
			}
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
