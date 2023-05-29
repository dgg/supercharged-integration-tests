export const subscriptionTopic = (topic: string, sharedGroup?: string): string => (
	sharedGroup ? `$share/${sharedGroup}/${topic}` : topic
)
