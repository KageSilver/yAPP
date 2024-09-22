import vue from "@vitejs/plugin-vue";
import path, { resolve } from "path";
import { defineConfig } from "vitest/config";

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [vue()],

	resolve: {
		alias: {
			"@": resolve(__dirname, "src"),
			"#root": resolve(__dirname),
			"./runtimeConfig": "./runtimeConfig.browser",
		},
	},

	build: {
		sourcemap: true,
	},
	optimizeDeps: {
		exclude: [],
	},
	assetsInclude: ["**/*.md"],
});
