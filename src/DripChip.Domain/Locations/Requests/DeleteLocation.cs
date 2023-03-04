using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Locations.Requests;

public sealed class DeleteLocation : RequestBase<DeleteLocation.Responce>
{
    public required long Id { get; set; }

    public sealed class Responce { }

    public sealed class Handler : IRequestHandler<DeleteLocation, Responce>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Responce> Handle(
            DeleteLocation request,
            CancellationToken cancellationToken
        )
        {
            var location = await _dbContext
                .Set<Location>()
                .Select(
                    x =>
                        new
                        {
                            Entity = x,
                            thereAreAnimalsThatAreChippedThere = _dbContext
                                .Set<Animal>()
                                .Any(y => y.ChippingLocationId == x.Id),
                            ThereAreRelatedLocationVisits = _dbContext
                                .Set<LocationVisit>()
                                .Any(y => y.LocationId == x.Id)
                        }
                )
                .FirstOrDefaultAsync(x => x.Entity.Id == request.Id, cancellationToken);

            if (location is null)
                throw new NotFoundException($"Location with id {request.Id} was not found");

            if (location.thereAreAnimalsThatAreChippedThere)
                throw new ValidationException(
                    $"There are animals that are chipped in location with id {request.Id}"
                );

            if (location.ThereAreRelatedLocationVisits)
                throw new ValidationException("There are related location visits");

            _dbContext.Remove(location.Entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Responce();
        }
    }

    public sealed class Validator : AbstractValidator<DeleteLocation>
    {
        public Validator()
        {
            RuleFor(x => x.Id).IsValidId();
        }
    }
}
