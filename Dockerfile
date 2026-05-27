FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Fantfoot.Web/Fantfoot.Web.csproj", "src/Fantfoot.Web/"]
RUN dotnet restore "src/Fantfoot.Web/Fantfoot.Web.csproj"
COPY . .
RUN dotnet publish "src/Fantfoot.Web/Fantfoot.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS="http://+:8080"
EXPOSE 8080
ENTRYPOINT ["dotnet", "Fantfoot.Web.dll"]
