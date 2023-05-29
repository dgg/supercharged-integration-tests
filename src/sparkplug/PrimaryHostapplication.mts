import { connect, type IClientOptions, type MqttClient } from "mqtt"

import type { MqttConfig } from "../infrastructure/MqttConfig.mjs"

import { ClientId } from "../infrastructure/ClientId.mjs"
import { MqttTopic } from "../infrastructure/MqttTopic.mjs"

import { State } from "./State.mjs"
import { StateHandler } from "./handlers/StateHandler.mjs"

export class PrimaryHostApplication {
	private readonly _mqtt: MqttClient
	private readonly _state: State

	constructor(public readonly name: string, cfg: MqttConfig) {
		this._state = new State(name)

		const phaOptions: IClientOptions = {
			...cfg.clientOptions,

			clientId: ClientId.build(`[PHA]${name}`),
			clean: true, // as per spec
			properties: {
				sessionExpiryInterval: 0, // as per spec
			},
			keepalive: 5,
			reschedulePings: false,
			connectTimeout: 5000,

			// protocolVersion: 5,

			will: this._state.will
		}

		this._mqtt = connect(phaOptions)
			.on("close", () => console.log("close"))
			.on("end", () => console.log("end"))
			.on("connect", () => {
				this._mqtt.subscribe(MqttTopic.forSubscription(this._state.topic, cfg.sharedGroup))
				this._mqtt.publish(this._state.topic, this._state.payload(true), this._state.options)
				// ... rest of sparkplug implementation
			})
			.on("message", (topic, payload, packet) => {
				new StateHandler(this._state, this._mqtt).tryHandle(topic, payload, packet)
			})
	}

	public stop(force = false): Promise<void> {
		this._mqtt.unsubscribe(this._state.topic)
		const death = new State(this._state.app)

		return new Promise((resolve, reject) => {
			this._mqtt.publish(death.topic, death.payload(false), death.options, () => {
				this._mqtt.end(force, {}, (e) => {
					e ? reject(e) : resolve()
				})
			})
		})
	}
}
