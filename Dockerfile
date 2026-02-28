FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS builder
WORKDIR /src

COPY ["src/EasyPicPay.csproj", "src/"]
RUN dotnet restore "src/EasyPicPay.csproj"

COPY src/ src/
WORKDIR "/src/src"
RUN dotnet publish "EasyPicPay.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

RUN apt-get update && \
    apt-get install -y libgssapi-krb5-2 && \
    rm -rf /var/lib/apt/lists/*

EXPOSE 80
EXPOSE 443

COPY --from=builder /app/publish .

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

ENTRYPOINT ["dotnet", "EasyPicPay.dll"]