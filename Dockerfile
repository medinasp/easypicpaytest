# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /app

# Copia o arquivo de projeto do local correto
COPY src/EasyPicPay/EasyPicPay.csproj ./EasyPicPay/
RUN dotnet restore ./EasyPicPay/EasyPicPay.csproj

# Copia todo o código fonte
COPY src/EasyPicPay/ ./EasyPicPay/

# Publica a aplicação
WORKDIR /app/EasyPicPay
RUN dotnet publish -c Release -o /app/out

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "EasyPicPay.dll"]
