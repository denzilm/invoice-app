using FrontendMentor.InvoiceApp.Domain.Entities;

namespace FrontendMentor.InvoiceApp.Application.Abstractions.Repositories;

public interface ICountryRepository
{
    /// <summary>
    /// Retrieves a collection of countries with names matching or partially matching the provided name.
    /// </summary>
    /// <param name="name">The partial or full name of the country to search. Can be null or empty, in which case all countries are retrieved.</param>
    /// <param name="limit">The maximum number of countries to return. Defaults to 100.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation</param>
    /// <returns>An asynchronous stream of <see cref="Country"/> Countries that match the search criteria</returns>
    IAsyncEnumerable<Country> FetchCountriesByNameAsync(string? name, int limit = 100, CancellationToken cancellationToken = default);
}
