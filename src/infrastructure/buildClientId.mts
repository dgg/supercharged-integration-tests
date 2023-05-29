import { hostname } from "node:os"

export const buildClientId = (original: string): string => {
	const built = `${original}_${hostname()}_${Math.random().toString(16).substring(2, 10)}`
	return built
}
