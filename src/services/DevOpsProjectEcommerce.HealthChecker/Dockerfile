FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build
WORKDIR /dpe-backend
COPY . .
RUN dotnet publish /dpe-backend/src/services/DevOpsProjectEcommerce.HealthChecker/DevOpsProjectEcommerce.HealthChecker.csproj \
    -c Release \
    -o publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /dpe-backend
COPY --from=build /dpe-backend/publish .
EXPOSE 443

RUN adduser dotnet --disabled-password
RUN apk add curl 

USER dotnet
HEALTHCHECK CMD curl -k --fail https://localhost:443/healthchecks/health
ENTRYPOINT ["dotnet", "/dpe-backend/DevOpsProjectEcommerce.HealthChecker.dll"]

