#!/bin/sh

# user secrets automatically available for development env
dotnet user-secrets set "TELEGRAM_BOT_TOKEN" "12345:abcde"
dotnet user-secrets set "ConnectionStrings:AreYouGoingDb" "Data Source=$HOME/Projects/AreYouGoingBot/AreYouGoing.sqlite"