# YourApi Solution

A modern, clean architecture solution template for building scalable applications with .NET 9. This solution provides a solid foundation for building complex enterprise applications using clean architecture principles.

## Features

- ✨ Clean Architecture
- 🎯 CQRS with MediatR
- ✅ FluentValidation
- 🗄️ Entity Framework Core
- 🔐 Keycloak Authentication
- 🌐 CORS Configuration for modern SPAs
- 🔄 Automatic Database Migrations

## Prerequisites

- .NET 9 SDK
- Docker
- SQL Server ()
- Keycloak Server (for authentication)

### Docker Configuration

The `.docker` folder contains the configuration files for Docker, including:

- `.env`: Environment variables for Docker.
- `docker-compose.yml`: Docker Compose configuration file.
- `volumes/`: Docker volumes.

### Nuxt Keycloak Integration

The `.nuxt-keycloak-integration` folder contains the configuration and composables for integrating Keycloak with Nuxt.js, including:

- `.env`: Environment variables for Nuxt.js.
- `composables/`: Reusable Vue.js composables.
- `layouts/`: Layouts for Nuxt.js pages.
- `nuxt.config.js`: Nuxt.js configuration file.
- `pages/`: Nuxt.js pages.

### Clean Architecture

This solution follows the Clean Architecture principles, which include:

- **YourApi.Api**: The entry point of the application, responsible for configuring and running the web server.
- **YourApi.Application**: Contains the application logic, including CQRS with MediatR and FluentValidation.
- **YourApi.Domain**: Contains the core domain entities and business logic.
- **YourApi.Infrastructure**: Contains the infrastructure code, including Entity Framework Core for data access.

### Project Structure
```
📁 .docker
   ├─🐳 docker-compose.yml (Container orchestration configuration)
   ├─🔒 .env (Environment variables and secrets)
📁 .nuxt-keycloak-integration
📁 scripts
   ├─📁 database
   │  └─⚙️ Manage-Migrations.ps1 (Database migration automation script)
   └─📁 maintenance
      └─📦 Update-Packages.ps1 (NuGet packages update automation)

📁 YourApi.Api
   ├─📁 Controllers
   ├─📁 Http
   └─📁 Properties

📁 YourApi.Application
   ├─📁 Common
   │  ├─📁 Behaviors
   │  ├─📁 Interfaces
   │  └─📁 Models
   │     └─📁 Authentication
   └─📁 Features
      └─📁 Posts
         └─📁 Commands
            └─📁 CreatePost

📁 YourApi.Domain
   ├─📁 Common
   └─📁 Entities

📁 YourApi.Infrastructure
   ├─📁 Health
   ├─📁 Migrations
   ├─📁 Persistence
   └─📁 Services
```
## Installation

1. Clone the repository:

```bash
git clone https://github.com/yourusername/yourapi.git
cd yourapi
