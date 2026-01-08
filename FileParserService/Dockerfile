FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Shared/Shared.csproj"

COPY ["FileParserService/FileParserService.csproj", "FileParserService/"]
RUN dotnet restore "FileParserService/FileParserService.csproj"

COPY Shared/ Shared/
COPY FileParserService/ FileParserService/

WORKDIR "/src/FileParserService"
RUN dotnet build "FileParserService.csproj" -c Release -o /app/build
RUN dotnet publish "FileParserService.csproj" -c Release -o /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY FileParserService/appsettings*.json ./
COPY XMLFiles/ ./XMLFiles/

ENTRYPOINT ["dotnet", "FileParserService.dll"]