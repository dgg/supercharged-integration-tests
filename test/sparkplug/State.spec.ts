import { describe, it } from "vitest"
import { expect } from "vitest"

import { jsonPayload } from "./support/jsonPayload.js"

import { State as Subject, type Payload } from "../../src/sparkplug/State.mjs"

describe(Subject.name, () => {
	describe("#unit", () => {
		describe("topic", () => {
			it("sparkplug-compliant topic", () => {
				const name = "the_name"
				const { topic } = new Subject(name)

				expect(topic).to.equal("spBv1.0/STATE/" + name)
			})
		})
		describe("options", () => {
			it("MQTT options for publication", () => {
				const { options } = new Subject("name")

				expect(options.qos).to.equal(1, "at least once")
				expect(options.retain).to.be.true
			})
		})

		describe(Subject.prototype.payload.name, () => {
			it("JSON-encoded online status", () => {
				const online = true
				const ts = new Date(0)
				const subject = new Subject("name", ts)

				const payload = jsonPayload(subject.payload(online))

				expect(payload).to.deep.equal({
					online,
					timestamp: 0
				})
			})
		})

		describe("will", () => {
			it("offline payload", () => {
				const ts = new Date(0)
				const { will } = new Subject("name", ts)

				const payload = jsonPayload(will.payload)
				expect(payload).to.deep.equal({
					online: false,
					timestamp: 0
				})
			})

			it("JSON content", () => {
				const ts = new Date(0)
				const { will } = new Subject("name", ts)

				expect(will.properties?.contentType).to.equal("application/json")
				expect(will.properties?.payloadFormatIndicator).to.be.true
			})

			it("sparkplug-compliant topic", () => {
				const name = "the_name"
				const { will, topic } = new Subject(name)

				expect(will.topic).to.equal(topic)
			})

			it("same options for publication", () => {
				const { options, will } = new Subject("name")

				expect(will.qos).to.equal(options.qos)
				expect(will.retain).to.equal(options.retain)
			})
		})

		describe(Subject.parsePayload.name, () => {
			it("types stringified payload", () => {
				const toParse: Payload = {
					online: false,
					timestamp: 1
				}
				const offline = Buffer.from(JSON.stringify(toParse))
				const parsed = Subject.parsePayload(offline)

				expect(parsed).to.deep.equal(toParse)
			})
		})
	})
})
