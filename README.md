MemoryBus [![Build status](https://ci.appveyor.com/api/projects/status/me2ruo9ph65s1nl3?svg=true)](https://ci.appveyor.com/project/Vasiliauskas/memorybus) 
--------------

_This is a library for in-memory messaging with pub/sub and RPC patterns_

To use MemoryBus library, create an instance of MemoryBus. It also comes with IBus interface.

Usage:
```c#
IBus bus = new MemoryBus(new DefaultConfig());
bus.Subscribe<string>(s => Console.WriteLine(s));
bus.Publish("Hello World");
```
