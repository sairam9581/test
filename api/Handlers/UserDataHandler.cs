using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using WebFormApp.Models;

namespace WebFormApp.Handlers
{
    public class UserDataHandler : IHttpHandler
    {
        private string _dataFilePath;
        private string _apiKey;

        public UserDataHandler()
        {
            _dataFilePath = ConfigurationManager.AppSettings["DataFilePath"];
            _apiKey = ConfigurationManager.AppSettings["ApiKey"];
            
            // Ensure the App_Data directory and JSON file exist at startup
            string appDataPath = HttpContext.Current.Server.MapPath("~/App_Data");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            string fullPath = HttpContext.Current.Server.MapPath($"~/{_dataFilePath}");
            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, "[]");
            }
        }

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            System.Diagnostics.Debug.WriteLine($"Received {context.Request.HttpMethod} request from {context.Request.UrlReferrer?.ToString() ?? "unknown"}");
            
            // Handle preflight OPTIONS request - CORS headers are set in Web.config
            if (context.Request.HttpMethod == "OPTIONS")
            {
                System.Diagnostics.Debug.WriteLine("Handling OPTIONS preflight request");
                context.Response.StatusCode = 200;
                context.Response.End();
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"Processing {context.Request.HttpMethod} request to {context.Request.RawUrl}");
            
            // Validate API key
            string apiKey = context.Request.Headers["X-API-Key"];
            if (string.IsNullOrEmpty(apiKey) || apiKey != _apiKey)
            {
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.Write(JsonConvert.SerializeObject(new { error = "Invalid API key" }));
                context.Response.End();
                return;
            }
            
            // Log API key for debugging
            System.Diagnostics.Debug.WriteLine($"API Key received: {apiKey}");
            System.Diagnostics.Debug.WriteLine($"Expected API Key: {_apiKey}");

            try
            {
                if (context.Request.HttpMethod == "GET")
                {
                    System.Diagnostics.Debug.WriteLine("Processing GET request");
                    HandleGetRequest(context);
                }
                else if (context.Request.HttpMethod == "POST")
                {
                    System.Diagnostics.Debug.WriteLine("Processing POST request");
                    HandlePostRequest(context);
                }
                else
                {
                    context.Response.Clear();
                    context.Response.ContentType = "application/json";
                    context.Response.Write(JsonConvert.SerializeObject(new { error = "Method not allowed" }));
                    context.Response.End();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing request: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.Write(JsonConvert.SerializeObject(new { error = $"An error occurred: {ex.Message}" }));
                context.Response.End();
            }
        }

        private void HandleGetRequest(HttpContext context)
        {
            try
            {
                var userData = ReadUserData();
                
                // Keep the original property names to match what's in the JSON file
                string json = JsonConvert.SerializeObject(userData, Formatting.None);
                
                // Log the data being returned
                System.Diagnostics.Debug.WriteLine($"Returning {userData.Count} records");
                System.Diagnostics.Debug.WriteLine($"JSON response: {json}");
                
                // Set response properties before writing content
                context.Response.Clear();
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.StatusCode = 200; // Set success status code
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                context.Response.Cache.SetNoStore();
                context.Response.Cache.SetExpires(DateTime.MinValue);
                context.Response.Write(json);
                context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HandleGetRequest: {ex.Message}");
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500; // Set error status code
                context.Response.Write(JsonConvert.SerializeObject(new { error = $"Error retrieving data: {ex.Message}" }));
                context.ApplicationInstance.CompleteRequest(); // Use CompleteRequest instead of End
            }
        }

        private void HandlePostRequest(HttpContext context)
        {
            UserData userData = null;
            try
            {
                // Read request body
                string requestBody = "";
                
                // Ensure the stream is readable
                if (context.Request.InputStream.CanRead)
                {
                    context.Request.InputStream.Position = 0; // Reset stream position
                    using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                    {
                        requestBody = reader.ReadToEnd();
                    }
                    
                    // Log the received data for debugging
                    System.Diagnostics.Debug.WriteLine($"Received data: {requestBody}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Request stream is not readable");
                    context.Response.Clear();
                    context.Response.ContentType = "application/json";
                    context.Response.Write(JsonConvert.SerializeObject(new { error = "Request stream is not readable" }));
                    context.Response.End();
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    System.Diagnostics.Debug.WriteLine("Empty request body");
                    context.Response.Clear();
                    context.Response.ContentType = "application/json";
                    context.Response.Write(JsonConvert.SerializeObject(new { error = "Empty request body" }));
                    context.Response.End();
                    return;
                }

                // Deserialize the JSON data
                try {
                    userData = JsonConvert.DeserializeObject<UserData>(requestBody);
                    System.Diagnostics.Debug.WriteLine($"Deserialized data: {JsonConvert.SerializeObject(userData)}");
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"JSON deserialization error: {ex.Message}");
                    context.Response.Clear();
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 400; // Bad Request
                    context.Response.Write(JsonConvert.SerializeObject(new { error = $"Invalid JSON format: {ex.Message}" }));
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Validate data
                if (userData == null || string.IsNullOrEmpty(userData.FirstName) || string.IsNullOrEmpty(userData.CityName))
                {
                    context.Response.StatusCode = 400; // Bad Request
                    context.Response.ContentType = "application/json";
                    context.Response.Cache.SetNoStore();
                    context.Response.Cache.SetExpires(DateTime.MinValue);
                    context.Response.Write(JsonConvert.SerializeObject(new { error = "Invalid data submitted" }));
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing request: {ex.Message}");
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                context.Response.Write(JsonConvert.SerializeObject(new { error = $"Error parsing request: {ex.Message}" }));
                context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Validate year of joining (last 5 years)
            int currentYear = DateTime.Now.Year;
            int minYear = currentYear - 4; // Last 5 years including current year
            if (userData.YearOfJoining < minYear || userData.YearOfJoining > currentYear)
            {
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 400; // Bad Request
                context.Response.Write(JsonConvert.SerializeObject(new { error = $"Year of joining must be between {minYear} and {currentYear}" }));
                context.ApplicationInstance.CompleteRequest();
                return;
            }

            try
            {
                // Save the data
                var existingData = ReadUserData();
                existingData.Add(userData);
                
                // Get the full physical path
                string fullPath = HttpContext.Current.Server.MapPath($"~/{_dataFilePath}");
                System.Diagnostics.Debug.WriteLine($"Saving data to file: {fullPath}");
                
                // Ensure directory exists
                string directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Serialize and save
                string json = JsonConvert.SerializeObject(existingData, Formatting.Indented);
                File.WriteAllText(fullPath, json);
                
                // Verify the file was written
                if (File.Exists(fullPath))
                {
                    string savedContent = File.ReadAllText(fullPath);
                    System.Diagnostics.Debug.WriteLine($"File content after save: {savedContent}");
                }
                
                // Log success for debugging
                System.Diagnostics.Debug.WriteLine($"Data saved successfully to {fullPath}");
                System.Diagnostics.Debug.WriteLine($"Current data count: {existingData.Count}");

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200; // Success
                context.Response.Write(JsonConvert.SerializeObject(new { message = "Data saved successfully" }));
                context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500; // Server error
                context.Response.Write(JsonConvert.SerializeObject(new { error = $"Error saving data: {ex.Message}" }));
                context.ApplicationInstance.CompleteRequest();
            }
        }

        private List<UserData> ReadUserData()
        {
            try
            {
                // Get the full physical path
                string fullPath = HttpContext.Current.Server.MapPath($"~/{_dataFilePath}");
                System.Diagnostics.Debug.WriteLine($"Reading data from: {fullPath}");
                
                // Ensure directory exists
                string directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    System.Diagnostics.Debug.WriteLine($"Created directory: {directory}");
                }

                if (!File.Exists(fullPath))
                {
                    // Create an empty JSON file if it doesn't exist
                    File.WriteAllText(fullPath, JsonConvert.SerializeObject(new List<UserData>()));
                    System.Diagnostics.Debug.WriteLine($"Created new empty JSON file: {fullPath}");
                    return new List<UserData>();
                }

                string json = File.ReadAllText(fullPath);
                System.Diagnostics.Debug.WriteLine($"Read JSON content: {json}");
                
                var result = JsonConvert.DeserializeObject<List<UserData>>(json) ?? new List<UserData>();
                System.Diagnostics.Debug.WriteLine($"Read {result.Count} records from {fullPath}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Return empty list on error to avoid crashing
                return new List<UserData>();
            }
        }

        private void SaveUserData(List<UserData> userData)
        {
            try
            {
                // Get the full physical path
                string fullPath = HttpContext.Current.Server.MapPath($"~/{_dataFilePath}");
                System.Diagnostics.Debug.WriteLine($"Saving data to: {fullPath}");
                
                // Ensure directory exists
                string directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    System.Diagnostics.Debug.WriteLine($"Created directory: {directory}");
                }

                string json = JsonConvert.SerializeObject(userData, Formatting.Indented);
                System.Diagnostics.Debug.WriteLine($"JSON to save: {json}");
                
                File.WriteAllText(fullPath, json);
                System.Diagnostics.Debug.WriteLine($"Saved {userData.Count} records to {fullPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to handle in the calling method
            }
        }
    }
}
