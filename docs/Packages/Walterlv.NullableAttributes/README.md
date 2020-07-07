# Walterlv.NullableAttributes

## NuGet 包

- 如果你不在意为你的项目引入一个额外的依赖库，那么可以安装 NuGet 包 Walterlv.NullableAttributes [![Walterlv.NullableAttributes](https://img.shields.io/nuget/v/Walterlv.NullableAttributes)](https://www.nuget.org/packages/Walterlv.NullableAttributes/)。
- 如果你要求你的项目不能引入新的依赖，必须要将相关类型直接嵌入到你的目标程序集中，那么请安装 NuGet 包 Walterlv.NullableAttributes.Source [![Walterlv.NullableAttributes.Source](https://img.shields.io/nuget/v/Walterlv.NullableAttributes.Source)](https://www.nuget.org/packages/Walterlv.NullableAttributes.Source/)。

因为 .NET Core 3.0 开始，已经内置支持了本包所带的所有类型，所以在 .NET Core 3.0 以上的项目安装此包后是不会引入任何依赖，也不会嵌入任何代码的。而 .NET Framework 4.8 及以下，.NET Standard 2.1 及以下，.NET Core 2.1 及以下的项目安装此包时，则会引用所需的类型，或者嵌入所需类型的源码。

你也不用担心跨框架引用项目时，会不会导致类型多余或者缺失。本包的引用版本和源码版本都已经做好了跨版本引用的处理。

## 介绍可空引用类型

C# 8.0 引入了可空引用类型，你可以通过 `?` 为字段、属性、方法参数、返回值等添加是否可为 null 的特性。

但是如果你真的在把你原有的旧项目迁移到可空类型的时候，你就会发现情况远比你想象当中复杂，因为你写的代码可能只在部分情况下可空，部分情况下不可空；或者传入空时才可为空，传入非空时则不可为空。

在开始迁移你的项目之前，你可能需要了解如何开启项目的可空类型支持：

- [C# 8.0 如何在项目中开启可空引用类型的支持 - walterlv](https://blog.walterlv.com/post/how-to-enable-nullable-reference-types)

可空引用类型是 C# 8.0 带来的新特性。

你可能会好奇，C# 语言的可空特性为什么在编译成类库之后，依然可以被引用它的程序集识别。也许你可以理解为有什么特性 `Attribute` 标记了字段、属性、方法参数、返回值的可空特性，于是可空特性就被编译到程序集中了。

确实，可空特性是通过 `NullableAttribute` 和 `NullableContextAttribute` 这两个特性标记的。

但你是否好奇，即使在古老的 .NET Framework 4.5 或者 .NET Standard 2.0 中开发的时候，你也可以编译出支持可空信息的程序集出来。这些古老的框架中没有这些新出来的类型，为什么也可以携带类型的可空特性呢？

实际上反编译一下编译出来的程序集就能立刻看到结果了。

看下图，在早期版本的 .NET 框架中，可空特性实际上是被编译到程序集里面，作为 `internal` 的 `Attribute` 类型了。

![反编译](/static/posts/2019-11-27-16-39-21.png)

所以，放心使用可空类型吧！旧版本的框架也是可以用的。

## 本包的功能：更灵活控制的可空特性

阻碍你将老项目迁移到可空类型的原因，可能还有你原来代码逻辑的问题。因为有些情况下你无法完完全全将类型迁移到可空。

例如：

1. 有些时候你不得不为非空的类型赋值为 `null` 或者获取可空类型时你能确保此时一定不为 `null`（待会儿我会解释到底是什么情况）；
1. 一个方法，可能这种情况下返回的是 `null` 那种情况下返回的是非 `null`；
1. 可能调用者传入 `null` 的时候才返回 `null`，传入非 `null` 的时候返回非 `null`。

为了解决这些情况，C# 8.0 还同时引入了下面这些 `Attribute`：

- `AllowNull`: 标记一个不可空的输入实际上是可以传入 null 的。
- `DisallowNull`: 标记一个可空的输入实际上不应该传入 null。
- `MaybeNull`: 标记一个非空的返回值实际上可能会返回 null，返回值包括输出参数。
- `NotNull`: 标记一个可空的返回值实际上是不可能为 null 的。
- `MaybeNullWhen`: 当返回指定的 true/false 时某个输出参数才可能为 null，而返回相反的值时那个输出参数则不可为 null。
- `NotNullWhen`: 当返回指定的 true/false 时，某个输出参数不可为 null，而返回相反的值时那个输出参数则可能为 null。
- `NotNullIfNotNull`: 指定的参数传入 null 时才可能返回 null，指定的参数传入非 null 时就不可能返回 null。
- `DoesNotReturn`: 指定一个方法是不可能返回的。
- `DoesNotReturnIf`: 在方法的输入参数上指定一个条件，当这个参数传入了指定的 true/false 时方法不可能返回。

想必有了这些描述后，你在具体遇到问题的时候应该能知道选用那个特性。但单单看到这些特性的时候你可能不一定知道什么情况下会用得着，于是我可以为你举一些典型的例子。

### 输入：`AllowNull`

设想一下你需要写一个属性：

```csharp
public string Text
{
    get => GetValue() ?? "";
    set => SetValue(value ?? "");
}
```

当你获取这个属性的值的时候，你一定不会获取到 `null`，因为我们在 `get` 里面指定了非 `null` 的默认值。然而我是允许你设置 `null` 到这个属性的，因为我处理好了 `null` 的情况。

于是，请为这个属性加上 `AllowNull`。这样，获取此属性的时候会得到非 `null` 的值，而设置的时候却可以设置成 `null`。

```diff
++  [AllowNull]
    public string Text
    {
        get => GetValue() ?? "";
        set => SetValue(value ?? "");
    }
```

### 输入：`DisallowNull`

与以上场景相反的一个场景：

```csharp
private string? _text;

public string? Text
{
    get => _text;
    set => _text = value ?? throw new ArgumentNullException(nameof(value), "不允许将这个值设置为 null");
}
```

当你获取这个属性的时候，这个属性可能还没有初始化，于是我们获取到 `null`。然而我却并不允许你将这个属性赋值为 `null`，因为这是个不合理的值。

于是，请为这个属性加上 `DisallowNull`。这样，获取此属性的时候会得到可能为 `null` 的值，而设置的时候却不允许为 `null`。

### 输出：`MaybeNull`

如果你有尝试过迁移代码到可空类型，基本上一定会遇到泛型方法的迁移问题：

```csharp
public T Find<T>(int index)
{
}
```

比如以上这个方法，找到了就返回找到的值，找不到就返回 `T` 的默认值。那么问题来了，`T` 没有指定这是值类型还是引用类型。

如果 `T` 是引用类型，那么默认值 `default(T)` 就会引入 `null`。但是泛型 `T` 并没有写成 `T?`，因此它是不可为 `null` 的。然而值类型和引用类型的 `T?` 代表的是不同的含义。这种矛盾应该怎么办？

这个时候，请给返回值标记 `MaybeNull`：

```diff
++  [return: MaybeNull]
    public T Find<T>(int index)
    {
    }
```

这表示此方法应该返回一个不可为 `null` 的类型，但在某些情况下可能会返回 `null`。

实际上这样的写法并没有从本质上解决掉泛型 `T` 的问题，不过可以用来给旧项目迁移时用来兼容 API 使用。

如果你可以不用考虑 API 的兼容性，那么可以使用新的泛型契约 `where T : notnull`。

```csharp
public T Find<T>(int index) where T : notnull
{
}
```

### 输出：`NotNull`

设想你有一个方法，方法参数是可以传入 `null` 的：

```csharp
public void EnsureInitialized(ref string? text)
{
}
```

然而这个方法的语义是确保此字段初始化。于是可以传入 `null` 但不会返回 `null` 的。这个时候请标记 `NotNull`：

```diff
--  public void EnsureInitialized(ref string? text)
++  public void EnsureInitialized([NotNull] ref string? text)
    {
    }
```

### `NotNullWhen`, `MaybeNullWhen`

`string.IsNullOrEmpty` 的实现就使用到了 `NotNullWhen`：

```csharp
bool IsNullOrEmpty([NotNullWhen(false)] string? value);
```

它表示当返回 `false` 的时候，`value` 参数是不可为 `null` 的。

这样，你在这个方法返回的 `false` 判断分支里面，是不需要对变量进行判空的。

当然，更典型的还有 TryDo 模式。比如下面是 `Version` 类的 `TryParse`：

```csharp
bool TryParse(string? input, [NotNullWhen(true)] out Version? result)
```

当返回 `true` 的时候，`result` 一定不为 `null`。

### `NotNullIfNotNull`

典型的情况比如指定默认值：

```csharp
[return: NotNullIfNotNull("defaultValue")]
public string? GetValue(string key, string? defaultValue)
{
}
```

这段代码里面，如果指定的默认值（`defaultValue`）是 `null` 那么返回值也就是 `null`；而如果指定的默认值是非 `null`，那么返回值也就不可为 `null` 了。
