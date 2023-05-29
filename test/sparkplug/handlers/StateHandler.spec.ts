import { afterAll, afterEach, beforeAll, beforeEach, describe, expect, it } from "vitest"

import { connect, type MqttClient } from "mqtt"
import { MqttBroker } from "../support/MqttBroker.js"

import { type Payload, State } from "../../../src/sparkplug/State.mjs"

import { Packet } from "../support/Packet.js"
import { asserting } from "../support/asserting.js"
import { jsonPayload } from "../support/jsonPayload.js"

import { StateHandler as Subject } from "../../../src/sparkplug/handlers/StateHandler.mjs"

describe(Subject.name, () => {
	describe("#in-memory", () => {
		let _broker: MqttBroker
		beforeAll(async () => {
			_broker = await MqttBroker.start()
		})
		afterAll(async () => {
			await _broker?.down()
		})

		let _testClient: MqttClient
		beforeEach(() => (new Promise((resolve) => {
			_testClient = connect(_broker.clientOptions)
				.once("connect", () => {
					resolve()
				})
		})))
		afterEach(() => (new Promise((resolve) => {
			_testClient.end(true, () => {
				resolve()
			})
		})))

		describe(Subject.prototype.tryHandle.name, () => {
			let _state: State
			beforeAll(() => {
				_state = new State("the_name")
			})
			describe("not a state message", () => {
				it("not handled", () => {
					const notState = "not_STATE"
					const payload = Buffer.from("")
					const packet = Packet.publish(notState, payload)

					const subject = new Subject(_state, _testClient)

					expect(subject.tryHandle(notState, payload, packet)).to.be.false
				})
			})

			describe("online STATE message", () => {
				it("is really difficult to assert things that do not happen", () => { })
			})

			describe("offline STATE message", () => {
				it("publish online", () => (new Promise((resolve, reject) => {
					_testClient
						.on("message", (topic, payload) => {
							const online = jsonPayload<Payload>(payload)
							asserting(resolve, reject, () => {
								expect(topic).to.equal(_state.topic)
								expect(online.online).to.be.true
							})
						})
						.subscribe("+/STATE/+")

					const offline = _state.payload(false)
					const packet = Packet.publish(_state.topic, offline)

					new Subject(_state, _testClient).tryHandle(_state.topic, offline, packet)
				})))
			})
		})
	})
})
