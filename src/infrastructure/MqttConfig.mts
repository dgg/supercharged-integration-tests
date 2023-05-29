import type { IClientOptions } from "mqtt"

export interface MqttConfig {
	sharedGroup?: string
	clientOptions: Omit<IClientOptions, "clientId">
}
