FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PatientApiClient/PatientApiClient.csproj", "PatientApiClient/"]
RUN dotnet restore "PatientApiClient/PatientApiClient.csproj"
COPY . .
WORKDIR "/src/PatientApiClient"
RUN dotnet publish "PatientApiClient.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PatientApiClient.dll"]