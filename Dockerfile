FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev

WORKDIR /app

# Variáveis para desenvolvimento e hot reload
ENV ASPNETCORE_URLS=http://0.0.0.0:5000 \
    ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_USE_POLLING_FILE_WATCHER=1

EXPOSE 5000

# Executa com watch para hot reload contínuo
CMD ["dotnet", "watch", "run", "--non-interactive", "--project", "src/ERP.Web/ERP.Web.csproj", "--urls", "http://0.0.0.0:5000"]
