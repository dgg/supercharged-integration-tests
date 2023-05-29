export class MqttTopic {
	public static forSubscription(topic: string, sharedGroup?: string): string {
		const theTopic = sharedGroup ? `$share/${sharedGroup}/${topic}` : topic
		return theTopic
	}
}
