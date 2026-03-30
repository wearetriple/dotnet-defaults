// Branch Policy

resource "azuredevops_branch_policy_min_reviewers" "branch_reviewers" {
  project_id = data.azuredevops_project.azdo_project.id

  settings {
    allow_completion_with_rejects_or_waits = false
    reviewer_count                         = 1
    submitter_can_vote                     = false
    on_push_reset_all_votes                = false
    on_push_reset_approved_votes           = false

    scope {
      repository_id  = data.azuredevops_git_repository.git_repo.id
      repository_ref = data.azuredevops_git_repository.git_repo.default_branch
      match_type     = "Exact"
    }
    scope {
      repository_id  = data.azuredevops_git_repository.git_repo.id
      repository_ref = "refs/heads/dev"
      match_type     = "Prefix"
    }
  }
}

resource "azuredevops_branch_policy_comment_resolution" "branch_comment_resolution" {
  project_id = data.azuredevops_project.azdo_project.id

  blocking = true
  enabled  = true

  settings {
    scope {
      repository_id  = data.azuredevops_git_repository.git_repo.id
      repository_ref = data.azuredevops_git_repository.git_repo.default_branch
      match_type     = "Exact"
    }
    scope {
      repository_id  = data.azuredevops_git_repository.git_repo.id
      repository_ref = "refs/heads/dev"
      match_type     = "Prefix"
    }
  }
}

resource "azuredevops_branch_policy_merge_types" "main_branch_merge_types" {
  project_id = data.azuredevops_project.azdo_project.id

  enabled = true

  settings {
    allow_basic_no_fast_forward   = false
    allow_rebase_and_fast_forward = var.branch_strategy == "dev_branches"
    allow_rebase_with_merge       = false
    allow_squash                  = var.branch_strategy == "main_only"

    scope {
      repository_id  = data.azuredevops_git_repository.git_repo.id
      repository_ref = data.azuredevops_git_repository.git_repo.default_branch
      match_type     = "Exact"
    }
  }
}

resource "azuredevops_branch_policy_merge_types" "dev_branch_merge_types" {
  project_id = data.azuredevops_project.azdo_project.id

  enabled = true

  settings {
    allow_basic_no_fast_forward   = false
    allow_rebase_and_fast_forward = false
    allow_rebase_with_merge       = false
    allow_squash                  = true

    scope {
      repository_id  = data.azuredevops_git_repository.git_repo.id
      repository_ref = "refs/heads/dev"
      match_type     = "Prefix"
    }
  }
}
