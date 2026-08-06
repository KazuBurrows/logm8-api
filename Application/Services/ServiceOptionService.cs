using System.Net;
using LogMate.Application.Interfaces;
using LogMate.Domain.Models;
using LogMate.Infrastructure.Data.Interfaces;

namespace LogMate.Application.Services;

public class ServiceOptionService : IServiceOptionService
{
    private readonly IServiceOptionRepository _repo;

    public ServiceOptionService(IServiceOptionRepository serviceOptionRepository)
    {
        _repo = serviceOptionRepository;
    }

    public async Task<ServiceHierarchy> GetServiceOptionHierarchyAsync()
    {
        var motorbikeOptions = await _repo.GetMotorbikeServiceOptions();
        var ownershipOptions = await _repo.GetOwnershipServiceOptions();

        return new ServiceHierarchy
        {
            MotorbikeOptions = BuildHierarchy(motorbikeOptions),
            OwnershipOptions = BuildHierarchy(ownershipOptions),
        };
    }

    public async Task<(HttpStatusCode Status, int Id)> AddServiceOptionAsync(AddServiceOptionRequest payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Name))
            return (HttpStatusCode.BadRequest, 0);

        var (serviceOptionId, alreadyExisted) = await _repo.AddServiceOptionAsync(
            payload.Name.Trim(),
            payload.Description,
            payload.CategoryId
        );

        if (alreadyExisted)
            return (HttpStatusCode.Conflict, serviceOptionId);

        var status = serviceOptionId > 0 ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
        return (status, serviceOptionId);
    }

    public async Task<HttpStatusCode> AddParentServiceOptionAsync(AddParentOptionRequest payload)
    {
        if (string.IsNullOrWhiteSpace(payload.OptionId.ToString()) || string.IsNullOrWhiteSpace(payload.ParentId.ToString()))
            return HttpStatusCode.BadRequest;

        int rowsAffected = await _repo.AddParentServiceOptionAsync(
            payload.OptionId,
            payload.ParentId
        );

        return rowsAffected > 0 ? HttpStatusCode.OK : HttpStatusCode.Conflict;
    }

    public async Task<HttpStatusCode> AddServiceOptionServiceTypeAsync(AddServiceTypeRequest payload)
    {
        if (string.IsNullOrWhiteSpace(payload.OptionId.ToString()) || string.IsNullOrWhiteSpace(payload.ServiceTypeId.ToString()))
            return HttpStatusCode.BadRequest;

        int rowsAffected = await _repo.AddServiceOptionServiceTypeAsync(
            payload.OptionId,
            payload.ServiceTypeId
        );

        return rowsAffected > 0 ? HttpStatusCode.OK : HttpStatusCode.Conflict;
    }

    private static List<ServiceOption> BuildHierarchy(List<FlatServiceOption> flatList)
    {
        var dict = flatList
            .GroupBy(x => x.Id)
            .Select(g => new ServiceOption
            {
                Id = g.Key,
                Name = g.First().Name,
                Description = g.First().Description,
                ServiceTypes = g.SelectMany(x => x.ServiceTypes).Distinct().ToList(),
            })
            .ToDictionary(x => x.Id);

        foreach (var item in flatList)
        {
            if (item.ParentId.HasValue && dict.ContainsKey(item.ParentId.Value))
            {
                dict[item.ParentId.Value].Children.Add(dict[item.Id]);
            }
        }

        return dict
            .Values.Where(x => !flatList.Any(f => f.Id == x.Id && f.ParentId.HasValue))
            .ToList();
    }
}
