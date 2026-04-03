// Build validation

resource "azuredevops_branch_policy_build_validation" "main_branch_validation" {
  for_each = { for key, val in azuredevops_build_definition.pipeline : key => val if length(local.pipeline_groups[key].path_filters) > 0 }

  project_id = data.azuredevops_project.azdo_project.id

  settings {
    display_name        = each.value.name
    build_definition_id = each.value.id

    # https://github.com/microsoft/terraform-provider-azuredevops/issues/481
    queue_on_source_update_only = false
    valid_duration              = 0

    filename_patterns = local.pipeline_groups[each.key].path_filters

    scope {
      repository_id  = local.pipeline_groups[each.key].repository.id
      repository_ref = local.pipeline_groups[each.key].repository.default_branch
      match_type     = "Exact"
    }
  }
}

resource "azuredevops_branch_policy_build_validation" "dev_branch_validation" {
  for_each = { for key, val in azuredevops_build_definition.pipeline : key => val if length(local.pipeline_groups[key].path_filters) > 0 }

  project_id = data.azuredevops_project.azdo_project.id

  settings {
    display_name        = each.value.name
    build_definition_id = each.value.id

    # https://github.com/microsoft/terraform-provider-azuredevops/issues/481
    queue_on_source_update_only = false
    valid_duration              = 0

    filename_patterns = local.pipeline_groups[each.key].path_filters

    scope {
      repository_id  = local.pipeline_groups[each.key].repository.id
      repository_ref = "refs/heads/dev"
      match_type     = "Prefix"
    }
  }
}
