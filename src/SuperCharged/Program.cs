using System.Text.Json;

using MQTTnet.Client;

using Sparkplug;

using (var pha = new PrimaryHostApplication("LHM"))
{
	var builder = new MqttClientOptionsBuilder()
		.WithTcpServer("localhost", 1884)
		.WithTcpServer("localhost", 1883)
		.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500);

	Console.WriteLine("Connecting...");
	await pha.ConnectAsync(builder);

	await Task.Delay(TimeSpan.FromSeconds(10));

	Console.WriteLine("...Disconnected");
}
