# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY oBiletCase.Domain/oBiletCase.Domain.csproj oBiletCase.Domain/
COPY oBiletCase.Application/oBiletCase.Application.csproj oBiletCase.Application/
COPY oBiletCase.Infrastructure/oBiletCase.Infrastructure.csproj oBiletCase.Infrastructure/
COPY oBiletCase.Web/oBiletCase.Web.csproj oBiletCase.Web/
RUN dotnet restore oBiletCase.Web/oBiletCase.Web.csproj

COPY oBiletCase.Domain/ oBiletCase.Domain/
COPY oBiletCase.Application/ oBiletCase.Application/
COPY oBiletCase.Infrastructure/ oBiletCase.Infrastructure/
COPY oBiletCase.Web/ oBiletCase.Web/
RUN dotnet publish oBiletCase.Web/oBiletCase.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "oBiletCase.Web.dll"]
