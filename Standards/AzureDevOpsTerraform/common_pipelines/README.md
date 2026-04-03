# Common pipelines

This folder contains some common pipelines we employ in every project. Most pipelines
can be simply copy-pasted into the `cicd/common` folder without any changes, except
for `pr-verify-pr.yml`.

## pr-verify-pr.yml

This pipeline is used to validate the title of a PR. This title should start with
the ticket name, which is unique for each project. Update the `$ticketPattern` to
match your projects tickets.
