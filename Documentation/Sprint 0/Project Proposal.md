# yAPP
A place to yap your heart out.

## Dev Team (*"The Searching Sisters"*)

- Cynthia Calvo

- Kelly Villamayor

- Qiwen Zhang

- Tara Boulanger

## Project Summary and Vision

yAPP aims to provide users, also known as Yappers, a safe and secure space to share their thoughts, ask questions to the general public, and open up to their friends, all anonymously. On traditional social media platforms, actions such as posting, commenting, or viewing are all tied to our accounts, leaving our identities vulnerable to being traced or even exposed. yAPP's anonymity allows Yappers to separate their lives from their online personas. 

Yappers will have accounts that they can use to anonymously post questions to the community of Yappers. This way, they can feel comfortable freely expressing themselves without fear of their peer’s expectations. While anyone in the Yapper community can see and interact with these questions, the poster can be sure they’ll be safe from having that post tied back to their personal life.
 
Yappers will be able to interact with anonymous posts in various ways. This gives Yappers a choice in how they want to engage with posts, including both question posts and replies to these posts. These interactions can be in the form of upvoting and downvoting, which allows Yappers to show their approval or agreement with a post with an upvote or their disagreement with a post with a downvote.

Yappers can be rewarded with various achievements for interactions with other Yappers and for using the application frequently. This gives Yappers gratification for their interactions on our application to mimic in-person interaction. Additionally, this incentivizes Yappers to continue using our application in the future and also incentivizes them to use the application multiple times a day.

With their accounts, Yappers can keep track of achievements, posts they've interacted with and posts they've written to both the community and their friends. They'll also be able to make connections with other Yappers by adding them as friends on their account, allowing them to

In addition to making anonymous question posts, Yappers can also pose questions to their friends once a day. These posts can either be anonymous or tied to their account, which Yappers can decide on a post-by-post basis. Yappers can see their friend's personal posts along with any personal posts they make in a calendar for easier organization.

We will make our application available for multiple platforms, including a website, desktop app, and IOS and Android app. By providing support for these various platforms, we allow more people to join our community of Yappers. This will also allow Yappers to access their account from various devices to stay engaged with their friends and our community of Yappers.

## Core Features, User Stories, and Acceptance Criteria

#### Posting

- As a Yapper, I want to anonymously ask questions to the general public so I can get multiple opinions.

    - Given that I am a registered Yapper, when I open the “make a post” page, then the system should show me an editor to create a post. When I am done writing my post and clicking the “make post” button, then the system should reassure me I made the post, and then show me where my post lies in the feed.

- As a Yapper, I want to publish one friends-only post per day to update/ask my friends indirectly. Each post can either be anonymous or tied to my account based on my preference.

    - Given that I am a registered Yapper, when I open the “make a post” page, then the system should show me an editor to create a post. I want to make this post only for my friends, and when I click on the toggle button to make it only for my friends, then the system should change the colour of the editor to tell me it’s a friends-only post. When I finish editing the post and click on the “post to friends” button, then the system should reassure me that the post was created successfully, and then show me my diary page where this post should be.

- As a Yapper, I want to be able to see all the anonymous posts I’ve made to the Yapper community.

    - Given that I am a registered Yapper, when I open the “my posts” page, then the system should bring me to a timeline/list where it will show all of the posts that I’ve made in the past.

#### Interactions 

- As a Yapper, I want to be able to view other public posts. 

    - Given that I am a registered Yapper, when I open the app, then the system should show me a timeline/list where it will show the most recently-created posts.

- As a Yapper, I want to be able to upvote posts.

    - Given that I am a registered Yapper, when I click on a post, then the system should show me all of the comments listed beneath it. I want to positively rate this post/comment, and when I click on the “upvote” button, then the system should fill in the button to show that I have successfully rated the post.

- As a Yapper, I want to be able to downvote posts.

    - Given that I am a registered Yapper, when I click on a post, then the system should show me all of the comments listed beneath it. I want to negatively rate this post/comment, and when I click on the “downvote” button, then the system should fill in the button to show that I have successfully rated the post.

- As a Yapper, I want to be able to reply to other public posts.

    - Given that I am a registered Yapper, when I click on a post, then the system should show me all of the comments listed beneath it. I want to add my own comment to the post, and when I click on the “comment” button, then the system should bring up the comment editor. When I fill out my comment and click submit, then the system should show that I have successfully commented and show me where my comment lies in the thread.

