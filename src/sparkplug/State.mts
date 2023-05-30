import type { IClientOptions, IClientPublishOptions } from "mqtt"

import { MessageTypes, NS } from "./common.mjs"

export type Will = Required<Pick<IClientOptions, "will">>["will"]

export interface Payload {
	online: boolean
	timestamp: number
}

export class State {
	public readonly topic: string
	constructor(public readonly app: string, private _timestamp = new Date()) {
		this.topic = `${NS}/${MessageTypes.State}/${app}`
	}

	public get options(): IClientPublishOptions {
		return {
			qos: 1,
			retain: true
		}
	}

	public payload(online: boolean): Buffer {
		const obj: Payload = {
			online,
			timestamp: this._timestamp.valueOf()
		}

		return Buffer.from(JSON.stringify(obj))
	}

	public get will(): Will {
		return {
			payload: this.payload(false),
			properties: {
				contentType: "application/json",
				payloadFormatIndicator: true
			},
			qos: 1,
			retain: true,
			topic: this.topic
		}
	}

	public static parsePayload(payload: Buffer): Payload {
		return JSON.parse(payload.toString()) as Payload
	}
}
