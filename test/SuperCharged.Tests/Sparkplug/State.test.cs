using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using SuperCharged.Sparkplug;
using SuperCharged.Tests.Sparkplug.Support;

namespace SuperCharged.Tests.Sparkplug;

[Category("unit")]
public class StateTester
{
	#region ctor
	[Test]
	public void State_ctor_SetsTopic()
	{
		string app = "the_app";
		var subject = new State(app);

		Assert.That(subject.Topic, Is.EqualTo("spBv1.0/STATE/" + app));
	}

	[Test]
	public void State_ctor_SetsOfflinePayload()
	{
		var subject = new State("app");

		Assert.That(subject.Payload.Online, Is.False);
	}
	#endregion

	#region Will
	[Test]
	public void State_Will_OfflinePayload()
	{
		MqttClientOptionsBuilder builder = OptionsBuilder.Empty();

		new State("app")
			.Will(ref builder);

		var options = builder.Build();

		Payload offline = PayloadParser.Parse(options.WillPayload);
		Assert.That(offline.Online, Is.False);
	}

	[Test]
	public void State_Will_JsonContent()
	{
		MqttClientOptionsBuilder builder = OptionsBuilder.Empty();

		new State("app")
			.Will(ref builder);

		var options = builder.Build();
		Assert.That(options.WillContentType, Is.EqualTo("application/json"));
	}

	[Test]
	public void State_Will_StateTopic()
	{
		MqttClientOptionsBuilder builder = OptionsBuilder.Empty();

		var subject = new State("app");
		subject.Will(ref builder);

		var options = builder.Build();
		Assert.That(options.WillTopic, Is.EqualTo(subject.Topic));
	}

	[Test]
	public void State_Will_RetainedAndQoS1()
	{
		MqttClientOptionsBuilder builder = OptionsBuilder.Empty();

		new State("app")
			.Will(ref builder);

		var options = builder.Build();
		Assert.That(options.WillQualityOfServiceLevel, Is.EqualTo(MqttQualityOfServiceLevel.AtLeastOnce));
		Assert.That(options.WillRetain, Is.True);
	}
	#endregion

	#region ForSubscription

	[Test]
	public void State_ForSubscription_SubscribeToTopic()
	{
		var subject = new State("app");
		MqttTopicFilterBuilder builder = subject.ForSubscription();

		Assert.That(builder.Build().Topic, Is.EqualTo(subject.Topic));
	}

	[Test]
	public void State_ForSubscription_QoS1()
	{
		MqttTopicFilterBuilder builder = new State("app").ForSubscription();

		Assert.That(builder.Build().QualityOfServiceLevel, Is.EqualTo(MqttQualityOfServiceLevel.AtLeastOnce));
	}
	#endregion

	#region ForPublication

	[Test]
	public void State_ForPublication_RetainedAndQoS1()
	{
		MqttApplicationMessageBuilder builder = new State("app").ForPublication(false);

		MqttApplicationMessage message = builder.Build();
		Assert.That(message.Retain, Is.True);
		Assert.That(message.QualityOfServiceLevel, Is.EqualTo(MqttQualityOfServiceLevel.AtLeastOnce));
	}

	[Test]
	public void State_ForPublication_Json()
	{
		MqttApplicationMessageBuilder builder = new State("app").ForPublication(false);

		MqttApplicationMessage message = builder.Build();
		Assert.That(message.ContentType, Is.EqualTo("application/json"));
	}

	[Test]
	public void State_ForPublication_PublishToTopic()
	{
		var subject = new State("app");
		MqttApplicationMessageBuilder builder = subject.ForPublication(false);

		MqttApplicationMessage message = builder.Build();
		Assert.That(message.Topic, Is.EqualTo(subject.Topic));
	}

	[Test]
	public void State_ForPublication_Online_OnlinePayload()
	{
		bool online = true;
		MqttApplicationMessageBuilder builder = new State("app").ForPublication(online);

		Payload payload = PayloadParser.Parse(builder);
		Assert.That(payload.Online, Is.True);
	}

	[Test]
	public void State_ForPublication_Offline_OfflinePayload()
	{
		bool offline = false;
		MqttApplicationMessageBuilder builder = new State("app").ForPublication(offline);

		Payload payload = PayloadParser.Parse(builder);
		Assert.That(payload.Online, Is.False);
	}

	#endregion
}
