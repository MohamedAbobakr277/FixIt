<h1 align="center">
  <br>
  🔧 FixIt
  <br>
</h1>

<h4 align="center">A centralized web-based platform for infrastructure maintenance issue reporting and lifecycle management.</h4>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
  <img src="https://img.shields.io/badge/ASP.NET-MVC-blue?style=for-the-badge&logo=microsoft&logoColor=white"/>
  <img src="https://img.shields.io/badge/Entity_Framework-Core_8-orange?style=for-the-badge"/>
  <img src="https://img.shields.io/badge/SQL_Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white"/>
  <img src="https://img.shields.io/badge/DEPI-Graduation_Project-green?style=for-the-badge"/>
</p>

---

## 📋 Table of Contents

- [About The Project](#-about-the-project)
- [Key Features](#-key-features)
- [System Users](#-system-users)
- [Issue Lifecycle](#-issue-lifecycle)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Team](#-team)

---

## 📖 About The Project

**FixIt** is a web-based infrastructure maintenance and issue reporting platform designed to streamline the reporting, tracking, and resolution of maintenance-related problems. Any registered citizen — whether at home, in a company, shop, public facility, or any other location — can submit and monitor maintenance issues digitally.

Reported issues span a wide range of infrastructure and maintenance trades including:

- ⚡ Electrical faults and wiring issues
- 💧 Water leaks and plumbing problems
- 🪵 Carpentry and structural damage
- 🏗️ General building and facility maintenance

The system provides a full **end-to-end lifecycle** for each reported issue — from initial submission through admin review, approval, maintenance scheduling, on-site resolution, and final citizen satisfaction rating.

> Developed as a graduation project under the **Digital Egypt Pioneers Initiative (DEPI)**.

---

## ✨ Key Features

| Feature | Description |
|---------|-------------|
| 📝 Issue Reporting | Citizens submit issues with title, description, category, location, and optional photo |
| 🔍 Admin Dashboard | Admins view, search, filter, and manage all submitted issues |
| ✅ Approve / Reject | Admin reviews issues and approves or rejects with notes |
| 📅 Maintenance Scheduling | Admin sets visit date/time and estimated cost |
| 🔄 Status Tracking | Citizens track their issue status in real-time |
| 🛠️ Maintenance Reports | Admin submits worker report with before/after images |
| ⭐ Rating System | Citizens rate the resolved service (1–5 stars + optional comment) |
| 👤 User Profile | Citizens view their full issue history and past reports |
| 🔐 Role-Based Access | Secure authentication separating Citizen and Admin access |

---

## 👥 System Users

### 🧑 Citizen
- Register and log in
- Submit infrastructure/maintenance issue reports
- Track issue status throughout the lifecycle
- View issue history in profile
- Rate the maintenance service after resolution

### 🛡️ Admin
- Secure login to the Admin Dashboard
- View all issues with search and filter (by status / category / date)
- Approve or reject issues and add notes
- Schedule maintenance visits and add estimated cost
- Update issue status through the workflow
- Submit maintenance report with before/after images
- View citizen ratings and basic analytics

---

## 🔄 Issue Lifecycle

```
[Citizen Submits Issue]
         │
         ▼
      PENDING  ◄─────────────────────────────────┐
         │                                        │
    Admin Reviews                                 │
         │                                        │
    ┌────┴────┐                                   │
    │         │                                   │
    ▼         ▼                                   │
REJECTED   APPROVED                               │
               │                                  │
          Admin Schedules                         │
          Visit + Cost                            │
               │                                  │
               ▼                                  │
          SCHEDULED                               │
               │                                  │
          Work Begins                             │
               │                                  │
               ▼                                  │
          IN PROGRESS                             │
               │                                  │
          Work Completed                          │
          Admin Submits Report                    │
               │                                  │
               ▼                                  │
           RESOLVED                               │
               │                                  │
      Citizen Rates Service (1–5 ⭐)              │
```

---

## 🛠️ Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Frontend** | ASP.NET Core MVC + Razor Views | Web interface for Citizen and Admin |
| **Backend** | ASP.NET Core 8 | Business logic and request handling |
| **Database** | SQL Server | Persistent data storage |
| **ORM** | Entity Framework Core 8 | Database access and object-relational mapping |
| **Authentication** | ASP.NET Core Identity | Secure login and role-based access control |
| **File Storage** | Local Storage (`wwwroot/uploads`) | Issue photos and maintenance report images |
| **UI Framework** | Bootstrap 5 | Responsive and modern UI components |
| **Version Control** | Git & GitHub | Source code management and collaboration |
| **IDE** | Visual Studio 2022 | Primary development environment |

---

## 🏗️ Architecture

FixIt is built using **Layered (N-Tier) Architecture** combined with the **Repository Pattern** and **Unit of Work Pattern**, ensuring clean separation of concerns, maintainability, and scalability.

```
┌─────────────────────────────────────────────────────────────────┐
│                        FixIt.PL                                 │
│              Presentation Layer (ASP.NET MVC)                   │
│         Controllers │ Views │ wwwroot │ Program.cs              │
└─────────────────────────┬───────────────────────────────────────┘
                          │ references
┌─────────────────────────▼───────────────────────────────────────┐
│                        FixIt.BLL                                │
│                  Business Logic Layer                           │
│             Interfaces │ Services │ DTOs                        │
└─────────────────────────┬───────────────────────────────────────┘
                          │ references
┌─────────────────────────▼───────────────────────────────────────┐
│                        FixIt.DAL                                │
│                   Data Access Layer                             │
│     Entities │ DbContext │ Repositories │ Unit of Work          │
└─────────────────────────┬───────────────────────────────────────┘
                          │ references
┌─────────────────────────▼───────────────────────────────────────┐
│                      FixIt.Common                               │
│                  Shared / Cross-Cutting                         │
│              Enums │ Constants │ Helpers                        │
└─────────────────────────────────────────────────────────────────┘
```

### Design Patterns Used

| Pattern | Layer | Purpose |
|---------|-------|---------|
| **Repository Pattern** | DAL | Abstracts and encapsulates all database query logic |
| **Unit of Work Pattern** | DAL | Coordinates multiple repositories in a single transaction |
| **Service Layer Pattern** | BLL | Encapsulates all business rules and application logic |
| **DTO Pattern** | BLL | Transfers data between layers without exposing entities |
| **Dependency Injection** | PL | Decouples components for loose coupling and testability |

---

## 📁 Project Structure

```
FixIt/                              ← Solution Root
│
├── FixIt.sln                       ← Visual Studio Solution File
│
├── FixIt.Common/                   ← Shared Layer (no dependencies)
│   ├── Enums/                      ← IssueStatus, IssueCategory, IssuePriority, Roles
│   ├── Constants/                  ← AppConstants (pagination, file paths, limits)
│   └── Helpers/                    ← DateHelper and shared utility methods
│
├── FixIt.DAL/                      ← Data Access Layer
│   ├── Entities/                   ← ApplicationUser, Issue, MaintenanceReport, Rating
│   ├── Data/                       ← AppDbContext (EF Core + Identity)
│   ├── Repositories/               ← IRepository<T>, GenericRepository, IIssueRepository
│   └── UnitOfWork/                 ← IUnitOfWork, UnitOfWork
│
├── FixIt.BLL/                      ← Business Logic Layer
│   ├── DTOs/                       ← IssueDtos, AccountDtos, ReportDtos, RatingDtos
│   ├── Interfaces/                 ← IIssueService, IReportService, IRatingService
│   └── Services/                   ← IssueService, ReportService, RatingService
│
└── FixIt.PL/                       ← Presentation Layer (ASP.NET Core MVC)
    ├── Controllers/                ← HomeController, AccountController, IssueController
    │                                  AdminController, RatingController
    ├── Views/
    │   ├── Home/                   ← Landing page
    │   ├── Account/                ← Login, Register
    │   ├── Issue/                  ← Index (my issues), Create, Details
    │   ├── Admin/                  ← Dashboard, IssueDetails, Schedule, AddReport
    │   ├── Rating/                 ← Rate (after resolution)
    │   └── Shared/                 ← _Layout, _AdminLayout, _ValidationScripts
    ├── wwwroot/
    │   ├── css/                    ← Stylesheets
    │   ├── js/                     ← JavaScript files
    │   ├── images/                 ← Static images
    │   └── uploads/
    │       ├── issues/             ← Citizen-uploaded issue photos
    │       └── reports/            ← Admin before/after maintenance images
    ├── Program.cs                  ← App entry point, DI registration, Identity setup
    └── appsettings.json            ← Connection string and configuration
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/MohamedAbobakr277/FixIt.git
   cd FixIt
   ```

2. **Configure the connection string**

   In `FixIt.PL/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FixItDb;Trusted_Connection=True;"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   cd FixIt.PL
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

---

## 👨‍💻 Team

| # | Name | Role |
|---|------|------|
| 1 | **Mohamed Abobakr Ahmed** | Team Leader & Full-Stack Developer |
| 2 | Zeyad Abdelmonem Abdo | Backend Developer |
| 3 | Mazen Adel Souliman | Frontend Developer |
| 4 | Rana Shehtta Gaber | UI/UX Designer & Frontend Developer |
| 5 | Jana Ashraf Mohamed | Database Engineer & Backend Developer |
| 6 | Habiba Mohamed Abdelazeam | QA Tester & Documentation Lead |

---

<p align="center">
  Made with ❤️ under the Digital Egypt Pioneers Initiative (DEPI)
</p>