#### Personal Calendar

- As a Yapper, I want to be able to view my diary entries through a personal calendar for easier access and tracking.

    - Given that I am a registered Yapper, when I click on my calendar, then the system should show me all of my diary entries that I’ve made for a given time period that I can change.

- As a Yapper, I want to be able to see my friends’ diary entries anonymously through the calendar view (i.e., the original poster won’t know who has seen their upload). 

    - Given that I am a registered Yapper, when I click on my calendar, then the system should show me all of my diary entries that I’ve made for a given time period that I can change. When I click on the “friend view” toggle, then the system should show all of the diary entries that my friends have made, and when they made them.

#### Hidden Achievement System

- As a Yapper, I want to be rewarded with an achievement if my post is highly upvoted.

    - Given that I am a registered Yapper, when one of my posts passes a certain upvote threshold, then the system should show me that my post did well by giving me an achievement. Given that I want to find that achievement, when I click on my “achievements” page, then the system should show me a list of the achievements that I have made and which posts helped me achieve them.

- As a Yapper, I want to be rewarded with an achievement if my post is highly downvoted.

    - Given that I am a registered Yapper, when one of my posts passes a certain downvote threshold, then the system should show me that my post did poorly by giving me an achievement. Given that I want to find that achievement, when I click on my “achievements” page, then the system should show me a list of the achievements that I have made and which posts helped me achieve them.

- As a Yapper, I want to be rewarded with an achievement if my post garners a lot of traffic.

    - Given that I am a registered Yapper, when one of my posts passes a certain number of views, then the system should show me that my post gained a lot of attention by giving me an achievement. Given that I want to find that achievement, when I click on my “achievements” page, then the system should show me a list of the achievements that I have made and which posts helped me achieve them.

- As a Yapper, I want to be rewarded with an achievement if I'm a frequent poster.

    - Given that I am a registered Yapper, when I make a lot of concurrent posts, then the system should show me that I’ve been posting a lot by giving me an achievement. Given that I want to find that achievement, when I click on my “achievements” page, then the system should show me a list of the achievements that I have made and how I achieved them.

#### Profile Management

- As a new user, I want to create a Yapp account. 

    - Given that I am not a registered Yapper, when I first open yAPP, then the system should show me a login page with a sign-up option. When I click on the sign-up option, then the system should bring me to a page where I can create an account. When I finish creating the account, the system should then notify me that I successfully created the account and send me back to the login page.

- As a Yapper, I want to log in to my account.

    - Given that I am a registered Yapper, when I open yAPP, then the system should show me a login page. When I enter my username and password, then the system should bring me to my home page.

- As a Yapper, I want my activity to be private and visible only to me, to prevent being identified by others. 

    - Given that I am a registered Yapper, when I use yAPP and interact with posts, then the system should hide my username from the post for people other than me.

- As a Yapper, I want to be able to make friends with other fellow Yappers through an identifier.
    - Given that me and my friend are registered Yappers, when I click on the “Add friend” button, then the system should bring me to a new page where I can enter my friend’s username, then the system should show me an option to add them. When I click on the “add” button, then the system should show me that I successfully sent a request to my friend. When my friend checks their friend invitations, then they should be able to see my invitation waiting for them.

#### Yapping Capacity

- Manage a user pool of 1,000 monthly active users (i.e., those who sign-up, sign-in, change passwords, update accounts, etc.)

- As a new Yapper, I should be able to create a public post within 5 minutes.

- As a new Yapper, I should be able to create a diary entry within 5 minutes.

## Trimmable Features, User Stories, and Acceptance Criteria

#### Notifications

- As a Yapper, I want to receive notifications when one of my friends makes a diary entry.

    - Given that I am a registered Yapper, when one of my friends makes a diary entry, then the system should send me a notification of a new diary entry waiting to be read.

- As a Yapper, I want to receive notifications when I get a new achievement.

    - Given that I am a registered Yapper, when I reach an achievement, then the system should send me a notification of my accomplishment.

- As a Yapper, I want to receive a notification when I get a new friend request.

    - Given that I am a registered Yapper, when someone sends me a friend request, then the system should send me a notification that I have a new friend request waiting for me.

