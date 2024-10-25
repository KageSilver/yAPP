# API Documentation

## Friends

### GetFriendsByStatus
 - Retrieves all friends of a user filtered by a specified status.
 - GET: api/friends/getFriendsByStatus?userName={username}&status={status}
    - userName: The username of the user whose friends are to be retrieved.
    - status: The status of the friendships to filter by (-1:All requests, 0: Pending, 1: Accepted, 2: Declined).
 - Status codes
    - 200 OK
    - 400 Bad Request: 
        - username is required

### SendFriendRequest
 - Sends a friend request from one user to another.
 - POST: api/friends/friendRequest
 - Request body: { "fromUserName": "username", "toUserId": "id" }
 - Status codes:
    - 200 OK
    - 400 Bad Request
        - request body is required and must contain username and friend's id
        - Failed to create friendship
    - 404 Not Found
        - Friend not found

### UpdateFriendRequest
 - Updates the status of an existing friend request.
 - PUT: api/friends/updateFriendRequest
 - Request body: { "fromUserName": "username", "toUserName": "username", "status": 1 }
 - Status codes
    - 200 OK
    - 400 Bad Request: 
        - request body is required and must contain username and friend's username
        - Failed to update friendship status
    - 404 Not Found: 
        - Friendship not found

## Posts

### GetPostById
 - Retrieves a post by a unique identifier.
 - GET: api/posts/getPostById?pid={pid}
    - pid: The unique identifier for a post
 - Status codes
    - 200 OK
    - 400 Bad Request
        - Post ID is required
    - 404 Not Found
        - Post does not exist

### GetPostsByUser
 - Retrieves all posts from a user, either all public posts or all diary entries.
 - GET: api/posts/getPostsByUser?uid={uid}&diaryEntry={diaryEntry}
    - uid: The uid used to find all posts created by a user
    - diaryEntry: If the query is for public posts or diary entries
 - Status codes
    - 200 OK
    - 400 Bad Request
        - uid is required

### GetRecentPosts
 - Gets recent posts from before a specified time.
 - GET: api/posts/getRecentPosts?since={since}&maxResults={maxResults}
    - since: Returns posts made after this time
    - maxResults: The maximum number of results to retrieve.
 - Status codes
    - 200 OK
    - 400 Bad Request
        - requires valid max result number and valid time

### CreatePost
 - Creates a new post.
 - POST: api/posts/createPost
 - Request body: { "uid": "uid", "postTitle": "title", "postBody": "body", "diaryEntry": false, "anonymous": false }
 - Status codes
    - 200 OK
    - 400 Bad Request
        - request body is required and must contain poster's uid, post title and post body
        - Failed to create post
    - 404 Not Found
        - Post creator not found

### UpdatePost
 - Edits an already existing post.
 - PUT: api/posts/updatePost
 - Request body: { "pid": "pid", "createdAt": "createdAt", "uid": "uid", "postTitle": "title", "postBody": "body", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": false }
 - Status codes
    - 200 OK
    - 400 Bad Request
        - request body is required and must contain uid, post title, post body

### DeletePost
 - Deletes a post from the database by a post id.
 - DELETE: api/posts/deletePost?pid={pid}
    - pid: The id of the post to be deleted
 - Status codes
    - 200 OK
    - 400 Bad Request
        - Post id is required

## Users

### GetUserById
 - Retrieves a user by their unique identifier.
 - GET: api/cognito/getUserById?id={id}
    - id: The unique identifier of the user
 - Status codes
    - 200 OK
    - 400 Bad Request
        - Id is required
    - 404 Not Found
        - User not found

### GetUserByName
 - Retrieves a user by their username.
 - GET: api/cognito/getUserByName?userName={username}
    - userName: The username of the user to retrieve
 - Status codes
    - 200 OK
    - 400 Bad Request
        - username is required
    - 404 Not Found
        - User not found

### UpdateUser
 - Updates a user's details.
 - POST: api/users/updateUser 
 - Request body: { "userName": "username", "nickName": "nickname", "name": "name" }
 - Status codes
    - 200 OK
    - 400 Bad Request
        - request body is required and must contain username and name
    - 404 Not Found
        - User not found