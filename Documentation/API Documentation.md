# API Documentation

## Awards

### GetAwardById
 - Retrieves an award by a unique identifier.
 - GET: api/awards/getAwardById?aid={aid}
   - aid: The unique identifier for an award
 - Response: returns the award object associated with the specified award id
   - { "aid": "aid", "pid": "pid", "uid": "uid", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "name": "name", "type": "type", "tier": 0 }
 - Status codes:
   - 200 OK
   - 400 Bad Request: award ID is required
   - 404 Not Found: Award does not exist

### GetAwardsByUser
 - Gets all awards from a user
 - GET: api/awards/getAwardsByUser?uid={uid}
   - uid: The user who earned the awards being fetched
 - Response: returns a list of award objects that have been earned by the user associated with the specified user id
   - [ { "aid": "aid", "pid": "pid", "uid": "uid", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "name": "name", "type": "type", "tier": 0 } ]
 - Status codes:
   - 200 Ok
   - 400 Bad Request: uid is required

### GetAwardsByPost
 - Gets all awards from a post
 - GET: api/awards/getAwardsByPost?pid={pid}
   - pid: The post on which the awards were earned
 - Response: returns a list of award objects that have been earned on a post associated with the specified post id
   - [ { "aid": "aid", "pid": "pid", "uid": "uid", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "name": "name", "type": "type", "tier": 0 } ]
 - Status codes:
   - 200 Ok
   - 400 Bad Request: pid is required

### GetNewAwardsByUser
 - Gets new awards a user has earned since this endpoint was last called
 - GET: api/awards/getNewAwardsByUser?uid={uid}
   - uid: The user who earned the awards being fetched
 - Response: returns a list of award objects that have been earned by the specified user through the checks done in this endpoint
   - [ { "aid": "aid", "pid": "pid", "uid": "uid", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "name": "name", "type": "type", "tier": 0 } ] 
 - Status codes:
   - 200 Ok
   - 400 Bad Request: uid is required
   - 404 Not Found: User does not exist

## Comments

### GetPostByCid
 - Gets the post a comment was made on from a comment ID.
 - GET: api/comments/getPostByCid?cid={cid}
    - cid: The comment id to find the parent post
 - Response: returns the post object associated with the comment based on the specified id
    - { "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "postTitle": "string", "postBody": "string", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": true }
 - Status codes:
    - 200 OK
    - 400 Bad Request: Comment ID is required
    - 404 Not Found: Post does not exist

