## Introduction

This document is a quick and dirty guide on getting the Renovate auto dependency updater running on a .net project. The actual documentation is here: https://docs.renovatebot.com/
Ideally you should be able to just copy paste the files from here with minimal editing and be updating packages in no time.

If you are worried about breaking things, you should test out your config in a copy/fork of your actual repo before running it on your real repo.

## Key concepts

Renovate works with two config files, one for the application itself (config.js), and one in the target repository (renovate.json).\
You *can* rename these files via env config, but there should not be a need.


### config.js
This file contains the settings to actually run the application, things like personal access tokens and how it can find your code.
Only add this file after creating the onboarding PR.

Example:
```js
module.exports = {
    hostRules: [ //Settings on how to connect to your repo
        {
            hostType: 'nuget',
            matchHost: 'https://pkgs.dev.azure.com/{OrganisationName}'
        },
    ],
    //settings on where to find your repo.
    endpoint: "https://dev.azure.com/{OrganisationName}",
    platform: "azure",
    token: process.env["RENOVATE_TOKEN"],// Put your renovate PAT here
    repositories: ['OrganisationName/Repo.Name']
};
```

### renovate.json
Stored in the root of your repository, contains settings on how renovate should behave.\
Example:

```js
{
    "$schema": "https://docs.renovatebot.com/renovate-schema.json",
    "extends": [ //Renovate works via a config inheritance scheme with community built recommended defaults.
        "config:recommended" //This is the recommended config, contains a lot of rules you usually want.
    ],
    "enabledManagers": ["nuget", "npm"], //If you don't specify this it will autodetect and also put in PR's for other package managers.
    "updateLockFiles": false //I try to use central package management without lockfiles in .net, so there's no lockfiles to update.
    "packageRules": [
        {
            "groupName": "Serilog Packages",
            "matchPackageNames": "/^Serilog/"
        },
        {
            "groupName": "EF Core Packages",
            "matchSourceUrlPrefixes": [
                "https://github.com/dotnet/efcore"
            ]
        },
        {
            "groupName": "ASP .NET Core Packages",
            "matchSourceUrlPrefixes": [
                "https://github.com/dotnet/aspnetcore"
            ]
        },
        {
            "groupName": ".NET Runtime Packages",
            "matchSourceUrlPrefixes": [
                "https://github.com/dotnet/runtime"
            ]
        }
    ]
}
```
### package.json
In order for npm to install renovate when executing the pipeline the best way is to add it as a dependency to the package.config.

