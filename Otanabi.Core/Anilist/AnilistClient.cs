using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otanabi.Core.Anilist.Enums;

namespace Otanabi.Core.Anilist;

public class AnilistClient
{
    private readonly HttpClient _httpClient;
    private readonly string _graphqlEndpoint = "https://graphql.anilist.co";

    public AnilistClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task<JObject> SendQueryAsync(string query, object variables)
    {
        var variablesJson = JsonConvert.SerializeObject(variables);
        var requestBody = new { query, variables = JsonConvert.DeserializeObject<object>(variablesJson) };

        var jsonRequest = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_graphqlEndpoint, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var jsonObjectResponse = JObject.Parse(jsonResponse);
        return jsonObjectResponse;
    }

    public async Task<JObject> SendQueryAsync(string query)
    {
        var requestBody = new { query };

        var jsonRequest = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_graphqlEndpoint, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var jsonObjectResponse = JObject.Parse(jsonResponse);
        return jsonObjectResponse;
    }

    public string GetQuery(QueryType queryType)
    {
        try
        {
            var queryName = queryType switch
            {
                QueryType.Search => "SearchQuery",
                QueryType.Seasonal => "Seasonal",
                QueryType.SeasonalFullDetail => "SeasonalFullDetail",
                QueryType.ById => "ById",
                QueryType.ByIdFullDetail => "ByIdFullDetail",
                QueryType.GetTags => "GetTags",
                _ => "SearchQuery",
            };

            var currPath = Assembly.GetExecutingAssembly().Location;
            var queryPath = Path.Combine(Path.GetDirectoryName(currPath), "Anilist", "Queries", $"{queryName}.graphql");
            return File.ReadAllText(queryPath);
        }
        catch (Exception)
        {
            return "";
        }
    }
}
