using System.Diagnostics;
using Common.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DripChip.Domain.PipelineBehaviors;

public sealed class LoggingPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    #region Constructor and dependencies

    private readonly ILogger _logger;

    public LoggingPipelineBehaviour(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("Domain.LoggingPipelineBehaviour");
    }

    #endregion


    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var requestTypeName = request.GetType().Name;
        _logger.LogDebug("Starting execute request \"{requestType}\"", requestTypeName);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Request \"{requestType}\" was executed successfully. Elapsed: {elapsed} ms.",
                requestTypeName,
                stopwatch.ElapsedMilliseconds
            );

            return response;
        }
        catch (Exception e)
        {
            stopwatch.Stop();

            if (e is InternalException internalException)
            {
                _logger.LogError(
                    internalException.InnerException,
                    "{exceptionType} occurred while executing request \"{requestType}\". Elapsed: {elapsed} ms.",
                    e.GetType().Name,
                    requestTypeName,
                    stopwatch.ElapsedMilliseconds
                );
            }
            else if (e is ValidationException or DomainExceptionBase)
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    "{exceptionType} occurred while executing request \"{requestType}\". Elapsed: {elapsed} ms.",
                    e.GetType().Name,
                    requestTypeName,
                    stopwatch.ElapsedMilliseconds
                );
            }

            throw;
        }
    }
}
