# Repository Protocol

## CI/CD Flow

- Branches
	- The `main` branch contains the stable, release-ready product. The application on this branch should always be functional.
	- The `dev` branch serves as the integration point for new features within a sprint. 
	- Each feature is broken down into dev-tasks, which have their own dedicated sub-branches that follow the naming format "`sprint#[#]_issue#[#]`".
- Environments
	- We have three environments to reflect the changes made across these branches: `main`, `dev`, `test`.
- Procedure
	- After completing a task, the respective dev task branch gets merged into the `test` environment for team review
		- The team should be notified before a push onto `test` will occur, as it can overwrite changes made on another branch. 
		- Due to cost limitations, we cannot made an environment per dev task branch. Thus, we chose to coordinate our pushes instead.
	- Once approved, the feature is merged back into the `dev` branch and is reflected in the `dev` environment.
	- At the end of the sprint, the new features in the `dev` environment are merged into `main`.

## Expectations
- Commits
	- Messages follow the format: "`[present tense verb] [subject]`".
	- Developers should commit often enough to keep the messages short.
- Pull as soon as you begin working.
- Pull before you push and ensure everything is working + tested.
	- Once you've confirmed this, notify everyone when you make a push.
	- This push should be reviewed by 2/4 members before another person is allowed to push onto the `dev` environment.

## Release Notes
- We will add release notes before deploying to the `test` environment.
- We use the following format: `devTaskIssue#[#]-FeatureIssue#[#]-devTaskDescription`

