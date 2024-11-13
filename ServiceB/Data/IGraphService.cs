using ServiceB.Data.Models;

namespace ServiceB.Data
{
    public interface IGraphService
    {
        Task<string?> UpdateFromMessageAsync(string message);

    }
}