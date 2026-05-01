FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SportsBetting.slnx", "./"]
COPY ["src/SportsBetting.Api/SportsBetting.Api.csproj", "src/SportsBetting.Api/"]
COPY ["src/SportsBetting.Application/SportsBetting.Application.csproj", "src/SportsBetting.Application/"]
COPY ["src/SportsBetting.Domain/SportsBetting.Domain.csproj", "src/SportsBetting.Domain/"]
COPY ["src/SportsBetting.Infrastructure/SportsBetting.Infrastructure.csproj", "src/SportsBetting.Infrastructure/"]

RUN dotnet restore "src/SportsBetting.Api/SportsBetting.Api.csproj"

COPY . .
RUN dotnet publish "src/SportsBetting.Api/SportsBetting.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SportsBetting.Api.dll"]
