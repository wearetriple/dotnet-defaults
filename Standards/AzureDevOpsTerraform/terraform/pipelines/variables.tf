variable "pat" {
  type        = string
  description = "Personal Access Token needed to create the Service Connections in Azure Devops"
}

variable "azdo_config" {
  type = object({
    org_service_url      = string
    project_name         = string
    git_repository       = string
    single_approver_team = string
  })
}

variable "pipeline_type_configs" {
  type = map(list(string))
}

variable "pipeline_filenames" {
  type = map(string)
}

variable "environment_configs" {
  type = map(list(string))
}

variable "pipeline_group_configs" {
  type = map(object({
    pipeline_type = string
    path_filters  = optional(map(list(string)))
  }))
}

variable "environments_with_xlock" {
  type = list(string)
}

variable "environments_with_single_approver" {
  type = list(string)
}

variable "branch_strategy" {
  type = string
  default = "main_only"
  validation {
    condition = contains(["main_only", "dev_branches"], var.branch_strategy)
    error_message = "Branch strategy must be main_only or dev_branches"
  }
}
