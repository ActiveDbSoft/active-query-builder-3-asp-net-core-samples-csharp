FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ./QueryBuilderApi.csproj ./
RUN dotnet restore /p:Configuration="Docker" QueryBuilderApi.csproj

COPY . ./

FROM build AS publish
RUN dotnet publish QueryBuilderApi.csproj -c Docker -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QueryBuilderApi.dll"]
