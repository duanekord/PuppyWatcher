# Dockerfile for PuppyWeightWatcher.Web (.NET 10, multi-stage build)
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["PuppyWeightWatcher.Web.csproj", "./"]
RUN dotnet restore "./PuppyWeightWatcher.Web.csproj"
COPY . .
RUN dotnet publish "PuppyWeightWatcher.Web.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PuppyWeightWatcher.Web.dll"]
