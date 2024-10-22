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

    const diaryEntryIsChecked = ref(false);
    const anonIsChecked = ref(true);

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
		newPost.diaryEntry = diaryEntryIsChecked.value;
		newPost.anonymous = anonIsChecked.value;

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

        diaryEntryIsChecked.value = !diaryEntryIsChecked.value;
		var anonymousToggle = document.getElementById("anonymous");
		var anonymousText = document.getElementById("anonymousText");

		if ( anonymousToggle.hidden == true ) {
			anonymousToggle.hidden = false;
			anonymousText.hidden = false;
		} else {
			anonymousToggle.hidden = true;
			anonymousText.hidden = true;
            anonIsChecked.value = true;
		}
	}

	function toggleAnonymous(){
        anonIsChecked.value = !anonIsChecked.value;
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

                <div class="border-2 border-gray-300 p-8 rounded-lg mb-4">
                    
                    <div class="mb-4 float-root">
                        <label class="float-left block text-gray-700 text-lg font-semibold">Diary Post?</label>

                        <label class="float-right cursor-pointer select-none items-center">
                            <div class="relative ml-2 mr-2">
                                <input type="checkbox" class="sr-only" @change="toggleDiaryEntry" />
                                <div
                                    :class="{ '!bg-[#A55678]': diaryEntryIsChecked }"
                                    class="block h-8 rounded-full box bg-[#9E9E9E] w-14"
                                ></div>
                                <div
                                    :class="{ 'translate-x-full': diaryEntryIsChecked }"
                                    class="dot absolute left-1 top-1 h-6 w-6 rounded-full bg-white transition"
                                ></div>
                            </div>
                        </label>
                    </div>

                    <div>
                        <label class="block text-gray-700 mb-2 mt-8">Diary entry posts will only be shown to your friends</label>
                    </div>

                    <div hidden id="anonymous" class="mb-4 float-root">
                        <label class="float-left block text-gray-700 text-lg font-semibold">Anonymous?</label>

                        <label class="float-right cursor-pointer select-none items-center">
                            <div class="relative ml-2 mr-2">
                                <input type="checkbox" class="sr-only" @change="toggleAnonymous" />
                                <div
                                    :class="{ '!bg-[#A55678]': anonIsChecked }"
                                    class="block h-8 rounded-full box bg-[#9E9E9E] w-14"
                                ></div>
                                <div
                                    :class="{ 'translate-x-full': anonIsChecked }"
                                    class="dot absolute left-1 top-1 h-6 w-6 rounded-full bg-white transition"
                                ></div>
                            </div>
                        </label>
                    </div>

                    <div hidden id="anonymousText">
                        <label class="block text-gray-700 mb-2 mt-10">Anonymous diary posts will not show your username to your friends</label>
                    </div>

                </div>

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