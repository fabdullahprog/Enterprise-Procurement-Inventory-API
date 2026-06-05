# SuperShop Management API

A production-style **ASP.NET Core 8.0 Web API** for procurement, inventory, warehouse, requisition, and master data management.

---

## Project Overview

SuperShop Management API is the backend engine for a retail, warehouse, and ERP-style management system. It centralizes business operations such as inventory control, warehouse management, procurement workflows, user authentication, authorization, and master data administration.

The system is designed using a layered architecture to ensure maintainability, scalability, and separation of concerns. It exposes RESTful APIs intended for frontend applications such as React and Angular.

---

## Features

* ASP.NET Core 8.0 Web API
* Entity Framework Core 8.0
* SQL Server / LocalDB Support
* ASP.NET Core Identity
* JWT Bearer Authentication
* Role-Based Authorization
* Custom Authorization Attributes
* Repository Pattern Implementation
* DTO-Based API Contracts
* Swagger / OpenAPI Documentation
* Automatic Database Migration
* Database Seeding
* CORS Configuration
* Warehouse Management APIs
* Inventory Management APIs
* Procurement Management APIs
* Requisition Management APIs
* Dashboard Statistics APIs
* Master Data Management

---

## Tech Stack

| Technology            | Purpose                              |
| --------------------- | ------------------------------------ |
| ASP.NET Core 8.0      | REST API framework                   |
| C#                    | Main programming language            |
| Entity Framework Core | ORM and database access              |
| SQL Server / LocalDB  | Relational database                  |
| ASP.NET Core Identity | User and role management             |
| JWT Authentication    | Secure stateless authentication      |
| Swagger / OpenAPI     | API documentation                    |
| Repository Pattern    | Data access abstraction              |
| DTOs                  | Clean API request/response contracts |

---

## Architecture

The project follows a layered architecture with clear separation of responsibilities.

### Layers

* Controllers
* DTOs
* Repositories
* Data Layer (EF Core)
* Entities / Models
* Authorization Attributes

---

## Installation

### Prerequisites

* .NET 8 SDK
* SQL Server or SQL Server LocalDB
* Visual Studio 2022 / VS Code

### Clone Repository

```bash
git clone <repository-url>
cd SuperShop_Management
```

### Restore Packages

```bash
dotnet restore
```

---

## Configuration

Primary configuration is stored in:

```text
appsettings.json
```

