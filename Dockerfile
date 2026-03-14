FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/JobMarket.API/JobMarket.API.csproj", "src/JobMarket.API/"]
COPY ["src/JobMarket.Application/JobMarket.Application.csproj", "src/JobMarket.Application/"]
COPY ["src/JobMarket.Domain/JobMarket.Domain.csproj", "src/JobMarket.Domain/"]
COPY ["src/JobMarket.Infrastructure/JobMarket.Infrastructure.csproj", "src/JobMarket.Infrastructure/"]

RUN dotnet restore "src/JobMarket.API/JobMarket.API.csproj"

COPY . .

RUN dotnet build "src/JobMarket.API/JobMarket.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/JobMarket.API/JobMarket.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=publish /app/publish .

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "JobMarket.API.dll"]
