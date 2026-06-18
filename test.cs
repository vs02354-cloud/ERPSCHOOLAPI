using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer("Server=tcp:erpschooldbserver.database.windows.net,1433;Initial Catalog=erpschooldb;Persist Security Info=False;User ID=vedsharma;Password=P@ssw0rd123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
using var context = new ApplicationDbContext(optionsBuilder.Options);

var users = context.Users.ToList();
foreach(var u in users) {
    Console.WriteLine($"{u.UserName} - IsActive: {u.IsActive}");
}
