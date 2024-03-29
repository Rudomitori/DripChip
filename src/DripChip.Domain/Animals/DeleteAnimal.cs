﻿using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Animals;

public sealed class DeleteAnimal : RequestBase<DeleteAnimal.Response>
{
    public long Id { get; set; }

    public sealed class Response { }

    public sealed class Handler : IRequestHandler<DeleteAnimal, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(
            DeleteAnimal request,
            CancellationToken cancellationToken
        )
        {
            if (request.Context.UserRole is not Role.Admin)
                throw new ForbiddenException("Only admin can delete animals");

            var dbResponse = await _dbContext
                .Set<Animal>()
                .Select(x => new { Animal = x, HasLocationVisits = x.LocationVisits!.Any() })
                .FirstOrDefaultAsync(x => x.Animal.Id == request.Id, cancellationToken);

            if (dbResponse is null)
                throw new NotFoundException($"Animal with id {request.Id} was not found");

            if (dbResponse.HasLocationVisits)
                throw new ValidationException(
                    $"Animal with id {request.Id} left the chipping location"
                );

            var animal = dbResponse.Animal;

            _dbContext.Remove(animal);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response();
        }
    }

    public sealed class Validator : AbstractValidator<DeleteAnimal>
    {
        public Validator()
        {
            RuleFor(x => x.Id).IsValidId();
        }
    }
}
