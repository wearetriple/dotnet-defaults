locals {
  environments = distinct(flatten([for environments in var.environment_configs : environments]))
  pipeline_groups = {
    for group in flatten([
      for group_name, pipeline_config in var.pipeline_group_configs : [
        for pipeline_type in var.pipeline_type_configs[pipeline_config.pipeline_type] : {
          key = "${group_name} - ${pipeline_type}"
          value = {
            group_name    = group_name
            pipeline_type = pipeline_type
            environments  = lookup(var.environment_configs, pipeline_type, [])

            repository = data.azuredevops_git_repository.git_repo

            path_filters = !startswith(pipeline_type, "pr") ? [] : pipeline_config.path_filters[replace(replace(pipeline_type, "pr - verify ", ""), " ", "_")]
          }
        }
      ]
    ]) : group.key => group.value
  }
  permission_groups = {
    for group in flatten([
      for group_name in keys(var.pipeline_group_configs) : [
        for pipeline_type in var.pipeline_type_configs[var.pipeline_group_configs[group_name].pipeline_type] : [
          for environment in lookup(var.environment_configs, pipeline_type, []) : {
            key = "${group_name} - ${pipeline_type} | ${environment}"
            value = {
              pipeline    = "${group_name} - ${pipeline_type}"
              environment = environment
            }
          }
        ]
      ]
    ]) : group.key => group.value
  }
}
