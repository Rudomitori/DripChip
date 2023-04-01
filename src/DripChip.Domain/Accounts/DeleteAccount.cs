using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Accounts;

public sealed class DeleteAccount : RequestBase<DeleteAccount.Response>
{
    public int Id { get; set; }

    public sealed class Response
    {
        public Account Account { get; set; }
    }

    public sealed class Validator : AbstractValidator<DeleteAccount>
    {
        public Validator()
        {
            RuleFor(x => x.Id).IsValidId();
        }
    }

    public sealed class Handler : IRequestHandler<DeleteAccount, Response>
    {
        #region Constructor and dependencies

        private readonly UserManager<Account> _userManager;
        private readonly DbContext _dbContext;

        public Handler(UserManager<Account> userManager, DbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        #endregion

        public async Task<Response> Handle(
            DeleteAccount request,
            CancellationToken cancellationToken
        )
        {
            if (request.Id != request.Context.UserId)
                throw new ForbiddenException($"You cannot delete account with id {request.Id}");

            var dbResponse = await _dbContext
                .Set<Account>()
                .Select(
                    x =>
                        new
                        {
                            Account = x,
                            HasAnimals = _dbContext.Set<Animal>().Any(y => y.ChipperId == x.Id)
                        }
                )
                .FirstOrDefaultAsync(
                    x => x.Account.Id == request.Id,
                    cancellationToken: cancellationToken
                );

            if (dbResponse is null)
                throw new NotFoundException($"Account with id {request.Id} was not found");

            if (dbResponse.HasAnimals)
                throw new ValidationException("Account is linked to animals");

            var account = dbResponse.Account;

            var identityResult = await _userManager.DeleteAsync(account);

            if (!identityResult.Succeeded)
            {
                throw new ValidationException(
                    identityResult.Errors.Select(
                        x =>
                            x.Code switch
                            {
                                "PasswordRequiresNonAlphanumeric"
                                or "PasswordRequiresDigit"
                                or "PasswordRequiresUpper"
                                or "PasswordTooShort"
                                    => new ValidationFailure(
                                        nameof(CreateAccount.Password),
                                        x.Description
                                    ),
                                _
                                    => new ValidationFailure(
                                        "other",
                                        $"{x.Description} Code: {x.Code}"
                                    ),
                            }
                    )
                );
            }

            return new Response { Account = account };
        }
    }
}
