import { fetchAuthSession } from "aws-amplify/auth";
import { createRouter, createWebHistory } from "vue-router";

import SignIn from "../components/SignIn.vue";
import Home from "../components/Home.vue";

const routes = [
	{ path: '/', name: 'signin', component: SignIn},
	{ path: '/home', name: 'home', component: Home},
]

//TODO: FIX ROUTING AFTERWARDS

const router = createRouter({
	history: createWebHistory(),
    routes,
});
router.beforeEach(async to => {
	try {
		const session = await fetchAuthSession();
	} catch (e: unknown) {
		// avoid infinite redirect
		if (to.name !== "login") {
			return {
				name: "signin",
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
