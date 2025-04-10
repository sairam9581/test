import { useState } from 'react';
import './App.css';

function App() {
  const [formData, setFormData] = useState({
    firstName: '',
    cityName: '',
    yearOfJoining: ''
  });
  const [retrievedData, setRetrievedData] = useState(null);
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState(''); // 'success' or 'error'
  
  // API configuration
  const API_URL = 'http://localhost:44300/api/userdata'; // Use HTTP for testing
  const API_KEY = 'api-key-123'; // Must match the key in Web.config
  
  // Get current year and calculate the last 5 years for validation
  const currentYear = new Date().getFullYear();
  const minYear = currentYear - 4; // Last 5 years including current year
  
  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value
    });
  };
  
  const handleSubmit = (e) => {
    e.preventDefault();
    setMessage('');
    setMessageType('');
    
    // Validate year of joining
    const yearValue = parseInt(formData.yearOfJoining);
    if (isNaN(yearValue) || yearValue < minYear || yearValue > currentYear) {
      setMessage(`Year of joining must be between ${minYear} and ${currentYear}`);
      setMessageType('error');
      return;
    }
    
    // Clear any previously retrieved data when submitting new data
    setRetrievedData(null);
    
    // Show saving message
    setMessage('Saving data...');
    setMessageType('info');
    
    try {
      // Create the data object
      const userData = {
        FirstName: formData.firstName,
        CityName: formData.cityName,
        YearOfJoining: yearValue
      };
      
      console.log('Sending data to API:', userData);
      console.log('API URL:', API_URL);
      console.log('API Key:', API_KEY);
      
      // Use XMLHttpRequest which has better compatibility with older browsers
      console.log('Using XMLHttpRequest for API communication');
      
      const xhr = new XMLHttpRequest();
      xhr.open('POST', API_URL, true);
      xhr.setRequestHeader('Content-Type', 'application/json');
      xhr.setRequestHeader('X-API-Key', API_KEY);
      xhr.setRequestHeader('Accept', 'application/json');
      
      xhr.onload = function() {
        console.log('Save response status:', xhr.status);
        console.log('Save response text:', xhr.responseText);
        
        if (xhr.status >= 200 && xhr.status < 300) {
          console.log('Success with XMLHttpRequest:', xhr.responseText);
          setMessage('Data saved successfully!');
          setMessageType('success');
          
          // Clear form after successful save
          setFormData({
            firstName: '',
            cityName: '',
            yearOfJoining: ''
          });
          
          // Automatically retrieve the updated data
          console.log('Automatically retrieving updated data...');
          setTimeout(handleRetrieve, 1000);
        } else {
          console.error('XHR error response:', xhr.status, xhr.statusText, xhr.responseText);
          setMessage(`Failed to save data: ${xhr.statusText || 'Unknown error'}`);
          setMessageType('error');
        }
      };
      
      xhr.onerror = function() {
        console.error('XHR request failed');
        setMessage('Network error. Please make sure the API is running and accessible.');
        setMessageType('error');
      };
      
      xhr.ontimeout = function() {
        console.error('XHR request timed out');
        setMessage('Request timed out. Please try again.');
        setMessageType('error');
      };
      
      // Send the request
      xhr.send(JSON.stringify(userData));
    } catch (error) {
      setMessage(`An error occurred: ${error.message}`);
      setMessageType('error');
      console.error('Error:', error);
    }
  };
  
  const handleRetrieve = () => {
    setMessage('');
    setMessageType('');
    
    // Show loading message
    setMessage('Retrieving data...');
    setMessageType('info');
    
    console.log('Retrieving data from API');
    console.log('API URL:', API_URL);
    console.log('API Key:', API_KEY);
    
    // Use fetch API instead of XMLHttpRequest for better compatibility
    fetch(API_URL, {
      method: 'GET',
      headers: {
        'X-API-Key': API_KEY,
        'Accept': 'application/json'
      }
    })
    
    .then(response => {
      console.log('Response received - Status:', response.status);
      console.log('Response ok:', response.ok);
      
      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }
      
      return response.text();
    })
    .then(responseText => {
      console.log('Response text:', responseText);
      console.log('Raw response length:', responseText.length);
      
      // Check if response is empty
      if (!responseText || responseText.trim() === '') {
        console.log('Empty response received');
        setMessage('No data returned from server.');
        setMessageType('info');
        setRetrievedData([]);
        return;
      }
      
      // Try to parse the response
      const data = JSON.parse(responseText);
      console.log('Retrieved data:', data);
      console.log('Data type:', typeof data);
      console.log('Is Array:', Array.isArray(data));
      
      // Inspect the structure of the data
      if (Array.isArray(data)) {
        data.forEach((item, index) => {
          console.log(`Item ${index}:`, item);
          console.log(`Item ${index} properties:`, Object.keys(item));
        });
      }
      
      // Make sure data is an array
      const dataArray = Array.isArray(data) ? data : [data];
      
      // Set the retrieved data
      setRetrievedData(dataArray);
      
      if (dataArray.length === 0) {
        setMessage('No data found.');
        setMessageType('info');
      } else {
        setMessage(`Data retrieved successfully! Found ${dataArray.length} records.`);
        setMessageType('success');
      }
    })
    .catch(error => {
      console.error('Error retrieving data:', error);
      setMessage(`Error retrieving data: ${error.message}`);
      setMessageType('error');
      setRetrievedData(null);
    });
  };
  
  return (
    <div className="App">
      <h1>User Data Form</h1>
      
      {message && (
        <div className={`message ${messageType}`}>
          {message}
        </div>
      )}
      
      <div className="api-info">
        <small>API URL: {API_URL}</small>
      </div>
      
      <div className="form-container">
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="firstName">First Name:</label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              value={formData.firstName}
              onChange={handleChange}
              required
            />
          </div>
          
          <div className="form-group">
            <label htmlFor="cityName">City Name:</label>
            <input
              type="text"
              id="cityName"
              name="cityName"
              value={formData.cityName}
              onChange={handleChange}
              required
            />
          </div>
          
          <div className="form-group">
            <label htmlFor="yearOfJoining">Year of Joining:</label>
            <input
              type="number"
              id="yearOfJoining"
              name="yearOfJoining"
              value={formData.yearOfJoining}
              onChange={handleChange}
              min={minYear}
              max={currentYear}
              required
            />
            <small>Must be between {minYear} and {currentYear}</small>
          </div>
          
          <div className="button-group">
            <button type="submit">Save Data</button>
            <button type="button" onClick={handleRetrieve}>Retrieve Data</button>
          </div>
        </form>
      </div>
      
      {retrievedData && (
        <div className="data-display">
          <h2>Retrieved Data {retrievedData.length > 0 ? `(${retrievedData.length} records)` : ''}</h2>
          {retrievedData.length > 0 ? (
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Name</th>
                  <th>City</th>
                  <th>Year</th>
                </tr>
              </thead>
              <tbody>
                {retrievedData.map((item, index) => {
                  console.log(`Rendering item ${index}:`, item);
                  
                  // Debug the exact item being rendered
                  console.log('Item keys:', Object.keys(item));
                  console.log('FirstName:', item.FirstName);
                  console.log('CityName:', item.CityName);
                  console.log('YearOfJoining:', item.YearOfJoining);
                  
                  return (
                    <tr key={index}>
                      <td style={{ backgroundColor: 'black' }}>{index + 1}</td>
                      <td style={{ backgroundColor: 'black' }}>{item.FirstName}</td>
                      <td style={{ backgroundColor: 'black' }}>{item.CityName}</td>
                      <td style={{ backgroundColor: 'black' }}>{item.YearOfJoining}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          ) : (
            <p>No data found. Please save some data first.</p>
          )}
        </div>
      )}
    </div>
  );
}

export default App;
