﻿using Common.Domain.Exceptions;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Domain.Accounts;

public sealed class CreateAccount : RequestBase<CreateAccount.Response>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }

    public sealed class Response
    {
        public Account Account { get; set; }
    }

    public sealed class Validator : AbstractValidator<CreateAccount>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<CreateAccount, Response>
    {
        #region Constructor and dependencies

        private readonly UserManager<Account> _userManager;

        public Handler(UserManager<Account> userManager)
        {
            _userManager = userManager;
        }

        #endregion

        public async Task<Response> Handle(
            CreateAccount request,
            CancellationToken cancellationToken
        )
        {
            if (request.Role is not Role.User && request.Context.UserRole is not Role.Admin)
                throw new ForbiddenException(
                    $"Only admin can create an account with role {request.Role}"
                );

            var emailIsAlreadyRegistered = await _userManager.Users.AnyAsync(
                x => x.NormalizedEmail == _userManager.NormalizeEmail(request.Email),
                cancellationToken
            );

            if (emailIsAlreadyRegistered)
                throw new ConflictException("Account with the same email is already registered");

            var newAccount = new Account
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            var identityResult = await _userManager.CreateAsync(newAccount, request.Password);

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

            return new Response { Account = newAccount, };
        }
    }
}
