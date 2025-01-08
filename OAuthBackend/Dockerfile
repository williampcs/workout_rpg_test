# --- 第一步：Build 階段，用於建置與打包 .NET 8 專案 ---
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

    WORKDIR /src
    COPY *.csproj ./
    RUN dotnet restore
    COPY . .
    RUN dotnet publish -c Release -o /app
    
    # --- 第二步：Runtime 階段 (Debian/Ubuntu-based) ---
    FROM mcr.microsoft.com/dotnet/aspnet:8.0
    
    WORKDIR /app
    COPY --from=build /app .
    
    # 安裝 CA Certificates、ntpdate 等 (Debian/Ubuntu)
    RUN apt-get update && \
        apt-get install -y --no-install-recommends ca-certificates ntpdate && \
        update-ca-certificates && \
        ntpdate -s time.nist.gov && \
        rm -rf /var/lib/apt/lists/*
    
    EXPOSE 80
    ENV ASPNETCORE_URLS=http://+:80
    ENTRYPOINT ["dotnet", "OAuthBackend.dll"]