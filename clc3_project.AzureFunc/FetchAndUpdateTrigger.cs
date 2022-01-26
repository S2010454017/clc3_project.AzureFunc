
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using clc3_project.AzureFunc;

namespace FetchDataFunction
{
    public class FetchAndUpdateTrigger
    {
        private readonly HttpClient _client;
        private readonly string URL = "https://openlibrary.org/api/books?bibkeys=ISBN:{0}&jscmd=data&format=json";
        public FetchAndUpdateTrigger(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient();
        }

        [FunctionName("FetchAndUpdateTrigger")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            // GET ISBN FROM ROUTE OR WHATEVER
            string id = req.Query["isbn"];
            log.LogInformation($"ISBN {id} wanted...");

            var resp = await _client.GetAsync(string.Format(URL, id));
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                if (!(string.IsNullOrEmpty(json) && json.Length > 2))
                {
                    var obj = $"{{\"ISBN\":\"{id}\",";
                    var tmp = json.Remove(0, obj.Length);
                    obj += tmp.Remove(tmp.Length - 1, 1);
                    log.LogInformation($"Retunred JSON VALUE:\n {obj}");
                    dynamic newBook = JsonConvert.DeserializeObject(obj);

                    var authors = new HashSet<string>();
                    foreach (var item in newBook.authors)
                    {

                        authors.Add((string)item.name);
                    }

                    var cat = new HashSet<string>();
                    foreach (var item in newBook.subjects)
                    {
                        cat.Add((string)item.name);
                    }


                    var b = new Book { Authors = authors, Category = cat, ISBN = newBook.ISBN, BookName = newBook.title, 
                                       Cover = newBook.cover["large"] };
                    return new OkObjectResult(b);
                }

            }
            log.LogInformation($"Nothing found for ISBN: {id}");
            return new NotFoundResult();
        }
    }
}
