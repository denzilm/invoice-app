using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using FrontendMentor.InvoiceApp.Application.Abstractions.Repositories;
using FrontendMentor.InvoiceApp.Domain.Entities;

namespace FrontendMentor.InvoiceApp.Data.Repositories;

public sealed class CountryRepository : ICountryRepository
{
    private readonly DbSet<Country> _countries;

    public CountryRepository(ApplicationDbContext context)
    {
        _countries = context.Set<Country>();
    }

    public async IAsyncEnumerable<Country> FetchCountriesByNameAsync(
        string? name, int limit = 100, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _countries
            .Where(c => name == null || c.Name.Contains(name))
            .OrderBy(c => c.Name)
            .Take(limit)
            .AsAsyncEnumerable();

        await foreach (var country in query.WithCancellation(cancellationToken))
        {
            yield return country;
        }
    }
}
