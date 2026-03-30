> NOTE: Copy this readme into the target repository.

# CICD Service Connections

## TL;DR

To create service connections for a new repository or environment run:

```powershell
terraform init
terraform workspace new {env_name}
terraform apply -var "pat={personal access token}"
```

To update service connections run:

```powershell
terraform init
terraform workspace select {env_name}
terraform apply -var "pat={personal access token}"
```

Make sure to never commit the .tfstate in the repo and always use a backend.
