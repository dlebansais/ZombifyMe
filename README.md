# Zombify Me
[![Build Status](https://travis-ci.com/dlebansais/ZombifyMe.svg?branch=master)](https://travis-ci.com/dlebansais/ZombifyMe)

ZombifyMe can restart a process that has unexpectedly crashed. It's an assembly that any programmer can link with and it used as follow:

## Process startup

At startup, enable Zombify me.

```csharp
Zombification Zombification = new Zombification("My unique process name");
Zombification.ZombifyMe();
```

At this point, if the process crashes, it will be automatiquely restarted.

## Process exit

Before the process exits normally, whether it's reporting success or an error, cancel the watcher:

```csharp
Zombification.Cancel();
```

## Parameters

+ The name provided in the constructor just needs to be unique vs other users of ZombifyMe. A simple string such as your product name or a guid is enough. Avoid special characters that have a meaning or are forbidden in file and directory names.

# Certification

This library is digitally signed with a [CAcert](https://www.cacert.org/) certificate.
