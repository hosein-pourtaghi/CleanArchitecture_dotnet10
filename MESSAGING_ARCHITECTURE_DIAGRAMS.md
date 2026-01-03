# Messaging Architecture Diagram

## Request Processing Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                         HTTP Request                            │
│                      (API Endpoint)                             │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
        ┌────────────────────────────────────────┐
        │    Parse Request → Create IRequest     │
        │  (ICommand / IQuery with parameters)   │
        └────────────────┬───────────────────────┘
                         │
                         ▼
        ┌────────────────────────────────────────┐
        │    IPipelineBehavior<TRequest, TResponse>
        │    Pipeline Execution                  │
        │  ┌──────────────────────────────────┐  │
        │  │ 1. Validation Behavior           │  │
        │  │    (Validate request)            │  │
        │  └────────────┬─────────────────────┘  │
        │               │                         │
        │  ┌────────────▼─────────────────────┐  │
        │  │ 2. Logging Behavior              │  │
        │  │    (Log start, perf metrics)     │  │
        │  └────────────┬─────────────────────┘  │
        │               │                         │
        │  ┌────────────▼─────────────────────┐  │
        │  │ 3. Transaction Behavior (optional)
        │  │    (Start transaction if needed) │  │
        │  └────────────┬─────────────────────┘  │
        │               │                         │
        │  ┌────────────▼─────────────────────┐  │
        │  │ 4. Custom Behaviors              │  │
        │  │    (Caching, Authorization, etc) │  │
        │  └────────────┬─────────────────────┘  │
        │               │                         │
        └───────────────┼───────────────────────┘
                        │
                        ▼
     ┌──────────────────────────────────────────┐
     │   Determine Handler Type                 │
     │                                          │
     │  ┌────────────┐      ┌──────────────┐   │
     │  │ ICommand   │      │ IQuery       │   │
     │  │ (Write)    │      │ (Read Only)  │   │
     │  └─────┬──────┘      └──────┬───────┘   │
     │        │                    │           │
     └────────┼────────────────────┼───────────┘
              │                    │
     ┌────────▼────────┐  ┌────────▼──────────┐
     │ ICommandHandler │  │ IQueryHandler     │
     │  <TCommand>     │  │  <TQuery, TResp>  │
     │                 │  │                   │
     │ Execute         │  │ Query & Map       │
     │ Business Logic  │  │ Data              │
     │                 │  │                   │
     │ Modify State    │  │ No Side Effects   │
     │ Publish Events  │  │                   │
     │                 │  │                   │
     │ Return          │  │ Return            │
     │ Result / Result │  │ Result<TResponse> │
     │     <T>         │  │                   │
     └────────┬────────┘  └────────┬──────────┘
              │                    │
              └────────┬───────────┘
                       │
                       ▼
     ┌─────────────────────────────────────────┐
     │    Result<TResponse>                    │
     │  ┌─────────────────────────────────────┐│
     │  │ Success: Contains Data/Unit         ││
     │  │ Failure: Contains Error Details     ││
     │  └─────────────────────────────────────┘│
     └────────────┬────────────────────────────┘
                  │
                  ▼
     ┌─────────────────────────────────────────┐
     │    Pipeline Returns Result              │
     │  (Commit transaction if successful)     │
     │  (Rollback if failed)                   │
     └────────────┬────────────────────────────┘
                  │
                  ▼
     ┌─────────────────────────────────────────┐
     │  Convert Result to HTTP Response        │
     │  result.Match(                          │
     │      success => Results.Ok(data),       │
     │      failure => CustomResults.Problem() │
     │  )                                      │
     └────────────┬────────────────────────────┘
                  │
                  ▼
     ┌─────────────────────────────────────────┐
     │         HTTP Response                   │
     │    200 Ok / 400 Bad Request / etc       │
     └─────────────────────────────────────────┘
```

## Type Hierarchy

```
                    ┌──────────────────────┐
                    │ IRequest<TResponse>  │ (Marker interface)
                    └──────────────────────┘
                              ▲
                    ┌─────────┼─────────┐
                    │         │         │
            ┌───────▼────┐ ┌──▼────────────┐
            │ ICommand   │ │ IQuery        │
            │(IRequest   │ │<TResponse>    │
            │ <Unit>)    │ │               │
            └───────┬────┘ │               │
                    │      │               │
        ┌───────────▼──┐   │               │
        │              │   │               │
┌───────▼──────┐  ┌────▼───▼────────────┐ │
│ ICommand     │  │ ICommand<TResponse> │ │
│ (No Return)  │  │ (Return TResponse)  │ │
└───────┬──────┘  └────┬─────────────────┘ │
        │              │                   │
