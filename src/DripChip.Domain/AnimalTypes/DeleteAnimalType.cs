using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.AnimalTypes;

public sealed class DeleteAnimalType : RequestBase<DeleteAnimalType.Response>
{
    public long Id { get; set; }

    public sealed class Response { }

    public sealed class Handler : IRequestHandler<DeleteAnimalType, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(
            DeleteAnimalType request,
            CancellationToken cancellationToken
        )
        {
            var animalType = await _dbContext
                .Set<AnimalType>()
                .Select(
                    x =>
                        new
                        {
                            Entity = x,
                            ThereAreAnimalsWithThisType = _dbContext
                                .Set<AnimalType2Animal>()
                                .Any(y => y.AnimalTypeId == x.Id)
                        }
                )
                .FirstOrDefaultAsync(x => x.Entity.Id == request.Id, cancellationToken);

            if (animalType is null)
                throw new NotFoundException($"Animal type with id {request.Id} was not found");

            if (animalType.ThereAreAnimalsWithThisType)
                throw new ValidationException($"There are animals of type with id {request.Id}");

            _dbContext.Remove(animalType.Entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response();
        }
    }

    public sealed class Validator : AbstractValidator<DeleteAnimalType>
    {
        public Validator()
        {
            RuleFor(x => x.Id).IsValidId();
        }
    }
}
