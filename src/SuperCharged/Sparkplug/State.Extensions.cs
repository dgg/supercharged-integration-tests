using MQTTnet.Client;

namespace SuperCharged.Sparkplug;
internal static class StateExtensions
{
	public static MqttClientOptionsBuilder WithStateOptions(this MqttClientOptionsBuilder builder, State state)
	{
		state.Will(ref builder);
		return builder;
	}
}
