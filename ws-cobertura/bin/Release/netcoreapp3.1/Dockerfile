FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS runtime
WORKDIR /app
COPY ./ ./
ENTRYPOINT ["dotnet", "ws-cobertura.dll"]