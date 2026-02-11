FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PatientApi/PatientApi.csproj", "PatientApi/"]
RUN dotnet restore "PatientApi/PatientApi.csproj"
COPY . .
WORKDIR "/src/PatientApi"
RUN dotnet build "PatientApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PatientApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "PatientApi.dll"]