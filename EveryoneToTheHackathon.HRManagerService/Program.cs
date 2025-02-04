using EveryoneToTheHackathon.Entities;
using EveryoneToTheHackathon.HRManagerService;
using EveryoneToTheHackathon.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", true, true);
builder.Logging.AddConsole();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8082);
});

Int32.TryParse(builder.Configuration["EMPLOYEES_NUM"] ?? throw new SettingsException(), out var employeesNumber);
var hrDirectorUrl = new Uri(builder.Configuration["HrDirectorUri"] ?? throw new SettingsException());

var connString =
    String.Format(
        "Host={0};Port={1};Database={2};Username={3};Password={4};SSLMode=Prefer;Pooling=false",
        builder.Configuration["Database:Host"] ?? throw new SettingsException(),
        builder.Configuration["Database:Port"] ?? throw new SettingsException(),
        builder.Configuration["Database:Database"] ?? throw new SettingsException(),
        builder.Configuration["Database:Username"] ?? throw new SettingsException(),
        builder.Configuration["Database:Password"] ?? throw new SettingsException()
    );

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseNpgsql(connString);
    options.EnableDetailedErrors();
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<HrManagerConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri("amqp://rabbitmq:5672/"));
        cfg.ReceiveEndpoint($"HRManager", e =>
        {
            e.ConfigureConsumers(context);
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(10)));
        });
    });
});

builder.Services.AddHttpClient<HrManagerBackgroundService>(options => options.BaseAddress = hrDirectorUrl);

builder.Services.AddOptions();
builder.Services.Configure<ControllerSettings>(settings => settings.EmployeesNumber = employeesNumber);

builder.Services.AddSingleton<IHackathonRepository, HackathonRepository>();
builder.Services.AddSingleton<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddSingleton<IWishlistRepository, WishlistRepository>();
builder.Services.AddSingleton<ITeamRepository, TeamRepository>();

builder.Services.AddSingleton<HRManager>(_ => new HRManager(new ProposeAndRejectAlgorithm()));
builder.Services.AddSingleton<HrManagerConsumer>();
builder.Services.AddSingleton<HrManagerService>();

builder.Services.AddHostedService<HrManagerBackgroundService>(s => 
    new HrManagerBackgroundService(
        s.GetRequiredService<ILogger<HrManagerBackgroundService>>(),
        s.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(HrManagerBackgroundService)),
        s.GetRequiredService<HrManagerService>()));

builder.Services.AddControllers().AddApplicationPart(typeof(HrManagerController).Assembly);

var app = builder.Build();

app.UseRouting();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints => endpoints.MapControllers());
#pragma warning restore ASP0014

await app.RunAsync();