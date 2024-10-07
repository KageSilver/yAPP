# API Documentation

## Friends

### GetFriendsByStatus
 - Retrieves all friends of a user filtered by a specified status.
 - GET: api/friends/getFriendsByStatus?userName={username}?status={status}
    - userName: The username of the user whose friends are to be retrieved.
    - status: The status of the friendships to filter by (-1:All requests, 0: Pending, 1: Accepted, 2: Declined).

### SendFriendRequest
 - Sends a friend request from one user to another.
 - POST: api/friends/friendRequest
 - Request body: { "fromUserName": "username", "toUserId": "id" }

### UpdateFriendRequest
 - Updates the status of an existing friend request.
 - PUT: api/friends/updateFriendRequest
 - Request body: { "fromUserName": "username", "toUserName": "username", "status": 1 }

## Posts

### GetPostById
 - Retrieves a post by a unique identifier.
 - GET: api/posts/getPostById?pid={pid}
    - pid: The unique identifier for a post

### GetPostsByUser
 - Retrieves all posts from a user, either all public posts or all diary entries.
 - GET: api/posts/getPostsByUser?userName={userName}&diaryEntry={diaryEntry}
    - userName: The username used to find all posts created by a user
    - diaryEntry: If the query is for public posts or diary entries

### GetRecentPosts
 - Gets recent posts from before a specified time.
 - GET: api/posts/getRecentPosts?since={since}&maxResults={maxResults}
    - since: Returns posts made after this time
    - maxResults: The maximum number of results to retrieve.

### CreatePost
 - Creates a new post.
 - POST: api/posts/createPost
 - Request body: { "userName": "username", "postTitle": "title", "postBody": "body", "diaryEntry": false, "anonymous": false }

### UpdatePost
 - Edits an already existing post.
 - PUT: api/posts/updatePost
 - Request body: { "pid": "pid", "createdAt": "createdAt", "userName": "username", "postTitle": "title", "postBody": "body", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": false }

### DeletePost
 - Deletes a post from the database by a post id.
 - DELETE: api/posts/deletePost?pid={pid}
    - pid: The id of the post to be deleted

## Users

### GetUserById
 - Retrieves a user by their unique identifier.
 - GET: api/cognito/getUserById?id={id}
    - id: The unique identifier of the user

### GetUserByName
 - Retrieves a user by their username.
 - GET: api/cognito/getUserByName?userName={username}
    - userName: The username of the user to retrieve

### UpdateUser
 - Updates a user's details.
 - POST: api/users/updateUser 
 - Request body: { "userName": "username", "nickName": "nickname", "name": "name" }