locals {
  environment_variables = {
    // add or remove entries based on the projects need
    "test" = {
      letter            = "T"
      subscription_name = "{subscription name}"
      subscription_id   = "{subscription id}"
      tenant_id         = "{tenant id}"
    }
    "acc" = {
      letter            = "A"
      subscription_name = "{subscription name}"
      subscription_id   = "{subscription id}"
      tenant_id         = "{tenant id}"
    }
    "prod" = {
      letter            = "P"
      subscription_name = "{subscription name}"
      subscription_id   = "{subscription id}"
      tenant_id         = "{tenant id}"
    }
  }

  // make sure to always add at least 2 owners
  // make sure to also add the name of the owner so its clear who it is
  service_principal_owners = [
    "{user id 1 (object id) in Azure Ad tenant}", // {user name 1}
    "{user id 2 (object id) in Azure Ad tenant}", // {user name 2}
  ]
}
