using Kommunist.Core.Models;

namespace Kommunist.Core.Services.Interfaces;

public interface IFilterService
{
    void SetFilters(FilterOptions filters);
    FilterOptions GetFilters();
    void ClearFilters();
}