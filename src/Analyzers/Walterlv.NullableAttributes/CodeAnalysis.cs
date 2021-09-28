#if NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0_OR_GREATER
// 新框架都包含基本的 Nullable Attributes。
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly:TypeForwardedTo(typeof(AllowNullAttribute))]
[assembly:TypeForwardedTo(typeof(DisallowNullAttribute))]
[assembly:TypeForwardedTo(typeof(MaybeNullAttribute))]
[assembly:TypeForwardedTo(typeof(NotNullAttribute))]
[assembly:TypeForwardedTo(typeof(MaybeNullWhenAttribute))]
[assembly:TypeForwardedTo(typeof(NotNullWhenAttribute))]
[assembly:TypeForwardedTo(typeof(NotNullIfNotNullAttribute))]
[assembly:TypeForwardedTo(typeof(DoesNotReturnAttribute))]
[assembly:TypeForwardedTo(typeof(DoesNotReturnIfAttribute))]
#else
// 旧框架需要包含 Nullable Attributes。
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// 标记一个不可空的输入实际上是可以传入 null 的。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    public sealed class AllowNullAttribute : Attribute { }

    /// <summary>
    /// 标记一个可空的输入实际上不应该传入 null。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    public sealed class DisallowNullAttribute : Attribute { }

    /// <summary>
    /// 标记一个非空的返回值实际上可能会返回 null，返回值包括输出参数。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    public sealed class MaybeNullAttribute : Attribute { }

    /// <summary>
    /// 标记一个可空的返回值实际上是不可能返回 null 的，返回值包括输出参数。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    public sealed class NotNullAttribute : Attribute { }

    /// <summary>
    /// 当返回指定的 true/false 时某个输出参数才可能为 null，而返回相反的值时那个输出参数则不可为 null。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class MaybeNullWhenAttribute : Attribute
    {
        /// <summary>
        /// 使用 true 或者 false 决定输出参数是否可能为 null。
        /// </summary>
        /// <param name="returnValue">如果方法返回值等于这个值，那么输出参数则可能为 null，否则输出参数是不可为 null 的。</param>
        public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        /// <summary>
        /// 获取返回值决定是否可为空的那个判断值。
        /// </summary>
        public bool ReturnValue { get; }
    }

    /// <summary>
    /// 当返回指定的 true/false 时，某个输出参数不可为 null，而返回相反的值时那个输出参数则可能为 null。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class NotNullWhenAttribute : Attribute
    {
        /// <summary>
        /// 使用 true 或者 false 决定输出参数是否不可为 null。
        /// </summary>
        /// <param name="returnValue">
        /// 如果方法或属性的返回值等于这个值，那么输出参数则不可为 null，否则输出参数是可能为 null 的。
        /// </param>
        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        /// <summary>
        /// 获取返回值决定是否不可为空的那个判断值。
        /// </summary>
        public bool ReturnValue { get; }
    }

    /// <summary>
    /// 指定的参数传入 null 时才可能返回 null，指定的参数传入非 null 时就不可能返回 null。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    public sealed class NotNullIfNotNullAttribute : Attribute
    {
        /// <summary>
        /// 使用一个参数名称决定返回值是否可能为 null。
        /// </summary>
        /// <param name="parameterName">
        /// 指定一个方法传入参数的名称，当这个参数传入非 null 时，输出参数或者返回值就是非 null；而这个参数传入可为 null 时，输出参数或者返回值就可为 null。
        /// </param>
        public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;

        /// <summary>
        /// 获取决定输出参数或者返回值是否可能为空的那个参数名称。
        /// </summary>
        public string ParameterName { get; }
    }

    /// <summary>
    /// 指定一个方法是不可能返回的。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class DoesNotReturnAttribute : Attribute { }

    /// <summary>
    /// 在方法的输入参数上指定一个条件，当这个参数传入了指定的 true/false 时方法不可能返回。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class DoesNotReturnIfAttribute : Attribute
    {
        /// <summary>
        /// 使用 true/false 决定方法是否可能返回。
        /// </summary>
        /// <param name="parameterValue">
        /// 在方法的输入参数上指定一个条件，当这个参数传入的值等于这里设定的值时，方法不可能返回。
        /// </param>
        public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;

        /// <summary>
        /// 获取决定方法是否可返回的那个参数的值。
        /// </summary>
        public bool ParameterValue { get; }
    }
}
#endif

#if NET5_0_OR_GREATER
// .NET 5.0 开始已包含更多 Nullable Attributes。
[assembly: TypeForwardedTo(typeof(MemberNotNullAttribute))]
[assembly: TypeForwardedTo(typeof(MemberNotNullWhenAttribute))]
#else
// 旧框架需要包含 Nullable Attributes。
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// 调用了此方法后，即可保证列表中所列出的字段和属性成员将不会为 null。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class MemberNotNullAttribute : Attribute
    {
        /// <summary>
        /// 指定调用了此方法后，所列出的字段和属性成员将不会为 null。
        /// </summary>
        /// <param name="member">
        /// 将保证不会为 null 的字段或属性名称。
        /// </param>
        public MemberNotNullAttribute(string member) => Members = new[] { member };

        /// <summary>
        /// 指定调用了此方法后，所列出的字段和属性成员将不会为 null。
        /// </summary>
        /// <param name="members">
        /// 将保证不会为 null 的字段或属性名称列表。
        /// </param>
        public MemberNotNullAttribute(params string[] members) => Members = members;

        /// <summary>
        /// 调用了此方法后保证不会为 null 的字段或属性名称列表。
        /// </summary>
        public string[] Members { get; }
    }

    /// <summary>
    /// 当返回指定的 true/false 时，即可保证列表中所列出的字段和属性成员将不会为 null。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class MemberNotNullWhenAttribute : Attribute
    {
        /// <summary>
        /// 使用 true 或者 false 决定是否所列出的字段和属性成员将不会为 null。
        /// </summary>
        /// <param name="returnValue">
        /// 如果方法或属性的返回值等于这个值，那么所列出的字段和属性成员将不会为 null。
        /// </param>
        /// <param name="member">
        /// 将保证不会为 null 的字段或属性名称列表。
        /// </param>
        public MemberNotNullWhenAttribute(bool returnValue, string member)
        {
            ReturnValue = returnValue;
            Members = new[] { member };
        }

        /// <summary>
        /// 使用 true 或者 false 决定是否所列出的字段和属性成员将不会为 null。
        /// </summary>
        /// <param name="returnValue">
        /// 如果方法或属性的返回值等于这个值，那么所列出的字段和属性成员将不会为 null。
        /// </param>
        /// <param name="members">
        /// 将保证不会为 null 的字段或属性名称列表。
        /// </param>
        public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
        {
            ReturnValue = returnValue;
            Members = members;
        }

        /// <summary>
        /// 获取返回值决定是否不可为空的那个判断值。
        /// </summary>
        public bool ReturnValue { get; }

        /// <summary>
        /// 调用了此方法后保证不会为 null 的字段或属性名称列表。
        /// </summary>
        public string[] Members { get; }
    }
}
#endif
