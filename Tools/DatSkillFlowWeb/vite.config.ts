import { defineConfig } from "vite";

// Optional legacy aid only. The required zero-dependency build never imports this file.

export default defineConfig({
    server: {
        host: "127.0.0.1",
        strictPort: true,
    },
    preview: {
        host: "127.0.0.1",
        strictPort: true,
    },
});
