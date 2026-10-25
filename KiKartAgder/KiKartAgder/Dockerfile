FROM mcr.microsoft.com/dotnet/aspnet:10.0-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 5270

ENV ASPNETCORE_URLS=http://+:5270

FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2022 AS build
ARG configuration=Release
WORKDIR /src
COPY ["KiKartAgder/KiKartAgder.csproj", "KiKartAgder/"]
RUN dotnet restore "KiKartAgder\KiKartAgder.csproj"
COPY . .
WORKDIR "/src/KiKartAgder"
RUN dotnet build "KiKartAgder.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "KiKartAgder.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KiKartAgder.dll"]
