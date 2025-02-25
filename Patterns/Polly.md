# [Polly.NET](https://www.pollydocs.org/)

Polly is a powerful library for .NET that helps you handle transient faults and improve the resilience of your applications in a fluent and thread-safe way. Polly lets you use and combine different resilience strategies to cope with various scenarios, such as:

* **Retry**: Try again if something fails. This can be useful when the problem is temporary and might go away.
* **Circuit Breaker**: Stop trying if something is broken or busy. This can benefit you by avoiding wasting time and making things worse. It can also support the system to recover.
* **Timeout**: Give up if something takes too long. This can improve your performance by freeing up space and resources.
* **Rate Limiter**: Limit how many requests you make or accept. This can enable you to control the load and prevent problems or penalties.
* **Fallback**: Do something else if something fails. This can improve your user experience and keep the program working.
* **Hedging**: Do more than one thing at the same time and take the fastest one. This can make your program faster and more responsive.

# [Resilience strategies](https://www.pollydocs.org/strategies/index.html)
Resilience strategies are essential components of Polly, designed to execute user-defined callbacks while adding an extra layer of resilience. These strategies can't be executed directly; they must be run through a resilience pipeline. Polly provides an API to construct resilience pipelines by incorporating one or more resilience strategies through the pipeline builders.

Polly categorizes resilience strategies into two main groups:

* **Reactive**: These strategies handle specific exceptions that are thrown, or results that are returned, by the callbacks executed through the strategy.
* **Proactive**: Unlike reactive strategies, proactive strategies do not focus on handling errors by the callbacks might throw or return. They can make proactive decisions to cancel or reject the execution of callbacks (e.g., using a rate limiter or a timeout resilience strategy).

## [Retry strategy](https://www.pollydocs.org/strategies/retry.html)
The retry reactive resilience strategy re-executes the same callback method if its execution fails. Failure can be either an Exception or a result object indicating unsuccessful processing. Between the retry attempts the retry strategy waits a specified amount of time. You have fine-grained control over how to calculate the next delay. The retry strategy stops invoking the same callback when it reaches the maximum allowed number of retry attempts or an unhandled exception is thrown / result object indicating a failure is returned.

``` mermaid
graph TD
    Start["Start Operation"] --> CheckFailure{Operation Fails?}
    CheckFailure -- No --> Success["Success"]
    CheckFailure -- Yes --> RetryCount{Retry Count < Max?}
    RetryCount -- No --> Failure["Failure"]
    RetryCount -- Yes --> Wait["Wait (if defined)"]
    Wait --> Retry["Retry Operation"]
    Retry --> CheckFailure
```

### Retry using the default options
```csharp
var options = new RetryStrategyOptions();
//ShouldHandle = any exceptions other than OperationCanceledException
//MaxRetryAttempts = 3
//BackoffType = Constant
//Delay = 2 seconds
new ResiliencePipelineBuilder().AddRetry(options);
```

### For instant retries with no delay
```csharp
var options = new RetryStrategyOptions
{
    Delay = TimeSpan.Zero
};
new ResiliencePipelineBuilder().AddRetry(options);
```

### For advanced control over the retry behavior, including the number of attempts, delay between retries, and the types of exceptions to handle.
```csharp
var options = new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true,  // Adds a random factor to the delay
    MaxRetryAttempts = 4,
    Delay = TimeSpan.FromSeconds(3),
};
new ResiliencePipelineBuilder().AddRetry(options);
```

### To use a custom function to generate the delay for retries
```csharp
var options = new RetryStrategyOptions
{
    MaxRetryAttempts = 2,
    DelayGenerator = static args =>
    {
        var delay = args.AttemptNumber switch
        {
            0 => TimeSpan.Zero,
            1 => TimeSpan.FromSeconds(1),
            _ => TimeSpan.FromSeconds(5)
        };

        // This example uses a synchronous delay generator,
        // but the API also supports asynchronous implementations.
        return new ValueTask<TimeSpan?>(delay);
    }
};
new ResiliencePipelineBuilder().AddRetry(options);
```

### To extract the delay from the result object
```csharp
var options = new RetryStrategyOptions<HttpResponseMessage>
{
    DelayGenerator = static args =>
    {
        if (args.Outcome.Result is HttpResponseMessage responseMessage &&
            TryGetDelay(responseMessage, out TimeSpan delay))
        {
            return new ValueTask<TimeSpan?>(delay);
        }

        // Returning null means the retry strategy will use its internal delay for this attempt.
        return new ValueTask<TimeSpan?>((TimeSpan?)null);
    }
};
new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(options);
```

