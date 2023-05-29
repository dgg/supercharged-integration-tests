export const asserting = (
	resolve: (_: unknown) => void, reject: (reason: unknown) => void,
	assertions: () => void
): void => {
	try {
		assertions()
		resolve()
	} catch (e) {
		reject(e)
	}
}
