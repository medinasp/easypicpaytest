# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /app

# Copia o arquivo de projeto e restaura
COPY src/EasyPicPay.csproj ./
RUN dotnet restore

# Copia todo o código fonte
COPY src/ ./

# Publica a aplicação
RUN dotnet publish -c Release -o out

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "EasyPicPay.dll"]
