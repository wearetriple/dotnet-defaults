# Azure DevOps Service Connections Terraform setup

Since we always setup our Azure DevOps service connections the same way, we should
capture this setup in code and automate its configuration. Just like cloud resources,
service connections should be captured in IaC and stored in a repository.

## General setup

1. Create or update a separate repository for storing these terraform templates. 

    Using a separate repository helps with keeping these changes separate from
    regular code changes, and prevents Azure DevOps changes becoming stuck in
    PRs with unrelated code. It also allows for grouping all Azure DevOps templates
    together (e.g. also for front-end pipelines). 

    Copy the `terraform/service-connections` folder into that separate repository. 

2. Configure the backend config in `providers.tf`. Use a storage account to store
the terraform state into. Since the state contains service principals keys, it should
never be committed in the repo.

3. Update the `locals.tf` and `variable.tf` files and fill out the constant variables.

4. Plan & apply the terraform template on your local machine. These terraform templates
are manually applied since they require a PAT  with extended privileges.

    a. Create a PAT with the following scopes:

    - Project and Team: `Read`
    - Service Connections: `Use and manage`

    b. Initialize the terraform template by running `terraform init`.

    c. Create the a new environment via `terraform workspace new {env_name}`.

    d. Apply the template by running `terraform apply`.
