# yApp

## Test Plan
 - The [test plan](Test%20Plan.md) for sprint 1 goes over

## Unit/Integration/Acceptance Tests
### Backend
#### API layer coverage
![image](./Images/APIunitTestCoverage.png)
 - Every method in our API layer has been tested up to at least 80% of the lines in the methods.

 #### Logic layer line coverage
 ![image](./Images/TotalUnitTestCoverage.png)
 - Every element of our logic layer has at least 80% line coverage.

 #### Integration test coverage
 ![image](./Images/IntegrationTestCoverage.png)
 - Our integration tests cover all of our logic layer classes, most of which have over 80% line coverage. Some classes have less than 80% line coverage due to it not being feasible to test every single exception within each method in these classes in the integration tests, though each method is tested within all of those classes.

### Frontend
#### Frontend logic layer
 - Our frontend has no logic layer, so we did not need to write any tests for that.

#### Acceptance tests

### Other information on our testing approach
 - Due to the way we set up our project, we need to run system tests to test the integration of the logic/api layer with the database. We called these integration tests, which is what the information in our integration tests section is referring to.
 - To have our integration tests run in a timely manner, we re-use the same test user in our live database. We do this by creating a user in one integration test and deleting it in another, which we ensure runs smoothly by enforcing the order these integration tests are executed. We start with the test that creates the test user, then any of the other tests that also use that user, and finally we run the test that will delete the user.
 - We are only able to provide a line coverage report as Rider, the IDE we’re using for development, does not offer class or method coverage reports.

## Test Importance
### Our 3 most important unit tests
1. 
2. 
3. 

### Our 3 most important integration tests
1. 
2. 
3. 

### Our 3 most important acceptance tests
1. 
2. 
3. 

## Reproducible Environments
#### Was the documentation clear enough to run their software? Did you get it running? Now long did it take, was it hard?
 - 
#### Could you run the unit tests (SHOW IMAGES)? Did they all work? What about integration tests and other tests?
 - 
#### Were there any issues you found when running the software? Connection issues, other problems, especially with it being a distributed system.
 - 
