using System.Diagnostics;
using DripChip.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DripChip.Domain.PipelineBehaviors;

public sealed class LoggingPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    #region Constructor and dependencies

    private readonly ILogger<TRequest> _logger;

    public LoggingPipelineBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    #endregion


    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(
            "Starting execute request \"{requestType}\"",
            request.GetType().Name
        );

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Request \"{requestType}\" was executed successfully. Elapsed: {elapsed} ms.",
                request.GetType().Name,
                stopwatch.ElapsedMilliseconds
            );

            return response;
        }
        catch (InternalException e)
        {
            stopwatch.Stop();

            _logger.LogError(
                e,
                "Exception occurred while executing request \"{requestType}\". Elapsed: {elapsed} ms.",
                request.GetType().Name,
                stopwatch.ElapsedMilliseconds
            );

            throw;
        }
        catch (ValidationException e)
        {
            stopwatch.Stop();

            _logger.LogError(
                "Exception occurred while executing request \"{requestType}\". Elapsed: {elapsed} ms.",
                request.GetType().Name,
                stopwatch.ElapsedMilliseconds
            );

            throw;
        }
        catch (DomainExceptionBase e)
        {
            stopwatch.Stop();

            _logger.LogError(
                "Exception occurred while executing request \"{requestType}\". Elapsed: {elapsed} ms.",
                request.GetType().Name,
                stopwatch.ElapsedMilliseconds
            );

            throw;
        }
    }
}
