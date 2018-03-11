using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ClassLibrary1
{

    public class Class1
    {
        public static async Task<JObject> GetJsonAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {

                // Handle both exceptions and return values in one policy
                HttpStatusCode[] httpStatusCodesWorthRetrying = {
                   HttpStatusCode.RequestTimeout, // 408
                   HttpStatusCode.InternalServerError, // 500
                   HttpStatusCode.BadGateway, // 502
                   HttpStatusCode.ServiceUnavailable, // 503
                   HttpStatusCode.GatewayTimeout // 504
                };

                var responseMessage = await
                   Policy
                  .Handle<HttpRequestException>()
                  .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                  .RetryAsync(3)
                  .ExecuteAsync(async () => await client.GetAsync(uri));

                return JObject.Parse(await responseMessage.Content.ReadAsStringAsync());
            }
        }

        public static async Task<IEnumerable<TResult>> ExecTasksInParallelAsync<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, Task<TResult>> task, int minDegreeOfParallelism = 1, int maxDegreeOfParallelism = 1)
        {
            var allTasks = new List<Task<TResult>>();

            using (var throttler = new SemaphoreSlim(minDegreeOfParallelism, maxDegreeOfParallelism))
            {
                foreach (var element in source)
                {
                    // do an async wait until we can schedule again
                    await throttler.WaitAsync();

                    Func<Task<TResult>> func = async () =>
                    {
                        try
                        {
                            return await task(element);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    };
                    allTasks.Add(func.Invoke());
                }

                // won't get here until all urls have been put into tasks
                return await Task.WhenAll(allTasks);
            }

            // won't get here until all tasks have completed in some way
            // (either success or exception)
        }
    }
}
