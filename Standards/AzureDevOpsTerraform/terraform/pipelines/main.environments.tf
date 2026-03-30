// Environments

resource "azuredevops_environment" "environment" {
  for_each = { for env in local.environments : env => env }

  project_id = data.azuredevops_project.azdo_project.id
  name       = each.key
}

resource "azuredevops_check_exclusive_lock" "exclusive_lock" {
  for_each = azuredevops_environment.environment

  project_id           = data.azuredevops_project.azdo_project.id
  target_resource_id   = each.value.id
  target_resource_type = "environment"

  timeout = 43200
}

// Approval and Checks

resource "azuredevops_check_approval" "single_approver" {
  for_each = { for env in var.environments_with_single_approver : env => env }

  project_id           = data.azuredevops_project.azdo_project.id
  target_resource_id   = azuredevops_environment.environment[each.key].id
  target_resource_type = "environment"

  requester_can_approve = true
  approvers = [
    data.azuredevops_group.single_approver.origin_id
  ]
}

// Pipeline permissions

resource "azuredevops_pipeline_authorization" "environment_permission" {
  for_each = { for key, value in local.permission_groups : key => value }

  project_id  = data.azuredevops_project.azdo_project.id
  type        = "environment"
  resource_id = azuredevops_environment.environment[each.value.environment].id
  pipeline_id = azuredevops_build_definition.pipeline[each.value.pipeline].id
}
