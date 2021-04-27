# Summary

This self-hosted service allows you to manage your long-term investments in financial instruments and see statistics. It can be especially useful in case of different brokers usage.  

# Features

## Portfolio

You can see your overall portfolio with last-day-close prices, filter it by broker and history period:
![portfolio](Docs/Screenshots/portfolio.png)
![portfolio_table](Docs/Screenshots/portfolio_table.png)

# Classification

You can set up multiple tags for each asset in your portfolio:
![tags](Docs/Screenshots/tags.png)

Tags is used in dashboards, for example to handle country and industry diversification:
![dashboard_1](Docs/Screenshots/dashboard_1.png)
![dashboard_1_view](Docs/Screenshots/dashboard_1_view.png)
![dashboard_2](Docs/Screenshots/dashboard_2.png)
![dashboard_2_view](Docs/Screenshots/dashboard_2_view.png)

You can set target for each tag in dashboard and see difference between real share of that tag and you target.
It's useful to understand which category is under or overloaded.

# Operation inspection

You can review all operations and filter them by different criteria: 
![operations](Docs/Screenshots/operations.png)

For example, you can filter operations related to specific asset:
![operations_asset](Docs/Screenshots/operations_asset.png)

# Import

You can import your broker reports to the service:
![import](Docs/Screenshots/import.png)

Import supported for different brokers:

| Broker      | Transfers | Share | ETF | Bond | Dividends | Coupons |
|-------------|:---------:|:-----:|:---:|:----:|:---------:|:-------:|
| AlphaDirect | +         | +     | +   | +    | +         | +       |
| Tinkoff     | +         | +     | +   | +    | -         | -       |

# Custom

You can manually insert your operations:
![custom](Docs/Screenshots/custom.png)

## Account/broker management

You can set up your brokers and accounts, assign currencies for each account: 
![accounts](Docs/Screenshots/accounts.png)

# Warranty notice

The service is in initial prototype stage and should not be used in production.
No guaranties provided, especially for complex cases, which not fully supported.
Statistics data may be inexact and should not be used as financial advice without additional investigations.
The service is not directed to short-term operations, data is collected with noticeable delay, which fine for long-term period.

# Installation guide

## Hardware

Supports both x86_64 and ARM processors

## Prerequisites

- Docker - https://docs.docker.com/get-docker/
- Docker Compose - https://docs.docker.com/compose/install/
- .NET 5 - https://dotnet.microsoft.com/download/dotnet/5.0
- Nuke - https://nuke.build/

## Recommendations

HTTPS is not provided by this service, but it's strongly recommended for security reasons. Please set up some HTTPS reverse proxy (like Nginx) to redirect requests to the service.

## Getting started

- Clone repository - `git clone https://github.com/KonH/InvestmentReporting.git`
- Create Production.env file:
```
MONGO_INITDB_ROOT_USERNAME=%MONGO_USERNAME%
MONGO_INITDB_ROOT_PASSWORD=%MONGO_PASSWORD%
TINKOFF_API_SANDBOX_KEY=%TINKOFF_API_KEY%
```
- Mongo credentials related to embedded mongo database, which will be created at start
- Tinkoff API key described here - https://tinkoffcreditsystems.github.io/invest-openapi/auth/ 
- Start services: `nuke --target Start --configuration Production`
- Navigate to http://localhost:8082

## Register

- Registration is based on token, which available in service logs 
- Lookup token in logs: `docker-compose logs invite-service` 
- Use that token in registration process
- Register is locked if you make invalid attempt, just restart invite-service to reset access timer