### GetCommentById
 - Gets the comment from a comment ID.
 - GET: api/comments/getCommentById?cid={cid}
    - cid: The comment id to find the comment
 - Response: returns the comment with the specified id
   - { "cid": "string", "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "commentBody": "string", "upvotes": 0, "downvotes": 0 }
 - Status codes:
    - 200 OK
    - 400 Bad Request: Comment ID is required
    - 404 Not Found: Comment does not exist  

### GetCommentsByUser
 - Gets all comments with given uid.
 - GET: api/comments/getCommentsByUser?uid={uid}
    - uid: the uid used to find all comments by that user
 - Response: returns a list of comment objects made by the specified user
    - [ { "cid": "string", "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "commentBody": "string", "upvotes": 0, "downvotes": 0 } ]
 - Status codes:
    - 200 OK
    - 400 Bad Request: uid is required

### GetCommentsByPid
 - Gets all comments with a given parent post id.
 - GET: api/comments/getCommentsByPid?pid={pid}
    - pid: the parent post id used to find associated comments
 - Response: returns a list of comments made on the parent post
    - [ { "cid": "string", "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "commentBody": "string", "upvotes": 0, "downvotes": 0 } ]
 - Status codes:
    - 200 OK
    - 400 Bad Request: Post ID is required

### CreateComment
 - Creates a comment.
 - POST: api/comments/createComment
 - Request body: { "uid": "uid", "commentBody": "body", "pid": "pid" }
 - Response: returns the comment objects created from the new comment
    - { "cid": "string", "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "commentBody": "string", "upvotes": 0, "downvotes": 0 }
 - Status codes:
    - 200 OK
    - 400 Bad Request: 
        - request body is required and must contain commenter's uid, comment body, and the original post's id.
        - Failed to create comment
    - 404 Not Found: Comment creator not found

### UpdateComment
 - Updates a comment.
 - PUT: api/comments/updateComment
 - Request body: { "cid": "cid", "pid": "pid", "createdAt": "createdAt", "updatedAt": "updatedAt", "uid": "uid", "commentBody": "body", "upvotes": "upvotes", "downvotes": "downvotes" }
 - Response: returns the updated comment object
    - { "cid": "string", "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "commentBody": "string", "upvotes": 0, "downvotes": 0 }
 - Status codes:
    - 200 OK
    - 400 Bad Request: Request body is required and must contain uid and comment body

### DeleteComment
 - Deletes a comment.
 - DELETE: api/comments/deleteComment?cid={cid}
    - cid: the comment id of the comment to delete
 - Response: returns whether the deletion was successful
    - true/false
 - Status codes:
    - 200 OK
    - 400 Bad Request: Comment ID is required

## Friends

### GetFriendsByStatus
 - Retrieves all friends of a user filtered by a specified status.
 - GET: api/friends/getFriendsByStatus?userName={username}&status={status}
    - userName: The username of the user whose friends are to be retrieved.
    - status: The status of the friendships to filter by (-1:All requests, 0: Pending, 1: Accepted, 2: Declined).
 - Response: returns the updated friend object
    - [ { "FromUserName": "string", "ToUserName": "string", "Status": 0, "CreatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "UpdatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ" } ]
 - Status codes:
    - 200 OK
    - 400 Bad Request: username is required

### SendFriendRequest
 - Sends a friend request from one user to another.
 - POST: api/friends/friendRequest
 - Request body: { "fromUserName": "username", "toUserId": "id" }
 - Response: returns a friend object created from the friend request
    - { "FromUserName": "string", "ToUserName": "string", "Status": 0, "CreatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "UpdatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ" }
 - Status codes:
    - 200 OK
    - 400 Bad Request:
        - request body is required and must contain username and friend's id
        - Failed to create friendship
    - 404 Not Found: Friend not found

### UpdateFriendRequest
 - Updates the status of an existing friend request.
 - PUT: api/friends/updateFriendRequest
 - Request body: { "fromUserName": "username", "toUserName": "username", "status": 1 }
 - Response: returns a list of friend objects with that status
    - { "FromUserName": "string", "ToUserName": "string", "Status": 0, "CreatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "UpdatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ" }
 - Status codes:
    - 200 OK
    - 400 Bad Request: 
        - request body is required and must contain username and friend's username
        - Failed to update friendship status
    - 404 Not Found: Friendship not found

## Posts

### GetPostById
 - Retrieves a post by a unique identifier.
 - GET: api/posts/getPostById?pid={pid}
    - pid: The unique identifier for a post
 - Response: returns the post with the specified id
    - { "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "postTitle": "string", "postBody": "string", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": true }
 - Status codes:
    - 200 OK
    - 400 Bad Request: Post ID is required
    - 404 Not Found: Post does not exist

### GetPostsByUser
 - Retrieves all posts from a user, either all public posts or all diary entries.
 - GET: api/posts/getPostsByUser?uid={uid}&diaryEntry={diaryEntry}
    - uid: The uid used to find all posts created by a user
    - diaryEntry: If the query is for public posts or diary entries
 - Response: returns a list of posts by the specified user
    - [ { "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "postTitle": "string", "postBody": "string", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": true } ]
 - Status codes:
    - 200 OK
    - 400 Bad Request: uid is required

### GetRecentPosts
 - Gets recent posts from before a specified time.
 - GET: api/posts/getRecentPosts?since={since}&maxResults={maxResults}
    - since: Returns posts made after this time
    - maxResults: The maximum number of results to retrieve.
 - Response: returns a list of posts created before a specified time with a specified maximum length
    - [ { "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "postTitle": "string", "postBody": "string", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": true } ]
 - Status codes:
    - 200 OK
    - 400 Bad Request: requires valid max result number and valid time

### GetDiariesByUser
 - Gets diary entry posts from a specified user from a specified date
 - GET: api/posts/getDiariesByUser?uid={uid}&current={current}
    - uid: The uid to find a diary entry by the user
    - current: The specified date to search for diary entries made on that date
 - Response: returns a diary entry post that was created by the specified user on a specified date
    - { "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "postTitle": "string", "postBody": "string", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": true }
 - Status codes:
    - 200 OK
    - 400 Bad Request: uid and valid datetime is required

### GetDiariesByFriends
 - Gets diary entry posts from all friends of a specified user
 - GET: api/posts/getDiariesByFriends?uid={uid}&current={current}
    - uid: The uid to find a diary entry by the user's friends
    - current: The specified date to search for diary entries made on that date
 - Response: returns a list of posts made by the users friends on a specified date
    - [ { "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "postTitle": "string", "postBody": "string", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": true } ]
 - Status codes:
    - 200 OK
    - 400 Bad Request: uid and valid datetime is required

### CreatePost
 - Creates a new post.
 - POST: api/posts/createPost
 - Request body: { "uid": "uid", "postTitle": "title", "postBody": "body", "diaryEntry": false, "anonymous": false }
 - Response: returns the post object created from the new post
    - { "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "postTitle": "string", "postBody": "string", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": true }
 - Status codes:
    - 200 OK
    - 400 Bad Request:
        - request body is required and must contain poster's uid, post title and post body
        - Failed to create post
    - 404 Not Found: Post creator not found

### UpdatePost
 - Edits an already existing post.
 - PUT: api/posts/updatePost
 - Request body: { "pid": "pid", "createdAt": "createdAt", "updatedAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "uid", "postTitle": "title", "postBody": "body", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": false }
 - Response: returns the updated post object
    - { "pid": "string", "createdAt": "yyyy-MM-ddTHH:mm:ss.FFFZ", "uid": "string", "postTitle": "string", "postBody": "string", "upvotes": 0, "downvotes": 0, "diaryEntry": false, "anonymous": true }
 - Status codes:
    - 200 OK
    - 400 Bad Request: request body is required and must contain uid, post title, post body

### DeletePost
 - Deletes a post from the database by a post id.
 - DELETE: api/posts/deletePost?pid={pid}
    - pid: The id of the post to be deleted
 - Response: returns whether the deletion was successful
    - true/false
 - Status codes:
    - 200 OK
    - 400 Bad Request: Post id is required

## Users

### GetUserById
 - Retrieves a user by their unique identifier.
 - GET: api/users/getUserById?id={id}
    - id: The unique identifier of the user
 - Response: returns the user object with the specified id
    - { "userName": "string", "nickName": "string", "id": "string", "name": "string", "email": "string", "Attributes": { "additionalProp1": "string", "additionalProp2": "string", "additionalProp3": "string" } }
 - Status codes:
    - 200 OK
    - 400 Bad Request: Id is required
    - 404 Not Found: User not found

### GetUserByName
 - Retrieves a user by their username.
 - GET: api/users/getUserByName?userName={username}
    - userName: The username of the user to retrieve
 - Response: returns the user object with the specified username
    - { "userName": "string", "nickName": "string", "id": "string", "name": "string", "email": "string", "Attributes": { "additionalProp1": "string", "additionalProp2": "string", "additionalProp3": "string" } }
 - Status codes:
    - 200 OK
    - 400 Bad Request: username is required
    - 404 Not Found: User not found

### UpdateUser
 - Updates a user's details.
 - POST: api/users/updateUser 
 - Request body: { "userName": "username", "nickName": "nickname", "name": "name" }
 - Response: returns the updated user object
    - { "userName": "string", "nickName": "string", "id": "string", "name": "string", "email": "string", "Attributes": { "additionalProp1": "string", "additionalProp2": "string", "additionalProp3": "string" } }
 - Status codes:
    - 200 OK
    - 400 Bad Request: request body is required and must contain username and name
    - 404 Not Found: User not found

## Votes

### GetVote
 - Get the given vote by uid, pid, and type
 - GET: api/votes/getVoteStatus?uid={uid}&pid={pid}&type={type}
   - uid: The uid of the current user
   - pid: The pid of the post or comment
   - type: Whether it's checking for an upvote/downvote
 - Response: returns a vote object that is associated with the given values
   - { "pid": "pid", "isPost": true, "type": true, "uid": "uid" }

### GetVotesByPid
 - Gets all votes with given PID
 - GET: api/votes/getVotesByPid?pid={pid}
   - pid: The pid to find a vote under
 - Response: returns a list of vote objects made on a post associated with the specified post id
   - [ { "pid": "pid", "isPost": true, "type": true, "uid": "uid" } ]
 - Status codes:
   - 200 OK
   - 400 Bad Request: Pid is required

### AddVote
 - Creates a new vote
 - POST: api/votes/addVote
 - Request body: { "pid": "pid", "isPost": "isPost", "type": "type", "uid": "uid" }
 - Response: returns the vote object created from the new vote
 - Status codes:
   - 200 OK
   - 400 Bad Request: 
      - Request body is required and must contain the post/comment's id and the user's id
      - Failed to create vote object
   - 404 Not Found: User not found

### RemoveVote
 - Remove the corresponding vote by pid and uid
 - DELETE: api/votes/removeVote?uid={uid}&pid={pid}&type={type}
   - uid: The uid of the current user
   - pid: The pid of the post or comment
   - type: Whether it's removing an upvote/downvote
 - Response: returns whether the deletion was successful
   - true/false
 - Status codes:
   - 200 OK
   - 400 Bad Request: User id and post/comment id is required
