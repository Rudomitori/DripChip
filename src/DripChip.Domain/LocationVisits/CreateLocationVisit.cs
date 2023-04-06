using Common.Core.Clock;
using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.LocationVisits;

public sealed class CreateLocationVisit : RequestBase<CreateLocationVisit.Response>
{
    public long AnimalId { get; set; }
    public long LocationId { get; set; }

    public sealed class Response
    {
        public LocationVisit LocationVisit { get; set; }
    }

    public sealed class Handler : IRequestHandler<CreateLocationVisit, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;
        private readonly IClock _clock;

        public Handler(DbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        #endregion

        public async Task<Response> Handle(
            CreateLocationVisit request,
            CancellationToken cancellationToken
        )
        {
            if (request.Context.UserRole is not Role.Admin and not Role.Chipper)
                throw new ForbiddenException("You can not create location visits");

            var animal = await _dbContext
                .Set<Animal>()
                .AsNoTracking()
                .Include(x => x.LocationVisits!.OrderBy(y => y.VisitedAt))
                .FirstOrDefaultAsync(x => x.Id == request.AnimalId, cancellationToken);

            if (animal is null)
                throw new NotFoundException($"Animal with id {request.AnimalId} was not found");

            if (animal.LifeStatus is LifeStatus.Dead)
                throw new ValidationException("Animal is dead");

            if (
                !animal.LocationVisits!.Any() && animal.ChippingLocationId == request.LocationId
                || animal.LocationVisits!.LastOrDefault()?.LocationId == request.LocationId
            )
                throw new ValidationException("The animal is already in the location");

            var location = await _dbContext
                .Set<Location>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.LocationId, cancellationToken);

            if (location is null)
                throw new NotFoundException($"Location with id {request.LocationId} was not found");

            var newLocationVisit = new LocationVisit
            {
                AnimalId = animal.Id,
                LocationId = location.Id,
                VisitedAt = _clock.UtcNow
            };

            _dbContext.Add(newLocationVisit);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response { LocationVisit = newLocationVisit };
        }
    }

    public sealed class Validator : AbstractValidator<CreateLocationVisit>
    {
        public Validator()
        {
            RuleFor(x => x.AnimalId).IsValidId();
            RuleFor(x => x.LocationId).IsValidId();
        }
    }
}
