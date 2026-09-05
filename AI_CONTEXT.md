# Gym Website Project

Build a gym website platform with two applications:

1. gym-web - public React website
2. gym-admin - admin React CMS
3. gym-api - ASP.NET Core Web API

## Stack
Frontend: React + TypeScript
Backend: ASP.NET Core Web API + C#
Database: SQL Server + EF Core
Auth: JWT
Architecture: Controller → Service → EF Core
No microservices.

## Current Goal
Build ONLY the admin CMS first.

Admin will manage the content displayed on the public website.

Initial modules:
- Login
- Dashboard
- Hero
- About
- Services
- Trainers
- Membership Plans
- Gallery
- Testimonials
- Contact
- Settings
- Roles & Permissions

Keep the admin UI simple, clean and CRUD-focused.

Public website should have a premium dark fitness design inspired by the provided reference image:
- dark/black background
- orange accent
- bold typography
- modern fitness aesthetic
- large hero image
- responsive design

## Authorization
Use:
- Admin
- Trainer

Implement permission-based authorization, not hardcoded UI-only role checks.

Admin = full access.

Trainer permissions must be configurable by Admin.

## Important
Do not build future CRM functionality yet:
- member attendance
- payments
- workout tracking
- diet tracking

These will be added later.

## Development Rules
- Do not create unnecessary files.
- Do not over-engineer.
- Reuse components.
- Use DTOs.
- Use dependency injection.
- Keep controllers thin.
- Put business logic in services.
- Validate API input.
- Handle errors properly.
- Do not change unrelated code.
- Before coding, inspect the existing structure.
- After changes, build/test the affected project.