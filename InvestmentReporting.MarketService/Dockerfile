ARG DOTNET_IMAGE_SUFFIX
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0.3-alpine3.13-$DOTNET_IMAGE_SUFFIX

EXPOSE 8087

ADD publish /publish

ENV ASPNETCORE_URLS=http://+:8087
ENV ASPNETCORE_ENVIRONMENT=Development
ENV TINKOFF_API_SANDBOX_KEY=none

WORKDIR /publish
ENTRYPOINT ["./InvestmentReporting.MarketService"]
