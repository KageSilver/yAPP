import { fetchAuthSession } from "aws-amplify/auth";
import { createRouter, createWebHistory } from "vue-router";

import SignIn from "../components/SignIn.vue";
import ProfileDashboard from "../components/ProfileDashboard.vue";

const routes = [
	{ path: '/', name: 'signin', component: SignIn},
	{ path: '/profile', name: 'profile', component: ProfileDashboard},
]

const router = createRouter({
	history: createWebHistory(),
    routes,
});
router.beforeEach(async to => {
	try {
		await fetchAuthSession();
	} catch (e: unknown) {
		// avoid infinite redirect
		if (to.name !== "signin") {
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
