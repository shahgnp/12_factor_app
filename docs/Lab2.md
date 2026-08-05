# Explore the modern application
*This application follows the 12 factors*

# Run the application (single instance)

*Open a Terminal in VScode*

```bash
ASPNETCORE_URLS=http://0.0.0.0:8080 dotnet run
```

# Run the application (Multiple Instances)
*Open a new terminal*

```bash
ASPNETCORE_URLS=http://0.0.0.0:8081 dotnet run
```

*Above only runs in debug mode*

# Running application by publishing

Step 1: Publish
```bash
dotnet publish -c Release -o ./publish
```

Step 2: Run
```
ASPNETCORE_URLS=http://0.0.0.0:8080 dotnet ElectricityTradingPortal.dll
```

# Update the config via ENV

```bash
export TradingLimits__MaxOrderValue=1000000
export MarketDataFeed__Url=https://market.example.com/api
export MarketDataFeed__PollIntervalSeconds=30

```
