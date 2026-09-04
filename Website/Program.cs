using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MurendeniConstructionArchitects.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    // Clients can only use /Portal, staff can only use /Admin -
    // enforced by the "role" claim each login sets.
    options.Conventions.AuthorizeFolder("/Portal", "ClientOnly");
    options.Conventions.AuthorizeFolder("/Admin", "StaffOnly");

    // Overrides for pages that need something stricter than "any staff":
    options.Conventions.AuthorizePage("/Admin/AddEmployee", "AdminOnly");
    options.Conventions.AuthorizePage("/Admin/ManagePortfolio", "AdminOnly");
    options.Conventions.AuthorizePage("/Admin/Reports", "AdminOnly");

    options.Conventions.AuthorizePage("/Admin/ManageClients", "SalesOrAdmin");
    options.Conventions.AuthorizePage("/Admin/EditClient", "SalesOrAdmin");
    options.Conventions.AuthorizePage("/Admin/ManageProjects", "SalesOrAdmin");
    options.Conventions.AuthorizePage("/Admin/AddProject", "SalesOrAdmin");
    options.Conventions.AuthorizePage("/Admin/EditProject", "SalesOrAdmin");
    options.Conventions.AuthorizePage("/Admin/ManageInquiries", "SalesOrAdmin");
    options.Conventions.AuthorizePage("/Admin/ViewInquiry", "SalesOrAdmin");

    options.Conventions.AuthorizePage("/Admin/MyProjects", "DesignerOrAdmin");
    options.Conventions.AuthorizePage("/Admin/UpdateProjectStatus", "DesignerOrAdmin");
    options.Conventions.AuthorizePage("/Admin/UploadDesignFile", "DesignerOrAdmin");

    options.Conventions.AuthorizePage("/Admin/ManageEmployees", "AdminOnly");
    options.Conventions.AuthorizePage("/Admin/EditEmployee", "AdminOnly");
});

// Cookie authentication: after a successful login we hand the browser a
// secure cookie. On future requests, that cookie tells us who's logged in.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        // Different from LoginPath: this is where we send someone who IS
        // logged in, but tries to open an area their role doesn't allow
        // (e.g. a Client opening /Admin).
        options.AccessDeniedPath = "/AccessDenied";
    });

// These two policies are what AuthorizeFolder above actually checks -
// they look at the "role" claim we put in the cookie at login time.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ClientOnly", policy => policy.RequireClaim("role", "Client"));
    options.AddPolicy("StaffOnly", policy => policy.RequireClaim("role", "Admin", "Designer", "Sales"));
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("role", "Admin"));
    options.AddPolicy("SalesOrAdmin", policy => policy.RequireClaim("role", "Admin", "Sales"));
    options.AddPolicy("DesignerOrAdmin", policy => policy.RequireClaim("role", "Admin", "Designer"));
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// UseAuthentication must come before UseAuthorization - it figures out
// WHO the visitor is; UseAuthorization then decides WHAT they're allowed to see.
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();