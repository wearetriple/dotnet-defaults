// Pipelines

resource "azuredevops_build_definition" "pipeline" {
  for_each = { for key, value in local.pipeline_groups : key => value }

  project_id = data.azuredevops_project.azdo_project.id
  name       = format("%s - %s", each.value.group_name, each.value.pipeline_type)
  path       = format("\\%s", each.value.group_name)

  ci_trigger {
    use_yaml = true
  }

  repository {
    repo_type   = "TfsGit"
    repo_id     = data.azuredevops_git_repository.git_repo.id
    branch_name = data.azuredevops_git_repository.git_repo.default_branch
    yml_path    = format("cicd/%s/%s", each.value.group_name, var.pipeline_filenames[each.value.pipeline_type])
  }
}
