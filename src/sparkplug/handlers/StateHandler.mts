import type { IPublishPacket, MqttClient } from "mqtt"

import { MessageTypes, NS } from "../common.mjs"
import { State } from "../State.mjs"

export class StateHandler {
	constructor(private readonly _birth: State, private readonly _mqtt: MqttClient) { }

	public tryHandle(topic: string, payload: Buffer, packet: IPublishPacket): boolean {
		let handled = false

		if (topic.startsWith(NS) && topic.includes(MessageTypes.State)) {
			handled = true
			this.doHandle(payload, packet)
		}
		return handled
	}

	private doHandle(payload: Buffer, _packet: IPublishPacket): void {
		const parsed = State.parsePayload(payload)
		if (!parsed.online) {
			this._mqtt.publish(this._birth.topic, this._birth.payload(true), this._birth.options)
		}
	}
}
