using MQTTnet;
using MQTTnet.Client;

namespace Sparkplug;

public class PrimaryHostApplication : IDisposable
{
	private bool _disposedValue;
	private IMqttClient _client;
	private readonly string _name;
	private readonly State _state;

	public PrimaryHostApplication(string name)
	{
		_name = name;
		_client = new MqttFactory().CreateMqttClient();
		_state = new State(name);
	}

	public async Task ConnectAsync(MqttClientOptionsBuilder partialBuilder, CancellationToken token = default(CancellationToken))
	{
		var willPayload = new
		{
			timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
			online = false
		};

		// setting PHA options on builder
		partialBuilder.WithClientId(ClientId.Build(_name))
			.WithCleanSession()
			.WithSessionExpiryInterval(0)
			.WithKeepAlivePeriod(TimeSpan.FromSeconds(5))
			.WithTimeout(TimeSpan.FromSeconds(5));

		_state.Will(ref partialBuilder);

		await _client.ConnectAsync(partialBuilder.Build(), token);


		return;
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
