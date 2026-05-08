# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy file dự án và restore (sửa lại đường dẫn nếu tên file .csproj của bạn khác)
COPY ["BookingAdmin.Web/BookingAdmin.Web.csproj", "BookingAdmin.Web/"]
RUN dotnet restore "BookingAdmin.Web/BookingAdmin.Web.csproj"

# Copy toàn bộ code và build
COPY . .
WORKDIR "/app/BookingAdmin.Web"
RUN dotnet publish -c Release -o /out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .

# Render dùng port 10000 mặc định cho web service
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "BookingAdmin.Web.dll"]
