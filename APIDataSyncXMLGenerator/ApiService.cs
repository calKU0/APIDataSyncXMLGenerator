using APIDataSyncXMLGenerator.Models;
using Newtonsoft.Json;
using Serilog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using XSystem.Security.Cryptography;

namespace APIDataSyncXMLGenerator
{
    public static class ApiService
    {
        private static readonly string _apiAcronym = ConfigurationManager.AppSettings["ApiAcronym"]?.ToString();
        private static readonly string _apiPerson = ConfigurationManager.AppSettings["ApiPerson"]?.ToString();
        private static readonly string _apiPassword = ConfigurationManager.AppSettings["ApiPassword"]?.ToString();
        private static readonly string _apiKey = ConfigurationManager.AppSettings["ApiKey"]?.ToString();
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]?.ToString();

        public static async Task<Product> FetchProductsData(int id)
        {
            Product product = null;

            try
            {
                var client = GetClientDefaultHeaders();
                string action = "product";

                var request = new RestRequest(action, Method.Get);
                request.AddParameter("lng", "pl");
                request.AddParameter("id", id);

                Log.Information("Sending API request: {Action} with parameters: id={ProductId}, lng=pl", action, id);
                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    HandleFailedRequest(response, id);
                    return product;
                }

                var wrapper = JsonConvert.DeserializeObject<ProductWrapper>(response.Content);
                if (wrapper?.Product != null)
                {
                    product = wrapper.Product;
                    product.Supplier = ConfigurationManager.AppSettings["Supplier"]?.ToString();
                    Log.Information("Successfully fetched product data for ID {ProductId}", id);
                }
                else
                {
                    Log.Warning("API response for ID {ProductId} did not contain valid product data", id);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception occurred while fetching product data for ID {ProductId}", id);
            }

            return product;
        }

        private static void HandleFailedRequest(RestResponse response, int productId)
        {
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                Log.Warning("API request failed due to too many requests (ID: {ProductId})", productId);
            }
            else
            {
                Log.Warning("API request failed for ID {ProductId}: {StatusCode} - {ErrorMessage}", productId, response.StatusCode, response.ErrorMessage);
            }
        }

        private static string GetSignature()
        {
            string body = $"acronym={_apiAcronym}&person={_apiPerson}&password={_apiPassword}&key={_apiKey}";
            using (SHA256Managed sha = new SHA256Managed())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(body));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        private static RestClient GetClientDefaultHeaders()
        {
            var client = new RestClient(_apiBaseUrl);
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_apiAcronym}|{_apiPerson}:{_apiPassword}"));

            client.AddDefaultHeader("Authorization", $"Basic {credentials}");
            client.AddDefaultHeader("X-Signature", GetSignature());

            return client;
        }
    }
}
