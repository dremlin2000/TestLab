using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ClassLibrary1;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Polly;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values/5
        [HttpGet]
        public string Get()
        {
            var jsonTask = ClassLibrary1.Class1.GetJsonAsync(new Uri("https://dog.ceo/api/breeds/list/all"));
            return jsonTask.Result.ToString();
        }

        [HttpGet("Get1000")]
        public async Task<IEnumerable<string>> Get1000()
        {
            var threadCollection = new List<Task<JObject>>();

            for (int i = 0; i < 1000; i++)
            {
                threadCollection.Add(Task.Run(async () =>
                {
                    return await ClassLibrary1.Class1.GetJsonAsync(new Uri("https://dog.ceo/api/breeds/list/all"));
                }));
            }

            var result = await Task.WhenAll(threadCollection);

            return result.Select(x => x.ToString());
        }

        [HttpGet("Get2")]
        public async Task<IEnumerable<string>> Get2()
        {
            using (var client = new HttpClient())
            {
                var source = Enumerable.Range(1, 100).Select(x => "https://dog.ceo/api/breeds/list/all");
                var result = await Class1.ExecTasksInParallelAsync(
                    source, async (x) =>
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
                          .ExecuteAsync(async () => await client.GetAsync(x));

                        return await responseMessage.Content.ReadAsStringAsync();
                    }, 100, 200);

                return result;
            }
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

      
    }
}
