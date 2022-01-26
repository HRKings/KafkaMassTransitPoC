# Kafka-RabbitMQ MassTransit Proof of Concept

When working with large distributed systems, is evident the need to scale. RPC is a indispensible tool when communicating between services.

This project is a PoC written in C# using .NET6, that aims to make a simple service that can consume both events and RPC requests.

## Kafka

Kafka is a distributed event bus that can be used in various cases, audit and logging for example. In this project it will process any event coming through its topic, an event being any message that don't require a response

## RabbitMQ

RabbitMQ is a message broker that is largely used for RPC (Remote Procedure Call), in this project is used to process and respond to any message marked as a request.
