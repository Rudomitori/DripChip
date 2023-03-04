using Common.Core.Clock;
using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Animals;

public class CreateAnimal : RequestBase<CreateAnimal.Response>
{
    public required List<long> AnimalTypeIds { get; set; }
    public required float Weight { get; set; }
    public required float Length { get; set; }
    public required float Height { get; set; }
    public required Gender Gender { get; set; }
    public required int ChipperId { get; set; }
    public required long ChippingLocationId { get; set; }

    public sealed class Response
    {
        public required Animal Animal { get; set; }
    }

    public sealed class Handler : IRequestHandler<CreateAnimal, Response>
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
            CreateAnimal request,
            CancellationToken cancellationToken
        )
        {
            var foundAnimalTypesCount = await _dbContext
                .Set<AnimalType>()
                .CountAsync(x => request.AnimalTypeIds.Contains(x.Id), cancellationToken);

            if (foundAnimalTypesCount < request.AnimalTypeIds.Count)
                throw new NotFoundException(
                    $"{request.AnimalTypeIds.Count - foundAnimalTypesCount} animal types were not found"
                );

            var chipperWasFound = await _dbContext
                .Set<Account>()
                .AnyAsync(x => x.Id == request.ChipperId, cancellationToken);

            if (!chipperWasFound)
                throw new NotFoundException($"Account with id {request.ChipperId} was not found");

            var locationWasFound = await _dbContext
                .Set<Location>()
                .AnyAsync(x => x.Id == request.ChippingLocationId, cancellationToken);

            if (!locationWasFound)
                throw new NotFoundException(
                    $"Location with id {request.ChippingLocationId} was not found"
                );

            var newAnimal = new Animal
            {
                AnimalType2Animals = request.AnimalTypeIds
                    .Select(x => new AnimalType2Animal { AnimalTypeId = x })
                    .ToList(),
                ChipperId = request.ChipperId,
                ChippingDateTime = _clock.UtcNow,
                ChippingLocationId = request.ChippingLocationId,
                Weight = request.Weight,
                Length = request.Length,
                Height = request.Height,
                Gender = request.Gender,
                LifeStatus = LifeStatus.Alive,
                LocationVisits = new List<LocationVisit>()
            };

            _dbContext.Add(newAnimal);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response { Animal = newAnimal };
        }
    }

    public sealed class Validator : AbstractValidator<CreateAnimal>
    {
        public Validator()
        {
            RuleFor(x => x.AnimalTypeIds).NotEmpty();
            RuleForEach(x => x.AnimalTypeIds).IsValidId();
            RuleFor(x => x.Weight).GreaterThan(0);
            RuleFor(x => x.Length).GreaterThan(0);
            RuleFor(x => x.Height).GreaterThan(0);
            RuleFor(x => x.ChipperId).IsValidId();
            RuleFor(x => x.ChippingLocationId).IsValidId();
        }
    }
}
