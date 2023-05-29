import { defineConfig } from "vitest/config"

import { join } from "node:path"
import { homedir } from "os"

export default defineConfig({
	test: {
		name: "Supercharging Integration Tests",
		reporters: ["verbose"],
		env: {
			TESTCONTAINERS_RYUK_DISABLED: "true",
			DOCKER_HOST: "unix://" + join(homedir(), ".docker", "run", "docker.sock")
		}
	}
})
