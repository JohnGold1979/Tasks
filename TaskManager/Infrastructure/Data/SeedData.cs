using Microsoft.EntityFrameworkCore;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Departments
            if (!await context.Departments.AnyAsync())
            {
                var departments = new[]
                {
                    new Department { Name = "IT" },
                    new Department { Name = "HR" }
                };
                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }

            // Roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Role { Name = "Chief" },
                    new Role { Name = "Employee" },
                    new Role { Name = "Observer" }
                };
                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Permissions
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new[]
                {
                    new Permission { Name = "tasks.create" },
                    new Permission { Name = "tasks.view.department" },
                    new Permission { Name = "tasks.view.own" },
                    new Permission { Name = "tasks.edit.all" },
                    new Permission { Name = "tasks.edit.own.status" },
                    new Permission { Name = "tasks.assign" },
                    new Permission { Name = "tasks.delete" }
                };
                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }

            // RolePermissions
            if (!await context.RolePermissions.AnyAsync())
            {
                var chief = await context.Roles.FirstAsync(r => r.Name == "Chief");
                var employee = await context.Roles.FirstAsync(r => r.Name == "Employee");
                var observer = await context.Roles.FirstAsync(r => r.Name == "Observer");

                var allPermissions = await context.Permissions.ToListAsync();

                foreach (var perm in allPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = chief.Id,
                        PermissionId = perm.Id
                    });
                }

                var employeePerms = allPermissions
                    .Where(p => p.Name == "tasks.view.own" ||
                               p.Name == "tasks.edit.own.status" ||
                               p.Name == "tasks.create");
                foreach (var perm in employeePerms)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = employee.Id,
                        PermissionId = perm.Id
                    });
                }

                var observerPerms = allPermissions
                    .Where(p => p.Name == "tasks.view.department");
                foreach (var perm in observerPerms)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = observer.Id,
                        PermissionId = perm.Id
                    });
                }

                await context.SaveChangesAsync();
            }

            // Users
            if (!await context.Users.AnyAsync())
            {
                var itDept = await context.Departments.FirstAsync(d => d.Name == "IT");
                var hrDept = await context.Departments.FirstAsync(d => d.Name == "HR");
                var chiefRole = await context.Roles.FirstAsync(r => r.Name == "Chief");
                var employeeRole = await context.Roles.FirstAsync(r => r.Name == "Employee");
                var observerRole = await context.Roles.FirstAsync(r => r.Name == "Observer");

                var users = new[]
                {
                    new User
                    {
                        Username = "chief@it",
                        Email = "chief@it.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                        DepartmentId = itDept.Id,
                        RoleId = chiefRole.Id
                    },
                    new User
                    {
                        Username = "employee@it",
                        Email = "employee@it.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                        DepartmentId = itDept.Id,
                        RoleId = employeeRole.Id
                    },
                    new User
                    {
                        Username = "observer@it",
                        Email = "observer@it.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                        DepartmentId = itDept.Id,
                        RoleId = observerRole.Id
                    },
                    new User
                    {
                        Username = "chief@hr",
                        Email = "chief@hr.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                        DepartmentId = hrDept.Id,
                        RoleId = chiefRole.Id
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
        }
    }
}