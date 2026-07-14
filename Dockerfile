FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN apt-get update && apt-get install -y curl && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && apt-get install -y nodejs && rm -rf /var/lib/apt/lists/*
COPY ["src/Fanfoot.Web/Fanfoot.Web.csproj", "src/Fanfoot.Web/"]
RUN dotnet restore "src/Fanfoot.Web/Fanfoot.Web.csproj"
COPY . .
RUN npm ci --prefix "src/Fanfoot.Web/ClientApp" && npm run build --prefix "src/Fanfoot.Web/ClientApp"
RUN dotnet publish "src/Fanfoot.Web/Fanfoot.Web.csproj" -c Release -o /app/publish -p:OpenApiGenerateDocumentsOnBuild=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS="http://+:8080"
EXPOSE 8080
ENTRYPOINT ["dotnet", "Fanfoot.Web.dll"]
