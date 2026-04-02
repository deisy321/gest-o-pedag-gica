# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o projeto e restaura dependências
COPY ["gestaopedagogica.csproj", "./"]
RUN dotnet restore "gestaopedagogica.csproj"

# Copia o resto dos ficheiros e compila
COPY . .
RUN dotnet publish "gestaopedagogica.csproj" -c Release -o /app

# Estágio Final de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "gestaopedagogica.dll"]