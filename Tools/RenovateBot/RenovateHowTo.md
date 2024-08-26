# Introduction

This document is a quick and dirty guide on getting the Renovate auto dependency updater running on a .net project. The actual documentation is here: https://docs.renovatebot.com/
Ideally you should be able to just copy paste the files from here with minimal editing and be updating packages in no time.

If you are worried about breaking things, you should test out your config in a copy/fork of your actual repo before running it on your real repo.

# Key concepts

Renovate works with two config files, one for the application itself (config.js), and one in the target repository (renovate.json).\
You *can* rename theese files via env config, but there should not be a need.


## config.js
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

## renovate.json
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

# Running it:

Renovate is a Node application, in theory you could just run `npx renovate` once in a while in a configured directory, though you probably want it automated, easiest way is probablyis putting it in a devops yaml pipeline:

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

# Immortal PR's

Renovate checks what PR's its already created by title, but certain groupings of PR's means it can't be sure it's not already suggested that specific update.\
When this happens the PR will be marked as immortal. There's settings to prevent this from happening, but you will have to figure them out based on your own config and needs. 

# ignoring packages

```js
  "packageRules": [
    {
      "matchPackagePatterns": ["postgres"],
      "matchManagers": ["docker-compose"],
      "enabled": false
    }
  ]