import { fetchAuthSession } from "aws-amplify/auth";
import { createRouter, createWebHistory } from "vue-router";

import SignIn from "../components/SignIn.vue";
import ProfileDashboard from "../components/ProfileDashboard.vue";
import AddFriends from "../components/AddFriends.vue";
import CreatePost from "../components/CreatePost.vue";
import PostDetails from "../components/PostDetails.vue";

const routes = [
	{ path: '/', name: 'signIn', component: SignIn},
	{ path: '/dashboard', name: 'dashboard', component: ProfileDashboard},
	{ path: '/add-friends', name: 'addFriends', component: AddFriends},
	{ path: '/create-post', name: 'createPost', component: CreatePost},
	{ path: '/post/:pid', name: 'postDetails', component: PostDetails},
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
		if (to.name !== "signIn") {
			return {
				name: "signIn",
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
