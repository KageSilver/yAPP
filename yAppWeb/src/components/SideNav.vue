<script setup lang="js">
	import { computed } from "vue";
	import { useRoute, useRouter } from "vue-router";
	import { signOut } from "aws-amplify/auth";

	const route = useRoute();
	const router = useRouter();
	const currentPath = computed(() => route.path);
	const isActive = path => {
		return route.path.includes(path);
	};

	const isProfile = computed(
		() => route.path.includes("profile") || route.path.includes("settings"),
	);

	function toggleSidebar() {
		const sidebar = document.getElementById("sidebar");
		sidebar.classList.toggle("open"); // Toggles sidebar visibility on mobile
	}

	function logOut() {
		signOut();
		router.push("/signIn");
	}
</script>

<template>
	<!-- Menu toggle button for mobile view -->
	<button
		id="menu-toggle"
		class="w-18 fixed right-0 top-4 z-30 mt-[7rem] bg-light-pink text-3xl text-white"
		@click="toggleSidebar"
	>
		<span class="material-icons">menu</span>
	</button>

	<!-- Responsive Sidebar/Bottom Bar Container -->
	<div
		id="sidebar"
		class="fixed inset-y-0 right-0 z-20 mt-[7rem] w-16 bg-white text-center text-xs md:w-16"
	>
		<!-- Navigation Menu -->
		<div class="flex h-full flex-col justify-between">
			<!-- Top Items -->
			<ul class="flex flex-col items-center justify-start py-8 text-gray-700">
				<!-- Home Item -->
				<li
					class="group w-full transition-all duration-300 hover:bg-light-purple"
				>
					<a href="/home" class="flex flex-col items-center justify-center p-2">
						<img
							src="../assets/navigation/home.svg"
							alt="Home Icon"
							:class="{
								'h-full w-full': true,
								'rounded-full bg-light-purple p-2': isActive('/home'),
							}"
						/>
						<span class="font-medium">Home</span>
					</a>
				</li>
				<!-- Profile Item -->
				<li
					class="group w-full transition-all duration-300 hover:bg-light-purple"
				>
					<a
						href="/profile/myPosts"
						class="flex flex-col items-center justify-center p-2"
					>
						<img
							src="../assets/navigation/profile.svg"
							alt="Profile Icon"
							:class="{
								'h-full w-full': true,
								'rounded-full bg-light-purple p-2': isActive('/profile'),
							}"
						/>
						<span class="font-medium">Profile</span>
					</a>
				</li>
				<!-- Calendar Item -->
				<li
					class="group w-full transition-all duration-300 hover:bg-light-purple"
				>
					<a
						href="/calendar"
						class="flex flex-col items-center justify-center p-2"
					>
						<img
							src="../assets/navigation/calendar.svg"
							alt="Calendar Icon"
							:class="{
								'h-full w-full': true,
								'rounded-full bg-light-purple p-2': isActive('/calendar'),
							}"
						/>
						<span class="font-medium">Calendar</span>
					</a>
				</li>

				<!-- Add post Item -->
				<li
					class="group w-full transition-all duration-300 hover:bg-light-purple"
				>
					<a
						href="/addPost"
						class="flex flex-col items-center justify-center p-2"
					>
						<img
							src="../assets/navigation/add-post.svg"
							alt="Add Icon"
							:class="{
								'h-full w-full': true,
								'rounded-full bg-light-purple p-2': isActive('/addPost'),
							}"
						/>
						<span class="font-medium">Create Post</span>
					</a>
				</li>
			</ul>
			<!-- Bottom Items -->
			<ul
				class="mt-auto flex flex-col items-center justify-start pb-8 text-gray-700"
			>
				<!-- Settings Item -->
				<li
					class="group w-full transition-all duration-300 hover:bg-light-purple"
					v-if="isProfile"
				>
					<a
						href="/settings"
						class="flex flex-col items-center justify-center p-2"
					>
						<img
							src="../assets/navigation/settings.svg"
							alt="Add Icon"
							:class="{
								'h-full w-full': true,
								'rounded-full bg-light-purple p-2': isActive('/settings'),
							}"
						/>
						<span class="font-medium">Settings</span>
					</a>
				</li>
				<!-- Logout Item -->
				<li
					class="group w-full transition-all duration-300 hover:bg-light-purple"
				>
					<button
						class="flex flex-col items-center justify-center p-2"
						@click="logOut"
					>
						<img
							src="../assets/navigation/logout.svg"
							alt="Add Icon"
							class="h-full w-full"
						/>
						<span class="font-medium">Logout</span>
					</button>
				</li>
			</ul>
		</div>
	</div>
</template>

<style scoped>
	/* Initial state for mobile view - Sidebar hidden off-screen to the right */
	#sidebar {
		transform: translateX(100%);
		transition: transform 0.3s ease;
	}

	/* State when the sidebar is open - Sidebar slides in */
	#sidebar.open {
		transform: translateX(0);
	}
	li {
		@apply mb-2;
	}

	@media (min-width: 768px) {
		/* Adjust this for your responsive breakpoint */
		#sidebar {
			transform: none;
			/* Sidebar always visible on larger screens */
		}

		#menu-toggle {
			/* Hide the toggle button on larger screens */
			display: none;
		}
	}
</style>
