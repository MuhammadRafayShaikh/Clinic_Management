# Step 1: Base image with .NET 8 runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Step 2: Build image with SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the entire project
COPY . .

# Restore dependencies and publish the project
RUN dotnet restore "Clinic_Management.csproj"
RUN dotnet publish "Clinic_Management.csproj" -c Release -o /app/publish

# Step 3: Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Run the app
ENTRYPOINT ["dotnet", "Clinic_Management.dll"]
