
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
    /// <summary>
    /// Class containing the logic for the Azure Function
    /// </summary>
    public class FetchAndUpdateTrigger
    {
        private readonly HttpClient _client;
        //URL to get information for new book
        private readonly string URL = "https://openlibrary.org/api/books?bibkeys=ISBN:{0}&jscmd=data&format=json";
        public FetchAndUpdateTrigger(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient();
        }

        /// <summary>
        /// The run method of the azure function. Can be triggered via the HTTP-Get method
        /// </summary>
        /// <param name="req">the httprequest</param>
        /// <param name="log">Logger</param>
        /// <returns>The book if data was obtained</returns>
        [FunctionName("FetchAndUpdateTrigger")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            //Get isbn from query
            string id = req.Query["isbn"];
            log.LogInformation($"ISBN {id} wanted...");

            //send get request to external side
            var resp = await _client.GetAsync(string.Format(URL, id));
            if (resp.IsSuccessStatusCode)
            {
                //read obtained data to string
                var json = await resp.Content.ReadAsStringAsync();
                if (!(string.IsNullOrEmpty(json) && json.Length > 2))
                {
                    //format obtained json to easier process it.
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

                    //create new book with obtained information and send it back to the caller.
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
