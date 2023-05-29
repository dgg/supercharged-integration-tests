import Aedes, { createBroker } from "aedes"
import type { IClientOptions } from "mqtt"

import { type AddressInfo, createServer, Server } from "node:net"

export class MqttBroker {
	private readonly _instance: Aedes
	private _server: Server
	constructor() {
		this._instance = createBroker()
		this._server = createServer(this._instance.handle)
	}

	public static async start(): Promise<MqttBroker> {
		const instance = new MqttBroker()
		await instance.up()
		return instance
	}

	public get clientOptions(): IClientOptions {
		const options: IClientOptions = {
			host: "0.0.0.0",
			port: (this._server.address() as AddressInfo).port,
			protocol: "mqtt"
		}
		return options
	}

	public up(): Promise<MqttBroker> {
		return new Promise((resolve) => {
			this._server.listen(() => {
				resolve(this)
			})
		})
	}

	public down(): Promise<MqttBroker> {
		return new Promise((resolve, reject) => {
			this._instance.close(() => {
				if (this._server.listening) {
					this._server.close((err?: Error) => {
						if (err) {
							reject(err)
						} else {
							resolve(this)
						}
					})
				} else {
					resolve(this)
				}
			})
		})
	}
}
