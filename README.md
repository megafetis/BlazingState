# BlazingState
This library provides powerful, robust, fast and yet simple state management with almost 0 boilerplate code. 

You can use this library for any project, not just with Blazor.

## Setup
Download the latest releases from NuGet.

| NuGet Package                                                                       | Version | Info                                   |
|-------------------------------------------------------------------------------------|---------|----------------------------------------|
| [BlazingState](https://www.nuget.org/packages/BlazingState)                         | 7.1.0   |                                        |
| [BlazingState.WebAssembly](https://www.nuget.org/packages/BlazingState.WebAssembly) | 7.1.0   | Needed for AutoState (more info below) |


### AutoState
AutoState is very simple to use and allows you to write no boilerplate code for state changes! It automatically notifies your components to rerender if the state has changed. \
**Be advised that AutoState uses internal framework classes under the hood to provide this functionality which could change with every Blazor update and break AutoState. Use it at your own risk.**

You don't need any other code inside your components for updating the state than this (more info in the next sections).
```csharp
@attribute [AutoState]
@inject IStateObserver<MyData> MyData;
@code {
    // ..
}
```


## Usage
### Registering state observers
Start by registering your tracked type to your services.

**Note:** You can only register **1 state observer per type**.
```csharp
// Sample class for the examples
public class MyData
{
    public string SomeString { get; set; }
}

// Register BlazingState
builder.Services.AddBlazingState()
    // Registers a state observer of this type with no initial value
    .AddStateObserver<MyData?>();
    // You can also initialize MyData with an initial value
    .AddStateObserver<MyData>(new MyData());
    // And even with an implementation factory
    .AddStateObserver<MyData>(sp =>
    {
        var service = sp.GetRequiredService<SomeOtherService>();
        return new MyData { SomeString = service.SomeText };
    });
```

For AutoState (WebAssembly only):
```csharp
// Register BlazingState
builder.Services.AddBlazingState()
    .AddAutoState() // Add this before adding any state observer
    .AddStateObserver<MyData>();
```

If you don't use DI or another DI framework you can also manually instantiate your state observers:
```csharp
var myStateObserver = new StateObserver<MyData>();

// And the same with an optional initial value
var myStateObserver = new StateObserver<MyData>(initialValue);
```

### Subscribing/Unsubscribing from state changes
Add the following property to your razor component (or if using a normal class, just use normal di):
```csharp
[Inject]
protected IStateObserver<MyData> MyData { get; set; } = null!;
// or
@inject IStateObserver<MyData> MyData
```

Subscribe using the ``OnParametersSet`` lifecycle event in Blazor (or use the ctor for normal classes):
```csharp
protected override void OnParametersSet()
{
    // Now if another component changed the value property of MyData, this callback gets executed
    MyData.Register(this, () =>
    {
        // Rerender the UI
        StateHasChanged();
    });

    // You can also use async callbacks (actually faster since the sync version gets wrapped)
    // Keep in mind that you can only register 1 event per instance, so the previous callback gets overriden by the async one
    MyData.Register(this, async () =>
    {
        await SomeMethodAsync();
    });
}
```

For AutoState (WebAssembly only): \
You only need to add the ``AutoStateAttribute`` and the state observer to the class. That's it!
```csharp
@attribute [AutoState]
@inject IStateObserver<MyData> MyData;
@code {
    // ..
}
```

You don't have to unsubscribe from the event, as soon as the GC collects the instance, the event is removed. \
If you really need the memory you should implement ``IDisposable`` and remove the event by yourself in the dispose method to cleanup resources immediately but that's not needed. An alternative would be to call ``GC.Collect()``, as that forces the GC to perform a collection which removes all unused events but I wouldn't recommend this.
```csharp
public void Dispose()
{
    MyData.Unregister(this);
}
```

### Reading/displaying the value
```xml
<p>Current value of MyData: @MyData.Value.SomeString</p>
```

### Updating the value of state observers
To update the current value of the observer you can just set the ``Value`` property of your observer.

```csharp
// Assuming you have a method which gets called when you want to perform an update (e.g. clicking on a button):
public void UpdateValue(string newValue)
{
    MyData.Value = new MyData { SomeString = newValue };
    // Faster way (skips notifying this instance again)
    MyData.SetValueAsync(new MyData { SomeString = newValue }, this);
}
```
That's all! All subscribers are recieving your update automatically.

You can also just set some properties of the value instance and then manually notify all subscribers:
```csharp
public async Task UpdateValue(string newValue)
{
    MyData.Value.SomeString = newValue;
    await MyData.NotifyStateChangedAsync(this); // Passing "this" skips notifying this instance again
}
```
This way you don't have to reallocate the object every time which can build up a lot of pressure on GC with bigger objects.

As mentioned at the start you can use an implementation factory for initialization. However sometimes you need to retrieve data from a data source (e.g. some web api) to initialize your value. \
For that use case you could create your own component like ``InitMyData`` with following code:
```csharp
@code {
    [Inject]
    protected IStateObserver<MyData> MyData { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        // Getting data from server
        var name = await GetName();
        MyData.Value = new MyData { SomeString = name };
    }

    private async Task<string> GetName()
    {
        await Task.Delay(2000);
        return "Name from Server";
    }
}
```

And add this component at the top of your used layout.
```xml
<InitMyData />
```

Obviously you can do the same with non Blazor projects by getting the your observer instance, retrieving the needed data and setting the ``Value`` property of the state observer.


## Sample projects
More samples can be found in the [Samples directory](/Samples).


## License
BlazingState is licensed under Apache License 2.0, see [LICENSE.txt](/LICENSE.txt) for more information.