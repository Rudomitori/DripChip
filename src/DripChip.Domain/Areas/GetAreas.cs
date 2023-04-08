using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Areas;

public sealed class GetAreas : QueryBase<GetAreas.Response>
{
    public List<long>? Ids { get; set; }

    public sealed record Response(List<Area> Areas);

    public sealed class Validator : AbstractValidator<GetAreas>
    {
        public Validator()
        {
            RuleFor(x => x.Ids).NotEmpty().When(x => x.Ids is { });
            RuleForEach(x => x.Ids).IsValidId().When(x => x.Ids is { });
        }
    }

    public sealed class Handler : IRequestHandler<GetAreas, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(GetAreas request, CancellationToken cancellationToken)
        {
            var queryable = _dbContext.Set<Area>().AsQueryable();

            if (request.Ids is { } ids)
                queryable = queryable.Where(x => ids.Contains(x.Id));

            return new Response(Areas: await queryable.ToListAsync(cancellationToken));
        }
    }
}
