MemoryBus [![Build status](https://ci.appveyor.com/api/projects/status/06nsa81vvf7c9ymh?svg=true)](https://ci.appveyor.com/project/Vasiliauskas/MemoryBus) 
--------------


_This is a library for in memory messaging with pub/sub and RPC patterns_


To use MemoryBus library, create an instance of MemoryBus. It also comes with IBus interface.

There are two modes of tracking
- Non persistent - meaning that each time you create a new session it will be treated as a new user as well
- Persistent - if your application can persist three specific tracking values then you can ensure user uniqueness
	- "First session timestamp"
	- "Current visit count (previous visit count + 1)"
	- "Random number created on first visit"

Usage:
```c#
IBus bus = new MemoryBus();

var session = new AnalyticsSession("DOMAIN.COM", "UA-XXXXXXXX-X", rndNumber, visitCount, firstVisitTimestamp);* // persistent

var page = session.CreatePageViewRequest("/Root/MyPage", "MyPage");
page.Send();
```