### To get notifications when a retry is performed
```csharp
var options = new RetryStrategyOptions
{
    MaxRetryAttempts = 2,
    OnRetry = static args =>
    {
        Console.WriteLine("OnRetry, Attempt: {0}", args.AttemptNumber);

        // Event handlers can be asynchronous; here, we return an empty ValueTask.
        return default;
    }
};
new ResiliencePipelineBuilder().AddRetry(options);
```

### To keep retrying indefinitely or until success use int.MaxValue.
```csharp
var options = new RetryStrategyOptions
{
    MaxRetryAttempts = int.MaxValue,
};
new ResiliencePipelineBuilder().AddRetry(options);
```

## [Circuit Breaker strategy](https://www.pollydocs.org/strategies/circuit-breaker.html)

The circuit breaker reactive resilience strategy shortcuts the execution if the underlying resource is detected as unhealthy. The detection process is done via sampling. If the sampled executions' failure-success ratio exceeds a predefined threshold then a circuit breaker will prevent any new executions by throwing a BrokenCircuitException. After a preset duration the circuit breaker performs a probe, because the assumption is that this period was enough for the resource to self-heal. Depending on the outcome of the probe, the circuit will either allow new executions or continue to block them. If an execution is blocked by the circuit breaker, the thrown exception may indicate the amount of time executions will continue to be blocked through its RetryAfter property.

``` mermaid
graph TD
    Start["Start Operation"] --> CheckState{Circuit State?}
    CheckState -- Closed --> Execute["Execute Operation"]
    Execute --> CheckFailure{Operation Fails?}
    CheckFailure -- No --> Success["Success"]
    CheckFailure -- Yes --> IncrementFailures["Increment Failure Count"]
    IncrementFailures --> MaxFailures{Failures > Threshold?}
    MaxFailures -- No --> Execute
    MaxFailures -- Yes --> OpenCircuit["Open Circuit"]

    CheckState -- Open --> Reject["Reject Operation"]
    OpenCircuit --> Wait["Wait (Break Duration)"]
    Wait --> HalfOpen["Switch to Half-Open State"]

    CheckState -- Half-Open --> TestOperation["Execute Test Operation"]
    TestOperation --> TestSuccess{Test Success?}
    TestSuccess -- Yes --> CloseCircuit["Close Circuit (Back to Normal)"]
    TestSuccess -- No --> OpenCircuit
```

### Circuit breaker with default options
```csharp
var options = new CircuitBreakerStrategyOptions();
//ShouldHandle = Any exceptions other than OperationCanceledException
//FailureRatio = 0.1
//MinimumThroughput = 100
//SamplingDuration = 30 seconds
//BreakDuration = 5 seconds
new ResiliencePipelineBuilder().AddCircuitBreaker(options);
```

### Circuit breaker with customized options: the circuit will break if more than 50% of actions result in handled exceptions,  within any 10-second sampling duration, and at least 8 actions are processed
```csharp
var options = new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5,
    SamplingDuration = TimeSpan.FromSeconds(10),
    MinimumThroughput = 8,
    BreakDuration = TimeSpan.FromSeconds(30),
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>()
};
new ResiliencePipelineBuilder().AddCircuitBreaker(options);
```

### Circuit breaker using BreakDurationGenerator: the break duration is dynamically determined based on the properties of BreakDurationGeneratorArguments
```csharp
var options = new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5,
    SamplingDuration = TimeSpan.FromSeconds(10),
    MinimumThroughput = 8,
    BreakDurationGenerator = static args => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(args.FailureCount)),
};
new ResiliencePipelineBuilder().AddCircuitBreaker(options);
```

### Handle specific failed results for HttpResponseMessage
```csharp
var options = new CircuitBreakerStrategyOptions<HttpResponseMessage>
{
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .Handle<SomeExceptionType>()
        .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError)
};
new ResiliencePipelineBuilder().AddCircuitBreaker(options);
```

### Monitor the circuit state, useful for health reporting
```csharp
var stateProvider = new CircuitBreakerStateProvider();
var options = new CircuitBreakerStrategyOptions<HttpResponseMessage>
{
    StateProvider = stateProvider
};
new ResiliencePipelineBuilder<HttpResponseMessage>().AddCircuitBreaker(options);

var circuitState = stateProvider.CircuitState;

/*
CircuitState.Closed - Normal operation; actions are executed.
CircuitState.Open - Circuit is open; actions are blocked.
CircuitState.HalfOpen - Recovery state after break duration expires; actions are permitted.
CircuitState.Isolated - Circuit is manually held open; actions are blocked.
*/
```

