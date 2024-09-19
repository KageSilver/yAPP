# yAPP
*A place to yap your heart out.*

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

## Core Features and User Stories

#### Posting

- As a Yapper, I want to anonymously ask questions to the general public so I can get multiple opinions.

- As a Yapper, I want to publish one friends-only post per day to update/ask my friends indirectly. Each post can either be anonymous or tied to my account based on my preference.

- As a Yapper, I want to be able to see all the anonymous posts I’ve made to the Yapper community

#### Interactions 

- As a Yapper, I want to be able to view other public posts. 

- As a Yapper, I want to be able to upvote posts.

- As a Yapper, I want to be able to downvote posts.

- As a Yapper, I want to be able to reply to other public posts.

#### Personal Calendar

- As a Yapper, I want to be able to view my diary entries through a personal calendar for easier access and tracking.

- As a Yapper, I want to be able to see my friends’ diary entries anonymously through the calendar view (i.e., the original poster won’t know who has seen their upload). 

#### Hidden Achievement System

- As a Yapper, I want to be rewarded with an achievement if my post is highly upvoted.

- As a Yapper, I want to be rewarded with an achievement if my post is highly downvoted.

- As a Yapper, I want to be rewarded with an achievement if my post garners a lot of traffic.

- As a Yapper, I want to be rewarded with an achievement if I'm a frequent poster.

#### Profile Management

- As a new user, I want to create a Yapp account. 

- As a Yapper, I want to log in to my account.

- As a Yapper, I want my activity to be private and visible only to me, to prevent being identified by others. 

- As a Yapper, I want to be able to make friends with other fellow Yappers through an identifier.

#### Yapping Capacity

- Manage a user pool of 1,000 monthly active users (i.e., those who sign-up, sign-in, change passwords, update accounts, etc.)

- As a new Yapper, I should be able to create a public post within 5 minutes.

- As a new Yapper, I should be able to create a diary entry within 5 minutes.

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

The final version of our project proposal can be found [here](./Comp%204350%20Finalized%20Proposal%20Presentation.pdf).
