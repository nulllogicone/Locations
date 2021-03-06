﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EnsureThat;

using Locations.Core.Application.DTOs;
using Locations.Core.Application.Features.Locations.Queries.GetNearestLocations;
using Locations.Core.Application.Interfaces.Services;
using Locations.Core.Domain.Entities;
using Locations.Core.Domain.Interfaces.Repositories;

namespace Locations.Infrastructure.Shared.Services
{
    public class NearestLocationsFinderServiceV1 : INearestLocationsFinderService
    {
        private readonly ILocationsRepositoryAsync _locationsRepository;

        public NearestLocationsFinderServiceV1(ILocationsRepositoryAsync locationsRepository)
        {
            EnsureArg.IsNotNull(locationsRepository, nameof(locationsRepository));

            _locationsRepository = locationsRepository;
        }

        public async Task<IEnumerable<LocationWithDistanceFromStartingPoint>> GetNearestLocations(StartingLocation startingLocation, int maxDistance, int maxResults)
        {
            EnsureArg.IsNotNull(startingLocation, nameof(startingLocation));
            EnsureArg.IsGte(maxDistance, 0, nameof(maxDistance));
            EnsureArg.IsGt(maxResults, 0, nameof(maxResults));

            // In the case of an actual database with lots of records, we can use caching to save time when 
            // fetching al the records.
            var allLocations = await _locationsRepository.GetAllAsync();

            var startLoc = new Location(startingLocation.Latitude, startingLocation.Longitude);

            // Time complexity depends on the size of M. Worst case if M is big then TC=O(M log M), else Omega(N)
            var res = allLocations
                .Select(l => new LocationWithDistanceFromStartingPoint(l, l.CalculateDistance(startLoc))) // Select: O(N)
                .Where(l => l.DistanceFromStartingPoint <= maxDistance) // Where: O(N)
                .OrderBy(l => l.DistanceFromStartingPoint) //O(M log M)
                .Take(maxResults); // Take: O(M)

            return res;
        }


    }
}
