import { defineConfig } from "vitest/config";

// Optional legacy aid only. Required tests use node:test against emitted JavaScript.

export default defineConfig({
    test: {
        environment: "node",
        include: ["tests/**/*.test.ts"],
        coverage: {
            reporter: ["text", "json-summary"],
        },
    },
});
