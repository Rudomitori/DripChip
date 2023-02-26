using Common.Domain.ValidationRules;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.AnimalTypes;

public sealed class GetAnimalTypes : IRequest<GetAnimalTypes.Response>
{
    public List<long>? Ids { get; set; }
    public string? Type { get; set; }

    public sealed class Response
    {
        public required List<AnimalType> AnimalTypes { get; set; }
    }

    public sealed class Handler : IRequestHandler<GetAnimalTypes, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(
            GetAnimalTypes request,
            CancellationToken cancellationToken
        )
        {
            var queryable = _dbContext.Set<AnimalType>().AsQueryable();

            if (request.Ids is { })
                queryable = queryable.Where(x => request.Ids.Contains(x.Id));

            if (request.Type is { })
                queryable = queryable.Where(x => x.Type.ToUpper().Contains(request.Type.ToUpper()));

            return new Response { AnimalTypes = await queryable.ToListAsync(cancellationToken) };
        }
    }

    public sealed class Validator : AbstractValidator<GetAnimalTypes>
    {
        public Validator()
        {
            RuleFor(x => x.Ids).NotEmpty().When(x => x.Ids is { });
            RuleForEach(x => x.Ids).IsValidId().When(x => x.Ids is { });

            RuleFor(x => x.Type).NotEmpty().When(x => x.Type is { });
        }
    }
}
