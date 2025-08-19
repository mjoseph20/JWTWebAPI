using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ResourceClient
{
    public class Program
    {
        // Configuration settings
        private static readonly string AuthServerBaseUrl = "https://localhost:7022"; // Authentication Server URL
        private static readonly string ResourceServerBaseUrl = "https://localhost:7267"; // Replace with your Resource Server's URL and port
        private static readonly string ClientId = "Client1"; // Must match a valid ClientId in Auth Server
        private static readonly string UserEmail = "pranaya@example.com"; // Replace with registered user's email
        private static readonly string UserPassword = "Password@123"; // Replace with registered user's password

        static async Task Main(string[] args)
        {
            try
            {
                // Step 1: Authenticate and obtain JWT token
                var token = await AuthenticateAsync(UserEmail, UserPassword, ClientId);
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Authentication failed. Exiting...");
                    return;
                }

                Console.WriteLine("Authentication successful. JWT Token obtained.\n");

                // Step 2: Consume Resource Server's ProductsController endpoints
                await ConsumeResourceServerAsync(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // Authenticates the user with the Authentication Server and retrieves a JWT token.
        private static async Task<string> AuthenticateAsync(string email, string password, string clientId)
        {
            var httpClient = new HttpClient();
            var loginUrl = $"{AuthServerBaseUrl}/api/Auth/Login";

            var loginData = new
            {
                Email = email,
                Password = password,
                ClientId = clientId
            };

            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            Console.WriteLine("Sending authentication request...");

            var response = await httpClient.PostAsync(loginUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Authentication failed with status code: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}\n");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);
            if (jsonDoc.RootElement.TryGetProperty("Token", out var tokenElement))
            {
                return tokenElement.GetString();
            }

            Console.WriteLine("Token not found in the authentication response.\n");
            return null;
        }

        // Consumes the Resource Server's ProductsController endpoints using the JWT token.
        private static async Task ConsumeResourceServerAsync(string token)
        {
            var httpClient = new HttpClient();

            // Set the Authorization header with the Bearer token
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create a new product
            var newProduct = new
            {
                Name = "Smartphone",
                Description = "A high-end smartphone with excellent features.",
                Price = 999.99
            };

            Console.WriteLine("Creating a new product...");
            var createResponse = await httpClient.PostAsync(
                $"{ResourceServerBaseUrl}/api/Products/Add",
                new StringContent(JsonSerializer.Serialize(newProduct), Encoding.UTF8, "application/json"));

            if (createResponse.IsSuccessStatusCode)
            {
                var createdProductJson = await createResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Product created successfully: {createdProductJson}\n");
            }
            else
            {
                Console.WriteLine($"Failed to create product. Status Code: {createResponse.StatusCode}");
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}\n");
            }

            // Step Retrieve all products
            Console.WriteLine("Retrieving all products...");
            var getAllResponse = await httpClient.GetAsync($"{ResourceServerBaseUrl}/api/Products/GetAll");

            if (getAllResponse.IsSuccessStatusCode)
            {
                var productsJson = await getAllResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Products: {productsJson}\n");
            }
            else
            {
                Console.WriteLine($"Failed to retrieve products. Status Code: {getAllResponse.StatusCode}");
                var errorContent = await getAllResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}\n");
            }

            // Step Retrieve a specific product by ID
            Console.WriteLine("Retrieving product with ID 1...");
            var getByIdResponse = await httpClient.GetAsync($"{ResourceServerBaseUrl}/api/Products/GetById/1");

            if (getByIdResponse.IsSuccessStatusCode)
            {
                var productJson = await getByIdResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Product Details: {productJson}\n");
            }
            else
            {
                Console.WriteLine($"Failed to retrieve product. Status Code: {getByIdResponse.StatusCode}");
                var errorContent = await getByIdResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}\n");
            }

            // Step Update a product
            var updatedProduct = new
            {
                Name = "Smartphone Pro",
                Description = "An upgraded smartphone with enhanced features.",
                Price = 1199.99
            };

            Console.WriteLine("Updating product with ID 1...");
            var updateResponse = await httpClient.PutAsync(
                $"{ResourceServerBaseUrl}/api/Products/Update/1",
                new StringContent(JsonSerializer.Serialize(updatedProduct), Encoding.UTF8, "application/json"));

            if (updateResponse.IsSuccessStatusCode || updateResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                Console.WriteLine("Product updated successfully.\n");
            }
            else
            {
                Console.WriteLine($"Failed to update product. Status Code: {updateResponse.StatusCode}");
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}\n");
            }

            // Step Delete a product
            Console.WriteLine("Deleting product with ID 1...");
            var deleteResponse = await httpClient.DeleteAsync($"{ResourceServerBaseUrl}/api/Products/Delete/1");

            if (deleteResponse.IsSuccessStatusCode || deleteResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                Console.WriteLine("Product deleted successfully.\n");
            }
            else
            {
                Console.WriteLine($"Failed to delete product. Status Code: {deleteResponse.StatusCode}");
                var errorContent = await deleteResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}\n");
            }
        }
    }
}
