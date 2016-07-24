# Readme

Start containerized NATS server:

``` docker run -d -p 4222:4222 --name nats-main nats ```

## Note
Currently only working with more than 100 messages. 
Unsubscribing throws a NullReferenceException.