### Example Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SuperShopFinalAPI;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyHereMin32CharactersLong123!",
    "Issuer": "SuperShopAPI",
    "Audience": "SuperShopClient",
    "ExpiryMinutes": 480
  }
}
```

### Environment Variables

| Variable                             | Description                |
| ------------------------------------ | -------------------------- |
| ConnectionStrings__DefaultConnection | Database connection string |
| JwtSettings__Secret                  | JWT signing key            |
| JwtSettings__Issuer                  | JWT issuer                 |
| JwtSettings__Audience                | JWT audience               |
| JwtSettings__ExpiryMinutes           | Token expiration time      |

---

## Database Setup

### Database Provider

* Microsoft SQL Server
* SQL Server LocalDB (Default)

### Automatic Setup

On startup the application automatically:

* Applies pending migrations
* Seeds Identity users
* Seeds Roles
* Seeds Initial Application Data

### Manual Migration Commands

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

---

## Running the Project

### Development Mode

```bash
dotnet run
```

### Production Build

```bash
dotnet publish -c Release
```

---

## Authentication

The application uses:

* ASP.NET Core Identity
* JWT Bearer Authentication

### JWT Claims

Generated tokens may contain:

* NameIdentifier
* Email
* Name
* Role Claims
* Permission Claims
* DepartmentId

### Authorization Attributes

```csharp
[Authorize]
[Authorize(Roles = "Admin")]
[AllowAnonymous]
```

### Security Recommendations

* Store JWT Secret securely
* Enable HTTPS in production
* Restrict production CORS origins
* Rotate JWT secrets periodically

---

## API Endpoints

### Authentication

| Method | Endpoint           | Description       | Auth Required |
| ------ | ------------------ | ----------------- | ------------- |
| POST   | /api/Auth/register | Register User     | No            |
| POST   | /api/Auth/login    | Login User        | No            |
| POST   | /api/Auth/logout   | Logout User       | Yes           |
| GET    | /api/Auth/me       | Current User Info | Yes           |

---

### Admin

| Method | Endpoint                                |
| ------ | --------------------------------------- |
| GET    | /api/Admin/roles                        |
| POST   | /api/Admin/roles                        |
| PUT    | /api/Admin/roles/{roleName}             |
| DELETE | /api/Admin/roles/{roleName}             |
| GET    | /api/Admin/roles/{roleName}/permissions |

---

### Department

| Method | Endpoint             |
| ------ | -------------------- |
| GET    | /api/Department      |
| GET    | /api/Department/{id} |
| POST   | /api/Department      |
| PUT    | /api/Department/{id} |
| DELETE | /api/Department/{id} |

---

### Item Category

| Method | Endpoint               |
| ------ | ---------------------- |
| GET    | /api/ItemCategory      |
| GET    | /api/ItemCategory/{id} |
| POST   | /api/ItemCategory      |
| PUT    | /api/ItemCategory/{id} |
| DELETE | /api/ItemCategory/{id} |

---

### Unit

| Method | Endpoint                    |
| ------ | --------------------------- |
| GET    | /api/Unit                   |
| GET    | /api/Unit/{id}              |
| GET    | /api/Unit/byset/{unitSetId} |
| POST   | /api/Unit                   |
| PUT    | /api/Unit/{id}              |
| DELETE | /api/Unit/{id}              |

---

### Batch

| Method | Endpoint        |
| ------ | --------------- |
| GET    | /api/Batch      |
| GET    | /api/Batch/{id} |
| POST   | /api/Batch      |
| PUT    | /api/Batch/{id} |
| DELETE | /api/Batch/{id} |

---

### Brand

| Method | Endpoint                                 |
| ------ | ---------------------------------------- |
| GET    | /api/Brand                               |
| GET    | /api/Brand/{id}                          |
| GET    | /api/Brand/bysubcategory/{subCategoryId} |
| POST   | /api/Brand                               |
| PUT    | /api/Brand/{id}                          |
| DELETE | /api/Brand/{id}                          |

---

### Dashboard

| Method | Endpoint             |
| ------ | -------------------- |
| GET    | /api/Dashboard/stats |

---

### Location

#### Warehouse

| Method | Endpoint                      |
| ------ | ----------------------------- |
| GET    | /api/location/warehouses      |
| GET    | /api/location/warehouses/{id} |
| POST   | /api/location/warehouses      |
| PUT    | /api/location/warehouses/{id} |
| DELETE | /api/location/warehouses/{id} |

#### Floor

| Method | Endpoint                                        |
| ------ | ----------------------------------------------- |
| GET    | /api/location/floors                            |
| GET    | /api/location/floors/{id}                       |
| GET    | /api/location/floors/by-warehouse/{warehouseId} |
| POST   | /api/location/floors                            |
| PUT    | /api/location/floors/{id}                       |
| DELETE | /api/location/floors/{id}                       |

#### Zone

| Method | Endpoint                               |
| ------ | -------------------------------------- |
| GET    | /api/location/zones                    |
| GET    | /api/location/zones/{id}               |
| GET    | /api/location/zones/by-floor/{floorId} |

---

## Project Structure

```text
SuperShop_Management/
│
├── Attributes/
│   └── Custom authorization and validation attributes
│
├── Controllers/
│   └── REST API endpoints and request handling
│
├── Data/
│   └── AppDbContext, database configuration, and seed data
│
├── DTOs/
│   └── Request and response models
│
├── Entities/
│   └── Domain entities and database models
│
├── Migrations/
│   └── Entity Framework Core migration files
│
├── Repositories/
│   ├── Interfaces/
│   │   └── Repository contracts
│   │
│   └── Implementations/
│       └── Repository implementations
│
├── Properties/
│   └── Launch settings and project configuration
│
├── appsettings.json
│   └── Application configuration settings
│
├── Program.cs
│   └── Application startup and middleware configuration
│
├── README.md
│   └── Project documentation
│
├── seed-output.log
│   └── Database seeding logs
│
└── SuperShop_Management.http
    └── HTTP request collection for API testing
    ```

### Important Directories

| Folder       | Purpose                    |
| ------------ | -------------------------- |
| Controllers  | API Endpoints              |
| DTOs         | Request & Response Models  |
| Entities     | Domain Entities            |
| Data         | EF Core DbContext          |
| Repositories | Data Access Layer          |
| Migrations   | Database Migrations        |
| Attributes   | Custom Authorization Logic |

---

## Dependencies

### Core Packages

* Microsoft.AspNetCore.Authentication.JwtBearer
* Microsoft.AspNetCore.Identity.EntityFrameworkCore
* Microsoft.AspNetCore.Identity.UI
* Microsoft.EntityFrameworkCore
* Microsoft.EntityFrameworkCore.SqlServer
* Microsoft.EntityFrameworkCore.Tools
* Swashbuckle.AspNetCore

---

## Middleware Pipeline

The request pipeline includes:

* CORS Policy (AllowAngular)
* Authentication Middleware
* Authorization Middleware
* Swagger Middleware (Development Only)
* Controller Routing

---

## Error Handling

Current error handling includes:

* ModelState Validation
* Duplicate Data Checks
* HTTP Status Codes
* Startup Database Error Handling

Common Responses:

* 200 OK
* 400 Bad Request
* 401 Unauthorized
* 403 Forbidden
* 404 Not Found

---

## Future Improvements

* Global Exception Handling Middleware
* API Versioning
* Standard API Response Wrapper
* Structured Logging
* Audit Logging
* Pagination & Filtering
* Integration Testing
* Unit Testing
* Enhanced Swagger Documentation
* Production Security Hardening

---

## Author

Abdullah Al Foysal
Shahriar Bin Iqbal
Tahmina Khan
Mohammad Sayem
Sayde Monirul Islam



