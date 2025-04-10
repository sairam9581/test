using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebFormApp.Models;

namespace WebFormApp.Controllers
{
    public class UserDataController : ApiController
    {
        private readonly string _dataFilePath;
        private readonly string _apiKey;

        public UserDataController()
        {
            _dataFilePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), "userData.json");
            _apiKey = ConfigurationManager.AppSettings["ApiKey"];
        }

        // GET: api/UserData
        public IHttpActionResult Get()
        {
            // Validate API key
            if (!ValidateApiKey())
            {
                return Unauthorized();
            }

            try
            {
                var userData = ReadUserData();
                return Ok(userData);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/UserData
        public IHttpActionResult Post([FromBody] UserData userData)
        {
            // Validate API key
            if (!ValidateApiKey())
            {
                return Unauthorized();
            }

            // Validate model
            if (!ModelState.IsValid || userData == null)
            {
                return BadRequest("Invalid data submitted");
            }

            // Validate year of joining (last 5 years)
            int currentYear = DateTime.Now.Year;
            int minYear = currentYear - 4; // Last 5 years including current year
            if (userData.YearOfJoining < minYear || userData.YearOfJoining > currentYear)
            {
                return BadRequest($"Year of joining must be between {minYear} and {currentYear}");
            }

            try
            {
                var existingData = ReadUserData();
                existingData.Add(userData);
                SaveUserData(existingData);
                
                return Ok("Data saved successfully");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private bool ValidateApiKey()
        {
            IEnumerable<string> apiKeyHeaders;
            if (Request.Headers.TryGetValues("X-API-Key", out apiKeyHeaders))
            {
                string providedApiKey = apiKeyHeaders.FirstOrDefault();
                return !string.IsNullOrEmpty(providedApiKey) && providedApiKey == _apiKey;
            }
            return false;
        }

        private List<UserData> ReadUserData()
        {
            if (!File.Exists(_dataFilePath))
            {
                // Create an empty JSON file if it doesn't exist
                File.WriteAllText(_dataFilePath, JsonConvert.SerializeObject(new List<UserData>()));
                return new List<UserData>();
            }

            string json = File.ReadAllText(_dataFilePath);
            return JsonConvert.DeserializeObject<List<UserData>>(json) ?? new List<UserData>();
        }

        private void SaveUserData(List<UserData> userData)
        {
            string json = JsonConvert.SerializeObject(userData, Formatting.Indented);
            File.WriteAllText(_dataFilePath, json);
        }
    }
}
