#SDK image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Download packages needed for image
COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

# Download runtime image
# Make image smaller using multipart build, no sdk as part of the final image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 
WORKDIR /app
COPY --from=build-env /app/out .
# Command to execute
ENTRYPOINT [ "dotnet", "PlatformService.dll" ]