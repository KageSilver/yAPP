<script setup="js">
  import { get } from "aws-amplify/api";
  import { useAuthenticator } from "@aws-amplify/ui-vue";
  import { useRouter } from 'vue-router'; // Import useRouter
import { time } from "console";
  const auth = useAuthenticator(); // Grab authenticator for username
  const router = useRouter(); // Use router hook

  const posts = [];
  const maxResults = 25;

  async function getRecentPosts() {
    var postElements = document.getElementById("post").elements;
    var since = time();
    if ( newPost.postTitle != '' && newPost.postBody != '' ) {
      newPost.userName = auth.user?.username;
      // Make API call to create the post
      try {
        const sendPostRequest = get({
          apiName: "yapp",
          path: "/api/posts/getRecentPosts?since={since}&maxResults={maxResults}",
          headers: {
            'Content-Type': 'application/json'
          },
          options: {
            body: newPost
          }
        });
        const {body} = await sendPostRequest.response;
        const response = await body.json();

        console.log("POST call succeeded", response);
        
        // Reset form fields after submission
        newPost.userName = '';
        newPost.postTitle, postElements[0].value = '';
        newPost.postBody, postElements[1].value = '';
        // Send to home page
        router.push({ name: 'dashboard' });
        // TODO: Show confirmation
        alert("Posted!");
      } catch (e) {
        console.log("POST call failed: ", e);
        alert("Post failed to create... Try agin!");
      }
    }
  }

  function clickPost() {
    var postElements = document.getElementById("post").elements;
    newPost.postTitle = postElements[0].value;
    newPost.postBody = postElements[1].value;
    if ( newPost.postTitle != '' || newPost.postBody != '' ) {
      console.log('Throwing away post...');
      if (confirm("Are you sure you want to throw away your changes??")) {
        // Send to home page
        router.push({ name: 'dashboard' });
      }
    } else {
      router.push({ name: 'dashboard' });
    }
  }
</script>

<template>
</template>