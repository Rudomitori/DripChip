using System.Diagnostics;
using Common.Domain.Exceptions;
using DripChip.Domain.Requests;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DripChip.Domain.PipelineBehaviors;

public sealed class LoggingPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : RequestBase<TResponse>
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
        var userId = request.Context.UserId;
        _logger.LogDebug("Starting execute request \"{requestType}\"", requestTypeName);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Request \"{requestType}\" was executed successfully. Elapsed: {elapsed} ms. UserId: {userId}",
                requestTypeName,
                stopwatch.ElapsedMilliseconds,
                userId
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
                    "{exceptionType} occurred while executing request \"{requestType}\". Elapsed: {elapsed} ms. UserId: {userId}",
                    e.GetType().Name,
                    requestTypeName,
                    stopwatch.ElapsedMilliseconds,
                    userId
                );
            }
            else if (e is ValidationException or DomainExceptionBase)
            {
                _logger.LogWarning(
                    "{exceptionType} occurred while executing request \"{requestType}\". Elapsed: {elapsed} ms. UserId: {userId}",
                    e.GetType().Name,
                    requestTypeName,
                    stopwatch.ElapsedMilliseconds,
                    userId
                );
            }

            throw;
        }
    }
}
