using Common.Domain.Exceptions;
using Common.Domain.ValidationRules;
using DripChip.Domain.Requests;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Location = DripChip.Entities.Location;

namespace DripChip.Domain.Locations.Requests;

public sealed class UpdateLocation : RequestBase<UpdateLocation.Response>
{
    public required long Id { get; set; }
    public required Point Coordinates { get; set; }

    public sealed class Response
    {
        public required Entities.Location Location { get; set; }
    }

    public sealed class Handler : IRequestHandler<UpdateLocation, Response>
    {
        #region Constructor and dependencies

        private readonly DbContext _dbContext;
        private readonly LocationOptions _locationOptions;

        public Handler(DbContext dbContext, IOptions<LocationOptions> locationOptions)
        {
            _dbContext = dbContext;
            _locationOptions = locationOptions.Value;
        }

        #endregion

        public async Task<Response> Handle(
            UpdateLocation request,
            CancellationToken cancellationToken
        )
        {
            var location = await _dbContext
                .Set<Location>()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (location is null)
                throw new NotFoundException($"Location with id {request.Id} was not found");

            var locationWithSameCoordinatesExists = await _dbContext
                .Set<Location>()
                .AnyAsync(
                    x =>
                        x.Id != location.Id
                        && x.Coordinates.Distance(request.Coordinates)
                            <= _locationOptions.MinLocationDistance,
                    cancellationToken
                );

            if (locationWithSameCoordinatesExists)
                throw new ConflictException("Location with the same coordinates already exists");

            location.Coordinates = request.Coordinates;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response { Location = location };
        }
    }

    public sealed class Validator : AbstractValidator<UpdateLocation>
    {
        public Validator()
        {
            RuleFor(x => x.Id).IsValidId();

            RuleFor(x => x.Coordinates)
                .Must(x => x.IsValid)
                .WithMessage("Coordinates must be valid");

            RuleFor(x => x.Coordinates.SRID)
                .Equal(LocationsUtils.LonLatGeometryServices.DefaultSRID);
            RuleFor(x => x.Coordinates.X).GreaterThanOrEqualTo(-180).LessThanOrEqualTo(180);
            RuleFor(x => x.Coordinates.Y).GreaterThanOrEqualTo(-90).LessThanOrEqualTo(90);
        }
    }
}
