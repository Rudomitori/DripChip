using Common.Domain.Exceptions;
using DripChip.Domain.Locations;
using DripChip.Domain.Requests;
using DripChip.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace DripChip.Domain.Areas;

public sealed class CreateArea : QueryBase<CreateArea.Response>
{
    public string Name { get; set; }
    public LinearRing Geometry { get; set; }

    public sealed record Response(Area Area);

    public sealed class Validator : AbstractValidator<CreateArea>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Geometry.SRID).Equal(LocationsUtils.LonLatGeometryServices.DefaultSRID);
            RuleFor(x => x.Geometry)
                .Must(x => x.IsValid)
                .WithMessage("The area geometry must be valid");
        }
    }

    public sealed class Handler : IRequestHandler<CreateArea, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;

        public Handler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        public Task<Response> Handle(CreateArea request, CancellationToken cancellationToken)
        {
            if (request.Context.UserRole is not Role.Admin)
                throw new ForbiddenException("Only admin can create areas");

            request.Geometry.Normalize();
        }
    }
}
