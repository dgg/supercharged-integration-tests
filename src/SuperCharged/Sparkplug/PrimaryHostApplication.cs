using MQTTnet;
using MQTTnet.Client;

namespace SuperCharged.Sparkplug;

public class PrimaryHostApplication : IDisposable
{
	private bool _disposedValue;
	private IMqttClient _client;
	public string Name { get; init; }
	private readonly State _state;
	private readonly StateHandler _handler;
	private readonly string? _sharedGroup;

	public PrimaryHostApplication(string name, string? sharedGroup = null)
	{
		Name = name;
		_sharedGroup = sharedGroup;

		_client = new MqttFactory().CreateMqttClient();
		_state = new State(name);
		_handler = new StateHandler(_state, _client);
	}

	public async Task StartAsync(MqttClientOptionsBuilder partialBuilder, CancellationToken token = default(CancellationToken))
	{
		_client.ApplicationMessageReceivedAsync += _handler.TryHandle;

		await connectAsPrimaryHostApp(partialBuilder, token);

		await subscribeToState(token);

		await publishBirth(token);

		return;
	}

	public async Task StopAsync(CancellationToken token = default(CancellationToken))
	{
		var death = new State(Name);

		await unsubscribeFromState(death, token);

		await publishDeath(death, token);

		await disconnectAsPrimaryHostApp(token);
	}

	private Task<MqttClientConnectResult> connectAsPrimaryHostApp(MqttClientOptionsBuilder partialBuilder, CancellationToken token)
	{
		partialBuilder.WithClientId(ClientId.Build(Name))
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
			string sharedTopic = $"$share/{_sharedGroup}/{subToState.Build().Topic}";
			subToState.WithTopic(sharedTopic);
		}
		return _client.SubscribeAsync(subToState.Build(), token);
	}

	private Task<MqttClientPublishResult> publishBirth(CancellationToken token)
	{
		return _client.PublishAsync(_state.ForPublication(true).Build(), token);
	}

	private Task<MqttClientUnsubscribeResult> unsubscribeFromState(State death, CancellationToken token)
	{
		string theTopic = _sharedGroup != null ? $"$share/{_sharedGroup}/{death.Topic}" : death.Topic;

		return _client.UnsubscribeAsync(theTopic, token);
	}

	private Task<MqttClientPublishResult> publishDeath(State death, CancellationToken token)
	{
		return _client.PublishAsync(death.ForPublication(false).Build(), token);
	}

	private Task disconnectAsPrimaryHostApp(CancellationToken token)
	{
		var disconnect = new MqttClientDisconnectOptionsBuilder()
			.WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection);

		return _client.DisconnectAsync(disconnect.Build(), token);
	}

	private Task onStateMessage(MqttApplicationMessageReceivedEventArgs args)
	{
		Console.WriteLine(args.ToJson());
		return Task.CompletedTask;
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
