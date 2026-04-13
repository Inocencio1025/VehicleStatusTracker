# VehicleStatusTracker

## Overview

VehicleStatusTracker is a full-stack application that simulates and visualizes live vehicle telemetry data in real time. It generates continuous vehicle updates in the background and displays them instantly on a live dashboard.

---

## Status
Completed (no active development planned)

---

## Why this exists

This project was built primarily as a learning exercise and to strengthen my backend + full-stack development skills.

---

## Key Features

- Real-time vehicle telemetry dashboard with live updates
- Simulated vehicle data stream for realistic tracking
- REST API + SignalR for real-time vehicle updates  
- Interactive React + Vite dashboard 
- Persistent data storage 
- Automated testing

---

## Architecture

The system is built around a real-time telemetry pipeline that simulates and displays vehicle data end-to-end.

Frontend (React + Vite)
→ Connects via REST API and SignalR
→ ASP.NET Core backend handles API requests and real-time communication
→ Background service generates continuous simulated vehicle telemetry
→ Entity Framework Core manages data persistence
→ SQLite stores vehicle state and history

---

## Tech Stack

Backend
- ASP.NET Core
- Entity Framework Core
- SignalR
- SQLite

Frontend
- React (Vite)
- TypeScript

Testing
- xUnit

DevOps / Tooling
- Docker
- CI pipeline using GitHub Actions  


---

## Running with Docker

```bash
docker compose up --build
```
- Frontend: http://localhost:5173 
- Backend: http://localhost:8080  
- Swagger: http://localhost:8080/swagger  

---

## Testing

dotnet test

Includes:
- Unit tests (in api controller and background service to simulate live data)
- Integration test (WebApplicationFactory smoke test)

---

## CI Pipeline

GitHub Actions workflow:
- Builds solution
- Runs unit tests
- Runs integration test
- Validates build integrity

---

## What I Learned

- How to structure a multi-project .NET solution cleanly  
- Building real-time applications using SignalR  
- Creating background services for continuous data simulation  
- Writing and organizing unit and integration tests  
- Using ILogger for structured logging instead of Console.WriteLine  
- Managing configuration across different environments  
- Writing meaningful Git commit messages  
- Setting up CI pipelines using GitHub Actions  
- Containerizing applications with Docker  
- Improving backend architecture and system design over time  

---

## Future Improvements

I probably won’t come back to this project in any serious way, but if I do, it would mostly be to experiment, learn new tools, or try out different software engineering practices to keep improving.
- Improve frontend UI/UX  
- Optimize Docker development workflow (hot reload)  
- Expand telemetry simulation realism  

---

## Notes

I started this project last year as a way to push myself forward after realizing I wasn’t getting many interview opportunities. At the time, most of my experience came from unfinished projects like games and a mobile app, so I didn’t have something solid and complete to point to.

While looking into local job markets, I noticed how common .NET was in the Michigan area, which led me down the path of learning it more seriously. From there, I chose to build a vehicle telemetry system since it aligned with the types of backend and full-stack roles I was seeing around me.

I’ll be honest, the concept itself wasn’t something I was particularly passionate about, but I didn’t mind that—instead, I used it as a practical way to learn and build something relevant while improving my skills.

Even though the project is relatively simple in design, I intentionally treated it as a learning environment. I focused less on “adding features” and more on improving how I work as a developer: writing better Git commits, practicing cleaner code structure, adding unit tests, and getting comfortable with tools and patterns I hadn’t used deeply before.

After stepping away from the project for a while, I came back to it more recently after reading *The Missing Semester of Your CS Education*. It made me realize I needed more hands-on repetition with real projects to properly internalize the concepts I had been learning. Since this codebase was already familiar and fairly clean, it was an easy place to jump back in and continue improving.

Since returning to it, I’ve focused on things like strengthening my Git workflow (including branching and resolving merge conflicts), expanding test coverage with unit and basic integration tests, replacing console logging with structured logging, improving configuration handling, and setting up CI with automated testing through GitHub Actions.

I also containerized the application using Docker Compose, which helped me better understand Dockerfiles, YAML configuration, and how ignore files like `.gitignore` and `.dockerignore` actually affect real workflows.

Some of the more challenging parts came from practical setup issues rather than the code itself—things like dealing with port conflicts and handling Docker environments where the application starts with an empty state. In a local environment, the data is already in memory, but inside a container everything starts fresh, so I had to account for that and ensure the backend wouldn’t fail on startup during tests.

Trying Vim for the first time on a project was also challenging. It is a work in progress lol.

At this point, I consider this project complete for my current goals. It already covers a wide range of concepts I’m still actively processing, and pushing it further would start to move beyond what I’m trying to learn right now. Overall, it served its purpose as a strong, practical exercise in real-world software development.

---

## Screenshot
When you get things up and running, the telemetry dashboard should look like this:

<img width="405" height="1061" alt="image" src="https://github.com/user-attachments/assets/778151f3-7af0-4a85-a0f6-9c3ef072f2f3" />
