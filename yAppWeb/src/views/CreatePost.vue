<script setup lang="js">
	import {
		post
	} from "aws-amplify/api";
	import {
		getCurrentUser
	} from 'aws-amplify/auth';
	import {
		onMounted,
		ref
	} from 'vue';
	import {
		useRouter
	} from 'vue-router'; // Import useRoute
import BackBtnHeader from "../components/BackBtnHeader.vue";

	const username = ref('');
	const jsonData = ref([]);
	const loading = false;

    var diaryEntry = false;
    var anonymous = true;
    // Retrieve the necessary data and function from the helper
    onMounted(async () => {
        const user = await getCurrentUser();
        username.value = user.username;
	});

	const router = useRouter(); // Use router hook
	var newPost = {
		"userName": "",
		"postTitle": "",
		"postBody": "",
		"diaryEntry": false,
		"anonymous": true
	};
	
	async function createPost(event) {
		event.preventDefault();
		var postElements = document.getElementById("post").elements;
		var createButton = document.getElementById("create-button");
		createButton.disabled = true;
		newPost.postTitle = postElements[0].value;
		newPost.postBody = postElements[1].value;
		newPost.diaryEntry = diaryEntry;
		newPost.anonymous = anonymous;

		if (diaryEntry) {
			checkDiaryEntryLimit();
		}

		if (newPost.postTitle !== '' && newPost.postBody !== '') {
			newPost.userName = username.value;
			// Make API call to create the post
			try {
				const sendPostRequest = post({
					apiName: "yapp",
					path: "/api/posts/createPost",
					headers: {
						'Content-Type': 'application/json'
					},
					options: {
						body: newPost
					}
				});
				const {
					body
				} = await sendPostRequest.response;
				const response = await body.json();

				console.log("POST call succeeded", response);

				// Reset form fields after submission
				newPost.userName = '';
				newPost.postTitle, postElements[0].value = '';
				newPost.postBody, postElements[1].value = '';
				createButton.disabled = false;
				// Send to home page
				router.push("/profile/myposts");
				// TODO: Show confirmation
				alert("Posted!");
			} catch (e) {
				alert("Post failed to create... Try agin!");
				createButton.disabled = false;
			}
		}
	}

	function discardPost(event) {
		event.preventDefault();
		var postElements = document.getElementById("post").elements;
		newPost.postTitle = postElements[0].value;
		newPost.postBody = postElements[1].value;
		if (newPost.postTitle != '' || newPost.postBody != '') {
			console.log('Throwing away post...');
			if (confirm("Are you sure you want to throw away your changes??")) {
				// Send to home page
				router.push({
					name: 'home'
				});
			}
		} else {
			router.push({
				name: 'home'
			});
		}
	}

	// This function is used for whether we want to show the anonymous toggle
	// Modify the diary entry and anonymous values here
	function toggleDiaryEntry() {
		diaryEntry = !diaryEntry;
		var anonymousToggle = document.getElementById("anonymous");
		if ( anonymousToggle.hidden == true ) {
			anonymousToggle.hidden = false;
			anonymous = false;
		} else {
			anonymousToggle.hidden = true;
			anonymous = true;
		}
	}

	function toggleAnonymous(){
		anonymous = !anonymous;
	}

	function checkDiaryEntryLimit(){
		// check if a diary entry has been made by this user that day already
	}
</script>

<template>

	<div class="backBtnDiv">
		<BackBtnHeader header="Create a new post!" subheader="Yapp your heart out."  />


		<div class="w-full md:px-16 md:mx-6 mt-3">
			<form class="post-heading bg-white p-8 rounded-lg" id="post">
				<div class="form-group w-full mb-4">
					<label for="title" class="block mb-2 text-gray-700">Title:</label>
					<input type="text" id="title" required placeholder="Insert your title here."
						class="input">
				</div>

				<div class="form-group w-full mb-4">
					<label for="content" class="block mb-2 text-gray-700">Content:</label>
					<textarea id="content" required
						placeholder="Insert the most heinous, confounding, baffling tea you've ever heard."
						class="input"></textarea>
				</div>

				<div class="mb-4">
					<label class="block text-gray-700 mb-2">Diary Post?</label>
					<label class="switch">
						<input type="checkbox" @click="toggleDiaryEntry">
						<span class="slider round"></span>
					</label>
					<text>Diary posts are only visible to your friends</text>
				</div>

				<div hidden id="anonymous" class="mb-4">
					<label class="block text-gray-700 mb-2">Anonymous?</label>
					<label class="switch">
						<input type="checkbox" @click="toggleAnonymous">
						<span class="slider round"></span>
					</label>
					<text>Anonymous diary posts will not show your username to your friends</text>
				</div>


				<div class="flex flex-col space-y-2 w-full">
					<button title="Create Post" id="create-button"
						class="bg-pink-purple text-white px-5 py-3 rounded-xl w-full" type="submit" @click="createPost">
						Create Post
					</button>
					<button title="Discard Post"
						class="bg-white text-dark px-5 py-3 rounded-xl w-full border border-gray-300"
						@click="discardPost">
						Discard
					</button>

				</div>


			</form>

		</div>
	</div>
</template>