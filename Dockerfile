# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /app
COPY src/EasyPicPay/EasyPicPay.csproj ./EasyPicPay/
RUN dotnet restore ./EasyPicPay/EasyPicPay.csproj
COPY src/EasyPicPay/ ./EasyPicPay/
WORKDIR /app/EasyPicPay
RUN dotnet publish -c Release -o /app/out

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

RUN apt-get update && \
    apt-get install -y libgssapi-krb5-2 && \
    rm -rf /var/lib/apt/lists/*

EXPOSE 80
EXPOSE 443
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "EasyPicPay.dll"]