# Cancellation of requests

When long-running or expensive API calls are called by clients, which have a tendency 
to cancel requests, an API controller should implement a cancellation token that 
is triggered when the client has cancelled the request. This prevents the API from 
doing additional work for a client which does not want to have the result anymore.

## Web Apps

- Controller: Add `CancellationToken token` as parameter to accept the cancellation 
token in the controller method.
- Pass the token to all subsequent async calls. 
- Optional: Catch the `TaskCanceledException` to log the cancellation.

## Function apps

- Function: Add `CancellationToken token` as parameter to accept the cancellation 
token in the function.
- Pass the token to all subsequent async calls. 
- Optional: Catch the `TaskCanceledException` to log the cancellation.

## Think about the state after canceling

Make sure that the system is not left in a weird state after cancelling. Group database
actions in a single transaction and only cancel that transaction. Pass `CancellationToken.None`
(or simply `default`) to certain methods that always need to happen, so it's clear 
those methods should not be cancelled.
