import { fetchAuthSession } from "aws-amplify/auth";
import { createRouter, createWebHistory } from "vue-router";

import CreatePost from "../components/CreatePost.vue";
import HelloWorld from "../components/HelloWorld.vue";


const routes = [
	//TODO:Create login view with AWS Amplify Authenticator
	{
		path: '/',
		name: 'HelloWorld',
		component: HelloWorld
	},
	{
		path: '/create-post',
		name: 'CreatePost',
		component: CreatePost
	},
];

const router = createRouter({
	history: createWebHistory(),
    routes,
});
/*router.beforeEach(async to => {
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
});*/

router.onError((error, to) => {
	if (
		error.message.includes("Failed to fetch dynamically imported module") ||
		error.message.includes("Importing a module script failed")
	) {
		window.location = to.fullPath as string & Location;
	}
});

export default router;
