## Introduction

This document is a quick and dirty guide on getting the Renovate auto dependency updater running on a .net project. The actual documentation is here: https://docs.renovatebot.com/
Ideally you should be able to just copy paste the files from here with minimal editing and be updating packages in no time.

If you are worried about breaking things, you should test out your config in a copy/fork of your actual repo before running it on your real repo.

## Key concepts

Renovate works with two config files, one for the application itself (renovate-config.js), and one in the target repository (renovate.json).\
You *can* rename these files via env config, but there should not be a need.


### renovate-config.js
This file contains the settings to actually run the application, things like personal access tokens and how it can find your code.

This file should be at the root of your project.

Example: [renovate-config.js](renovate-config.js)

Documentation: [global config](https://docs.renovatebot.com/getting-started/running/#global-config) and [Azure DevOps config](https://docs.renovatebot.com/modules/platform/azure/#create-a-configjs-file)

### renovate.json
Stored in the root of your repository, contains settings on how renovate should behave.
Only add this file after creating the onboarding PR.

Example: [renovate.json](renovate.json)

Documentation: [configuration options](https://docs.renovatebot.com/configuration-options/)

### package.json
In order for npm to install renovate when executing the pipeline the best way is to add it as a dependency to the package.config.

Example: [package.json](package.json)

This way renovate will update itself once a new version becomes available.
If you do not want to add this in your package.json you can replace `npm i` with `npm install renovate` in [adding your pipeline](#adding-your-pipeline).

## Setting it up

### Initial setup
We start by introducing renovate to our repository locally.
1. In order to get renovate to work in your repository you want to create the [renovate-config.js](renovate-config.js) file as described above.
2. create a PAT and set that as the environment variable `RENOVATE_TOKEN`.
3. Then you can run `npm i -g renovate` and `npx renovate`. 
4. If renovate can access your repository you should now see a new pull request, called your onboarding PR.
In this PR renovate tells you what packages it detected, a summary of your configuration and what to expect when you merge the onboarding process.

### Configuration
Now you do not have any configurations yet. 
1. To get renovate to adapt this pull request you have to checkout `renovate\configure`, which is the branch renovate wants to merge in the PR. 
2. You can then add your `renovate-config.js` and `renovate.json` files in the root of your project.
3. Once you have pushed your code, you can run `npx renovate` again locally, which will then update your PR description to your new configuration.
4. You can keep updating your `renovate.json`, pushing it to the branch, and rerunning `npx renovate` until you are satisfied with the result in the PR.

When you are satisfied with your configuration you can merge the pull request.

### Adding your pipeline
In order to be able to run renovate periodically we will add a pipeline.
1. Place this pipeline together with your other pipelines and call it something like `renovate-pipeline.yaml`.
2. Add your secrets to a library
   1.  A [devops PAT](#ownership) called `RENOVATE_TOKEN`
   2.  A [github PAT](#release-notes) called `GITHUB_TOKEN`
3. Add the pipeline to devops.

Example: [renovate-pipeline.yml](renovate-pipeline.yml)

Documentation: [Running renovate in Azure Pipelines](https://docs.renovatebot.com/modules/platform/azure/#create-a-configjs-file)

## Release notes
In order for renovate to display release notes of most packages you need to add a github PAT with read-only rights, as described [here](https://docs.renovatebot.com/getting-started/running/#githubcom-token-for-changelogs).
Renovate recommends just making an empty github account for this.

You can then go and add the PAT to the `GITHUB_COM_TOKEN` env variable in the yaml, as shown above.

## Ignoring updates
Most renovate PRs are not immortal, meaning that when you close them renovate will not reopen them.
Renovate does this by matching the update on branch name and pull request title.

When you close a major upgrade, for example v3 to v4, renovate will not suggest minor updates for that version, so you will not see a request for v4.1.
It will however suggest a new major when it becomes available, so you will see a PR for v5 (as explained [here](https://docs.renovatebot.com/key-concepts/pull-requests/#normal-prs)).

Renovate however has the ability to make immortal PRs, which will keep reappearing after closing. 
More about immortal PRs can be found [here](https://docs.renovatebot.com/key-concepts/pull-requests/#immortal-prs).

## Ownership
As azure devops makes its pull requests in the name of the person who provides their PAT, some thought must be put into this.
According to the guidelines of triple, you cannot approve your own PRs, so many projects have this set as a rule.
Therefore, the person who does have access to the repository, but does the least for that project needs to provide their PAT.
This way the people most actively working for the project can approve renovates PRs.