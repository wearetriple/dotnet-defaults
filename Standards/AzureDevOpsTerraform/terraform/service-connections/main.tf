# AZURE Providers

// Generate the SPN
resource "azuread_application" "spn_app" {
  for_each     = local.environment_variables
  display_name = "${var.project_name}-ADOS-${each.key}"
  owners       = local.service_principal_owners
}

resource "azuread_service_principal" "spn" {
  for_each  = local.environment_variables
  client_id = azuread_application.spn_app[each.key].client_id
  owners    = local.service_principal_owners
}

resource "azuread_service_principal_password" "spn_password" {
  for_each             = local.environment_variables
  service_principal_id = azuread_service_principal.spn[each.key].object_id
}

locals {
  list_of_subscription_ids = [for v in local.environment_variables : format("/subscriptions/%s", v.subscription_id)]

  // add or remove roles that the service connection should be able to assign
  azure_key_vault_reader_role_id         = "21090545-7ca7-4776-b22c-e363652d74d2"
  azure_key_vault_secrets_user_role_id   = "4633458b-17de-408a-b874-0445c86b69e6"
  azure_container_registry_pull_role_id  = "7f951dda-4ed3-4680-a7ca-43fe172d538d"
  event_hubs_data_reader_role_id         = "a638d3c7-ab3a-418d-83e6-5f17a39d4fde"
  event_hubs_data_sender_role_id         = "2b629674-e913-4c01-ae53-ef4638d8f975"
  storage_blob_data_contributor_role_id  = "ba92f5b4-2d11-453d-a403-e96b0029c9fe"
  storage_table_data_contributor_role_id = "0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3"
  storage_queue_data_contributor_role_id = "974c5e8b-45b9-4653-ba55-5f855dd0fb88"

  assignable_roles = [
    local.azure_key_vault_reader_role_id,
    local.azure_key_vault_secrets_user_role_id,
    local.azure_container_registry_pull_role_id,
    local.event_hubs_data_reader_role_id,
    local.event_hubs_data_sender_role_id,
    local.storage_blob_data_contributor_role_id,
    local.storage_table_data_contributor_role_id,
    local.storage_queue_data_contributor_role_id
  ]
}

// Create and Grant role
resource "azurerm_role_definition" "ados" {
  name        = "${var.project_name} Azure Devops Contributor"
  scope       = local.list_of_subscription_ids[0]
  description = "Grants limited access to manage specific resources but does not allow you to assign roles in Azure RBAC, manage assignments in Azure Blueprints, share image galleries, or delete resources."
  permissions {
    actions = [
      // add or remove resources the service connection should be able to manage
      "Microsoft.ApiManagement/service/*",                              // Required for APIM usage
      "Microsoft.Authorization/locks/Read",                             // Required to be able to lock data resources (e.g. storage accounts, key vaults, etc.)
      "Microsoft.Authorization/locks/Write",                            // Required to be able to lock data resources (e.g. storage accounts, key vaults, etc.)
      "Microsoft.ContainerRegistry/registries/*",                       // Required for Container Registry usage
      "Microsoft.App/managedEnvironments/*",                            // Required for Container Apps usage
      "Microsoft.App/containerApps/*",                                  // Required for Container Apps usage
      "Microsoft.App/jobs/*",                                           // Required for Container App Jobs usage
      "Microsoft.EventHub/namespaces/*",                                // Required for Event Hubs usage
      "Microsoft.Insights/*",                                           // Required for App Insights configuration usage
      "Microsoft.KeyVault/vaults/*",                                    // Required for key vault usage
      "Microsoft.ManagedIdentity/userAssignedIdentities/Assign/Action", // Required for assigning UIDs to function apps
      "Microsoft.ManagedIdentity/userAssignedIdentities/Read",          // Required for reading UIDs for function app assignment
      "Microsoft.ManagedIdentity/userAssignedIdentities/Write",         // Required for updating UIDs for function app assignment
      "Microsoft.Network/*",                                            // Required for virtual networks, public IPs
      "Microsoft.OperationalInsights/workspaces/*",                     // Required for log analytics usage
      "Microsoft.Storage/storageAccounts/*",                            // Required for storage accounts usage 
      "Microsoft.Web/certificates/*",                                   // Required for certificates usage in for APIM 
      "Microsoft.Web/serverfarms/*",                                    // Required for Service plans
      "Microsoft.Web/sites/*",                                          // Required for web apps
      "Microsoft.Resources/subscriptions/resourcegroups/Read",          // Required for key vault deployment
    ]
    not_actions = ["*/Delete"]
    data_actions = [
      "Microsoft.KeyVault/vaults/*/read",                     // Required for creating key vault secrets
      "Microsoft.KeyVault/vaults/secrets/getSecret/action",   // Required for creating key vault secrets
      "Microsoft.KeyVault/vaults/secrets/setSecret/action",   // Required for creating key vault secrets
      "Microsoft.KeyVault/vaults/secrets/readMetadata/action" // Required for creating key vault secrets
    ]
  }

  assignable_scopes = local.list_of_subscription_ids
}

