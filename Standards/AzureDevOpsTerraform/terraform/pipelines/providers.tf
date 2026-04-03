terraform {
  required_providers {
    azuredevops = {
      source  = "microsoft/azuredevops"
      version = "=1.15.0"
    }
  }

  // remove this block when storing state in local file
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

provider "azuredevops" {
  org_service_url       = var.azdo_config.org_service_url
  personal_access_token = var.pat
}
