MemoryBus [![Build status](https://ci.appveyor.com/api/projects/status/me2ruo9ph65s1nl3?svg=true)](https://ci.appveyor.com/project/Vasiliauskas/memorybus) 
--------------

_This is a library for in-memory messaging with pub/sub and RPC patterns_

To use MemoryBus library, create an instance of MemoryBus. It also comes with IBus interface.

Usage:
```c#
IBus bus = new MemoryBus(new DefaultConfig());
bus.Subscribe<string>(Console.WriteLine);
bus.Publish("Hello World");
```

* Bus supports multicasting for Publish<T> and PublishAsync<T>
* Exception will be thrown if Reqest<T, U> or RequestAsync<T, U> after filters applied still has more or less than one responder
* Async pub/sub and async RPC should be used from both ends. PublishAsync<T> will not trigger Subscribe<T> members, but only SubscribeAsync<T>
