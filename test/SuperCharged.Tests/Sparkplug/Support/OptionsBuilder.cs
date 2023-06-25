using MQTTnet.Client;

namespace SuperCharged.Tests.Sparkplug.Support;

static class OptionsBuilder
{
	public static MqttClientOptionsBuilder Empty()
	{
		return new MqttClientOptionsBuilder()
			.WithTcpServer("localhost")
			.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500);
	}
}
