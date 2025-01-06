# Sử dụng image chính thức của .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Vô hiệu hóa cache
ARG CACHE_BUSTER=1

# Sao chép file .csproj và restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Sao chép toàn bộ source code
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "MACS.dll"]
