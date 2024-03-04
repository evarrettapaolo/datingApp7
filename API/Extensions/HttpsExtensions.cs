using System.Text.Json;
using API.Helpers;

namespace API.Extensions
{
  public static class HttpsExtensions
  {
    public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
    {
      //set casing config
      var jsonOptions = new JsonSerializerOptions()
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };

      //configure header for pagination sections
      response.Headers.Add("Pagination", JsonSerializer.Serialize(header, jsonOptions));
      //CORS
      response.Headers.Add("Access-Control-Expose-Headers", "Pagination");

    }
  }
}