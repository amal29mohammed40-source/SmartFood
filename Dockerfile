# استخدام صورة SDK للبناء
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# نسخ ملف المشروع واستعادة الحزم
COPY *.csproj ./
RUN dotnet restore

# نسخ باقي الملفات والبناء
COPY . .
RUN dotnet publish -c Release -o out

# استخدام صورة ASP.NET للتشغيل
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "SmartFood.dll"]