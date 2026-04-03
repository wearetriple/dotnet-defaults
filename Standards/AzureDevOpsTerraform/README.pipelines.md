# Azure DevOps Pipelines Terraform setup

Since we always set up our Azure DevOps pipelines the same way, we should capture
this setup in code and automate its configuration. Just like cloud resources, pipelines 
and branch policies should be capture in IaC and stored in a repository.

## General setup

1. Create or update a separate repository for storing these terraform templates. 

    Using a separate repository helps with keeping these changes separate from
    regular code changes, and prevents Azure DevOps changes becoming stuck in
    PRs with unrelated code. It also allows for grouping all Azure DevOps templates
    together (e.g. also for front-end pipelines). 

    Copy the `terraform/pipelines` folder into that separate repository. Copy the 
    contents of `common_pipelines` folder into the `cicd/common` folder of your 
    code repository.

2. Configure the backend config in `providers.tf`. Use a storage account to store
the terraform state into, or commit the state into DevOps. Keeping the state of DevOps
stored in DevOps is fine (there are not secrets stored in that state file), but 
if the project (will) have a storage account to store state files of other templates, 
use that storage to store DevOps state too.

3. Create a new variable file. Each repository in Azure DevOps should have
its own file. See `example.tfvars` as example. Configure the following:

    a. `azdo_config`: General Azure DevOps config.

    b. `pipeline_type_configs`: Types of pipelines within the project. By default 
    there are 3 types defined, `regular`, `main_only` and `common`. The `common` variant
    is used to define all pipelines that handle cross-cutting concerns, like format
    validation.

    c. `pipeline_filenames`: Maps the name of a pipeline type to the corresponding
    yaml file name.

    d. `environments_configs`: Environments within the project. These environments
    are referenced by the `environment` property of `jobs.deployment`s in 
    [yaml pipelines](https://learn.microsoft.com/en-us/azure/devops/pipelines/yaml-schema/jobs-deployment?view=azure-pipelines).
    
    e. `environments_with_xlock`: Configures which environments have exclusive
    locks. Exclusive locks prevent concurrent deployment.

    f. `environments_with_single_approver`: Configures which environment require
    approval from `azdo_config.single_approver_team` before deployment is allowed
    to start.

    g. `pipeline_group_configs`: Configures the pipelines. For example:

    ```hcl
    pipeline_group_configs = {
        common = {
            pipeline_type = "common"
            path_filters = {
                bicep      = ["*.bicep"]
                terraform  = ["*.tf"]
                formatting = ["/src/*.cs"]
                pr         = ["/*"]
            }
        }
        api-v1 = {
            pipeline_type   = "regular"
            path_filters = {
                pr = ["/src/Client.Api*"]
            }
        }
    }
    ```
    
    h. `branch_strategy`: Choose between the following branch strategies:

    - **main_only**: All branches are typically merged into main, via squash & rebase.
    - **dev_branches**: Feature branches are merged into dev-branches, via squash
    & rebase, and dev-branches are merged into main, via rebase & fast-forward.

4. Plan & apply the terraform template on your local machine. These terraform templates
are manually applied since they require a PAT with extended privileges.

    a. Create a PAT with the following scopes:

    - Build: `Read & Execute`
    - Code: `Read`
    - Environment: `Read & Manage`
    - Graph: `Read & Manage`
    - Project and Team: `Read`
    - Security: `Manage`

    b. Initialize the terraform templates by running `terraform init`. Run
    `terraform init -backend=false` when storing the state in the repo itself.

    c. Create a new workspace for the repo and select it using:
    `terraform workspace new {repo_name}`. Use `terraform workspace select {repo_name}`
    when the workspace has been created.

    d. Apply the template while selecting the correct variable file using:
    `terraform apply -var-file="variables/{repo_name}.tfvars" -var "pat={personal access token}"`.
