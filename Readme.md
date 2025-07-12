# Readme

* Started off with test project `Redis.Tests`
* it includes tests using `Redis` and `Nats`
* `Nats` tests now working

> **Note**\
> Publishing without a subscription seems to lead to later published message also not being received!

* `WebApp` added in addition and validates that sending and receiving in the same process is working
* Now displaying `stats` and added an endpoint that allows querying stats
