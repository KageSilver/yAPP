<template>
  <div class="create-post">
    <h2 class="post-heading">Create a New Post!</h2>
    <form @submit.prevent="createPost" class="post-heading">
      <div class="form-group">
        <label for="title">Title:</label>
        <input
          type="text"
          v-model="post.title"
          id="title"
          required
          placeholder="Enter post title"
        />
      </div>
      
      <div class="form-group">
        <label for="content">Content:</label>
        <textarea
          v-model="post.content"
          id="content"
          required
          placeholder="Enter post content"
        ></textarea>
      </div>
      <button title="Discard Post" class="back-button" @click="discardPost">Discard</button>
      <button title="Create Post" type="submit" @click="createPost">Create Post</button>
    </form>
  </div>
</template>
  
<script>
  export default {
    data() {
      return {
        post: {
          title: '',
          content: '',
        }
      };
    },
    methods: {
      createPost() {
        // Add API call or local state mutation here
        console.log('Post created:', this.post);
        // Reset form fields after submission
        this.post.title = '';
        this.post.content = '';
      },
      discardPost() {
        console.log('Throwing away post...')
        // Add call to go back to the previous page
        // Also maybe add a confirmation popup
      }
    }
  };
</script>
  
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
  
  button {
    padding: 10px 20px;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
  }
  
  button:hover {
    background-color: #72395e;
  }

  .back-button {
    padding: 5px;
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    background-color: #19234b;
    margin: 0 15px;
  }
</style>
  