```json
  "dependencies": {
    "renovate": "39.7.1" //please check latest version before updating
  }
```
This way renovate will update itself once a new version becomes available.
If you do not want to add this in your package.json you can replace `npm i` with `npm install renovate` in  [running it](#running-it).

## Setting it up
In order to get renovate to work in your repository you want to create the `config.js` file as described above, 
create a PAT and set that path as the environment variable `RENOVATE_TOKEN`.
You do not want to add the renovate.json already, as renovate will delete it automatically
Then you can run`npx renovate`. 
If renovate can access your repository you should now see a new pull request, called your onboarding PR.
In this PR renovate tells you what packages it detected, a summary of your configuration and what to expect when you merge the onboarding process.

Now you do not have any configurations yet. To get renovate to adapt this pull request to your configuration you have to
add the `renovate.json` file and push it to the branch `renovate\configure`, which is the branch renovate wants to merge in the PR. 
Once you have pushed your code, you can run `npx renovate` again locally, which will then update your PR description to your new configuration.

When you are completely satisfied with your configuration, you can merge renovates PR.

## Running it:

Renovate is a Node application, in theory you could just run `npx renovate` once in a while in a configured directory, 
though you probably want it automated, easiest way is probably putting it in a devops yaml pipeline:

```yaml
schedules:
- cron: '0 4 * * 1-5' #Thanks crontab.guru
  displayName: Run renovate bot Monday through Friday at 4:00 AM.
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
             versionSpec: 22.x
             checkLatest: true
        - task: PowerShell@2
          displayName: 'Run Renovate'         
          env: #Inject secret as secrets don't go into env by themselves.
            RENOVATE_CONFIG_FILE: './renovate-config.js' #needed when you name config file something else than config.js
            GITHUB_COM_TOKEN: $(GITHUB_TOKEN) #Insert github PAT here to see changelogs in PRs
            RENOVATE_TOKEN: $(RENOVATE_TOKEN)
          inputs:
            targetType: 'inline' 
            script: |
              npm i
              npx renovate

```

## Release notes
In order for renovate to display release notes of most packages you need to add a github PAT with read-only rights, as described [here](https://docs.renovatebot.com/getting-started/running/#githubcom-token-for-changelogs).
Renovate recommends just making an empty github account for this.

You can then go and add the PAT to the `GITHUB_COM_TOKEN` env variable in the yaml, as shown above.

## Immortal PR's

Immortal PR's are PR's that Renovate will just reopen when closed. From the renovate docs:
> First off, we don't have immortal PRs for some philosophical reason like: "don't ignore this update, it's good for you!". We have no good way to ignore some PRs after they're closed.\
> ...\
> Renovate uses the branch name and PR title like a cache key. If the same key exists and the PR was closed, then we ignore the PR.

So with certain groupings of PR's means it can't be sure it's not already suggested that specific update.\
When this happens the PR will be marked as immortal. To prevent this you would need to configure Renovate in a way that it can recognize that it already suggested that specific set of updates and should not remake them.


## Package rules
[Package rules](https://docs.renovatebot.com/configuration-options/#packagerules) is a functionality which lets you apply rules to individual or groups of packages.
You can use this to ignore, group or handle some packages differently than others.

### Matching packages
In order for renovate to match your packages to a rule, you have to define a matcher in the rule.\
This matcher can match packages on different criteria, such as:
  - Datasource
  - Source url
  - File name
  - Repository
  - Package name
  - and [more](https://docs.renovatebot.com/configuration-options/#matchbasebranches)

If you define multiple matches in a rule, all of them must match in order for the rule to be applied.
One matcher can contain multiple patterns.

#### Ignoring packages

If you want to exclude certain packages, you do so via the package rules property.\
https://docs.renovatebot.com/string-pattern-matching

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
This example uses a regex (by starting the pattern with a `/`, case-sensitive!) and disables any package upgrades for anything that starts with `Umbraco.CMS`. 
```js
  "packageRules": [
    {
      "matchPackagePatterns": ["/^Umbraco\\.CMS"], 
      "matchManagers": ["nuget"],
      "enabled": false
    }
  ]
  ```
### Separation and grouping
Within renovate there is a lot of freedom to group and separate package updates to your liking.
The [recommended config](https://docs.renovatebot.com/presets-config/#configrecommended) automatically groups mono repos and known recommended groupings.
Lists for what each group contains exactly are also discoverable in the documentation.

By default, renovate groups all .NET packages together in the [aspnet extensions monorepo](https://docs.renovatebot.com/presets-group/#groupaspnet-extensionsmonorepo).
As you can see in the example renovate.json, it is also possible to separate specific parts of this group by defining package rules.
These package rules are considered top to bottom, so keep that in mind when creating overlapping rules.

## Scheduling
In this example, scheduling is done entirely by the pipeline in azure.
Another possibility is to add a schedule in the [renovate configuration](https://docs.renovatebot.com/key-concepts/scheduling/).
When renovate is triggered, it checks if the current time falls within the schedule.
This also uses [Cron syntax](https://crontab.guru/crontab.5.html), just like in the azure pipeline.
You could use this as a second check for whether the bot should trigger.
For renovate, the minute field has to be always (*).
So for example, you can use `* 5-6 * * 1-5` to trigger between 5 and 6 AM from Monday through Friday.
Then when the pipeline is triggered, renovate checks whether the time is indeed between 5 and 6.

## Ownership
As azure devops makes its pull requests in the name of the person who provides their PAT, some thought msut be put into this.
According to the guidelines of triple, you cannot approve your own PRs, so many projects have this set as a rule.
Therefore the person who does have access to the repository, but does the least for that project needs to provide their PAT.
This way the people most actively working for the project can approve renovates PRs.

As you can see in the yaml in the section [running it](#running-it) you can set the git user to be renovate.
This does cause the commit user to be renovate, but not the PR creator.