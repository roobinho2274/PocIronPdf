FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore ./OcrServer/OcrServer.csproj
RUN dotnet publish ./OcrServer/OcrServer.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS="http://0.0.0.0:8080"
EXPOSE 8080

ENTRYPOINT ["dotnet", "OcrServer.dll"]
