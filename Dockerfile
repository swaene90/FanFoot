FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Fanfoot.Web/Fanfoot.Web.csproj", "src/Fanfoot.Web/"]
RUN dotnet restore "src/Fanfoot.Web/Fanfoot.Web.csproj"
COPY . .
RUN dotnet publish "src/Fanfoot.Web/Fanfoot.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS="http://+:8080"
EXPOSE 8080
ENTRYPOINT ["dotnet", "Fanfoot.Web.dll"]
