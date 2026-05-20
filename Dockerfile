# 1. استخدمي صورة SDK الخاصة بـ .NET 6 للبناء
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# 2. استخدمي صورة ASP.NET الخاصة بـ .NET 6 للتشغيل
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "SmartFood.dll"]