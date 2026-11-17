FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props .
COPY Marketplace.Contracts/Marketplace.Contracts.csproj Marketplace.Contracts/
COPY Marketplace.Api/Marketplace.Api.csproj Marketplace.Api/
RUN dotnet restore Marketplace.Api/Marketplace.Api.csproj
COPY Marketplace.Contracts Marketplace.Contracts
COPY Marketplace.Api Marketplace.Api
RUN dotnet publish Marketplace.Api/Marketplace.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__Default="Data Source=/data/marketplace.db"
VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "Marketplace.Api.dll"]
