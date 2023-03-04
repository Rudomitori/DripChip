using System.Diagnostics;
using Common.Core.Clock;
using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Animals;

public class UpdateAnimal : RequestBase<UpdateAnimal.Response>
{
    public required long Id { get; set; }
    public float? Weight { get; set; }
    public float? Height { get; set; }
    public float? Length { get; set; }
    public Gender? Gender { get; set; }
    public LifeStatus? LifeStatus { get; set; }
    public int? ChipperId { get; set; }
    public long? ChippingLocationId { get; set; }
    public List<long>? AnimalTypeIdsToAdd { get; set; }
    public List<long>? AnimalTypeIdsToRemove { get; set; }

    public sealed class Response
    {
        public required Animal Animal { get; set; }
    }

    public sealed class Handler : IRequestHandler<UpdateAnimal, Response>
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
            UpdateAnimal request,
            CancellationToken cancellationToken
        )
        {
            var animal = await _dbContext
                .Set<Animal>()
                .AsSplitQuery()
                .Include(x => x.AnimalType2Animals)
                .Include(x => x.LocationVisits)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (animal is null)
                throw new NotFoundException($"Animal with id {request.Id} was not found");

            if (request.ChipperId is { } chipperId && animal.ChipperId != chipperId)
            {
                var accountWasFound = await _dbContext
                    .Set<Account>()
                    .AnyAsync(x => x.Id == chipperId, cancellationToken);

                if (!accountWasFound)
                    throw new NotFoundException($"Account with id {chipperId} was not found");

                animal.ChipperId = chipperId;
            }

            if (
                request.ChippingLocationId is { } chippingLocationId
                && animal.ChippingLocationId != chippingLocationId
            )
            {
                if (animal.LocationVisits!.FirstOrDefault()?.LocationId == chippingLocationId)
                    throw new ValidationException(
                        "The animal's first visited location has id that equals to new chipping location id"
                    );

                var locationWasFound = await _dbContext
                    .Set<Location>()
                    .AnyAsync(x => x.Id == chippingLocationId, cancellationToken);

                if (!locationWasFound)
                    throw new NotFoundException(
                        $"Location with id {chippingLocationId} was not found"
                    );

                animal.ChippingLocationId = chippingLocationId;
            }

            if (request.AnimalTypeIdsToAdd is { } animalTypeIdsToAdd)
            {
                var alreadyAddedTypeCount = animal.AnimalType2Animals!
                    .Join(animalTypeIdsToAdd, l => l.AnimalTypeId, r => r, (_, _) => Unit.Value)
                    .Count();

                if (alreadyAddedTypeCount > 0)
                    throw new ConflictException(
                        $"{alreadyAddedTypeCount} are already added to the animal"
                    );

                var foundedTypesCount = await _dbContext
                    .Set<AnimalType>()
                    .CountAsync(x => animalTypeIdsToAdd.Contains(x.Id), cancellationToken);

                var notFoundedTypesCount = animalTypeIdsToAdd.Count - foundedTypesCount;

                if (notFoundedTypesCount > 0)
                    throw new NotFoundException(
                        $"{notFoundedTypesCount} animal types were not found by ids"
                    );

                animal.AnimalType2Animals!.AddRange(
                    animalTypeIdsToAdd.Select(
                        x => new AnimalType2Animal { AnimalId = animal.Id, AnimalTypeId = x }
                    )
                );
            }

            if (request.AnimalTypeIdsToRemove is { } animalTypeIdsToRemove)
            {
                var countOfTypesThatAnimalDoesntHave = animalTypeIdsToRemove
                    .Except(animal.AnimalType2Animals!.Select(x => x.AnimalTypeId))
                    .Count();

                if (countOfTypesThatAnimalDoesntHave > 0)
                    throw new NotFoundException(
                        $"Animal doesn't have {countOfTypesThatAnimalDoesntHave} of types to delete"
                    );

                animal.AnimalType2Animals = animal.AnimalType2Animals!
                    .ExceptBy(animalTypeIdsToRemove, x => x.AnimalTypeId)
                    .ToList();

                if (!animal.AnimalType2Animals.Any())
                    throw new ValidationException("Animal must have at least one type");
            }

            animal.Weight = request.Weight ?? animal.Weight;
            animal.Height = request.Height ?? animal.Height;
            animal.Length = request.Length ?? animal.Length;
            animal.Gender = request.Gender ?? animal.Gender;

            if (request.LifeStatus is { } lifeStatus)
            {
                (animal.LifeStatus, animal.DeathDateTime) = (animal.LifeStatus, lifeStatus) switch
                {
                    (Entities.LifeStatus.Alive, Entities.LifeStatus.Alive)
                        => (Entities.LifeStatus.Alive, null as DateTime?),
                    (Entities.LifeStatus.Alive, Entities.LifeStatus.Dead)
                        => (Entities.LifeStatus.Dead, _clock.UtcNow),
                    (Entities.LifeStatus.Dead, Entities.LifeStatus.Alive)
                        => throw new ValidationException("Animal can not be resurrected"),
                    (Entities.LifeStatus.Dead, Entities.LifeStatus.Dead)
                        => (Entities.LifeStatus.Dead, animal.DeathDateTime),
                    _ => throw new UnreachableException()
                };
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response { Animal = animal };
        }
    }

    public sealed class Validation : AbstractValidator<UpdateAnimal>
    {
        public Validation()
        {
            RuleFor(x => x.Id).IsValidId();
            RuleFor(x => x.Weight).GreaterThan(0);
            RuleFor(x => x.Height).GreaterThan(0);
            RuleFor(x => x.Length).GreaterThan(0);
            RuleFor(x => x.ChipperId).IsValidId();
            RuleFor(x => x.ChippingLocationId).IsValidId();
            RuleFor(x => x.AnimalTypeIdsToAdd)
                .NotEmpty()
                .ForEach(x => x.IsValidId())
                .When(x => x.AnimalTypeIdsToAdd is { });
            RuleFor(x => x.AnimalTypeIdsToRemove)
                .NotEmpty()
                .ForEach(x => x.IsValidId())
                .When(x => x.AnimalTypeIdsToRemove is { });
            RuleFor(x => x.AnimalTypeIdsToAdd)
                .Must(
                    (request, idsToAdd) =>
                        idsToAdd!.Intersect(request.AnimalTypeIdsToRemove!).Any() is false
                )
                .WithMessage("Trying to add and remove the same type is meaningless")
                .When(x => x.AnimalTypeIdsToAdd is { } && x.AnimalTypeIdsToRemove is { });
        }
    }
}
