using System.Text;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.Models;
using IS_AdvokatskeKancelarije.Repositories;
using IS_AdvokatskeKancelarije.Repositories.Invoices;
using IS_AdvokatskeKancelarije.Repositories.Meetings;
using IS_AdvokatskeKancelarije.Repositories.Trials;
using IS_AdvokatskeKancelarije.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<ICaseRepository,CaseRepository>();
builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
builder.Services.AddScoped<ITrialsRepository,TrialsRepository>();
builder.Services.AddScoped<IInvoicesRepository,InvoicesRepository>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<FileStorageService>();


// AI pretraga dokumenata (izdvajanje teksta, chunking, embeddings, pretraga)
builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection("Ollama"));
builder.Services.AddHttpClient<OllamaService>((sp, client) =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddSingleton<TextExtractionService>();
builder.Services.AddSingleton<ChunkingService>();
builder.Services.AddSingleton<IndexingQueue>();
builder.Services.AddScoped<DocumentIndexer>();
builder.Services.AddScoped<SearchService>();
builder.Services.Configure<RagSettings>(builder.Configuration.GetSection("Rag"));
builder.Services.AddScoped<RagService>();
builder.Services.AddHostedService<IndexingWorker>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
    );

builder.Services.AddAutoMapper(cfg => {
    cfg.LicenseKey = "Licence_KEY";
    cfg.AddMaps(typeof(Program).Assembly);
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException("Connection String is not found"));
    Console.WriteLine($"Connection String: {builder.Configuration.GetConnectionString("DefaultConnection")}");
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddRoles<IdentityRole>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudiences = builder.Configuration.GetSection("Jwt:Audiences").Get<List<string>>(),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

app.UseCors(policy =>
    policy.WithOrigins("http://localhost:4200")//https://melina-bipyramidal-tyler.ngrok-free.dev // Angular dev server
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()
          .WithExposedHeaders("Content-Disposition"));


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseRouting();




app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
