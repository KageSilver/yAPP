# API Endpoints

## Friends

### GetFriendsByStatus
 - GET: api/friends/getFriendsByStatus?userName={username}?status={status}
 - userName: The username of the user whose friends are to be retrieved
 - status: The status of the friendships to filter by (-1:All requests, 0: Pending, 1: Accepted, 2: Declined)

### SendFriendRequest
 - POST: api/friends/friendRequest
 - Request body: { "fromUserName": "username", "toUserId": "id" }

### UpdateFriendRequest
 - PUT: api/friends/updateFriendRequest
 - Request body: { "fromUserName": "username", "toUserName": "username", "status": 1 }

## Posts

### GetPostById
 - GET: api/posts/getPostById?pid={pid}
 - pid: The unique identifier for a post

### GetPostsByUser
 - GET: api/posts/getPostsByUser?userName={userName}&diaryEntry={diaryEntry}
 - userName: The username used to find all posts created by a user
 - diaryEntry: If the query is for public posts or diary entries

### GetRecentPosts
 - GET: api/posts/getRecentPosts?since={since}&maxResults={maxResults}
 - since: Returns posts made after this time
 - maxResults: The maximum number of results to retrieve.

### CreatePost
 - POST: api/posts/createPost
 - Request body: { "userName": "username", "postTitle": "title", "postBody": "body", "diaryEntry": false, "anonymous": false }

### UpdatePost
 - PUT: api/posts/updatePost
 - Request body: { "pid": "pid", "createdAt": "createdAt", "userName": "username", "postTitle": "title", "postBody": "body", "upvotes": "upvotes", "downvotes": "downvotes", "diaryEntry": false, "anonymous": false }

### DeletePost
 - DELETE: api/posts/deletePost?pid={pid}
 - pid: The id of the post to be deleted

## Users

### GetUserById
 - GET: api/cognito/getUserById?id={id}
 - id: The unique identifier of the user

### GetUser
 - GET: api/cognito/getUserByName?userName={username}
 - userName: The username of the user to retrieve

### UpdateUser
 - POST: api/users/updateUser 
 - Request body: