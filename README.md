# Survey API

A robust RESTful API for managing surveys, built with **C#** and **ASP.NET Core**. The project follows a classic **N-Tier (Multi-layer) Architecture** and implements the **Controller-Service-Repository** pattern to ensure clean separation of concerns, scalability, and maintainability.

## 🏗️ Architecture & Project Structure

The solution (`SurveyApplication.sln`) is divided into four main projects:

*   **`Entities`**: The Domain layer. Contains all the classes and core business models that represent the database tables.
*   **`SurveyDataAccessLayer`**: The Data Access Layer (DAL). Built using **ADO.NET**. This layer handles all direct database interactions, connection management (`SqlConnection`), execution of raw SQL queries/stored procedures (`SqlCommand`).
*   **`SurveyBusinessLayer`**: The Business Logic Layer (BLL). Contains the Service classes. It acts as the "brain" of the application, validating business rules before communicating with the Data Access Layer.
*   **`SurveyApplication`**: The Presentation layer (Web API). Contains the REST API Controllers that receive HTTP requests, pass data to the Business Layer, and return HTTP responses.

## 🚀 Tech Stack

*   **Language:** C#
*   **Framework:** .NET 8 (ASP.NET Core Web API)
*   **Data Access:** ADO.NET
*   **Architecture:** N-Tier, Controller-Service-Repository Pattern

## ⚙️ Prerequisites

Before you begin, ensure you have the following installed:
*   [.NET SDK](https://dotnet.microsoft.com/download)
*   An IDE such as [Visual Studio 2022](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or JetBrains Rider.
*   SQL Server (or your preferred database engine) to host the database.

## 🛠️ Getting Started

Follow these steps to get a local copy up and running:

### 1. Clone the repository
```bash
git clone https://github.com/sulieman-Albuaeshi/Survey-API.git
cd Survey-API
```

### 2. Configure the Database
1. Create a new database in your SQL Server.
2. Execute the necessary SQL scripts (e.g., `schema.sql` or table creation scripts) directly in your database manager (like SQL Server Management Studio) to set up the tables required for the surveys. *(Note: Add the location of your SQL scripts here if you have them in the repo).*
3. Navigate to the `SurveyDataAccessLayer` folder.
4. Open `Dbhealper.cs` file
5. Update the `ConnectionStrings` method with your local ADO.NET database connection string.

### 3. Restore Dependencies
Open your terminal in the root directory and run:
```bash
# Restore NuGet packages for the solution
dotnet restore
```

### 4. Run the Application
You can run the application directly from Visual Studio by pressing `F5`, or run it via the .NET CLI:
```bash
cd SurveyApplication
dotnet run
```
If Swagger is enabled, navigate to `https://localhost:<5226>/swagger` in your browser to test the API endpoints interactively.

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! 
Feel free to check the [issues page](https://github.com/sulieman-Albuaeshi/Survey-API/issues).

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---
*Developed by [sulieman-Albuaeshi](https://github.com/sulieman-Albuaeshi)*
Developed by sulieman-Albuaeshi
