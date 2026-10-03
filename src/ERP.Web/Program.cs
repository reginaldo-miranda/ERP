using ERP.Application;
using ERP.Infrastructure;
using ERP.Infrastructure.Data;
using ERP.Infrastructure.Identity;
using ERP.Web.Components;
using ERP.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Adicionar Serviços das Camadas Clean Architecture
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHostedService<ERP.Infrastructure.Services.VencimentoNotificacaoService>();

// MudBlazor Services
builder.Services.AddMudServices();

// API Controllers (para endpoints REST)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services do Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Autenticação no Blazor Server
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ERP.Web.Services.CustomAuthStateProvider>();
builder.Services.AddScoped<ERP.Web.Services.CustomAuthStateProvider>(sp => (ERP.Web.Services.CustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

var app = builder.Build();

// Seed automático do Banco de Dados no Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao popular o banco de dados inicial.");
    }
}

// Pipelines HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
