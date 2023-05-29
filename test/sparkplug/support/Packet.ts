import type { IPublishPacket } from "mqtt";

export class Packet {
	public static publish(topic: string, payload: Buffer, retain = false, dup = false): IPublishPacket {
		return {
			cmd: "publish",
			dup,
			retain,
			qos: 0,
			payload,
			topic
		}
	}
}
