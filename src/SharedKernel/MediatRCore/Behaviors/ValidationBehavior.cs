// src/MediatRCore/Behaviors/ValidationBehavior.cs
using System.Diagnostics;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;

namespace MediatRCore.Behaviors;

/// <summary>
/// MediatR pipeline behavior for handling FluentValidation
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        if (!_validators.Any())
        {
            _logger.LogDebug("No validators found for {RequestName}", requestName);
            return await next();
        }

        _logger.LogDebug("Validating {RequestName}", requestName);

        var validationContext = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        stopwatch.Stop();

        if (failures.Any())
        {
            _logger.LogWarning(
                "Validation failed for {RequestName}. Errors: {Errors}, Duration: {Duration}ms",
                requestName,
                string.Join(", ", failures.Select(f => f.ErrorMessage)),
                stopwatch.ElapsedMilliseconds);

            throw new MyValidationException(
                "VALIDATION.ERROR",
                "One or more validation errors occurred",
                failures.Select(f => new ValidationError(
                    f.PropertyName,
                    f.ErrorCode,
                    f.ErrorMessage)).ToList());
        }

        _logger.LogDebug(
            "Validation succeeded for {RequestName}, Duration: {Duration}ms",
            requestName,
            stopwatch.ElapsedMilliseconds);

        return await next();
    }
}
