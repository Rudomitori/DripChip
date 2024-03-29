﻿using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.LocationVisits;

public sealed class UpdateLocationVisit : RequestBase<UpdateLocationVisit.Response>
{
    public long Id { get; set; }
    public long AnimalId { get; set; }
    public long LocationId { get; set; }

    public sealed class Response
    {
        public LocationVisit LocationVisit { get; set; }
    }

    public sealed class Handler : IRequestHandler<UpdateLocationVisit, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(
            UpdateLocationVisit request,
            CancellationToken cancellationToken
        )
        {
            if (request.Context.UserRole is not Role.Admin and not Role.Chipper)
                throw new ForbiddenException("You can not update location visits");

            var animal = await _dbContext
                .Set<Animal>()
                .Include(x => x.LocationVisits!.OrderBy(y => y.VisitedAt))
                .FirstOrDefaultAsync(x => x.Id == request.AnimalId, cancellationToken);

            if (animal is null)
                throw new NotFoundException($"Animal with id {request.AnimalId} was not found");

            var locationVisitToUpdate = animal.LocationVisits!.FirstOrDefault(
                x => x.Id == request.Id
            );

            if (locationVisitToUpdate is null)
                throw new NotFoundException(
                    $"Location visit with id {request.Id} was not found for animal with id {request.AnimalId}"
                );

            if (locationVisitToUpdate.LocationId == request.LocationId)
                throw new ValidationException(
                    $"Location visit with {request.Id} has the same location id"
                );

            var prevLocationId =
                animal.LocationVisits!
                    .LastOrDefault(x => x.VisitedAt < locationVisitToUpdate.VisitedAt)
                    ?.LocationId ?? animal.ChippingLocationId;

            if (prevLocationId == request.LocationId)
                throw new ValidationException(
                    $"The previous location visit has the same location id"
                );

            var nextLocationId = animal.LocationVisits!
                .FirstOrDefault(x => x.VisitedAt > locationVisitToUpdate.VisitedAt)
                ?.LocationId;

            if (nextLocationId == request.LocationId)
                throw new ValidationException($"The next location visit has the same location id");

            var locationExists = await _dbContext
                .Set<Location>()
                .AnyAsync(x => x.Id == request.LocationId, cancellationToken);
            if (!locationExists)
                throw new NotFoundException($"Location with id {request.LocationId} was not found");

            locationVisitToUpdate.LocationId = request.LocationId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response { LocationVisit = locationVisitToUpdate };
        }
    }

    public sealed class Validator : AbstractValidator<UpdateLocationVisit>
    {
        public Validator()
        {
            RuleFor(x => x.Id).IsValidId();
            RuleFor(x => x.AnimalId).IsValidId();
            RuleFor(x => x.LocationId).IsValidId();
        }
    }
}
