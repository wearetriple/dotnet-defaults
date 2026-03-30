data "azuredevops_project" "azdo_project" {
  name = var.azdo_config.project_name
}

data "azuredevops_git_repository" "git_repo" {
  project_id = data.azuredevops_project.azdo_project.id
  name       = var.azdo_config.git_repository
}

data "azuredevops_group" "single_approver" {
  project_id = data.azuredevops_project.azdo_project.id
  name       = var.azdo_config.single_approver_team
}
