FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore ./OcrServer/OcrServer.csproj
RUN dotnet publish ./OcrServer/OcrServer.csproj -c Release -o /out -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

RUN apt-get update && apt-get install -y \
    libtesseract5 \
    libleptonica-dev \
    libpng16-16 \
    libgdiplus \
    libglib2.0-0 \
    libicu-dev \
    libjpeg62-turbo \
    wget \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /out .

ENV ASPNETCORE_URLS="http://0.0.0.0:8080"
EXPOSE 8080

ENTRYPOINT ["dotnet", "OcrServer.dll"]