- As a Yapper, I want to receive daily notifications reminding me about yAPP.

    - Given that I am a registered Yapper, when I haven’t opened the app in over a day, then the system should send me a notification reminding me to check my feed.

#### Post Tags

- As a Yapper, I want to add relevant tags to posts I make to the general Yapper community.

    - Given that I am a registered Yapper, when I create a post, then the system should show me an option where I can add specific tags to my post. When the post is published, then I should be able to see my post with the associated tag I put on it.

- As a Yapper, I want to be able to search for posts with specific tags.

    - Given that I am a registered Yapper, when I open the search menu, then the system should allow me to select a custom tag by typing it in. When I finish adding what tags to filter by, then I should be able to see all of the posts that match my criteria.

- As a Yapper, I would like to filter out posts with specific tags.

    - Given that I am a registered Yapper, when I am looking at my feed, then the system should allow me to filter out posts with certain tags so that I don’t need to see them.

## Tools and Technology

We selected ASP.NET for our backend development due to its alignment with current industry trends, robust endpoint management capabilities, and strong security features. The integration with Swagger in ASP.NET streamlines the process of generating models for front-end use, which boosts development efficiency. Additionally, unit testing in C# with MOCK is simpler and more straightforward compared to Python, especially regarding setup and execution in a CI/CD pipeline.

For the front-end, we chose Vue.js with TypeScript to ensure a well-structured and maintainable web application. TypeScript offers improved structure and maintainability over JavaScript. We selected Tailwind CSS as our CSS library because of its flexibility in creating customized user interfaces. Although Vue.js typically uses NPM as its package manager, we opted for PNPM due to its efficiency, lightweight nature, and faster performance. For our mobile application, we are using .NET MAUI, which supports both iOS and Android platforms and integrates seamlessly with ASP.NET and AWS Lambda, allowing for effective local testing.

Our application will be integrated with Amazon Web Services. We anticipate that, given our project setup and personal experience, we will remain within the free tier limits. Potential costs are expected mainly from CloudWatch and the database. AWS Amplify will manage a significant portion of the deployment process and will integrate with Cognito User Pool to simplify user management. API Gateway will secure our endpoints, CloudWatch will assist with debugging, and CloudFront will handle domain provisioning for our application. We would like to use the Dynamo DB because it is suitable for less complex data objects and most importantly, compared to the RDS, the price is more reasonable.

## Architecture

![image](./yAPP%20HLA.png)

 Using the various services and technologies mentioned in the previous section, we have opted to use an n-tier architecture. This is due to the fact that the majority of our application will be hosted using AWS, which requires many different interactions between services that cannot be contained within a single tier. With this in mind, we have separated our application into various layers. We’ll be using a Presentation, Logic, Data, and Objects layer.

For the Presentation layer, you can see that the client is listed underneath it. We plan on keeping most, if not all, of the presentation code on the client side, which allows us to keep the majority of the logic on the server-side. This means that our clients will all be thin clients. With this, we will be able to support various platforms without introducing tedious churn and reducing the ops burden of code management.

To facilitate deployment of our application, we will be using other various AWS software that did not fit into a singular layer within our architecture. The various technologies that were chosen to be separated into the different layers can be found in the previous section.

This proposed architecture would work for our use case because it allows for us to abstract between the different layers without losing out functionality. We want to keep our clients thin since this is an application that can be downloaded onto mobile devices, where there are more space constraints. This also allows for us to be able to make changes on the server’s side without needing to touch the client at all. Additionally, using an n-tier architecture allows for us to interact with the various services provided to us by AWS without restricting ourselves. This opens up an entire realm of possibility for development tools.

## Project Proposal Presentation

The final version of our project proposal can be found [here](./Comp%204350%20Finalized%20Proposal%20Presentation.pdf)!

## Work Division

We’re planning on dividing work for this project by feature. Two people will work on each feature as we aim to complete two features per sprint for the first two sprints. From there, work will be divided through time estimates, aiming to have the time estimates for all the work evenly distributed between all group members. The work on a single feature will be divided in a way that allows each person working on that feature to work on all the layers and tiers of the project to maximize learning for each of us. The initial planning for distributing the work will be done in a shared spreadsheet to allow all of us to work on the rough draft together and make quick changes where needed. Once the work has been distributed, we will use Issues in GitHub attached to our project repository to keep track of what work is completed and in progress. 
