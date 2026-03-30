terraform {
  required_providers {
    azuredevops = {
      source  = "microsoft/azuredevops"
      version = "=1.15.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "=3.8.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=4.66.0"
    }
  }

  // the terraform state must be stored in a container as its contains the service principals key
  backend "azurerm" { // Backend configs do not support variables
    resource_group_name  = "{resource group name}"
    subscription_id      = "{guid}"
    storage_account_name = "{storage account name}"
    container_name       = "terraform-state-pipelines"
    key                  = "terraform.tfstate"
    use_azuread_auth     = true
    tenant_id            = "{guid}"
  }
}

provider "azuread" {
}

provider "azurerm" {
  tenant_id = local.environment_variables[terraform.workspace].tenant_id
  resource_provider_registrations = "none"
  
  subscription_id = local.environment_variables[terraform.workspace].subscription_id
  
  features {}
}

provider "azuredevops" {
  org_service_url       = "https://dev.azure.com/${var.organization_name}"
  personal_access_token = var.pat
}
