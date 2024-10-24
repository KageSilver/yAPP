# Profile Management Acceptance Tests
 - Acceptance tests for our website and mobile app are written instructions and run manually by a person using the website or mobile app.

 ## Website Acceptance Tests

### Account Creation - Website
1. Navigate to the yAPP website
2. Select the "Create Account" tab at the top of the login panel
3. Enter account details
    - Enter and confirm a valid email address you have access to, as you will need to retrieve the confirmation code for a later step
    - Enter and confirm your password
4. Select the "Create Account" button at the bottom of the panel
5. Enter the confirmation code abtained from the email address entered in step 3
    - If the confirmation code was lost, select the "Resend Code" button and repeat this step
6. Select the "Confirm" button
7. Done!

### Account Login - Website
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Email
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Done!

### Edit Account Details - Website
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Username, which will be the email address (for now)
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Select the "Profile" button on the panel on the right of the screen
6. Select the "Settings" button on the bottom of the panel on the right of the screen
7. To change your password:
    - Enter your old password in the textfield below "Old Password"
    - Enter your new password in the textfield below "New Password"
    - Select the "Submit Changes" button at the bottom-left of the panel
    - A pop-up should appear notifying you that your password has been changed
8. Done!

### Send a Friend Request - Website
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Email
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Select the "Profile" button on the panel on the right of the screen
6. Select the icon at the top right of the screen, just left of the navigation panel
7. Select the "Add a Friend!" button on the top right f the screen
8. Enter in your friend's UUID in the textfield below "Enter in their UUID:"
9. Select the "Send Request" button
10. A pop-up should appear verifying the friend request has been sent
11. Done!

### Accept/Decline a Friend Request - Website
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Email
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Select the "Profile" button on the panel on the right of the screen
6. Select the icon at the top right of the screen, just left of the navigation panel
7. All pending friendships will be listed here
8. To accept a friend request, select the "Accept" button correlating to a specific user
9. To decline a friend request, select the "Decline" button correlating to a specific user
10. Done!

### View Friends - Website
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Email
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Select the "Profile" button on the panel on the right of the screen
6. Select the "My Friends" tab on the top of the panel
7. All friends will be shown here
8. Done!

### Unfriend a Friend - Website
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Email
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Select the "Profile" button on the panel on the right of the screen
6. Select the "My Friends" tab on the top of the panel
7. Select the "Unfollow" button next to the name of the friend you would like to unfriend
8. A pop-up will appear confirming if you want to unfriend this person
    - Select "Yes" to unfollow
    - Select "Cancel" to keep that person as a friend
9. The friend you unfollowed should disappear from the "My Friends" view
10. Done!

## Mobile Acceptance Tests

### Account Creation - Mobile
1. Navigate to the yAPP mobile app
2. You will be redirected to a browser webpage
3. To create a new account, select the "Sign up" text at the bottom below the "Sign in" button
4. Enter account details
    - Enter and confirm a valid email address you have access to, as you will need to retrieve the confirmation code for a later step
    - Enter and confirm your password
5. Select the "Create Account" button at the bottom of the panel
6. Enter the confirmation code abtained from the email address entered in step 3
    - If the confirmation code was lost, select the "Send a new code" button and repeat this step
7. Select the "Confirm Account" button
8. Done!

### Account Login - Mobile
1. Navigate to the yAPP mobile app
2. To sign in, you will be redirected to a browser webpage
3. Enter account details
    - Email
    - Password
4. Select the "Sign in" button at the bottom of the panel
5. Done!

### Edit Account Details - Mobile
- Password resets must be done through the website

### Send a Friend Request - Mobile
1. Navigate to the yAPP mobile app
2. To sign in, you will be redirected to a browser webpage
3. Enter account details
    - Email
    - Password
4. Select the "Sign in" button at the bottom of the panel
5. Select the "Profile" button on the bottom navigation bar
6. Select the person icon on the upper right corner of the screen
7. Select the "Add a friend!" button on the bottom right of the screen
8. Enter in your friend's UUID in the textfield below "UUID:"
9. Select the "Send Request" button at the bottom of the panel
10. A pop-up should appear verifying the friend request has been sent
11. Done!

### Accept/Decline a Friend Request - Mobile
1. Navigate to the yAPP mobile app
2. To sign in, you will be redirected to a browser webpage
3. Enter account details
    - Email
    - Password
4. Select the "Sign in" button at the bottom of the panel
5. Select the "Profile" button on the bottom navigation bar
6. Select the person icon on the upper right corner of the screen
7. To accept a friend request, select the "Accept" button correlating to a specific user
8. To decline a friend request, select the "Decline" button correlating to a specific user
9. If you accepted a friend request, that friend will appear under the "My Friends" tab
10. Done!

### View Friends - Mobile
1. Navigate to the yAPP mobile app
2. To sign in, you will be redirected to a browser webpage
3. Enter account details
    - Email
    - Password
4. Select the "Sign in" button at the bottom of the panel
5. Select the "Profile" button on the bottom navigation bar
6. Select the "My Friends" tab 
7. All friends will be shown here
8. Done!

### Unfriend a Friend - Mobile
1. Navigate to the yAPP mobile app
2. To sign in, you will be redirected to a browser webpage
3. Enter account details
    - Email
    - Password
4. Select the "Sign in" button at the bottom of the panel
5. Select the "Profile" button on the bottom navigation bar
6. Select the "My Friends" tab 
7. Select the "Unfollow" button next to the name of the friend you would like to unfriend
8. A pop-up will appear to confirm if you want to unfriend this person
    - Select "Yes" to unfollow
    - Select "No, I was just playing" to keep that person as a friend
9. The friend you unfollowed should disappear from the "My Friends" view
10. Done!
