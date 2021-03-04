ARG DOTNET_IMAGE_SUFFIX
FROM mcr.microsoft.com/dotnet/aspnet:5.0

EXPOSE 8083

ADD publish publish

ENV ASPNETCORE_URLS=http://+:8083

WORKDIR ./publish
ENTRYPOINT ["dotnet", "InvestmentReporting.TestService.dll"]
