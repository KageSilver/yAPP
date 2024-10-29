# Test Plan
## Overview

### Functionality testing
 - This form of testing aims to ensure our software works as intended.
 - For our application, this is achieved through unit, integration, and system tests.

### Usability testing
 - This form of testing aims to ensure our software is easy to use and provides a good user experience. 
 - For our application, this is achieved through automated and manual acceptance tests.

### Security testing
 - This form of testing aims to uncover potential problems with the security within our software.
 - We have not implemented anything to test for this...yet.

### Performance testing
 - This form of testing aims to ensure the efficiency and capacity of our software.
 - We have not implemented anything to test for this...yet.

## Tools
 - We will be using xUnit to write our unit and integration tests.
 - We will be using Moq to mock components of our project for unit testing, including other classes and our database.

## Feature Testing
 - We will have test plans for each individual feature, as the nature of each varies and thus requires different forms of testing.
 - In general, we will be testing average cases, edge cases, and cases that should cause exceptions.

### Profile Management
#### Unit tests
 - Test average cases and expected exceptions for FriendshipActions with unit tests for the following methods:
    - CreateFriendship()
    - UpdateFriendshipStatus()
    - GetAllFriends()
    - GetFriendship()
    - DeleteFriendship()
 - Test average cases and expected exceptions for FriendshipActions with unit tests for the following methods:
    - SendFriendRequest()
    - UpdateFriendRequest()
    - GetFriends()
 - Test average cases and expected exceptions for CognitoActions with unit tests for the following methods:
    - CreateUser()
    - DeleteUser()
    - GetUserById()
    - GetUser()
    - UpdateUser()
 - Test average cases and expected exceptions for UserController with unit tests for the following methods:
    - GetUserById()
    - GetUser()
    - UpdateUser()
#### Integration tests
 - Test average cases and some expected exceptions for FriendshipController with integration tests for api endpoints:
    - GET: api/friends/getFriendsByStatus?userName={username}?status={status}
    - POST: api/friends/friendRequest
    - PUT: api/friends/updateFriendRequest
 - Test average cases and some expected exceptions for UserController with integration tests for api endpoints:
    - GET: api/cognito/getUserById?id={id}
    - GET: api/cognito/getUserByName?userName={username}
    - POST: api/users/updateUser
#### Acceptance tests
 - For each of the following user stories that belong to this feature, write two manual acceptance tests, one for the website and one for the mobile app:
    - As a new user, I want to create a Yapp account.
    - As a Yapper, I want to log in to my account.
    - As a Yapper, I want to change my account details to my liking.
    - As a Yapper, I want to be able to make friends with other fellow Yappers through an identifier.

### Posting
#### Unit tests
 - Test average cases and expected exceptions for PostActions with unit tests for the following methods:
    - CreatePost()
    - DeletePost()
    - UpdatePost()
    - GetPostById()
    - GetPostsByUser()
    - GetRecentPosts()
 - Test average cases and expected exceptions for PostController with unit tests for the following methods:
    - CreatePost()
    - DeletePost()
    - UpdatePost()
    - GetPostById()
    - GetPostsByUser()
    - GetRecentPosts()
#### Integration tests
 - Test average cases and some expected exceptions for PostController with integration tests for api endpoints:
    - POST: api/posts/createPost
    - DELETE: api/posts/deletePost?pid={pid}
    - PUT: api/posts/updatePost
    - GET: api/posts/getPostById?pid={pid}
    - GET: api/posts/getPostsByUser?userName={userName}&diaryEntry={diaryEntry}
    - GET: api/posts/getRecentPosts?since={since}&maxResults={maxResults}
#### Acceptance tests
 - For each of the following user stories that belong to this feature, write two manual acceptance tests, one for the website and one for the mobile app:
    - As a Yapper, I want to anonymously ask questions to the general public so I can get multiple opinions.
    - As a Yapper, I want to be able to view other public posts.
    - As a Yapper, I want to be able to see all the anonymous posts I’ve made to the Yapper community.

### Interactions with Posts
#### Unit tests
 - Test average cases and expected exceptions for CommentActions with unit tests for the following methods:
    - CreateComment()
    - GetCommentById()
    - GetCommentsByUid()
    - GetCommentsByPid()
    - DeleteComment()
    - DeleteComments()
    - UpdateComment()
 - Test average cases and expected exceptions for CommentController with unit tests for the following methods:
    - CreateComment()
    - GetCommentById()
    - GetCommentsByUid()
    - GetCommentsByPid()
    - DeleteComment()
    - UpdateComment()
#### Integration tests
 - Test average cases and some expected exceptions for CommentController with integration tests for api endpoints:
    - GET: api/comments/getPostByCid?cid={cid}
    - GET: api/comments/getCommentById?cid={cid}
    - GET: api/comments/getCommentsByUser?uid={uid}
    - GET: api/comments/getCommentsByPid?pid={pid}
    - POST: api/comments/createComment
    - PUT: api/comments/updateComment
    - DELETE: api/comments/deleteComment?cid={cid}
#### Acceptance tests
 - For each of the following user stories that belong to this feature, write two manual acceptance tests, one for the website and one for the mobile app:
    - As a Yapper, I want to be able to reply to other public posts.
    - As a Yapper, I want to be able to edit and delete posts and comments I make.

### Personal Calendar
#### Unit tests
 - Test average cases and expected exceptions for PostActions with unit tests for the following methods:
    - GetDiariesByUser()
    - GetDiariesByFriends()
 - Test average cases and expected exceptions for PostController with unit tests for the following methods:
    - GetDiariesByUser()
    - GetDiariesByFriends()
#### Integration tests
 - Test average cases and some expected exceptions for CommentController with integration tests for api endpoints:
    - GET: api/posts/getDiariesByUser?uid={uid}&current={current}
    - GET: api/posts/getDiariesByFriends?uid={uid}&current={current}
#### Acceptance tests
 - For each of the following user stories that belong to this feature, write two manual acceptance tests, one for the website and one for the mobile app:
    - As a Yapper, I want to publish one friends-only post per day to update/ask my friends indirectly. Each post can either be anonymous or tied to my account based on my preference.
    - As a Yapper, I want to be able to view my diary entries through a personal calendar for easier access and tracking.
    - As a Yapper, I want to be able to see my friends’ diary entries anonymously through the calendar view.

## Regression Tests
 - Nothing here...yet.
