# FixTrail Security - Deployment Guide
## Prerequisites
- **PostgreSQL 16+** server running.
- **.NET 8.0 Runtime** for the API.
- **Node.js / Nginx** for the Angular Client.
## Backend Deployment (ASP.NET Core)
1. Set the environment variable `ASPNETCORE_ENVIRONMENT=Production`.
2. Override the `DefaultConnection` string in `appsettings.Production.json` or via environment variables to point to the production PostgreSQL instance.
3. Run EF Core migrations on the production database: `dotnet ef database update --project FixTrailSecurity.Api`.
4. Build and publish: `dotnet publish -c Release -o ./publish`.
5. Host the `./publish` directory using IIS (Windows) or Kestrel + Nginx reverse proxy (Linux).
## Frontend Deployment (Angular)
1. Update `src/app/services/api.service.ts` to replace `http://localhost:5000/api` with the production API URL (or use environment.ts).
2. Build the application: `npm run build`.
3. The output will be in `dist/FixTrailSecurity.Client`.
4. Copy the contents of the `dist` folder to a static file server (e.g., Nginx, Apache, or Azure Static Web Apps).
5. Ensure the server redirects all 404 traffic to `index.html` (for Angular Routing to work).