### Manually control the Circuit Breaker state
```csharp
var manualControl = new CircuitBreakerManualControl();
var options = new CircuitBreakerStrategyOptions
{
    ManualControl = manualControl
};
new ResiliencePipelineBuilder().AddCircuitBreaker(options);

// Manually isolate a circuit, e.g., to isolate a downstream service.
await manualControl.IsolateAsync();

// Manually close the circuit to allow actions to be executed again.
await manualControl.CloseAsync();
```

## [Timeout strategy](https://www.pollydocs.org/strategies/timeout.html)
The timeout proactive resilience strategy cancels the execution if it does not complete within the specified timeout period. If the execution is canceled by the timeout strategy, it throws a TimeoutRejectedException. The timeout strategy operates by wrapping the incoming cancellation token with a new one. Should the original token be canceled, the timeout strategy will transparently honor the original cancellation token without throwing a TimeoutRejectedException.

``` mermaid
graph TD
    Start["Start Operation"] --> StartTimer["Start Timeout Timer"]
    StartTimer --> ExecuteOperation["Execute Operation"]
    ExecuteOperation --> CheckCompletion{Operation Completed?}
    CheckCompletion -- Yes --> Success["Success"]
    CheckCompletion -- No --> CheckTimeout{Timeout Expired?}
    CheckTimeout -- No --> ExecuteOperation
    CheckTimeout -- Yes --> Timeout["Timeout Exception"]
```

### To add a timeout with a custom TimeSpan duration
```csharp
new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromSeconds(3));
```

### Timeout using the default options
```csharp
var options = new TimeoutStrategyOptions();
//Timeout = 30 seconds
new ResiliencePipelineBuilder().AddTimeout(options);
```

### To add a timeout using a custom timeout generator function
```csharp
var options = new TimeoutStrategyOptions
{
    TimeoutGenerator = static args =>
    {
        // Note: the timeout generator supports asynchronous operations
        return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
    }
};
new ResiliencePipelineBuilder().AddTimeout(options);
```

### To add a timeout and listen for timeout events
```csharp
var options = new TimeoutStrategyOptions
{
    TimeoutGenerator = static args =>
    {
        // Note: the timeout generator supports asynchronous operations
        return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
    },
    OnTimeout = static args =>
    {
        Console.WriteLine($"{args.Context.OperationKey}: Execution timed out after {args.Timeout.TotalSeconds} seconds.");
        return default;
    }
};
new ResiliencePipelineBuilder().AddTimeout(options);
```

## [Rate Limiter strategy](https://www.pollydocs.org/strategies/rate-limiter.html)
The rate limiter proactive resilience strategy controls the number of operations that can pass through it. This strategy is a thin layer over the API provided by the System.Threading.RateLimiting package. This strategy can be used in two flavors: to control inbound load via a rate limiter and to control outbound load via a concurrency limiter.

``` mermaid
graph TD
    Start["Start Operation"] --> CheckLimit{Requests Below Limit?}
    CheckLimit -- Yes --> AllowRequest["Allow Request"]
    AllowRequest --> Execute["Execute Operation"]
    Execute --> Success["Return Success Result"]
    CheckLimit -- No --> Throttle["Throttle Request"]
    Throttle --> RetryLater["Delay or Deny Request"]
    RetryLater --> End["End Operation"]
```

### Add rate limiter with default options
```csharp
//PermitLimit = 1000
//QueueLimit = 0
new ResiliencePipelineBuilder().AddRateLimiter(new RateLimiterStrategyOptions());
```

### Create a rate limiter to allow a maximum of 100 concurrent executions and a queue of 50
```csharp
new ResiliencePipelineBuilder().AddConcurrencyLimiter(100, 50);
```

### Create a rate limiter that allows 100 executions per minute
```csharp
new ResiliencePipelineBuilder()
    .AddRateLimiter(new SlidingWindowRateLimiter(
        new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1)
        }));
```

## [Fallback strategy](https://www.pollydocs.org/strategies/fallback.html)
The fallback reactive resilience strategy provides a substitute if the execution of the callback fails. Failure can be either an Exception or a result object indicating unsuccessful processing. Typically this strategy is used as a last resort, meaning that if all other strategies failed to overcome the transient failure you could still provide a fallback value to the caller.

