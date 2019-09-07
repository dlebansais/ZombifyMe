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

## Parameters

+ The name provided in the constructor just needs to be unique vs other users of ZombifyMe. A simple string such as your product name or a guid is enough. Avoid special characters that have a meaning or are forbidden in file and directory names.
+ Change the `Delay` property to add a delay before the process is restarted.

```csharp
Zombification.Delay = TimeSpan.FromMinutes(1);
```

# Certification

This library is digitally signed with a [CAcert](https://www.cacert.org/) certificate.
