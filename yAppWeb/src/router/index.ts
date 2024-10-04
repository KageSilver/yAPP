import { fetchAuthSession } from "aws-amplify/auth";
import { createRouter, createWebHistory } from "vue-router";

import Authenticator from "../components/Authenticator.vue";

const routes = [
	{ path: '/', component: Authenticator}
]

const router = createRouter({
	history: createWebHistory(),
    routes,
});
router.beforeEach(async to => {
	try {
		const session = await fetchAuthSession();
	
	} catch (e: unknown) {
		if (to.name !== "login") {
			return {
				name: "login",
				query: { redirect: to.name?.toString() },
			};
		}
	}
});

router.onError((error, to) => {
	if (
		error.message.includes("Failed to fetch dynamically imported module") ||
		error.message.includes("Importing a module script failed")
	) {
		window.location = to.fullPath as string & Location;
	}
});

export default router;
