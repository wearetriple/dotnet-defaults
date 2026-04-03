variable "pat" {
  type        = string
  description = "Personal Access Token needed to create the Service Connections in Azure Devops"
}

variable "project_name" {
  type        = string
  description = "The name of the project in Azure Devops"
  default     = "{project_name}"
}

variable "organization_name" {
  type        = string
  description = "The name of the organization in Azure Devops"
  default     = "{org_name}"
}
