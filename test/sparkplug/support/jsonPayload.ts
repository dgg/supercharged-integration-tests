export const jsonPayload = <T>(buffer: Buffer | string): T => JSON.parse(buffer.toString()) as T
