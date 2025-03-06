﻿module.exports = {
    hostRules: [ //Settings on how to connect to your repo
        {
            hostType: 'nuget',
            matchHost: 'https://pkgs.dev.azure.com/{OrganisationName}'
        },
    ],
    // Settings on where to find your repo.
    endpoint: "https://dev.azure.com/{OrganisationName}",
    platform: "azure",
    token: process.env["RENOVATE_TOKEN"],
    repositories: ['OrganisationName/Repo.Name']
};