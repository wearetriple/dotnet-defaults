> NOTE: Copy this readme into the target repository.

# CICD Pipelines

## TL;DR

To create branch policies/pipelines for a new repository, run:

```powershell
terraform init
terraform workspace new {repo_name}
terraform apply -var-file="variables/{repo_name}.tfvars" -var "pat={personal access token}"
```

To update branch policies or to add a new pipeline, run:

```powershell
terraform workspace select {repo_name}
terraform apply -var-file="variables/{repo_name}.tfvars" -var "pat={personal access token}"
```

Run `terraform init` if terraform complains about it being run for the first time. 
The PAT should have at least quite some privileges which are stated under Initialization.

Make sure you also commit the .tfstate when pushing new terraform changes to the repo.

## Installation

Follow these steps to install Terraform on MacOS, Linux and Windows:

### MacOS

You can use Homebrew to install Terraform on MacOS.

```bash
brew tap hashicorp/tap
brew install hashicorp/tap/terraform
```

### Linux

You can download a binary package for Terraform from the [releases page](https://releases.hashicorp.com/terraform/) 
or install it from a package manager. See instructions for APT (Debian package 
manager) below.

```bash
wget -O- https://apt.releases.hashicorp.com/gpg | sudo gpg --dearmor -o /usr/share/keyrings/hashicorp-archive-keyring.gpg
echo "deb [signed-by=/usr/share/keyrings/hashicorp-archive-keyring.gpg] https://apt.releases.hashicorp.com $(lsb_release -cs) main" | sudo tee /etc/apt/sources.list.d/hashicorp.list
sudo apt update && sudo apt install terraform
```

### Windows

You can use Chocolatey to install Terraform on Windows or [download the binary](https://developer.hashicorp.com/terraform/downloads).

```bash
choco install terraform
```

You can also download the latest terraform.exe, store it somewhere and add it to PATH.

If you run into any installation issues, please refer to the [official documentation](https://developer.hashicorp.com/terraform/downloads)

## Initialization

You also require an [Azure DevOps PAT token](https://dev.azure.com/wearetriple/_usersSettings/tokens) 
that has the appropriate access rights configured, including the following scopes:

- Build: `Read & Execute`
- Code: `Read`
- Environment: `Read & Manage`
- Graph: `Read & Manage`
- Project and Team: `Read`
- Security: `Manage`

You can set its secret value in the `local.auto.tfvars` file.

```bash
cp local.auto.tfvars.example local.auto.tfvars
vi local.auto.tfvars
```

After these are set, you can initialize your Terraform workspace.

```bash
terraform init
```

This command initializes your Terraform workspace, which includes downloading the 
Azure DevOps provider, setting up a backend on Azure, and configuring other necessary 
settings.

## Executing

Before executing the scripts, you should check what changes Terraform will perform 
with:

```bash
terraform plan
```

If the plan looks good, you can apply the changes with:

```bash
terraform apply
```

This command will create or update your build definitions, environments, and pipeline 
permissions.

## Importing

The only resources which we are able to import are `azuredevops_environment` and 
`azuredevops_build_definition`. See the example below:

```hcl
terraform import 'azuredevops_environment.environment["TEST"]' "backend/67"
terraform import 'azuredevops_build_definition.pipeline["example - development"]' "backend/42"
```

Importing via PowerShell is a nightmare as the quotes are elided. The import really
needs those quotes, so its better to import using WSL.
