﻿using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.AnimalTypes;

public sealed class UpdateAnimalType : RequestBase<UpdateAnimalType.Response>
{
    public long Id { get; set; }
    public string Type { get; set; }

    public sealed class Response
    {
        public AnimalType AnimalType { get; set; }
    }

    public sealed class Handler : IRequestHandler<UpdateAnimalType, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(
            UpdateAnimalType request,
            CancellationToken cancellationToken
        )
        {
            if (request.Context.UserRole is not Role.Admin and not Role.Chipper)
                throw new ForbiddenException("You can not update animal types");

            var animalType = await _dbContext
                .Set<AnimalType>()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (animalType is null)
                throw new NotFoundException($"Animal type with id {request.Id} was not found");

            var animalTypeAlreadyExists = await _dbContext
                .Set<AnimalType>()
                .AnyAsync(x => x.Id != request.Id && x.Type == request.Type, cancellationToken);

            if (animalTypeAlreadyExists)
                throw new ConflictException("Animal type with the same name already exists");

            animalType.Type = request.Type;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response { AnimalType = animalType };
        }
    }

    public sealed class Validator : AbstractValidator<UpdateAnimalType>
    {
        public Validator()
        {
            RuleFor(x => x.Id).IsValidId();
            RuleFor(x => x.Type).NotEmpty();
        }
    }
}
