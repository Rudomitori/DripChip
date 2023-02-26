using Common.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Location = DripChip.Entities.Location;

namespace DripChip.Domain.Locations.Requests;

public sealed class CreateLocation : IRequest<CreateLocation.Response>
{
    public required Point Coordinates { get; set; }

    public sealed class Response
    {
        public required Location Location { get; set; }
    }

    public sealed class Handler : IRequestHandler<CreateLocation, Response>
    {
        #region Constractor and dependencies

        private readonly DbContext _dbContext;
        private readonly LocationOptions _locationOptions;

        public Handler(DbContext dbContext, IOptions<LocationOptions> locationOptions)
        {
            _dbContext = dbContext;
            _locationOptions = locationOptions.Value;
        }

        #endregion

        public async Task<Response> Handle(
            CreateLocation request,
            CancellationToken cancellationToken
        )
        {
            var locationsAlreadyExists = await _dbContext
                .Set<Location>()
                .AnyAsync(
                    x =>
                        x.Coordinates.Distance(request.Coordinates)
                        <= _locationOptions.MinLocationDistance,
                    cancellationToken
                );

            if (locationsAlreadyExists)
                throw new ConflictException("Location with the same coordinates already exists");

            var newLocation = new Location { Coordinates = request.Coordinates };

            _dbContext.Add(newLocation);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response { Location = newLocation };
        }
    }

    public sealed class Validator : AbstractValidator<CreateLocation>
    {
        public Validator()
        {
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
