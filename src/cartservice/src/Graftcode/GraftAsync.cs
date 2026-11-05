using System;
using System.Threading.Tasks;

namespace cartservice.Graftcode;

internal static class GraftAsync
{
    // Bridge until GG supports async public methods.
    internal static T Run<T>(Func<Task<T>> operation) =>
        Task.Run(operation).ConfigureAwait(false).GetAwaiter().GetResult();
}
