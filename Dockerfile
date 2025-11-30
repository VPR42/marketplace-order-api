FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY ./publish/ ./
EXPOSE 33308
ENTRYPOINT ["dotnet", "MarketPlace.dll"]
