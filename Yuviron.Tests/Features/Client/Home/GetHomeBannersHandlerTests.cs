using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Yuviron.Application.Features.Client.Home.Queries.GetHomeBanners;
using Yuviron.Domain.Entities;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Tests.Features.Client.Home;

public class GetHomeBannersHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Active_Banners_Randomized()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var utcNow = DateTime.UtcNow;

        var b1 = Banner.Create("B1", "url1", "target1", true, utcNow);
        var b2 = Banner.Create("B2", "url2", "target2", true, utcNow);
        var b3 = Banner.Create("B3", "url3", "target3", true, utcNow);

        dbContext.AddRange(b1, b2, b3);
        await dbContext.SaveChangesAsync();

        var handler = new GetHomeBannersHandler(dbContext, TimeProvider.System);

        var result = await handler.Handle(new GetHomeBannersQuery(Limit: 3), CancellationToken.None);

        result.Should().HaveCount(3);
        result.Select(x => x.Title).Should().Contain(new[] { "B1", "B2", "B3" });
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Targeting()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        var utcNow = DateTime.UtcNow;

        var genreId = Guid.NewGuid();
        var b1 = Banner.Create("USA Only", "url1", "target1", true, utcNow, targetCountries: "US");
        var b2 = Banner.Create("Rock Only", "url2", "target2", true, utcNow, targetGenres: genreId.ToString());
        var b3 = Banner.Create("Global", "url3", "target3", true, utcNow);

        dbContext.AddRange(b1, b2, b3);
        await dbContext.SaveChangesAsync();

        var handler = new GetHomeBannersHandler(dbContext, TimeProvider.System);

        // Case 1: User from UA - Should see Global only
        var resultUA = await handler.Handle(new GetHomeBannersQuery(CountryCode: "UA"), CancellationToken.None);
        resultUA.Should().HaveCount(1);
        resultUA.First().Title.Should().Be("Global");

        // Case 2: User from US - Should see USA Only + Global
        var resultUS = await handler.Handle(new GetHomeBannersQuery(CountryCode: "US"), CancellationToken.None);
        resultUS.Should().HaveCount(2);
        resultUS.Select(x => x.Title).Should().Contain(new[] { "USA Only", "Global" });

        // Case 3: User likes Rock - Should see Rock Only + Global
        var resultRock = await handler.Handle(new GetHomeBannersQuery(FavoriteGenreIds: new List<Guid> { genreId }), CancellationToken.None);
        resultRock.Should().HaveCount(2);
        resultRock.Select(x => x.Title).Should().Contain(new[] { "Rock Only", "Global" });
    }
}
