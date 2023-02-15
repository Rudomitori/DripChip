﻿using System.Data;
using DripChip.Domain.Exceptions;
using DripChip.Domain.Utils;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.LocationVisits;

public sealed class GetLocationVisits : IRequest<GetLocationVisits.Response>
{
    public long? VisitedByAnimalId { get; set; }
    public DateTime? MinVisitedAt { get; set; }
    public DateTime? MaxVisitedAt { get; set; }
    public required int Offset { get; set; }
    public required int Size { get; set; }

    public sealed class Response
    {
        public required List<LocationVisit> LocationVisits { get; set; }
    }

    public sealed class Handler : IRequestHandler<GetLocationVisits, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(
            GetLocationVisits request,
            CancellationToken cancellationToken
        )
        {
            var queryable = _dbContext.Set<LocationVisit>().AsNoTracking().AsQueryable();

            if (request.VisitedByAnimalId is { } visitedByAnimalId)
            {
                var animalExists = await _dbContext
                    .Set<Animal>()
                    .AnyAsync(x => x.Id == visitedByAnimalId, cancellationToken);

                if (!animalExists)
                    throw new NotFoundException(
                        $"Animal with id {visitedByAnimalId} was not found"
                    );

                queryable = queryable.Where(x => x.AnimalId == visitedByAnimalId);
            }

            if (request.MinVisitedAt is { } minVisitedAt)
                queryable = queryable.Where(x => x.VisitedAt >= minVisitedAt);

            if (request.MaxVisitedAt is { } maxVisitedAt)
                queryable = queryable.Where(x => x.VisitedAt <= maxVisitedAt);

            return new Response
            {
                LocationVisits = await queryable
                    .WithPaging(x => x.VisitedAt, request.Offset, request.Size)
                    .ToListAsync(cancellationToken)
            };
        }
    }

    public sealed class Validator : AbstractValidator<GetLocationVisits>
    {
        public Validator()
        {
            RuleFor(x => x.VisitedByAnimalId).GreaterThan(0).When(x => x.VisitedByAnimalId is { });
            RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Size).GreaterThan(0);
        }
    }
}
