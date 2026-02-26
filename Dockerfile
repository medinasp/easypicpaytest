# Estágio 1: Build com SDK 10.0 preview
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura dependências
COPY ["src/EasyPicPay.csproj", "src/"]
RUN dotnet restore "src/EasyPicPay.csproj"

# Copia todo o código fonte
COPY src/ src/
WORKDIR "/src/src"

# Publica a aplicação - importante: usar a mesma versão
RUN dotnet publish "EasyPicPay.csproj" -c Release -o /app/publish

# Estágio 2: Runtime com ASP.NET 10.0 preview
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copia os arquivos publicados
COPY --from=build /app/publish .

# Garante que a cultura não cause problemas
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Define o entrypoint
ENTRYPOINT ["dotnet", "EasyPicPay.dll"]