``` mermaid
graph TD
    Start["Start Operation"] --> TryOperation["Try Primary Operation"]
    TryOperation --> CheckSuccess{Operation Succeeded?}
    CheckSuccess -- Yes --> Success["Return Success Result"]
    CheckSuccess -- No --> TriggerFallback["Trigger Fallback"]
    TriggerFallback --> ExecuteFallback["Execute Fallback Action"]
    ExecuteFallback --> ReturnFallback["Return Fallback Result"]
```

### A fallback/substitute value if an operation fails
```csharp
var options = new FallbackStrategyOptions<UserAvatar>
{
    ShouldHandle = new PredicateBuilder<UserAvatar>()
        .Handle<SomeExceptionType>()
        .HandleResult(r => r is null),
    FallbackAction = static args => Outcome.FromResultAsValueTask(UserAvatar.Blank)
};
new ResiliencePipelineBuilder<UserAvatar>().AddFallback(options);
```

### Use a dynamically generated value if an operation fails
```csharp
var options = new FallbackStrategyOptions<UserAvatar>
{
    ShouldHandle = new PredicateBuilder<UserAvatar>()
        .Handle<SomeExceptionType>()
        .HandleResult(r => r is null),
    FallbackAction = static args =>
    {
        var avatar = UserAvatar.GetRandomAvatar();
        return Outcome.FromResultAsValueTask(avatar);
    }
};
new ResiliencePipelineBuilder<UserAvatar>().AddFallback(options);
```

### Use a default or dynamically generated value, and execute an additional action if the fallback is triggered
```csharp
var options = new FallbackStrategyOptions<UserAvatar>
{
    ShouldHandle = new PredicateBuilder<UserAvatar>()
        .Handle<SomeExceptionType>()
        .HandleResult(r => r is null),
    FallbackAction = static args =>
    {
        var avatar = UserAvatar.GetRandomAvatar();
        return Outcome.FromResultAsValueTask(UserAvatar.Blank);
    },
    OnFallback = static args =>
    {
        // Add extra logic to be executed when the fallback is triggered, such as logging.
        return default; // Returns an empty ValueTask
    }
};
new ResiliencePipelineBuilder<UserAvatar>().AddFallback(options);
```

## [Hedging strategy](https://www.pollydocs.org/strategies/hedging.html)
The hedging reactive strategy enables the re-execution of the callback if the previous execution takes too long. This approach gives you the option to either run the original callback again or specify a new callback for subsequent hedged attempts. Implementing a hedging strategy can boost the overall responsiveness of the system. However, it's essential to note that this improvement comes at the cost of increased resource utilization. If low latency is not a critical requirement, you may find the retry strategy more appropriate. This strategy also supports multiple concurrency modes to flexibly tailor the behavior for your own needs.

``` mermaid
graph TD
    Start["Start Operation"] --> LaunchTasks["Launch Multiple Parallel Tasks"]
    LaunchTasks --> CheckFirstCompletion{Any Task Completed?}
    CheckFirstCompletion -- No --> Wait["Continue Waiting for Completion"]
    CheckFirstCompletion -- Yes --> CancelRemaining["Cancel Remaining Tasks"]
    CancelRemaining --> Success["Return Result from First Successful Task"]
    Wait --> CheckFirstCompletion
```

### Hedging with default options
```csharp
var options = new HedgingStrategyOptions<HttpResponseMessage>();
new ResiliencePipelineBuilder<HttpResponseMessage>().AddHedging(options);
```

### A customized hedging strategy that retries up to 3 times if the execution takes longer than 1 second or if it fails due to an exception or returns an HTTP 500 Internal Server Error.
```csharp
var options = new HedgingStrategyOptions<HttpResponseMessage>
{
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .Handle<SomeExceptionType>()
        .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError),
    MaxHedgedAttempts = 3,
    Delay = TimeSpan.FromSeconds(1),
    ActionGenerator = static args =>
    {
        Console.WriteLine("Preparing to execute hedged action.");

        // Return a delegate function to invoke the original action with the action context.
        // Optionally, you can also create a completely new action to be executed.
        return () => args.Callback(args.ActionContext);
    }
};
new ResiliencePipelineBuilder<HttpResponseMessage>().AddHedging(options);
```