┌───────▼───────────────▼──────┐          │
│    ICommandHandler           │          │
│  <TCommand>                  │          │
│  <TCommand, TResponse>       │          │
└──────────────────────────────┘          │
                                          │
                       ┌──────────────────▼───┐
                       │  IQueryHandler       │
                       │ <TQuery, TResponse>  │
                       └──────────────────────┘
```

## Execution Flow with Decorators

```
┌─────────────────────────────────────────────────────┐
│                   API Endpoint                      │
│   Receives: ICommandHandler<LoginUserCommand, JWT> │
│             (Injected from DI container)            │
└────────────────────┬────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────┐
│ LoggingDecorator<LoginUserCommand, JWT>             │
│                                                     │
│ ┌───────────────────────────────────────────────┐   │
│ │ logger.LogInformation("Processing command")   │   │
│ └───────────────────┬───────────────────────────┘   │
│                     │                               │
│     ┌───────────────▼───────────────┐               │
│     │ CALL NEXT HANDLER             │               │
│     └───────────────┬───────────────┘               │
│                     │                               │
└─────────────────────┼───────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────┐
│ ValidationDecorator<LoginUserCommand, JWT>          │
│                                                     │
│ ┌───────────────────────────────────────────────┐   │
│ │ Validate(command)                              │   │
│ │ - Check Email is not empty                     │   │
│ │ - Check Password is not empty                  │   │
│ │ - If validation fails → Return Result.Failure  │   │
│ └───────────────────┬───────────────────────────┘   │
│                     │                               │
│     ┌───────────────▼───────────────┐               │
│     │ CALL NEXT HANDLER             │               │
│     └───────────────┬───────────────┘               │
│                     │                               │
└─────────────────────┼───────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────┐
│ LoginUserCommandHandler (ACTUAL HANDLER)            │
│                                                     │
│ ┌───────────────────────────────────────────────┐   │
│ │ 1. Query database for user by email           │   │
│ │ 2. Hash and verify password                   │   │
│ │ 3. Generate JWT token                         │   │
│ │ 4. Return Result<JWT>                         │   │
│ └───────────────────────────────────────────────┘   │
└────────────────────┬────────────────────────────────┘
                     │
                     ▼
              Result<JWT> (Success/Failure)
                     │
                     ▼
┌─────────────────────────────────────────────────────┐
│ ValidationDecorator unwraps and returns result      │
└────────────────────┬────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────┐
│ LoggingDecorator logs result and returns            │
│ logger.LogInformation("Completed command")          │
└────────────────────┬────────────────────────────────┘
                     │
                     ▼
              Result<JWT>
                     │
                     ▼
┌─────────────────────────────────────────────────────┐
│ API Endpoint receives Result<JWT>                   │
│ result.Match(                                       │
│     jwt => Results.Ok(jwt),                         │
│     error => CustomResults.Problem(error)           │
│ )                                                   │
└────────────────────┬────────────────────────────────┘
                     │
                     ▼
          HTTP 200 Ok or 400 Bad Request
```

## Command vs Query Comparison

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  COMMAND                         │    QUERY                │
│  ──────────────────────────────  │    ────────────────────│
│                                  │                         │
│  Purpose: Modify State           │    Read Data            │
│  Side Effects: YES               │    No Side Effects      │
│  Idempotent: Usually NO          │    YES (always safe)    │
│  Can be Cached: NO               │    YES                  │
│  Publish Events: YES             │    NO                   │
│  Transactional: Usually YES      │    Usually NO           │
│                                  │                         │
│  Example:                        │    Example:             │
│  ┌────────────────────────────┐  │  ┌──────────────────┐   │
│  │ CreateOrderCommand         │  │  │ GetOrdersQuery   │   │
│  │  - AddressId: Guid         │  │  │  - UserId: Guid  │   │
│  │  - Items: OrderItem[]      │  │  │                  │   │
│  │  - Returns: OrderId        │  │  │  Returns:        │   │
│  └────────────────────────────┘  │  │  List<OrderDto>  │   │
│                                  │  └──────────────────┘   │
│  Handler Creates:                │                         │
│  ✓ Order entity                  │  Handler Queries:       │
│  ✓ Order items                   │  ✓ Filters by UserId    │
│  ✓ Publishes OrderCreatedEvent   │  ✓ Maps to DTOs         │
│  ✓ Commits transaction           │  ✓ Returns immediately  │
│                                  │                         │
└─────────────────────────────────────────────────────────────┘
```
