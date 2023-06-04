import { afterAll, afterEach, beforeAll, beforeEach, describe, expect, it } from "vitest"

import { GenericContainer, Wait, type StartedTestContainer } from "testcontainers"

import { connect, type MqttClient } from "mqtt"
import { lookup } from "node:dns/promises"

import { PrimaryHostApplication as Subject } from "../../src/sparkplug/PrimaryHostapplication.mjs"
import type { MqttConfig } from "../../src/infrastructure/MqttConfig.mjs"
import { asserting } from "./support/asserting.js"
import { jsonPayload } from "./support/jsonPayload.js"
import { type Payload } from "../../src/sparkplug/State.mjs"

describe(Subject.name, () => {
	describe("#supercharged", () => {

		let _container: StartedTestContainer
		beforeAll(async () => {
			_container = await new GenericContainer("emqx/nanomq:0.18.2")
				.withExposedPorts(1883)
				.withWaitStrategy(Wait.forLogMessage("NanoMQ Broker is started successfully"))
				.withEnvironment({
					NANOMQ_BROKER_URL: "nmq-tcp://0.0.0.0:1883",
					NANOMQ_TLS_ENABLE: "false",
					NANOMQ_WEBSOCKET_ENABLE: "false",
					NANOMQ_HTTP_SERVER_ENABLE: "false"
				})
				.start()
		})
		afterAll(async () => {
			await _container.stop()
		})

		let _testClient: MqttClient
		beforeEach(() => (new Promise((resolve) => {
			lookup(_container.getHost(), 4)
				.then((lookupAddress) => {
					_testClient = connect({
						host: lookupAddress.address,
						port: _container.getMappedPort(1883),
						protocol: "mqtt"
					}).once("connect", () => {
						resolve()
					})
				})
		})))

		afterEach(() => (new Promise((resolve) => {
			_testClient.end(true, () => {
				resolve()
			})
		})))

		describe("ctor", () => {
			it("published_birth", (ctx) => (new Promise((resolve, reject) => {
				_testClient.on("message", (_, payload, packet) => {
					asserting(resolve, reject, () => {
						const birth = jsonPayload<Payload>(payload)
						expect(birth.online, "online").to.be.true
						expect(packet.retain, "retained").to.be.true
						expect(packet.qos, "at least once").to.equal(1)
					})
				}).subscribe("spBv1.0/STATE/" + ctx.meta.name, { qos: 1 })
				const cfg: MqttConfig = {
					sharedGroup: "grp",
					clientOptions: {
						host: _testClient.options.host!,
						port: _testClient.options.port!,
						protocol: "mqtt"
					}
				}
				new Subject(ctx.meta.name, cfg)
			})))
		})

		describe(Subject.prototype.stop.name, () => {
			it("published_death", (ctx) => (new Promise((resolve, reject) => {
				_testClient.on("message", (_, payload, packet) => {
					const state = jsonPayload<Payload>(payload)
					// birth will be caught as well, but we want to terminate test on death
					if (!state.online) {
						asserting(resolve, reject, () => {
							expect(packet.retain, "retained").to.be.true
							expect(packet.qos, "at least once").to.equal(1)
						})
					}
				}).subscribe("spBv1.0/STATE/" + ctx.meta.name, { qos: 1 })

				const cfg: MqttConfig = {
					sharedGroup: "grp",
					clientOptions: {
						host: _testClient.options.host!,
						port: _testClient.options.port!,
						protocol: "mqtt"
					}
				}
				new Subject(ctx.meta.name, cfg).stop()
			})))
		})

		describe("multiple PHAs", () => {
			it("multiple_births", (ctx) => (new Promise((resolve, reject) => {
				let birthCounter = 0
				_testClient.on("message", (_, payload) => {
					const state = jsonPayload<Payload>(payload)
					if (state.online) {
						birthCounter++
					} else {
						reject(new Error("not a birth"))
					}

					if (birthCounter === 2) {
						resolve(null)
					}
				}).subscribe("spBv1.0/STATE/" + ctx.meta.name, { qos: 1 })

				const cfg: MqttConfig = {
					sharedGroup: "grp",
					clientOptions: {
						host: _testClient.options.host!,
						port: _testClient.options.port!,
						protocol: "mqtt"
					}
				}
				new Subject(ctx.meta.name, cfg)
				new Subject(ctx.meta.name, cfg)
			})))
		})
		describe("one stops", () => {
			it("republish_birth", (ctx) => (new Promise((resolve) => {
				let birthCounter = 0
				let deathCounter = 0
				_testClient.on("message", async (_, payload) => {
					const state = jsonPayload<Payload>(payload)
					if (state.online) {
						birthCounter++
					} else {
						deathCounter++
					}
					if (birthCounter === 2 && state.online) {
						await stoppable.stop()
					} else if (birthCounter === 3 && deathCounter === 1) {
						resolve(null)
					}
				}).subscribe("spBv1.0/STATE/" + ctx.meta.name, { qos: 1 })

				const cfg: MqttConfig = {
					sharedGroup: "grp",
					clientOptions: {
						host: _testClient.options.host!,
						port: _testClient.options.port!,
						protocol: "mqtt"
					}
				}
				const stoppable = new Subject(ctx.meta.name, cfg)
				new Subject(ctx.meta.name, cfg)
			})))
		})
	})
})
