<script setup lang="js">
	import { post } from "aws-amplify/api";
	import { getCurrentUser } from "aws-amplify/auth";
	import { onMounted, ref } from "vue";
	import { useRouter } from "vue-router"; // Import useRoute
	import BackBtnHeader from "../components/BackBtnHeader.vue";
	import LoadingScreen from "../components/LoadingScreen.vue";

	const uid = ref("");
	const jsonData = ref([]);
	const loading = ref(false);

	const diaryEntryIsChecked = ref(false);
	const anonIsChecked = ref(true);

	// Retrieve the necessary data and function from the helper
	onMounted(async () => {
		const user = await getCurrentUser();
		uid.value = user.userId;
	});

	const router = useRouter(); // Use router hook
	var newPost = {
		uid: "",
		postTitle: "",
		postBody: "",
		diaryEntry: false,
		anonymous: true,
	};

	const postPost = async () => {
		loading.value = true;
		try {
			const sendPostRequest = post({
				apiName: "yapp",
				path: "/api/posts/createPost",
				headers: {
					"Content-Type": "application/json",
				},
				options: {
					body: newPost,
				},
			});
			const { body } = await sendPostRequest.response;
			const response = await body.json();
			console.log("POST call succeeded", response);
			exitCreatePost();
			// Reset form fields after submission
		} catch (e) {
			console.log("POST call failed", e);

			if (e.response.body == '"Cannot make more than one diary entry a day"') {
				alert(
					"Woah there! You can't make more than one diary entry per day :(",
				);
			}
		}
		loading.value = false;
	};

	function exitCreatePost() {
		if (newPost.diaryEntry) {
			// go to calendar view if user created a diary entry
			router.push({
				name: "calendar",
			});
		} else {
			// go to profile if user created a public post
			router.push({
				name: "profile",
			});
		}
	}

	const createPost = async event => {
		//check validation
		newPost.postTitle = document.getElementById("title").value.trim();
		newPost.postBody = document.getElementById("content").value.trim();
		if (newPost.postTitle === "" || newPost.postBody === "") {
			alert("Please fill out all fields!");
			return;
		}

		var createButton = document.getElementById("create-button");
		createButton.disabled = true;
		newPost.diaryEntry = diaryEntryIsChecked.value;
		newPost.anonymous = anonIsChecked.value;
		newPost.uid = uid.value;
		resetToggles();

		await postPost();

		createButton.disabled = false;
	};

	function discardPost(event) {
		event.preventDefault();
		newPost.postTitle = document.getElementById("title").value;
		newPost.postBody = document.getElementById("content").value;
		newPost.postTitle = document.getElementById("title").value;
		newPost.postBody = document.getElementById("content").value;
		if (newPost.postTitle != "" || newPost.postBody != "") {
			console.log("Throwing away post...");
			if (confirm("Are you sure you want to throw away your changes??")) {
				// Send to home page
				router.push({
					name: "home",
				});
			}
		} else {
			router.push({
				name: "home",
			});
		}
	}

	// This function is used for whether we want to show the anonymous toggle
	// Modify the diary entry and anonymous values here
	function toggleDiaryEntry() {
		diaryEntryIsChecked.value = !diaryEntryIsChecked.value;
		var anonymousToggle = document.getElementById("anonymous");
		var anonymousText = document.getElementById("anonymousText");

		if (anonymousToggle.hidden == true) {
			anonymousToggle.hidden = false;
			anonymousText.hidden = false;
		} else {
			anonymousToggle.hidden = true;
			anonymousText.hidden = true;
			anonIsChecked.value = true;
		}
	}

	function resetToggles() {
		diaryEntryIsChecked.value = false;
		document.getElementById("anonymous").hidden = true;
		document.getElementById("anonymousText").hidden = true;
		anonIsChecked.value = true;
	}

	function toggleAnonymous() {
		anonIsChecked.value = !anonIsChecked.value;
	}
</script>

<template>
	<LoadingScreen v-if="loading" class="" />
	<div v-else class="backBtnDiv">
		<BackBtnHeader
			header="Create a new post!"
			subheader="Yapp your heart out."
		/>

		<div class="mt-3 w-full md:mx-6 md:px-16">
			<form class="post-heading rounded-lg bg-white p-8" id="post">
				<div class="mb-4 rounded-lg border-2 border-gray-300 p-8">
					<div class="float-root mb-4">
						<label class="float-left block text-lg font-semibold text-gray-700"
							>Diary Post?</label
						>

						<label class="float-right cursor-pointer select-none items-center">
							<div class="relative ml-2 mr-2">
								<input
									type="checkbox"
									class="sr-only"
									@change="toggleDiaryEntry"
								/>
								<div
									:class="{ '!bg-[#A55678]': diaryEntryIsChecked }"
									class="box block h-8 w-14 rounded-full bg-[#9E9E9E]"
								></div>
								<div
									:class="{ 'translate-x-full': diaryEntryIsChecked }"
									class="dot absolute left-1 top-1 h-6 w-6 rounded-full bg-white transition"
								></div>
							</div>
						</label>
					</div>

					<div>
						<label class="mb-2 mt-8 block text-gray-700"
							>Diary entry posts will only be shown to your friends</label
						>
					</div>

					<div hidden id="anonymous" class="float-root mb-4">
						<label class="float-left block text-lg font-semibold text-gray-700"
							>Anonymous?</label
						>

						<label class="float-right cursor-pointer select-none items-center">
							<div class="relative ml-2 mr-2">
								<input
									type="checkbox"
									class="sr-only"
									@change="toggleAnonymous"
								/>
								<div
									:class="{ '!bg-[#A55678]': anonIsChecked }"
									class="box block h-8 w-14 rounded-full bg-[#9E9E9E]"
								></div>
								<div
									:class="{ 'translate-x-full': anonIsChecked }"
									class="dot absolute left-1 top-1 h-6 w-6 rounded-full bg-white transition"
								></div>
							</div>
						</label>
					</div>

					<div hidden id="anonymousText">
						<label class="mb-2 mt-10 block text-gray-700"
							>Anonymous diary posts will not show your username to your
							friends</label
						>
					</div>
				</div>

				<div class="form-group mb-4 w-full">
					<label for="title" class="mb-2 block text-gray-700">Title:</label>
					<input
						type="text"
						id="title"
						required
						placeholder="Insert your title here."
						class="input"
					/>
				</div>
				<div class="form-group mb-4 w-full">
					<label for="content" class="mb-2 block text-gray-700">Content:</label>
					<textarea
						id="content"
						required
						placeholder="Insert the most heinous, confounding, baffling tea you've ever heard."
						class="input"
					></textarea>
				</div>
				<div class="flex w-full flex-col space-y-2">
					<button
						title="Create Post"
						id="create-button"
						class="w-full rounded-xl bg-pink-purple px-5 py-3 text-white"
						@click="createPost"
					>
						Create Post
					</button>
					<button
						title="Discard Post"
						class="w-full rounded-xl border border-gray-300 bg-white px-5 py-3 text-dark"
						@click="discardPost"
					>
						Discard
					</button>
				</div>
			</form>
		</div>
	</div>
</template>
