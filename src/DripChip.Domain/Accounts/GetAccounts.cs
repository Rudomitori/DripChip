using Common.Core.Extensions;
using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Accounts;

public sealed class GetAccounts : RequestBase<GetAccounts.Response>
{
    public List<int>? Ids { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }

    public int Offset { get; set; }
    public int Size { get; set; }

    public sealed class Response
    {
        public List<Account> Accounts { get; set; }
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
            var userId = request.Context.UserId!.Value;
            var queryable = _dbContext.Set<Account>().AsQueryable();

            queryable = (request.Ids, request.Context.UserRole) switch
            {
                (not { }, Role.Admin) => queryable,
                ({ } ids, Role.Admin) => queryable.Where(x => ids.Contains(x.Id)),
                ({ } ids, _) when !ids.SequenceEqual(new[] { userId }) => 
                    throw new ForbiddenException("You cannot read not your accounts"),
                _ => queryable.Where(x => x.Id == userId)
            };

            var notAllowedFilterUsed = 
                request.Context.UserRole is not Role.Admin 
                    && request is not {FirstName: null, LastName: null, Email: null};

            if (notAllowedFilterUsed)
                throw new ForbiddenException("Only admin can use additional filters");

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
            RuleForEach(x => x.Ids).IsValidId().When(x => x.Ids is { });

            RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Size).IsValidId();
        }
    }
}
