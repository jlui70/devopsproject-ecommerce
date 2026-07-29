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

USER dotnet
ENTRYPOINT ["dotnet", "/dpe-backend/DevOpsProjectEcommerce.HealthChecker.dll"]

