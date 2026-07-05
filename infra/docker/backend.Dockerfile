# syntax=docker/dockerfile:1.7

# Multi-stage build for the ClauseLens API.
# - Build stage: restores + publishes
# - Runtime stage: non-root, distroless-friendly base, signed image produced by CI

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/global.json ./
COPY backend/ClauseLens.sln ./
COPY backend/src/ClauseLens.Domain/ ./src/ClauseLens.Domain/
COPY backend/src/ClauseLens.Application/ ./src/ClauseLens.Application/
COPY backend/src/ClauseLens.Infrastructure/ ./src/ClauseLens.Infrastructure/
COPY backend/src/ClauseLens.Api/ ./src/ClauseLens.Api/
RUN dotnet restore ClauseLens.sln
RUN dotnet publish src/ClauseLens.Api/ClauseLens.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "ClauseLens.Api.dll"]
