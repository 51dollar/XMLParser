FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Shared/Shared.csproj"

COPY ["DataProcessorService/DataProcessorService.csproj", "DataProcessorService/"]
RUN dotnet restore "DataProcessorService/DataProcessorService.csproj"

COPY Shared/ Shared/
COPY DataProcessorService/ DataProcessorService/

WORKDIR "/src/DataProcessorService"
RUN dotnet build "DataProcessorService.csproj" -c Release -o /app/build
RUN dotnet publish "DataProcessorService.csproj" -c Release -o /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY DataProcessorService/appsettings*.json ./

ENTRYPOINT ["dotnet", "DataProcessorService.dll"]