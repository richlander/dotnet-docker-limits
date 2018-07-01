FROM microsoft/dotnet:2.1-sdk AS publish
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o out

FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY --from=publish /app/out .
ENTRYPOINT [ "dotnet","hiver.dll" ]