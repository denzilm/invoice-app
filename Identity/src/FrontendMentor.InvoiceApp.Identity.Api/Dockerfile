FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "Directory.Packages.props"]
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Aspire/FrontendMentor.InvoiceApp.AspireUtilities/FrontendMentor.InvoiceApp.AspireUtilities.csproj", "Aspire/FrontendMentor.InvoiceApp.AspireUtilities/"]
COPY ["Aspire/FrontendMentor.InvoiceApp.ServiceDefaults/FrontendMentor.InvoiceApp.ServiceDefaults.csproj", "Aspire/FrontendMentor.InvoiceApp.ServiceDefaults/"]
COPY ["Identity/src/FrontendMentor.InvoiceApp.Identity.Infrastructure/FrontendMentor.InvoiceApp.Identity.Infrastructure.csproj", "Identity/src/FrontendMentor.InvoiceApp.Identity.Infrastructure/"]
COPY ["Identity/src/FrontendMentor.InvoiceApp.Identity.Api/FrontendMentor.InvoiceApp.Identity.Api.csproj", "Identity/src/FrontendMentor.InvoiceApp.Identity.Api/"]
RUN dotnet restore "Identity/src/FrontendMentor.InvoiceApp.Identity.Api/FrontendMentor.InvoiceApp.Identity.Api.csproj"
COPY . .
WORKDIR "/src/Identity/src/FrontendMentor.InvoiceApp.Identity.Api"
RUN dotnet build "./FrontendMentor.InvoiceApp.Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FrontendMentor.InvoiceApp.Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FrontendMentor.InvoiceApp.Identity.Api.dll"]
