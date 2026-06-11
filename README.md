# SNUS Project 2026
Distributed sensor monitoring system for collecting, processing and storing temperature readings from sensor nodes.

## Team members
- Marko Pavlovic, SV5/2023,
- Ognjen Miletic, SV47/2023,
- Lazar Vilotic, SV51/2023 

## Project Structure

- `SNUS.SensorClient` - console client for simulating sensors
- `SNUS.IngestionService` - Web API service for receiving sensor readings
- `SNUS.ConsensusService` - worker service for calculating consensus values
- `SNUS.NotificationService` - service for real-time alarm notifications
- `SNUS.Shared` - shared DTOs, enums and common models
- `SNUS.Persistence` - database entities, DbContext and migrations

## Technologies

- C#
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker
- Kubernetes
