using System.Net;

public interface IServiceOptionService
{
    Task<ServiceHierarchy> GetServiceOptionHierarchyAsync();
    Task<HttpStatusCode> AddServiceOptionAsync(AddServiceOptionRequest payload);
    Task<HttpStatusCode> AddParentServiceOptionAsync(AddParentOptionRequest payload);
    Task<HttpStatusCode> AddServiceOptionServiceTypeAsync(AddServiceTypeRequest payload);
    // Task UpdateServiceOptionAsync(ServiceOption serviceOption);
    // Task DeleteServiceOptionAsync(int id);
}