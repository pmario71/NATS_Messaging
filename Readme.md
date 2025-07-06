# Readme

* Started off with test project `Redis.Tests`
* it includes tests using `Redis` and `Nats`
* `Nats` tests are not working, because `SubscribeAsync` is blocking and no messages are received **(!!!)**

* `WebApp` added in addition and validates that sending and receiving in the same process is working
