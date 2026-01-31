public interface IServiceOptionRepository
{
    Task<List<FlatServiceOption>> GetMotorbikeServiceOptions();
    Task<List<FlatServiceOption>> GetOwnershipServiceOptions();
    Task<int> AddServiceOptionAsync(string name, string? description, int? categoryId);
    Task<int> AddParentServiceOptionAsync(int optionId, int parentId);
    Task<int> AddServiceOptionServiceTypeAsync(string optionId, string serviceTypeId);
    // Task UpdateServiceOptionAsync(ServiceOption serviceOption);
    // Task DeleteServiceOptionAsync(int id);
}