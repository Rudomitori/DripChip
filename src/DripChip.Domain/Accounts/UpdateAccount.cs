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

public sealed class UpdateAccount : RequestBase<UpdateAccount.Response>
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }

    public sealed class Response
    {
        public Account Account { get; set; }
    }

    public sealed class Validator : AbstractValidator<UpdateAccount>
    {
        public Validator()
        {
            RuleFor(x => x.Id).IsValidId();
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<UpdateAccount, Response>
    {
        #region Constructor and dependencies

        private readonly UserManager<Account> _userManager;

        public Handler(UserManager<Account> userManager)
        {
            _userManager = userManager;
        }

        #endregion

        public async Task<Response> Handle(
            UpdateAccount request,
            CancellationToken cancellationToken
        )
        {
            if (request.Id != request.Context.UserId && request.Context.UserRole is not Role.Admin)
                throw new ForbiddenException($"You cannot update account with id {request.Id}");

            var account = await _userManager.Users.FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken: cancellationToken
            );

            if (account is null)
                throw new NotFoundException($"Account with id {request.Id} was not found");

            if (account.Role != request.Role && request.Context.UserRole is not Role.Admin)
                throw new ForbiddenException($"Only admin can change role");

            var emailIsAlreadyRegistered =
                account.NormalizedEmail != _userManager.NormalizeEmail(request.Email)
                && await _userManager.Users.AnyAsync(
                    x => x.NormalizedEmail == _userManager.NormalizeEmail(request.Email),
                    cancellationToken
                );

            if (emailIsAlreadyRegistered)
                throw new ConflictException("Account with the same email is already registered");

            account.FirstName = request.FirstName;
            account.LastName = request.LastName;
            account.Email = request.Email;
            account.PasswordHash = _userManager.PasswordHasher.HashPassword(
                account,
                request.Password
            );
            var identityResult = await _userManager.UpdateAsync(account);

            if (!identityResult.Succeeded)
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

            return new Response { Account = account, };
        }
    }
}
