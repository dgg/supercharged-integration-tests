using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using SuperCharged.Sparkplug;
using MQTTnet.Protocol;

namespace SuperCharged.Tests.Sparkplug;

[Category("super-charged")]
public class PrimaryHostApplicationTester
{
	private readonly MqttFactory _factory = new MqttFactory();

	IContainer? _broker;
	[OneTimeSetUp]
	public async Task StartBroker()
	{
		_broker = new ContainerBuilder()
			.WithImage("emqx/nanomq:0.18.2")
			.WithPortBinding(1883)
			.WithEnvironment("NANOMQ_BROKER_URL", "nmq-tcp://0.0.0.0:1883")
			.WithEnvironment("NANOMQ_TLS_ENABLE", "false")
			.WithEnvironment("NANOMQ_WEBSOCKET_ENABLE", "false")
			.WithEnvironment("NANOMQ_HTTP_SERVER_ENABLE", "false")
			.WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("NanoMQ Broker is started successfully"))
		.Build();

		await _broker.StartAsync().ConfigureAwait(false);
	}

	[OneTimeTearDown]
	public async Task StopBroker()
	{
		await _broker!.DisposeAsync();
	}

	MqttClientOptionsBuilder? _clientBuilder;
	IMqttClient? _client;
	[SetUp]
	public async Task StartClient()
	{
		_clientBuilder = new MqttClientOptionsBuilder()
					.WithTcpServer(_broker!.Hostname, _broker!.GetMappedPublicPort(1883))
					.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500);

		_client = _factory.CreateMqttClient();
		await _client.ConnectAsync(_clientBuilder.Build());
	}

	[TearDown]
	public async Task StopClient()
	{
		await _client!.DisconnectAsync();
		_client!.Dispose();
	}

	[Test]
	public async Task StartAsync_PublishesBirth()
	{
		string phaName = "birth";
		string birthTopic = $"spBv1.0/STATE/{phaName}";

		await _client.SubscribeAsync(birthTopic, MqttQualityOfServiceLevel.AtLeastOnce);

		MqttApplicationMessage? birth = null;
		_client!.ApplicationMessageReceivedAsync += e =>
		{
			birth = e.ApplicationMessage;
			return Task.CompletedTask;
		};

		var sut = new PrimaryHostApplication(phaName, "birth_group");
		await sut.StartAsync(_clientBuilder!);

		await Task.Delay(50);

		Assert.That(birth, Is.Not.Null);

		Assert.That(birth!.QualityOfServiceLevel, Is.EqualTo(MqttQualityOfServiceLevel.AtLeastOnce));
		Assert.That(birth!.Retain, Is.False);
		Assert.That(birth!.Topic, Is.EqualTo(birthTopic));

		Payload payload = State.Parse(birth!.ConvertPayloadToString());
		Assert.That(payload.Online, Is.True);
	}

	[Test]
	public async Task StopAsync_PublishesDeath()
	{
		string phaName = "death";
		string deathTopic = $"spBv1.0/STATE/{phaName}";

		await _client.SubscribeAsync(deathTopic, MqttQualityOfServiceLevel.AtLeastOnce);

		MqttApplicationMessage? death = null;
		_client!.ApplicationMessageReceivedAsync += e =>
		{
			death = e.ApplicationMessage;
			return Task.CompletedTask;
		};

		var sut = new PrimaryHostApplication(phaName, "death_group");
		await sut.StartAsync(_clientBuilder!);
		await sut.StopAsync();

		await Task.Delay(50);

		Assert.That(death, Is.Not.Null);

		Assert.That(death!.QualityOfServiceLevel, Is.EqualTo(MqttQualityOfServiceLevel.AtLeastOnce));
		Assert.That(death!.Retain, Is.False);
		Assert.That(death!.Topic, Is.EqualTo(deathTopic));

		Payload payload = State.Parse(death!.ConvertPayloadToString());
		Assert.That(payload.Online, Is.False);
	}


	[Test]
	public async Task MultiplePHAs_MultipleBirths()
	{
		string phaName = "multi";
		string stateTopic = $"spBv1.0/STATE/{phaName}";

		await _client.SubscribeAsync(stateTopic, MqttQualityOfServiceLevel.AtLeastOnce);

		byte birthCount = 0;
		_client!.ApplicationMessageReceivedAsync += e =>
		{
			Payload payload = State.Parse(e.ApplicationMessage.ConvertPayloadToString());
			if (payload.Online)
			{
				birthCount++;
			}
			return Task.CompletedTask;
		};

		await new PrimaryHostApplication(phaName, "multi_group")
			.StartAsync(_clientBuilder!);

		await new PrimaryHostApplication(phaName, "multi_group")
			.StartAsync(_clientBuilder!);

		await Task.Delay(50);

		Assert.That(birthCount, Is.EqualTo(2));
	}

	[Test]
	public async Task MultiplePHAs_OneStops_BirthRepublished()
	{
		string phaName = "multi";
		string stateTopic = $"spBv1.0/STATE/{phaName}";

		await _client.SubscribeAsync(stateTopic, MqttQualityOfServiceLevel.AtLeastOnce);

		var stoppable = new PrimaryHostApplication(phaName, "multi_group");

		byte birthCount = 0;
		byte deathCount = 0;
		_client!.ApplicationMessageReceivedAsync += async e =>
		{
			Payload payload = State.Parse(e.ApplicationMessage.ConvertPayloadToString());
			if (payload.Online)
			{
				birthCount++;
			}
			else
			{
				deathCount++;
			}

			if (birthCount == 2 && payload.Online)
			{
				await stoppable.StopAsync();
			}

			return;
		};

		await stoppable.StartAsync(_clientBuilder!);
		await new PrimaryHostApplication(phaName, "multi_group").StartAsync(_clientBuilder!);

		await Task.Delay(100);

		Assert.That(birthCount, Is.EqualTo(3));
		Assert.That(deathCount, Is.EqualTo(1));
	}
}
