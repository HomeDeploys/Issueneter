## Test plan prompt

Write test plan for @file tests. Write all cases as single sentences like 'When <Action>, service should <Outcome>". 
Cases should be grouped by meaning and have h2 header with short description
Cases with same behavior but with different variations of one parameter must be one parametrized case
Cases should contains minimum required count of happy pathes and case for every error path
Write result to markdown file

## Generate tests

Generate unit tests for <> according to plan @plan
Use codestyle from @AiCodestyle
Use Xunit, FluentAssertions and Moq
Do not add regions