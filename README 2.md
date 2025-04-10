# Web Form Application

A web application that allows users to enter and retrieve data using a React frontend and a C# .NET Framework 4.8 Web API backend.

## Project Structure

- `/client` - React/Vite frontend
- `/api` - C# .NET Framework 4.8 Web API

## Requirements

- Node.js and npm for the frontend
- Visual Studio 2019+ with .NET Framework 4.8 for the backend

## Setup and Run Instructions

### Backend (C# Web API)

1. Open the solution in Visual Studio
2. Build the solution to restore NuGet packages
3. Run the API project (it should start on https://localhost:44300)

### Frontend (React/Vite)

1. Navigate to the client directory
2. Install dependencies:
   ```
   npm install
   ```
3. Start the development server:
   ```
   npm run dev
   ```
4. Open the application in your browser at the URL shown in the terminal

## Features

- Form to enter user data (First Name, City Name, Year of Joining)
- Year of Joining is restricted to the last 5 years
- Save button to store data in a JSON file via the API
- Retrieve button to fetch all saved data from the API
- API Key authentication for security

## API Endpoints

- `POST /api/userdata` - Save user data
- `GET /api/userdata` - Retrieve all user data

## Security

The API requires an API Key for authentication, which should be passed in the `X-API-Key` header.
