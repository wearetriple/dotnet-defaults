azdo_config = {
  org_service_url      = "https://dev.azure.com/${ORG_NAME_HERE}"
  project_name         = "${PROJECT_NAME_HERE}"
  git_repository       = "${REPO_NAME_HERE}"
  single_approver_team = "${PROJECT_TEAM_NAME_HERE}"
}

pipeline_type_configs = {
  regular   = ["pr", "main"]
  main_only = ["main"]
  common = [
    "main",
    "pr - verify bicep",
    "pr - verify terraform",
    "pr - verify formatting",
    "pr - verify pr"
  ]
}

pipeline_filenames = {
  main                     = "main.yml"
  pr                       = "pr.yml"
  "pr - verify bicep"      = "pr-verify-bicep.yml"
  "pr - verify terraform"  = "pr-verify-terraform.yml",
  "pr - verify formatting" = "pr-verify-formatting.yml",
  "pr - verify pr"         = "pr-verify-pr.yml"
}

environment_configs = {
  main = ["TEST", "ACC", "PROD"]
}

/*
NOTE:
the path filter used by each PR validation is determined by the name of the pipeline:
pr                        -> pr_path_filters
"pr - verify terraform"   -> pr_verify_terraform_path_filters
"pr - verify pr"          -> pr_verify_pr_path_filters

the release type is used in the prefix folder of the pipelines, and should be common, libraries, or containers.
*/
pipeline_group_configs = {
  common = {
    pipeline_type = "common"
    path_filters = {
      bicep      = ["*.bicep"]
      terraform  = ["*.tf"]
      formatting = ["/src/*.cs"]
      pr         = ["/*"]
    }
  }
  example-v1 = {
    pipeline_type = "regular"
    path_filters = {
      pr = ["/src/Client.Example*"]
    }
  }
}

environments_with_xlock           = ["TEST", "ACC", "PROD"]
environments_with_single_approver = ["ACC", "PROD"]
