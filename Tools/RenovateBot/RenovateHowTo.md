## Introduction

This document is a quick and dirty guide on getting the Renovate auto dependency updater running on a .net project. The actual documentation is here: https://docs.renovatebot.com/
Ideally you should be able to just copy paste the files from here with minimal editing and be updating packages in no time.

If you are worried about breaking things, you should test out your config in a copy/fork of your actual repo before running it on your real repo.

## Key concepts

Renovate works with two config files, one for the application itself (config.js), and one in the target repository (renovate.json).\
You *can* rename these files via env config, but there should not be a need.


### config.js
This file contains the settings to actually run the application, things like personal access tokens and how it can find your code.
Example:
```js
module.exports = {
    hostRules: [ //Settings on how to connect to your repo
        {
            hostType: 'npm',
            matchHost: 'pkgs.dev.azure.com',
            username: 'apikey',
            password: process.env["RENOVATE_TOKEN"],//A devops PAT should go in here.
        },
    ],
    //settings on where to find your repo.
    endpoint: "https://dev.azure.com/OrganisationName",
    platform: "azure",
    repositories: ['OrganisationName/Repo.Name']
};
```

### renovate.json
Stored in the root of your repository, contains settings on how renovate should behave.\
Example:

```js
{
    "extends": [ //Renovate works via a config inheritance scheme with community built recommended defaults.
        "config:recommended", 
        "group:recommended" //This is the grouping for which packages share a PR.
    ],
    "enabledManagers": ["nuget"], //If you don't specify this it will autodetect and also put in PR's for other package managers.
    "packageRules": [
        {
          "matchUpdateTypes": ["patch", "minor"] //By default renovate just upgrades everything, this is how you make it not do that.
        }
    ], 
    "automerge": false, //By default renovate is happy to merge your PR's for you, if tests pass of course. this is how you make it not.
    "updateLockFiles": false //I try to use central package management without lockfiles in .net, so there's no lockfiles to update.
}
```

## Running it:

Renovate is a Node application, in theory you could just run `npx renovate` once in a while in a configured directory, though you probably want it automated, easiest way is probably putting it in a devops yaml pipeline:

```yaml
schedules:
- cron: '0 7-20 * * 1-5' #Thanks crontab.guru
  displayName: Run renovate bot hourly around office hours.
  branches:
    include:
    - main 
stages:
- stage: Run
  jobs:
    - job: run_renovate
      displayName: 'Run Renovate'
      variables:
      - group: renovate
      steps:  
        - task: NodeTool@0
          inputs:
             versionSource: 'spec'
             versionSpec: 20.x
             checkLatest: true
        - task: PowerShell@2
          displayName: 'Run Renovate'         
          env: #Inject secret as secrets don't go into env by themselves.
            RENOVATE_TOKEN: $(RENOVATE_TOKEN)
          inputs:
            targetType: 'inline' 
            script: |
              npm install renovate@38.54.1 #Specific version because this would be pretty bad if it got supply chain attacked.
              npx renovate

```

## Immortal PR's

Immortal PR's are PR's that Renovate will just reopen when closed. From the renovate docs:
> First off, we don't have immortal PRs for some philosophical reason like: "don't ignore this update, it's good for you!". We have no good way to ignore some PRs after they're closed.\
> ...\
> Renovate uses the branch name and PR title like a cache key. If the same key exists and the PR was closed, then we ignore the PR.

So with certain groupings of PR's means it can't be sure it's not already suggested that specific update.\
When this happens the PR will be marked as immortal. To prevent this you would need to configure Renovate in a way that it can recognize that it already suggested that specific set of updates and should not remake them.

## Ignoring packages


If you want to exclude certain packages, you do so via the "packagerules" property.\
https://docs.renovatebot.com/configuration-options/#packagerules \
https://docs.renovatebot.com/string-pattern-matching

The property has various 'matchers' (package/version/etc...) which are then evaluated to determine various bits of behaviour, the totality is quite complex so I'd recommend actually reading the documentation if you need more complicated restrictions. Know that there's regex involved.

The following example would disable any auto updates for packages matching the pattern "umbraco" (In this case a 'glob' match, so it just looks for the string "umbraco" anywhere, non case sensitive)

```js
  "packageRules": [
    {
      "matchPackagePatterns": ["umbraco"], //The package has 'umbraco' in its name
      "matchManagers": ["nuget"], //Our manager has 'nuget' in its name
      "enabled": false //Turn off renovate for this situation.
    }
  ]
```
This example uses a regex (by starting the pattern with a `/`, case sensitive!) and disables any package upgrades for anything that starts with `Umbraco.CMS`. 
```js
  "packageRules": [
    {
      "matchPackagePatterns": ["/^Umbraco\\.CMS"], 
      "matchManagers": ["nuget"],
      "enabled": false
    }
  ]
  ```