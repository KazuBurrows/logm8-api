using System.Net;
using LogMate.Domain.Models;

namespace LogMate.Application.Interfaces;

public interface IServiceOptionService
{
    Task<ServiceHierarchy> GetServiceOptionHierarchyAsync();
    Task<(HttpStatusCode Status, int Id)> AddServiceOptionAsync(AddServiceOptionRequest payload);
    Task<HttpStatusCode> AddParentServiceOptionAsync(AddParentOptionRequest payload);
    Task<HttpStatusCode> AddServiceOptionServiceTypeAsync(AddServiceTypeRequest payload);
    
}
