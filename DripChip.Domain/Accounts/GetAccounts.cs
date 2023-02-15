using DripChip.Domain.Utils;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Accounts;

public sealed class GetAccounts : IRequest<GetAccounts.Response>
{
    public List<int>? Ids { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }

    public required int Offset { get; set; }
    public required int Size { get; set; }

    public sealed class Response
    {
        public required List<Account> Accounts { get; set; }
    }

    public sealed class Handler : IRequestHandler<GetAccounts, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(GetAccounts request, CancellationToken cancellationToken)
        {
            var queryable = _dbContext.Set<Account>().AsQueryable();

            if (request.Ids is { })
                queryable = queryable.Where(x => request.Ids.Contains(x.Id));

            if (request.FirstName is { })
                queryable = queryable.Where(
                    x => x.FirstName.ToUpper().Contains(request.FirstName.ToUpper())
                );

            if (request.LastName is { })
                queryable = queryable.Where(
                    x => x.LastName.ToUpper().Contains(request.LastName.ToUpper())
                );

            if (request.Email is { })
                queryable = queryable.Where(
                    x => x.Email.ToUpper().Contains(request.Email.ToUpper())
                );

            var accounts = await queryable
                .WithPaging(x => x.Id, request.Offset, request.Size)
                .ToListAsync(cancellationToken);

            return new Response { Accounts = accounts };
        }
    }

    public sealed class Validator : AbstractValidator<GetAccounts>
    {
        public Validator()
        {
            RuleFor(x => x.Ids).NotEmpty().When(x => x.Ids is { });
            RuleForEach(x => x.Ids).GreaterThan(0).When(x => x.Ids is { });

            RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Size).GreaterThan(0);
        }
    }
}
