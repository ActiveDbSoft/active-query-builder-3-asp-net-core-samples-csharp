FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY CrossDomain.csproj ./
RUN dotnet restore /p:Configuration="Docker"

# Copy everything else and build
COPY . ./
RUN dotnet publish CrossDomain.csproj -c Docker -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "CrossDomain.NETCore.dll"]