### Subscribe to hedging events
```csharp
var options = new HedgingStrategyOptions<HttpResponseMessage>
{
    OnHedging = static args =>
    {
        Console.WriteLine($"OnHedging: Attempt number {args.AttemptNumber}");
        return default;
    }
};
new ResiliencePipelineBuilder<HttpResponseMessage>().AddHedging(options);
```

# [Usage](https://www.pollydocs.org/getting-started.html)
To use Polly, you must provide a callback and execute it using a resilience pipeline. A resilience pipeline is a combination of one or more resilience strategies such as retry, timeout, and rate limiter. Polly uses builders to integrate these strategies into a pipeline.

You can create a ResiliencePipeline using the ResiliencePipelineBuilder class as shown below:
```csharp
// Create an instance of builder that exposes various extensions for adding resilience strategies
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions()) // Add retry using the default options
    .AddTimeout(TimeSpan.FromSeconds(10)) // Add 10 seconds timeout
    .Build(); // Builds the resilience pipeline

// Execute the pipeline asynchronously
await pipeline.ExecuteAsync(static async token => { /* Your custom logic goes here */ }, cancellationToken);
```

If you prefer to define resilience pipelines using IServiceCollection, you'll need to install the Polly.Extensions package. Then you can define your resilience pipeline using the AddResiliencePipeline(...) extension method as shown:
```csharp
var services = new ServiceCollection();

// Define a resilience pipeline with the name "my-pipeline"
services.AddResiliencePipeline("my-pipeline", builder =>
{
    builder
        .AddRetry(new RetryStrategyOptions())
        .AddTimeout(TimeSpan.FromSeconds(10));
});

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

// Retrieve a ResiliencePipelineProvider that dynamically creates and caches the resilience pipelines
var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

// Retrieve your resilience pipeline using the name it was registered with
ResiliencePipeline pipeline = pipelineProvider.GetPipeline("my-pipeline");

// Alternatively, you can use keyed services to retrieve the resilience pipeline
pipeline = serviceProvider.GetRequiredKeyedService<ResiliencePipeline>("my-pipeline");

// Execute the pipeline
await pipeline.ExecuteAsync(static async token =>
{
    // Your custom logic goes here
});
```

# Use cases

## Retry

**Transient Errors:** Retry operations prone to temporary failures, such as network issues or throttling by an external service.
**HTTP Requests:** Retrying failed API calls when encountering HTTP 5xx or timeout errors.
**Database Operations:** Retrying transient errors like deadlocks or temporary connection issues.

**Example:** Retrying a failed API call a few times with increasing delays can significantly improve success rates.

## Circuit Breaker

**Downstream Service Protection:** Preventing an overwhelmed or failing API from cascading failures to other parts of your system.
**Database Fallback:** Avoiding overloading a database experiencing temporary instability.
**Microservices:** Ensuring stability in distributed systems by preventing overloading of unstable services.
 
**Example:** If a service is consistently unavailable, the circuit breaker "opens," blocking further requests until a timeout period elapses or a successful test call is made.

## Timeout

**Long-Running Operations:** Preventing operations (e.g., API calls, database queries) from hanging indefinitely.
**Service-Level Agreements (SLAs):** Enforcing time limits for responses from third-party services.
**User Experience:** Ensuring UI responsiveness by cancelling long-running tasks.

**Example:** Setting a timeout for a long-running operation ensures that the application doesn't wait forever for a response.  

## Fallback

**Graceful Degradation:** Providing a default value or alternative response when an operation fails.
**Failover Logic:** Redirecting requests to a secondary service when the primary service is down.
**User Notifications:** Showing a meaningful error message or default content to users.
 
**Example:** If a primary data source is unavailable, a fallback mechanism can retrieve data from a secondary source or provide default values.

## Rate Limiter

**Shared Resources Protection:** Limiting the number of concurrent requests to a service or resource (e.g., database, file system).
**System Stability:** Isolating failures in one subsystem to prevent cascading effects across the application.
  
**Example:** Limiting the number of requests to an external API within a specific time window can prevent your application from being throttled.   

## Hedging

**High-Availability Systems:** Running redundant tasks in parallel to reduce latency (e.g., querying multiple replicas of a database).
**Critical Services:** Ensuring a response is returned quickly by hedging requests to multiple endpoints.
**Geographically Distributed APIs:** Sending requests to different regions and accepting the first successful response.

**Example:** If one part of a service fails, it won't impact other parts due to resource limitations.