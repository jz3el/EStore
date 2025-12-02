namespace EStore.Services.Interfaces.GenericClient
{
    public interface IGenericUserClientRepository
    {
        Task<T> GetAsync<T>(string address);
        Task<T> GetAsync<T>(string address, dynamic dynamicRequest);
        Task<T> GetWithId<T>(string address);
        Task<TResponse> PostAsAsync<TResponse>(string address, dynamic dynamicRequest);
        Task<TResponse> PostAsAsync<TResponse>(string address);
        Task<TResponse> UpdateAsync<TResponse>(string address, dynamic dynamicRequest);
        Task<TResponse> DeleteAsync<TResponse>(string address);
    }
}
