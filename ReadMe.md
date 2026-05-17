# Technical Assessment: Trade Management & Reporting System
---

## 1. Architectural Overview
This solution is built using a decoupled **Clean Architecture (N-Tier)** approach targeting **.NET 8** and **C# 12**. The architecture isolates business logic from data access and external integration boundaries, solving historical stabilization issues and ensuring multi-year longevity.

As shown in the solution layout, the project is structured into 4 logical areas:
- **Docs:** Solution documentation and design reference artifacts.
- **Tracker.API (Presentation Layer):** An ASP.NET Core 8 Web API utilizing highly scalable, dependency-injected Controllers to expose the HTTP REST endpoints.
- **Tracker.Core (Domain Layer):** The absolute core of the application containing domain models, business entities, interfaces, and core exceptions. It is strictly cross-platform and has zero external dependencies.
- **Tracker.Infrastructure (Data & Integration Layer):** Manages external system boundaries. It houses Entity Framework Core 8 for high-performance SQL Server operations and manages communication with legacy subsystems.
- **Tracker.LegacySoapStub (Integration Testing Stub):** A dedicated WCF/SOAP host stub included within the solution boundary. This simulates the legacy enrichment service dependency locally, ensuring the assessment runs completely standalone without external network requirements.

---

## 2. Technology Stack Justification & Engineering Decisions

### A. Reporting Aggregation: EF Core 8 vs. Stored Procedures
The system requirements dictate that **all reporting calculations must compute on the database engine**. This architecture fulfills this using **Entity Framework Core 8 LINQ Deferred Execution** rather than traditional database-resident Stored Procedures due to the following strategic principles:

1. **Type Safety & Build-Time Validation:** LINQ queries compile natively against the strongly typed C# Domain Entities. If a column or database schema mapping modifies, compilation fails instantly. Conversely, Stored Procedures create string-typed application decoupling where structural breaking changes are dangerously discovered only at runtime in production environments.
2. **True Server-Side Database Execution:** By applying `.Where()`, `.GroupBy()`, `.Sum()`, and `.Average()` operations directly against an `IQueryable` pipeline, EF Core compiles the expression tree into highly optimized native SQL operators (`GROUP BY`, `SUM`, `AVG`). Computation happens entirely within the SQL Server instance; only the lean, calculated reporting payload rows are sent over the network socket back to the application.
3. **Prevention of Business Logic Fragmentation:** Traditional setups fracture system rules across application codebases and individual SQL Server servers. By centralizing operations inside the C# Domain Layer, code reviews, version control (Git tracking), and operational maintainability are standardized across the tech stack.

### B. Strategic Use-Cases for Stored Procedures
While EF Core 8 satisfies this transactional report elegantly, Stored Procedures remain valid for alternative workloads:
- **Massive Batch Adjustments:** Bulk modifications targeting millions of rows simultaneously where individual entity change-tracking overhead degrades system memory allocations.
- **Highly Complex Recursive Layouts:** Massive cross-table queries containing high amounts of arbitrary hierarchical recursion loops that strain compiler-driven LINQ expression translations.
- **DBA-Siloed Environments:** Corporate settings requiring strict execution plan locking and deep indexing isolation protocols managed exclusively by external Database Teams.

---

## 3. Step-by-Step Guide to Run the Application

Follow these steps to build, initialize, and test the entire multi-project system using **Visual Studio 2022** on Windows constraints.

### Prerequisites
- **IDE:** Visual Studio 2022 (Version 17.8 or newer highly recommended).
- **Workloads Installed:** - *ASP.NET and web development*
  - *Data storage and processing* (Includes SQL Server LocalDB).
- **SDK:** .NET 8.0 SDK.

### Step 1: Open the Solution
1. Locate the master solution file: `Tracker.API.sln`.
2. Open it using **Visual Studio 2022**.
3. Verify that all 4 projects (`Tracker.API`, `Tracker.Core`, `Tracker.Infrastructure`, `Tracker.LegacySoapStub`) load cleanly in your Solution Explorer.

### Step 2: Database Preparation (SQL Server LocalDB)
The data layer leverages SQL Server 2019 LocalDB using an automatic, portable physical file injection strategy (`|DataDirectory|`).
1. The Entity Framework Core integration will automatically verify the local data repository instance on launch.
2. If executing manual adjustments via the Package Manager Console, ensure your default project target points directly to `Tracker.Infrastructure`.

### Step 3: Configure Multiple Startup Projects
Because the system interacts with a legacy service, both the API endpoint layer and the legacy SOAP service stub must execute concurrently:
1. Right-click the top-level **Solution 'Tracker.API'** node in the Solution Explorer and select **Properties**.
2. Under **Common Properties**, select **Startup Project**.
3. Toggle the radio button to **Multiple startup projects**.
4. Configure the following execution states:
   - `Tracker.LegacySoapStub` -> Set Action to **Start** (This spins up the mock WCF/SOAP dependency).
   - `Tracker.API` -> Set Action to **Start** (This hosts your modern REST HTTP API).
5. Click **Apply** and then click **OK**.

### Step 4: Execute and Run the System
1. Press **F5** (or click the green **Start** arrow on the top toolbar).
2. Visual Studio 2022 will compile the solution, start the WCF background hosting lifecycle, and launch your browser.
3. The `Tracker.API` layer exposes interactive **Swagger UI** middleware out-of-the-box. Use the Swagger console interface (`/swagger/index.html`) to pass parameter inputs (`from` and `to` date strings) to verify the database aggregation logic directly from your browser windows.

---

## Step 5 AI Engineering Assistance
In alignment with modern DevOps and accelerated delivery workflows, generative AI was utilized strictly as an architectural sounding board and code-review assistant. Below are the minimal, highly targeted prompts leveraged during development:

1. **Architecture Validation:**
   > *"Validate a 3-tier Clean Architecture structure using .NET 8 and a legacy WCF component to ensure strict isolation of the domain layer."*
   
2. **EF Core Translation Check:**
   > *"Verify that an EF Core 8 LINQ GroupBy and Sum expression properly defers execution and aggregates entirely on the SQL Server instance without in-memory evaluation."*

3. **C# 12 Best Practices:**
   > *"Provide example syntax for C# 12 Primary Constructors specifically for Dependency Injection in an ASP.NET Core Web API controller to minimize boilerplate."*