using Common.Core.Extensions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Animals;

public sealed class GetAnimals : RequestBase<GetAnimals.Response>
{
    public List<long>? Ids { get; set; }
    public DateTime? MinChippingDateTime { get; set; }
    public DateTime? MaxChippingDateTime { get; set; }
    public int? ChipperId { get; set; }
    public long? ChippingLocationId { get; set; }
    public LifeStatus? LifeStatus { get; set; }
    public Gender? Gender { get; set; }
    public required int Offset { get; set; }
    public required int Size { get; set; }

    public sealed class Response
    {
        public required List<Animal> Animals { get; set; }
    }

    public sealed class Handler : IRequestHandler<GetAnimals, Response>
    {
        #region Constructor ans dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(GetAnimals request, CancellationToken cancellationToken)
        {
            var queryable = _dbContext
                .Set<Animal>()
                .Include(x => x.LocationVisits)
                .Include(x => x.AnimalType2Animals)
                .AsNoTracking()
                .AsQueryable();

            if (request.Ids is { })
                queryable = queryable.Where(x => request.Ids.Contains(x.Id));

            if (request.MinChippingDateTime is { })
                queryable = queryable.Where(x => x.ChippingDateTime >= request.MinChippingDateTime);

            if (request.MaxChippingDateTime is { })
                queryable = queryable.Where(x => x.ChippingDateTime <= request.MaxChippingDateTime);

            if (request.ChipperId is { })
                queryable = queryable.Where(x => x.ChipperId == request.ChipperId);

            if (request.ChippingLocationId is { })
                queryable = queryable.Where(
                    x => x.ChippingLocationId == request.ChippingLocationId
                );

            if (request.LifeStatus is { })
                queryable = queryable.Where(x => x.LifeStatus == request.LifeStatus);

            if (request.Gender is { })
                queryable = queryable.Where(x => x.Gender == request.Gender);

            return new Response
            {
                Animals = await queryable
                    .WithPaging(x => x.Id, request.Offset, request.Size)
                    .ToListAsync(cancellationToken)
            };
        }
    }

    public sealed class Validator : AbstractValidator<GetAnimals>
    {
        public Validator()
        {
            RuleFor(x => x.Ids).NotEmpty().When(x => x.Ids is { });
            RuleForEach(x => x.Ids).IsValidId().When(x => x is { });
            RuleFor(x => x.ChipperId).IsValidId().When(x => x.ChipperId is { });
            RuleFor(x => x.ChippingLocationId)
                .IsValidId()
                .When(x => x.ChippingLocationId is { });

            RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Size).IsValidId();
        }
    }
}
