import { getCurrentUser } from "aws-amplify/auth";
import { createRouter, createWebHistory } from "vue-router";

import Home from "../views/Home.vue";
import Index from "../views/Index.vue";
import PostDetails from "../views/PostDetails.vue";
import Signin from "../views/Signin.vue";
import MyPosts from "../views/MyPosts.vue";
import MyFriends from "../views/MyFriends.vue";
import MyRequests from "../views/MyRequests.vue";
import AddFriends from "../views/AddFriends.vue";
import CreatePost from "../views/CreatePost.vue";
import AccountSettings from "../views/AccountSettings.vue";
import Calendar from "../views/Calendar.vue";
import MyAwards from "../views/MyAwards.vue";

const routes = [
	{
		path: "/",
		component: Index,
		name: "index",
		meta: { requiresAuth: true },
		redirect: to => {
			return "home";
		},
		children: [
			{
				path: "/home",
				name: "home",
				component: Home,
				meta: { requiresAuth: true },
			},
			{
				path: "/post/:pid",
				name: "details",
				component: PostDetails,
				meta: { requiresAuth: true },
			},
			{
				path: "/addPost",
				name: "addPost",
				component: CreatePost,
				meta: { requiresAuth: true },
			},
			{
				path: "/profile/myPosts",
				name: "profile",
				component: MyPosts,
				meta: { requiresAuth: true },
			},
			{
				path: "/profile/friends",
				name: "friends",
				component: MyFriends,
				meta: { requiresAuth: true },
			},
			{
				path: "/profile/friendRequests",
				name: "requests",
				component: MyRequests,
				meta: { requiresAuth: true },
			},
			{
				path: "/profile/addFriends",
				name: "addFriends",
				component: AddFriends,
				meta: { requiresAuth: true },
			},
			{
				path: "/profile/awards",
				name: "awards",
				component: MyAwards,
				meta: { requiresAuth: true },
			},
			{
				path: "/settings",
				name: "settings",
				component: AccountSettings,
				meta: { requiresAuth: true },
			},
			{
				path: "/calendar",
				name: "calendar",
				component: Calendar,
				meta: { requiresAuth: true },
			},
		],
	},
	{
		path: "/signIn",
		name: "signIn",
		component: Signin,
	},
];
			

const router = createRouter({
	history: createWebHistory(),
	routes,
});
router.beforeEach(async to => {
	try {
		var user = await getCurrentUser();
		if (!user) { 
			throw new Error("User not signed in");
		}
		

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
