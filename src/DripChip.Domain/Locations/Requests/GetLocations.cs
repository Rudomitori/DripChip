using Common.Domain.ValidationRules;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Locations.Requests;

public sealed class GetLocations : IRequest<GetLocations.Responce>
{
    public List<long>? Ids { get; set; }

    public sealed class Responce
    {
        public required List<Location> Locations { get; set; }
    }

    public sealed class Handler : IRequestHandler<GetLocations, Responce>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Responce> Handle(
            GetLocations request,
            CancellationToken cancellationToken
        )
        {
            var queryable = _dbContext.Set<Location>().AsQueryable();

            if (request.Ids is { })
                queryable = queryable.Where(x => request.Ids.Contains(x.Id));

            return new Responce { Locations = await queryable.ToListAsync(cancellationToken) };
        }
    }

    public sealed class Validator : AbstractValidator<GetLocations>
    {
        public Validator()
        {
            RuleFor(x => x.Ids).NotEmpty().When(x => x.Ids is { });
            RuleForEach(x => x.Ids).IsValidId().When(x => x.Ids is { });
        }
    }
}