# Zombify Me
[![Build Status](https://travis-ci.com/dlebansais/ZombifyMe.svg?branch=master)](https://travis-ci.com/dlebansais/ZombifyMe)
[![CodeFactor](https://www.codefactor.io/repository/github/dlebansais/zombifyme/badge)](https://www.codefactor.io/repository/github/dlebansais/zombifyme)
[![codecov](https://codecov.io/gh/dlebansais/ZombifyMe/branch/master/graph/badge.svg)](https://codecov.io/gh/dlebansais/ZombifyMe)

A library that can restart the program it's linked with if it crashes unexpectedly.

To use it, just include ZombifyMe.dll as a reference in your project and add the following code.

## Process startup

At startup, after initialization completed successfully, enable ZombifyMe.

```csharp
Zombification Zombification = new Zombification("My unique name");
Zombification.ZombifyMe();
```

At this point, if the process crashes, it will be automatiquely restarted.

## Process exit

When you know the process is going to exit normally, whether it's reporting success or an error, cancel the watcher:

```csharp
Zombification.Cancel();
```

## Restarted process

A program can know if it has been started normally, or restarted, by reading the static property `Zombification.IsRestart`.
Note that this value is set only if the `NoWindow` flag was set (see below).

## Parameters

+ The name provided in the constructor just needs to be unique vs other users of ZombifyMe. A simple string such as your product name or a guid is enough. Avoid special characters that have a meaning or are forbidden in file and directory names.
+ Change the `Delay` property to add a delay before the process is restarted (accuracy is about 10 seconds).

```csharp
Zombification.Delay = TimeSpan.FromMinutes(1);
```

+ You can customize some messages:
  * The message when watching over the process begins is set by the `WatchingMessage` property. The default message is null (no message).
  * The message when a process has been restarted is set in the `RestartMessage` property.
+ How the process is restarted can be controlled with some flags:
  * `NoWindow`: the new process is created with no window.
  * `ForwardArguments` (ON by default): arguments of the original process are reused when starting the new process.
+ You can enable symmetric monitoring setting `IsSymmetric` to `true`. When enabled, the original process and the process that monitors it watch over each other: if one is killed or crashes the other restarts it. When this is enabled it becomes quite difficult to manually kill any of these processes, so setting a large delay such as one minute is recommended.
+ In case `IsSymmetric` is set, symmetric monitoring could prevent the crash of the original process because there is still a thread alive: the monitoring thread. To prevent this unexpected side effect, it is recommended to set `AliveTimeout` to a non-zero value and call `SetAlive` regularly. If `SetAlive` is not called, the monitoring thread will exit, allowing the process to exit prematurely so it can be restarted. Calling `SetAlive` while `IsSymmetric` is false or `AliveTimeout` is zero has no effect.

The following example sets all parameters to their default value.
```csharp
Zombification.Delay = TimeSpan.Zero;
Zombification.WatchingMessage = null;
Zombification.RestartMessage = "ZombifyMe Alert:\nA protected process has been restarted";
Zombification.Flags = Flags.ForwardArguments;
Zombification.IsSymmetric = false;
Zombification.AliveTimeout = TimeSpan.Zero;
```

# Certification

This library is digitally signed with a [CAcert](https://www.cacert.org/) certificate.
