import type { MqttConfig } from "./infrastructure/MqttConfig.mjs"

import { PrimaryHostApplication } from "./sparkplug/PrimaryHostapplication.mjs"

import { setTimeout } from "node:timers/promises"

const cfg: MqttConfig = {
	sharedGroup: "pha",
	clientOptions: {
		host: "localhost",
		hostname: "localhost",
		port: 1883,
		protocol: "mqtt",
	}
}

const pha = new PrimaryHostApplication("LHM", cfg)
console.log(process.pid, "waiting...")
await setTimeout(10 * 1000)
console.log("...night, night")
await pha.stop()
