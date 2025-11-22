# Рантайм .NET 8 (ASP.NET)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish/ ./
EXPOSE 33300
ENTRYPOINT ["sh", "-lc", "exec dotnet /app/*.dll"]
