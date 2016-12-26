﻿MemoryBus [![Build status](https://ci.appveyor.com/api/projects/status/me2ruo9ph65s1nl3?svg=true)](https://ci.appveyor.com/project/Vasiliauskas/memorybus) 
--------------

[![Join the chat at https://gitter.im/Vasiliauskas/MemoryBus](https://badges.gitter.im/Vasiliauskas/MemoryBus.svg)](https://gitter.im/Vasiliauskas/MemoryBus?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

_This is a library for in-memory messaging with pub/sub and RPC patterns_

To use MemoryBus library, create an instance of MemoryBus. It also comes with IBus interface.

Usage:
```c#
IBus bus = new MemoryBus(new DefaultConfig());
bus.Subscribe<string>(Console.WriteLine);
bus.Publish("Hello World"); // Hello World

bus.Respond<string, string>(s=> s + " World");
var result = bus.Request<string, string>("Hello");
Console.WriteLine(result); // Hello World
```

* Bus supports multicasting for Publish<T> and PublishAsync<T>
* Exception will be thrown if Reqest<T, U> or RequestAsync<T, U> after filters applied still has more or less than one responder
* Each subscription or responder registration returns IDisposable handle, if called Dispose(), it will remove original subscriber or responder from internal bus registry
