import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig({
  plugins: [react()],

  resolve: {
    dedupe: ["react", "react-dom"],
    alias: {
      react: path.resolve(__dirname, "node_modules/react"),
      "react-dom": path.resolve(__dirname, "node_modules/react-dom")
    }
  },

  // 🔽 DAS FEHLT NOCH
  server: {
    proxy: {
      "/api": {
        target: "http://localhost:5222", // <-- BauFahrplanMonitor.API
        changeOrigin: true,
        secure: false
      }
    }
  }
});
