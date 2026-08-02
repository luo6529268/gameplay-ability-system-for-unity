import { defineConfig } from "@playwright/test";

// Optional legacy acceptance aid. Playwright is intentionally not a package dependency.

const port = 41739;

export default defineConfig({
    testDir: "tests/e2e",
    fullyParallel: false,
    use: {
        baseURL: `http://127.0.0.1:${port}`,
        trace: "retain-on-failure",
    },
    webServer: {
        command: `npm run build && node dist-server/server/cli.js --root dist --port ${port}`,
        url: `http://127.0.0.1:${port}/api/health`,
        reuseExistingServer: false,
        timeout: 120_000,
    },
});
