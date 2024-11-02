# yAPP // Sprint 2 Worksheet

## Regression Testing
1. We run our regression testing by ...
2. Here are the various links for regression testing.
    - [Here](.) is the regression testing script.
    - Here is the last snapshot of the execution:
    - Here are the results of regression testing:

## Testing Slowdown
We have been able to keep all of our unit tests and our integration tests in our test plan as we haven't experienced any significant slowdown. We have simply added on to our test plan from Sprint 1 rather than creating an entirely new test plan for Sprint 2. All of our tests can be run under a minute in our local deployments and are then run automatically by our CI/CD pipeline. Due to how quickly it can run and the necessity of our tests, we decided not to change our test plan for the different releases.

## Not Testing
Here is our current system diagram:
[System Diagram](./Images/architectureDiagram.png)

Here is what we haven't tested:
* We were unable to automate testing for the presentation layer of our application, and as such, we have written manual acceptance tests. These tests can be executed by an operator to ensure that everything is working as intended.
* We also did not test our models/objects as they as simply structs without any testable functions within them.

And here is the testing for each of the layers in our tiers:
- Presentation:
    Not automatically implemented, so line coverage is not included as a coverage report. However, due to us writing manual tests, we have covered every *intended* use case of our application, covering all of our user stories.
    * Website layer: **Fully tested (80%+)**
    * Mobile layer: **Fully tested (80%+)**

- Logic:
    * AWS Interactions: These are tested with our integration tests as we are interacting diretly with our database on the cloud and with our endpoints. 
        - **Fully tested (80%+)** 
        - [Coverage Report]()
    * Controllers: These are tested with both our integration tests and our unit tests. It acts as the bridge between our cloud infrastructure and the logic with our APIs that are managed in the `Actions` files. 
        - **Fully tested (80%+)**
        - [Coverage Report]()
    * Actions: These are tested simply through unit tests. These execute our API calls and require database mocks for the expected behaviour of our program.
        - **Fully tested (80%+)**
        - [Coverage Report]()

- Data:
    * DynamoDB: This tier/layer is tested through the integration tests for our controllers, but do not have their own dedicated tests. As such, there is no actual coverage report despite its usage being fully covered.
        - **Fully tested (80%+)**

- Objects/Models:
    * These object models are simply models for the database and do not require testing.
        - **Not tested**

## Profiler
- Slowest Endpoint:
    - The slowest endpoint is ____

- Is it fixable:
    - This could probably be fixed by reducing the amount of time that we spend waiting for a response, however, since we're only using the free tier for AWS, we are unable to use the fastest tier and expect our API calls to require more time to load.

- Profiler output:
    - [Here is the output]().

## Last Dash
The main issue that we can see running into for the next sprint is storage issues for our CI/CD. We have already been running into these problems and know that we can come across it in the upcoming sprint as it is soemthing that is out of our control.

As for our code and remaining feature issues we plan on implementing, we are confident that we can complete the work in the allotted time as the majority of the framework has already been built, and we plan on improving our code should we come across any problems once we complete these issues.

## Show Off
- Cynthia:
    Stuff here with commit.
- Kelly:
    Stuff here with commit.
- Qiwen:
    Stuff here with commit.
- Tara:
    I did things.
