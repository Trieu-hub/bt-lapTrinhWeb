# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**FILMIX** — a Netflix-inspired movie/TV streaming web app built with ASP.NET Core 9 MVC (Razor Views). The project name in code is `untitled1` (namespace and assembly).

## Commands

### Run the app
```powershell
dotnet run
```
The app auto-creates/seeds the database on first launch via `db.Database.EnsureCreated()` in `Program.cs`.

### Build
```powershell
dotnet build
```

### Switch database provider
Edit `appsettings.json`:
- `"DbProvider": "MySql"` — uses Pomelo with `Server=localhost;Database=filmix_db;User=root;Password=123456;`
- `"DbProvider": "SqlServer"` — uses SQL Server with the same `DefaultConnection` string

> **Note:** If the schema changes (new entities added), the app detects mismatches at startup, drops the DB, and recreates it with seed data automatically.

## Architecture

### Data layer
- `Data/ApplicationDbContext.cs` — single DbContext with all DbSets and full seed data (categories, movies, movie-category links, episodes, movie images) defined in `OnModelCreating`.
- No migrations are used; the app relies on `EnsureCreated` + `EnsureDeleted` for schema management.

### Domain model (`Models/Entities/Entities.cs`)
- `Movie` — core entity; `IsTVSeries` flag distinguishes films from TV series
- `Category` — genre categories (5 seeded: Hành Động, Kinh Dị, Viễn Tưởng, Tình Cảm, Kịch Tính)
- `MovieCategory` — explicit many-to-many join table between `Movie` and `Category`
- `Episode` — one-to-many child of `Movie` (has `SeasonNumber` + `EpisodeNumber`)
- `MovieImage` — one-to-many still images per movie

### Controllers & Routes (default route: `{controller=Home}/{action=Index}/{id?}`)
| Controller | Route | Purpose |
|---|---|---|
| `HomeController` | `/` | Landing/homepage |
| `TVShowsController` | `/TVShows` | Lists movies where `IsTVSeries = true` |
| `MoviesController` | `/Movies` | Lists movies where `IsTVSeries = false` |
| `NewHotController` | `/NewHot` | "New & Hot" browse page |
| `ProductController` | `/Product/List`, `/Product/Detail/{id}` | Browse with category filter + detail page |
| `AccountController` | `/Account/Auth` | Login/register UI (no backend auth yet) |

### Key patterns
- **Detail page query**: Always use `.AsSplitQuery()` when loading a `Movie` with `.Include(Episodes)` + `.Include(MovieCategories)` + `.Include(MovieImages)` together — avoids EF Core cartesian explosion crash.
- **Watchlist**: Stored entirely in browser `localStorage` (key not server-side). The save/load logic lives in `Views/Product/Detail.cshtml` as inline JavaScript.
- **Category filter** in `ProductController.List`: optional `categoryId` query param filters via `MovieCategories.Any(mc => mc.CategoryId == ...)`.

### Frontend
- `Views/Shared/_Layout.cshtml` — main layout with navbar linking all sections
- Page-specific CSS in `wwwroot/css/`: `site.css` (global), `style.css`, `movies.css`, `newhot.css`
- `Views/Product/List.cshtml` has a two-tab layout: "Khám Phá Phim" (filter by category) and "Danh Sách Của Tôi" (watchlist from localStorage)
- Static images: `wwwroot/images/movies/` (1.jpg–10.jpg + stills), `wwwroot/images/hero/`, `wwwroot/images/tvshows/`, `wwwroot/images/auth/`

### Scaffolded but empty
- `Areas/Admin/` and `Areas/Customer/` — directories exist but contain no files
- `Repository/IRepository/` and `Repository/Repository/` — directories exist but contain no files
- `Models/DTOs/` — directory exists but contains no files
