<script setup lang="js">
  import { post } from "aws-amplify/api";
  import { useAuthenticator } from "@aws-amplify/ui-vue";
  import { useRouter } from 'vue-router'; // Import useRouter

  const auth = useAuthenticator(); // Grab authenticator for username
  const router = useRouter(); // Use router hook
  var newPost = {
    "userName": "",
    "postTitle": "",
    "postBody": "",
    "diaryEntry": false,
    "anonymous": true
  };

  async function createPost(event) 
  {
    event.preventDefault();
    var postElements = document.getElementById("post").elements;
    var createButton = document.getElementById("create-button");
    createButton.disabled = true;
    newPost.postTitle = postElements[0].value;
    newPost.postBody = postElements[1].value;
    if ( newPost.postTitle !== '' && newPost.postBody !== '' ) 
    {
      newPost.userName = auth.user?.username;
      // Make API call to create the post
      try 
      {
        const sendPostRequest = post({
          apiName: "yapp",
          path: "/api/posts/createPost",
          headers: 
          {
            'Content-Type': 'application/json'
          },
          options: 
          {
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
        createButton.disabled = false;
        // Send to home page
        router.push({ name: 'dashboard' });
        // TODO: Show confirmation
        alert("Posted!");
      } 
      catch (e) 
      {
        alert("Post failed to create... Try agin!");
        createButton.disabled = false;
      }
    }
  }

  function discardPost(event) 
  {
    event.preventDefault();
    var postElements = document.getElementById("post").elements;
    newPost.postTitle = postElements[0].value;
    newPost.postBody = postElements[1].value;
    if ( newPost.postTitle != '' || newPost.postBody != '' ) 
    {
      console.log('Throwing away post...');
      if (confirm("Are you sure you want to throw away your changes??")) 
      {
        // Send to home page
        router.push({ name: 'dashboard' });
      }
    } 
    else 
    {
      router.push({ name: 'dashboard' });
    }
  }

  // This function is used for whether we want to show the anonymous toggle
  // Modify the diary entry and anonymous values here
  function toggleAnonymous() 
  {
    var anonymousToggle = document.getElementById("anonymous");
    if ( anonymousToggle.hidden == true ) 
    {
      anonymousToggle.hidden = false;
    } 
    else 
    {
      anonymousToggle.hidden = true;
    }
  }
</script>

<template>
  <div class="create-post">
    <h2 class="post-heading">Create a New Post!</h2>
    <form class="post-heading" id="post">
      <div class="form-group">
        <label for="title">Title:</label>
        <input
          type="text"
          id="title"
          required
          placeholder="Enter post title"
        />
      </div>
      
      <div class="form-group">
        <label for="content">Content:</label>
        <textarea
          id="content"
          required
          placeholder="Enter post content"
        ></textarea>
      </div>
<!-- This area is for when we add in the diary entry creation. Remove the hidden tag from the first div when ready. -->
      <div hidden>
        <label class="switch-label">Diary Post?</label>
        <label class="switch">
          <input type="checkbox" @click="toggleAnonymous">
          <span class="slider round"></span>
        </label>
      </div>
      <div hidden id="anonymous">
        <label class="switch-label">Anonymous?</label>
        <label class="switch">
          <input type="checkbox">
          <span class="slider round"></span>
        </label>
      </div>

      <button title="Discard Post" class="back-button" @click="discardPost">
        Discard
      </button>
      <button title="Create Post" id="create-button" class="createPostButton signoutButton" type="submit" @click="createPost">
        Create Post
      </button>
    </form>
  </div>
</template>
  
<style scoped>
  .post-heading {
    color: #000000;
  }

  .create-post {
    max-width: 600px;
    width: auto;
    height: auto;
    margin: 0 auto;
    padding: 20px;
    background-color: #fae9f4;
    border-radius: 8px;
    text-align: center;
  }
  
  .form-group {
    margin-bottom: 15px;
  }
  
  .form-group label {
    display: block;
    margin-bottom: 5px;
  }
  
  .form-group input,
  .form-group textarea {
    width: 80%;
    height: 100%;
    padding: 20px;
    font-size: 16px;
    border: 1px solid #ccc;
    border-radius: 4px;
  }
  
  .back-button {
    padding: 5px;
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    background-color: #19234b;
    margin: 0 50px;
  }
  .back-button:hover {
    background-color: #72395e;
  }

  .createPostButton {
    float: none !important;
  }

  /* Section is all for the diary entries. 
  This is all for the switch - the box around the slider */
  .switch {
    position: relative;
    display: inline-block;
    width: 60px;
    height: 34px;
    margin: 10px;
  }

  .switch-label {
    margin-left: 20px;
  }

  /* Hiding the default HTML checkbox */
  .switch input {
    opacity: 0;
    width: 0;
    height: 0;
  }

  /* The slider itself */
  .slider {
    position: absolute;
    cursor: pointer;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: #ccc;
    -webkit-transition: .4s;
    transition: .4s;
  }

  .slider:before {
    position: absolute;
    content: "";
    height: 26px;
    width: 26px;
    left: 4px;
    bottom: 4px;
    background-color: white;
    -webkit-transition: .4s;
    transition: .4s;
  }

  input:checked + .slider {
    background-color: #a75779;
  }

  input:focus + .slider {
    box-shadow: 0 0 1px #a75779;
  }

  input:checked + .slider:before {
    -webkit-transform: translateX(26px);
    -ms-transform: translateX(26px);
    transform: translateX(26px);
  }

  /* Making the slider rounded */
  .slider.round {
    border-radius: 34px;
  }

  .slider.round:before {
    border-radius: 50%;
  }

  .diary {
    margin: 10px;
  }
</style>
