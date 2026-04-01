# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o projeto e restaura
COPY ["gestaopedagogica.csproj", "./"]
RUN dotnet restore "gestaopedagogica.csproj"

# Copia tudo e publica
COPY . .
RUN dotnet publish "gestaopedagogica.csproj" -c Release -o /app

# Estágio de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "gestaopedagogica.dll"]