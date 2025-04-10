@echo off
echo ===================================
echo Web Form Application Setup and Run
echo ===================================

echo.
echo Step 1: Setting up the API project
echo ---------------------------------
echo Please open the WebFormApp.sln file in Visual Studio and run the API project.
echo The API should be accessible at https://localhost:44300/

echo.
echo Step 2: Setting up the React frontend
echo -----------------------------------
cd client
echo Installing npm dependencies...
echo Note: If npm is not installed, please install Node.js from https://nodejs.org/
call npm install

echo.
echo Starting the React development server...
call npm run dev

echo.
echo If the browser doesn't open automatically, please visit:
echo http://localhost:3000/
echo.

pause
