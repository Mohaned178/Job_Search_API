using JobMarket.Application.Common.Interfaces;
using JobMarket.Infrastructure.AI;
using JobMarket.Infrastructure.Auth;
using JobMarket.Infrastructure.Extraction;
using JobMarket.Infrastructure.Jobs;
using JobMarket.Infrastructure.Options;
using JobMarket.Infrastructure.Persistence;
using JobMarket.Infrastructure.Scrapers;
using JobMarket.Infrastructure.Storage;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;

namespace JobMarket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Postgres")!;
        string redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, o =>
                o.UseVector()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<OpenAiOptions>(configuration.GetSection("OpenAi"));
        services.Configure<Options.ScraperOptions>(configuration.GetSection("Scrapers"));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJobEnqueuer, HangfireJobEnqueuer>();

        services.AddScoped<IEmbeddingService, OpenAiEmbeddingService>();
        services.AddScoped<ICvParsingService, OpenAiCvParsingService>();
        services.AddScoped<IAtsService, AtsService>();
        services.AddScoped<IJobEnrichmentService, OpenAiJobEnrichmentService>();
        services.AddScoped<IVectorSearchService, VectorSearchService>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ICvTextExtractor, PdfPigTextExtractor>();
        services.AddScoped<PdfPigTextExtractor>();
        services.AddScoped<DocxTextExtractor>();
        services.AddScoped<CvTextExtractorFactory>();

        services.AddScoped<ICvProcessingJob, CvProcessingJob>();
        services.AddScoped<IJobIngestionJob, JobIngestionJob>();
        services.AddScoped<JobEnrichmentJob>();

        IAsyncPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        services.AddHttpClient<WuzzufScraperStrategy>()
            .AddPolicyHandler(retryPolicy);
        services.AddHttpClient<IndeedScraperStrategy>()
            .AddPolicyHandler(retryPolicy);
        services.AddHttpClient<AdzunaApiStrategy>()
            .AddPolicyHandler(retryPolicy);
        services.AddHttpClient<RemotiveApiStrategy>()
            .AddPolicyHandler(retryPolicy);

        services.AddScoped<IScraperStrategy, WuzzufScraperStrategy>();
        services.AddScoped<IScraperStrategy, IndeedScraperStrategy>();
        services.AddScoped<IScraperStrategy, AdzunaApiStrategy>();
        services.AddScoped<IScraperStrategy, RemotiveApiStrategy>();
        services.AddScoped<ScraperFactory>();

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection));

        services.AddHangfire(cfg =>
            cfg.UsePostgreSqlStorage(o => o.UseNpgsqlConnection(connectionString)));
        services.AddHangfireServer();

        return services;
    }
}
