import { defineConfig } from "vite";
import tailwindcss from "@tailwindcss/vite";
import vue from "@vitejs/plugin-vue";
import { fileURLToPath } from "node:url";

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  // Backend CORS only allows http://localhost:5173 and :5174. Pin the dev port
  // and fail fast if it's taken, so Vite never silently drifts to a port the
  // browser will CORS-block (which surfaces as "无法获取列表" with no backend error).
  server: {
    port: 5173,
    strictPort: true,
  },
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
});
