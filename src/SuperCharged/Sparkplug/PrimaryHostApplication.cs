using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

using System.Net.Mime;

using System.Text;
using System.Text.Json;

public class PrimaryHostApplication : IDisposable
{
	private bool _disposedValue;
	private IMqttClient _client;
	private readonly string _name;

	public PrimaryHostApplication(string name)
	{
		_name = name;
		_client = new MqttFactory().CreateMqttClient();
	}

	public Task<MqttClientConnectResult> ConnectAsync(MqttClientOptionsBuilder partialBuilder, CancellationToken token = default(CancellationToken))
	{
		var willPayload = new
		{
			timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
			online = false
		};

		// set PHA options on builder
		partialBuilder.WithClientId(ClientId.Build(_name))
			.WithCleanSession()
			.WithSessionExpiryInterval(0)
			.WithKeepAlivePeriod(TimeSpan.FromSeconds(5))
			.WithTimeout(TimeSpan.FromSeconds(5))
			// TODO: send partial to State to set will-related options
			.WithWillRetain()
			.WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
			.WithWillContentType(MediaTypeNames.Application.Json)
			.WithWillTopic("spBv1.0/STATE/" + _name)
			.WithWillPayload(JsonSerializer.Serialize(willPayload));

		return _client.ConnectAsync(partialBuilder.Build(), token);
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
