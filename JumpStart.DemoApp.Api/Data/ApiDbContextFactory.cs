// Copyright ©2026 Scott Blomfield
/*
 *  This program is free software: you can redistribute it and/or modify it under the terms of the
 *  GNU General Public License as published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
 *  even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along with this program. If not,
 *  see <https://www.gnu.org/licenses/>. 
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JumpStart.DemoApp.Api.Data;

/// <summary>
/// Design-time factory for creating ApiDbContext instances for EF Core tools.
/// </summary>
public class ApiDbContextFactory : IDesignTimeDbContextFactory<ApiDbContext>
{
    /// <summary>
    /// Creates a new instance of ApiDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A configured ApiDbContext instance.</returns>
    public ApiDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApiDbContext>();

        // Use SQL Server with a default connection string for migrations
        // This should match the connection string in appsettings.json
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=JumpStart_DemoApp;Trusted_Connection=True;MultipleActiveResultSets=true",
            b => b.MigrationsAssembly("JumpStart.DemoApp.Api"));

        return new ApiDbContext(optionsBuilder.Options);
    }
}
