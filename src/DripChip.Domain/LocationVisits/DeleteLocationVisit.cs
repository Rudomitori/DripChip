using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.LocationVisits;

public sealed class DeleteLocationVisit : RequestBase<DeleteLocationVisit.Response>
{
    public long AnimalId { get; set; }
    public long Id { get; set; }

    public sealed class Response { }

    public sealed class Handler : IRequestHandler<DeleteLocationVisit, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(
            DeleteLocationVisit request,
            CancellationToken cancellationToken
        )
        {
            var animal = await _dbContext
                .Set<Animal>()
                .Include(x => x.LocationVisits!.OrderBy(y => y.VisitedAt))
                .FirstOrDefaultAsync(x => x.Id == request.AnimalId, cancellationToken);

            if (animal is null)
                throw new NotFoundException($"Animal with id {request.AnimalId} was not found");

            var locationVisitToDelete = animal.LocationVisits!.FirstOrDefault(
                x => x.Id == request.Id
            );

            if (locationVisitToDelete is null)
                throw new NotFoundException(
                    $"Location visit with id {request.Id} was not found for animal with id {request.AnimalId}"
                );

            var prevLocationId =
                animal.LocationVisits!
                    .LastOrDefault(x => x.VisitedAt < locationVisitToDelete.VisitedAt)
                    ?.LocationId ?? animal.ChippingLocationId;

            var nextLocationVisit = animal.LocationVisits!.FirstOrDefault(
                x => x.VisitedAt > locationVisitToDelete.VisitedAt
            );

            if (nextLocationVisit?.LocationId == prevLocationId)
                animal.LocationVisits!.Remove(nextLocationVisit);

            animal.LocationVisits!.Remove(locationVisitToDelete);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response();
        }
    }

    public sealed class Validator : AbstractValidator<DeleteLocationVisit>
    {
        public Validator()
        {
            RuleFor(x => x.Id).IsValidId();
            RuleFor(x => x.AnimalId).IsValidId();
        }
    }
}
