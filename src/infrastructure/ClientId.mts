import { hostname } from "node:os"

export class ClientId {
	public static build(original: string): string {
		const built = `${original}_${hostname()}_${Math.random().toString(16).substring(2, 10)}`
		return built
	}
}
