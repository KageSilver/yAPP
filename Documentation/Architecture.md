# Architecture

- What the planned [High Level Architecture Diagram](./Sprint%200/yAPP%20HLA.png) is for the project as of sprint 0.
- Here is our updated [High Level Architecture Diagram](./Sprint%203/Images/architectureDiagram.png) as of sprint 3. We have it separated into three different layers; the Presentation Layer where all of the UI code resides, the Logic Layer where all of the computation code and authentication resides, and the Data Layer which contains the database we'll be using.
- This final [Architecture Diagram](./Sprint%203/Images/architectureDiagram.png) for sprint 3 introduces our new features. You can view the architecture diagram from sprint 2 [here](./Sprint%202/Images/architectureDiagram.png). You can also view the architecture diagram from sprint 1 [here](./Sprint%201/Images/architectureDiagram.png).
    - Our presentation layer consists of two different UIs, a website and an Android mobile app. In this layer, we have both a presentation tier and fetching tier, though these two are built into the same files due to the software we are using to build these UIs.
    - Our logic layer is split into two tiers, controllers, which interact with the client through our API, and actions, which interact the database. We also have AWS interactions within our logic layer as we use some 3rd party software to outsource validation.
    - Our objects/models do not exist within a single layer, as they are passed between the layers of the application.
    - Our data layer consists of a DynamoDB database hosted with AWS.