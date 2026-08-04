FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY Ordivo.slnx ./
COPY src/Ordivo.Api/Ordivo.Api.csproj src/Ordivo.Api/
COPY src/Ordivo.Application/Ordivo.Application.csproj src/Ordivo.Application/
COPY src/Ordivo.Domain/Ordivo.Domain.csproj src/Ordivo.Domain/
COPY src/Ordivo.Infrastructure/Ordivo.Infrastructure.csproj src/Ordivo.Infrastructure/
COPY src/Ordivo.SharedKernel/Ordivo.SharedKernel.csproj src/Ordivo.SharedKernel/
RUN dotnet restore src/Ordivo.Api/Ordivo.Api.csproj

FROM restore AS publish
COPY src/ src/
RUN dotnet publish src/Ordivo.Api/Ordivo.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "Ordivo.Api.dll"]