resource "azurerm_role_definition" "adosdeploy" {
  name        = "${var.project_name} Azure Devops Deployment Admin"
  scope       = local.list_of_subscription_ids[0]
  description = "Grants access to fully manage deployments - deployments require delete action to groom the deployment history, so this role should be used together with Azure Devops Contributor which allows for specific resource creation and management."
  permissions {
    actions = [
      "Microsoft.Resources/deployments/*", // Required for ARM deployments
    ]
    not_actions = []
  }

  assignable_scopes = local.list_of_subscription_ids
}


data "azurerm_subscription" "scope" {
  for_each        = local.environment_variables
  subscription_id = each.value.subscription_id
}

resource "azurerm_role_assignment" "ados" {
  for_each             = local.environment_variables
  scope                = data.azurerm_subscription.scope[each.key].id
  role_definition_name = azurerm_role_definition.ados.name
  principal_id         = azuread_service_principal.spn[each.key].object_id
}

resource "azurerm_role_assignment" "adosdeploy" {
  for_each             = local.environment_variables
  scope                = data.azurerm_subscription.scope[each.key].id
  role_definition_name = azurerm_role_definition.adosdeploy.name
  principal_id         = azuread_service_principal.spn[each.key].object_id
}

locals {
  assignable_roles_csv = join(",", local.assignable_roles)
  role_expression      = "(@Request[Microsoft.Authorization/roleAssignments:RoleDefinitionId] ForAnyOfAnyValues:GuidEquals {${local.assignable_roles_csv}} AND @Request[Microsoft.Authorization/roleAssignments:PrincipalType] ForAnyOfAnyValues:StringEqualsIgnoreCase {'ServicePrincipal'})"
}

resource "azurerm_role_assignment" "ados_uaa" {
  for_each             = local.environment_variables
  scope                = data.azurerm_subscription.scope[each.key].id
  role_definition_name = "Role Based Access Control Administrator"
  principal_id         = azuread_service_principal.spn[each.key].object_id
  condition_version    = "2.0"
  condition            = local.role_expression
}

# // Generate the Service Connection
data "azuredevops_project" "project" {
  name = var.project_name
}

data "azuread_client_config" "current" {}

resource "azuredevops_serviceendpoint_azurerm" "initial_serviceconnection" {
  for_each                               = local.environment_variables
  project_id                             = data.azuredevops_project.project.id
  service_endpoint_name                  = azuread_application.spn_app[each.key].display_name
  description                            = "Managed by Terraform"
  service_endpoint_authentication_scheme = "ServicePrincipal"
  credentials {
    serviceprincipalid  = azuread_service_principal.spn[each.key].client_id
    serviceprincipalkey = azuread_service_principal_password.spn_password[each.key].value
  }
  azurerm_spn_tenantid      = data.azuread_client_config.current.tenant_id
  azurerm_subscription_id   = data.azurerm_subscription.scope[each.key].subscription_id
  azurerm_subscription_name = data.azurerm_subscription.scope[each.key].display_name
}

data "azurerm_storage_account" "terraform_state" {
  name                = "{storage account name}"
  resource_group_name = "{resource group name}"
}

resource "azurerm_role_assignment" "terraform_state_access" {
  for_each = local.environment_variables

  principal_id         = azuread_service_principal.spn[each.key].object_id
  scope                = data.azurerm_storage_account.terraform_state.id
  role_definition_name = "Storage Blob Data Contributor"
}
