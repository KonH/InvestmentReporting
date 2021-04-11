ARG DOTNET_IMAGE_SUFFIX
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0.3-alpine3.13-$DOTNET_IMAGE_SUFFIX

EXPOSE 8085

ADD publish /publish

ENV ASPNETCORE_URLS=http://+:8085
ENV ASPNETCORE_ENVIRONMENT=Development

WORKDIR /publish
ENTRYPOINT ["./InvestmentReporting.MarketService"]
