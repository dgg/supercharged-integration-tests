export const NS = "spBv1.0"

export const MessageTypes = {
	DeviceBirth: "DBIRTH",
	DeviceCommand: "DCMD",
	DeviceData: "DDATA",
	DeviceDeath: "DDEATH",
	NodeBirth: "NBIRTH",
	NodeCommand: "NCMD",
	NodeData: "NDATA",
	NodeDeath: "NDEATH",
	State: "STATE"
} as const

export type MessageType = typeof MessageTypes[keyof typeof MessageTypes]
