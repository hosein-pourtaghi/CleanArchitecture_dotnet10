using System.Diagnostics;
using LoadSimulator.Utilities;
using LoadSimulator.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoadSimulator.Services;

/// <summary>
/// Service for simulating individual user behavior
/// </summary>
public class UserSimulationService : IUserSimulationService
{
    private readonly IAuthClient _authClient;
    private readonly IProductClient _productClient;
    private readonly IOrderClient _orderClient;
    private readonly IProductCacheService _cacheService;
    private readonly MockDataGenerator _dataGenerator;
    private readonly ILogger<UserSimulationService> _logger;
    private readonly SimulatorSettings _settings;

    public UserSimulationService(
        IAuthClient authClient,
        IProductClient productClient,
        IOrderClient orderClient,
        IProductCacheService cacheService,
        MockDataGenerator dataGenerator,
        ILogger<UserSimulationService> logger,
        IOptions<SimulatorSettings> settings)
    {
        _authClient = authClient ?? throw new ArgumentNullException(nameof(authClient));
        _productClient = productClient ?? throw new ArgumentNullException(nameof(productClient));
        _orderClient = orderClient ?? throw new ArgumentNullException(nameof(orderClient));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _dataGenerator = dataGenerator ?? throw new ArgumentNullException(nameof(dataGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<UserSimulationResult> SimulateUserAsync(
        int userId,
        int ordersPerUser,
        int maxProductsPerOrder,
        int delayMinMs,
        int delayMaxMs,
        double normalDistMean,
        double normalDistStdDev,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new UserSimulationResult { UserId = userId };
        
        try
        {
            // Step 1: Register or Login
            var email = _dataGenerator.GenerateEmail();
            var password = _dataGenerator.GeneratePassword();
            var username = _dataGenerator.GenerateUsername();

            _logger.LogInformation("User {UserId}: Starting simulation with email {Email}", userId, email);

            var userSession = await _authClient.RegisterAsync(email, password, username, cancellationToken)
                .ConfigureAwait(false);

            if (userSession == null)
            {
                userSession = await _authClient.LoginAsync(email, password, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (userSession == null)
            {
                result.Success = false;
                result.Errors.Add("Failed to authenticate");
                _logger.LogWarning("User {UserId}: Authentication failed", userId);
                return result;
            }

            await DelayAsync(delayMinMs, delayMaxMs, normalDistMean, normalDistStdDev, cancellationToken)
                .ConfigureAwait(false);

            // Step 2: Simulate Orders
            for (int orderIndex = 0; orderIndex < ordersPerUser && !cancellationToken.IsCancellationRequested; orderIndex++)
            {
                var orderStopwatch = Stopwatch.StartNew();

                try
                {
                    // Get products
                    var page = _dataGenerator.GeneratePage(5);
                    var products = await _cacheService.GetCachedProductsAsync(
                        $"page_{page}",
                        async () => await _productClient.GetProductsAsync(
                            page,
                            _settings.DefaultPageSize,
                            userSession.JwtToken,
                            cancellationToken).ConfigureAwait(false),
                        cancellationToken).ConfigureAwait(false);

                    if (products == null || products.Count == 0)
                    {
                        result.OrdersFailed++;
                        result.Errors.Add($"No products available");
                        continue;
                    }

                    await DelayAsync(delayMinMs, delayMaxMs, normalDistMean, normalDistStdDev, cancellationToken)
                        .ConfigureAwait(false);

                    // Create order
                    var order = await _orderClient.CreateOrderAsync(userSession.JwtToken, cancellationToken)
                        .ConfigureAwait(false);

                    if (order == null)
                    {
                        result.OrdersFailed++;
                        result.Errors.Add("Failed to create order");
                        continue;
                    }

                    await DelayAsync(delayMinMs, delayMaxMs, normalDistMean, normalDistStdDev, cancellationToken)
                        .ConfigureAwait(false);

                    // Add items to order
                    var itemCount = _dataGenerator.GenerateQuantity(maxProductsPerOrder);
                    var selectedProductIds = _dataGenerator.GenerateProductIds(itemCount, maxProductsPerOrder);

                    var addItemsSuccess = true;
                    foreach (var productId in selectedProductIds)
                    {
                        var product = products.FirstOrDefault(p => p.Id == productId);
                        if (product == null)
                            product = products[Random.Shared.Next(products.Count)];

                        var quantity = _dataGenerator.GenerateQuantity(5);
                        var added = await _orderClient.AddOrderItemAsync(
                            order.Id,
                            product.Id,
                            quantity,
                            product.Price,
                            userSession.JwtToken,
                            cancellationToken).ConfigureAwait(false);

                        if (!added)
                        {
                            addItemsSuccess = false;
                            break;
                        }

                        await DelayAsync(delayMinMs / 2, delayMaxMs / 2, normalDistMean / 2, normalDistStdDev / 2, cancellationToken)
                            .ConfigureAwait(false);
                    }

                    if (!addItemsSuccess)
                    {
                        result.OrdersFailed++;
                        result.Errors.Add($"Failed to add items to order {order.Id}");
                        continue;
                    }

                    await DelayAsync(delayMinMs, delayMaxMs, normalDistMean, normalDistStdDev, cancellationToken)
                        .ConfigureAwait(false);

                    // Submit order
                    var submitted = await _orderClient.SubmitOrderAsync(
                        order.Id,
                        userSession.JwtToken,
                        cancellationToken).ConfigureAwait(false);

                    if (submitted)
                    {
                        result.OrdersCreated++;
                        _logger.LogDebug("User {UserId}: Order {OrderId} submitted successfully", userId, order.Id);
                    }
                    else
                    {
                        result.OrdersFailed++;
                        result.Errors.Add($"Failed to submit order {order.Id}");
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    result.OrdersFailed++;
                    result.Errors.Add($"Order error: {ex.Message}");
                    _logger.LogError(ex, "User {UserId}: Error during order simulation", userId);
                }
                finally
                {
                    orderStopwatch.Stop();
                    result.TotalResponseTimeMs += orderStopwatch.ElapsedMilliseconds;
                }
            }

            result.Success = true;
            _logger.LogInformation(
                "User {UserId}: Completed with {OrdersCreated} successful orders and {OrdersFailed} failures",
                userId,
                result.OrdersCreated,
                result.OrdersFailed);
        }
        catch (OperationCanceledException)
        {
            result.Errors.Add("Simulation cancelled");
            _logger.LogInformation("User {UserId}: Simulation cancelled", userId);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Critical error: {ex.Message}");
            _logger.LogError(ex, "User {UserId}: Critical error during simulation", userId);
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    private async Task DelayAsync(
        int minMs,
        int maxMs,
        double normalDistMean,
        double normalDistStdDev,
        CancellationToken cancellationToken)
    {
        try
        {
            var delay = _dataGenerator.GenerateNormalDistributionThinkTime(normalDistMean, normalDistStdDev);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }
}
