using Common.Domain.Exceptions;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.AnimalTypes;

public sealed class CreateAnimalType : RequestBase<CreateAnimalType.Response>
{
    public required string Type { get; set; }

    public sealed class Response
    {
        public required AnimalType AnimalType { get; set; }
    }

    public sealed class Handler : IRequestHandler<CreateAnimalType, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(
            CreateAnimalType request,
            CancellationToken cancellationToken
        )
        {
            var animalTypeAlreadyExists = await _dbContext
                .Set<AnimalType>()
                .AnyAsync(x => x.Type == request.Type, cancellationToken);

            if (animalTypeAlreadyExists)
                throw new ConflictException("Animal type with the same name already exists");

            var newAnimalType = new AnimalType { Type = request.Type };

            _dbContext.Add(newAnimalType);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response { AnimalType = newAnimalType };
        }
    }

    public sealed class Validator : AbstractValidator<CreateAnimalType>
    {
        public Validator()
        {
            RuleFor(x => x.Type).NotEmpty();
        }
    }
}
