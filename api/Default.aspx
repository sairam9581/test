<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Web Form API</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
            line-height: 1.6;
        }
        .container {
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 5px;
        }
        h1 {
            color: #333;
        }
        .endpoint {
            background-color: #f5f5f5;
            padding: 10px;
            border-left: 4px solid #0066cc;
            margin-bottom: 10px;
        }
        code {
            background-color: #f0f0f0;
            padding: 2px 4px;
            border-radius: 3px;
            font-family: Consolas, monospace;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>Web Form API</h1>
        <p>This is the backend API for the Web Form Application. The API provides endpoints for saving and retrieving user data.</p>
        
        <h2>API Endpoints</h2>
        
        <div class="endpoint">
            <h3>GET /api/userdata</h3>
            <p>Retrieves all saved user data.</p>
            <p><strong>Required Headers:</strong></p>
            <ul>
                <li><code>X-API-Key</code>: Your API key</li>
            </ul>
        </div>
        
        <div class="endpoint">
            <h3>POST /api/userdata</h3>
            <p>Saves user data to the database.</p>
            <p><strong>Required Headers:</strong></p>
            <ul>
                <li><code>Content-Type</code>: application/json</li>
                <li><code>X-API-Key</code>: Your API key</li>
            </ul>
            <p><strong>Request Body:</strong></p>
            <pre><code>{
  "FirstName": "John",
  "CityName": "New York",
  "YearOfJoining": 2023
}</code></pre>
        </div>
        
        <h2>Notes</h2>
        <ul>
            <li>The Year of Joining must be within the last 5 years.</li>
            <li>All data is stored in a JSON file.</li>
            <li>The API key must match the one configured in Web.config.</li>
        </ul>
    </div>
</body>
</html>
