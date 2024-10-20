# Posting Acceptance Tests

## Mobile
 - Acceptance tests for our Android app are written in JUnit and run through Android Studio.
 - These acceptance tests can be found under yAppMobile > app > src > androidTest > tests > PostingAT.java

## Website
 - Acceptance tests for our website are written instructions and run manually by a person using the website.

 ### Creating New Post
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Email
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Select the "Create Post" button on the panel on the right of the screen
6. Enter a post title in the textfield under "Title:"
7. Enter the post contents in the textfield under "Content:"
8. To discard the post, select the "Discard" button at the bottom of the panel
9. To create the post, select the "Create Post" button at the bottom of the panel
10. A pop-up should appear confirming you have created a new post
11. Done!

 ### Viewing User's Own Posts
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Email
    - Password
4. Select the "Profile" button on the panel on the right of the screen
5. Select the "My Posts" tab on the panel if not already selected
6. Any public posts you make will appear here
7. Done!

 ### Viewing Public Posts
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Email
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Select the "Home" button on the panel on the right of the screen if not already selected
6. Public posts made by any user will appear here, sorted by posts made most recent
7. Done!
