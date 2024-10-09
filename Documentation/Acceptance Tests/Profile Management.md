# Profile Management Acceptance Tests

## Mobile
 - Acceptance tests for our Android app are written in JUnit and run through Android Studio.
 - These acceptance tests can be found under yAppMobile > app > src > androidTest > tests > ProfileManagementAT.java

## Website
 - Acceptance tests for our website are written instructions and run manually by a person using the website.

### Account Creation
1. Navigate to the yAPP website
2. Select the "Create Account" tab at the top of the login panel
3. Enter account details
    - Username needs to be the same as the email address (this will be fixed later)
    - Enter and confirm your password
    - Enter a valid email address you have access to, as you will need to retrieve the confirmation code for a later step
4. Select the "Create Account" button at the bottom of the panel
5. Enter the confirmation code abtained from the email address entered in step 3
    - If the confirmation code was lost, select the "Resend Code" button and repeat this step
6. Select the "Confirm" button
7. Done!

### Account Login
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Username, which will be the email address (for now)
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Done!

### Edit Account Details
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Username, which will be the email address (for now)
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Select the "Go to Dashboard" button
6. Select the "Account Settings" tab at the top-right of the panel
7. To change your password:
    - Enter your old password in the textfield below "Old Password"
    - Enter your new password in the textfield below "New Password"
    - Select the "Submit Changes" button at the bottom-left of the panel
    - A pop-up should appear notifying you that your password has been changed
8. Done!

### Make Friends
1. Navigate to the yAPP website
2. If not already selected, select the "Sign In" tab at the top of the panel
3. Enter account details
    - Username, which will be the email address (for now)
    - Password
4. Select the "Sign In" button at the bottom of the panel
5. Select the "Go to Dashboard" button
6. To make a friend request:
    - Select the "+ Add Friend" button on the top-right of the screen
    - Enter in your friend's UUID in the textfield below "Enter in their UUID:"
    - Select "Send Friend Request"
    - A pop-up should appear verifying the friend request has been sent
7. To accept/decline a friend request:
    - Select the "My Friend Requests" tab on the top of the panel
    - All pending friendships will be listed here
    - To accept a friend request, select the "Accept" button correlating to a specific user
    - To decline a friend request, select the "Decline" button correlating to a specific user
8. To view friends:
    - Select the "My Friends" tab on the top of the panel
    - All friends will be shown here
9. Done!

