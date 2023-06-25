using MQTTnet.Client;

using SuperCharged.Sparkplug;

using (var pha = new PrimaryHostApplication("LHM", "pha"))
{
	var builder = new MqttClientOptionsBuilder()
		.WithTcpServer("localhost", 1884)
		.WithTcpServer("localhost", 1883)
		.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500);

	Console.WriteLine("Connecting...");
	await pha.StartAsync(builder);

	await Task.Delay(TimeSpan.FromSeconds(5));

	await pha.StopAsync();

	Console.WriteLine("...Disconnected");
}
