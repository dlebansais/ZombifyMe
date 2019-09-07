# Zombify Me
[![Build Status](https://travis-ci.com/dlebansais/ZombifyMe.svg?branch=master)](https://travis-ci.com/dlebansais/ZombifyMe)

A library that can restart the program its linked with if it crashes unexpectedly.

To use it, just include ZombifyMe.dll as a reference in your project and add the following code.

## Process startup

At startup, after initialization completed successfully, enable Zombify me.

```csharp
Zombification Zombification = new Zombification("My unique process name");
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
Note that this value is set only if the NoWindow flag is set (see below).

## Parameters

+ The name provided in the constructor just needs to be unique vs other users of ZombifyMe. A simple string such as your product name or a guid is enough. Avoid special characters that have a meaning or are forbidden in file and directory names.
+ Change the `Delay` property to add a delay before the process is restarted.

```csharp
Zombification.Delay = TimeSpan.FromMinutes(1);
```

+ You can customize some messages:
  * The message when watching over the process begins is set by the `WatchingMessage` property. The default message is null (no message).
  * The message when a process has been restarted is set in the `RestartMessage` property.
+ How the process is restarted can be controlled with some flags:
  * `NoWindow`: the new process is created with no window.
  * `ForwardArguments` (ON by default): arguments of the original process are reused when starting the new process.

# Certification

This library is digitally signed with a [CAcert](https://www.cacert.org/) certificate.
