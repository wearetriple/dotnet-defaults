# Probes

Probes are health checks used by Kubernetes to gauge the health of of your application
instance. See [the docs of kubernetes](https://kubernetes.io/docs/concepts/configuration/liveness-readiness-startup-probes/)
for more information. 

These probes are also used by Azure Container Apps, and can double as monitors for
things like Azure Traffic Manager endpoints or Azure Front Door, or our own internal
monitoring setup. Therefore, every application we develop should at least expose
1 generic health probe.

The easiest way to implement health probes in ASP.NET is via Health checks. See
[the docs for Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks).

## Internal monitoring setup

When developing a probe for our own internal monitoring setup, we should return some
more information than a simple `200 OK` vs `500 Internal Server Error`. For example,
such a probe can return the following json structure:

```json
{
  "containerName": "{{resource name in azure}}",
  "expiryDate": "{{date}}",
  "isExpired": false,
  "status": {
    "internalOk": "OK",
    "externalOk": "OK",
    "allOk": "OK"
  },
  "internal": {
    "configuration::Some.Configuration.Class": "OK",
    "internalGateways::internalClient": "OK",
    "dbContext::select 1": "OK",
    "dbContext::readOnly: READ_WRITE": "OK",
    "dbContext::readWrite: READ_WRITE": "OK",
    "dbContext::partner state: CATCH_UP": "OK"
  },
  "external": {
    "thirdPartyGateway::accessToken": "OK"
  },
  "statistics": {
    "applicationInsights::requestFailureRate": "Reported request failure rate is at 0.00%",
    "applicationInsights::exceptionCount": "Reported exceptions rate is 0 per 10 minutes",   
  }
}
```

This example is from a project where:

- Each `IConfigurationSection` also publishes the validation status of its config.
- Each gateway publishes the status of its auth config, and also monitors the responses
it receivers from the server.
- The database setup performs `SELECT 1` frequently to check for database status,
and monitors the multi-region replication periodically.
- The application reads its own application insights data back, and checks whether
it might be misbehaving.

At the top of the response there are some simple flags (`status.internalOk`). These
allow for the monitoring software to trigger certain alerts. Generally speaking,
the `externalOk` is flagged as `NOT-OK` when something external (for example, a third-
party API) is misbehaving and triggers a 8x5 alert that should be picked up by an
developer during working hours. When `internalOk` is flagged `NOT-OK`, something is 
critically wrong (for example, the database is down) and triggers a 24x7 alert that
should be picked up asap by the on-call engineer.

This monitoring was setup using:

- Each monitorable entity within the application implemented `ISelfCheck`, and
was also inserted under that interface in the Dependency Injection container.
- The `ISelfCheck` interface contains a `SelfCheckAsync`, which returned an object
containing multiple self check results. Each result specifies its level (internal,
external, etc), description and whether it's healthy.
- The monitoring probe resolved all self checks by requesting `IEnumerable<ISelfCheck>`
from DI and aggregated all results based in level and healthiness.
