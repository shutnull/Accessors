using System;
using System.Reflection;

public interface IAccessor
{
    object GetValue(object target);

    void SetValue(object target, object value);
}

public interface ISetterAccessor
{
    void SetValue(object target, object value);
}

public interface IGetterAccessor
{
    object GetValue(object target);
}

/// <summary>
/// IAccessorの実装
/// </summary>
/// <typeparam name="TTarget"></typeparam>
/// <typeparam name="TProperty"></typeparam>
internal sealed class Accessor<TTarget, TProperty> : IAccessor
{
    private readonly Func<TTarget, TProperty> Getter;
    private readonly Action<TTarget, TProperty> Setter;

    public Accessor(Func<TTarget, TProperty> getter, Action<TTarget, TProperty> setter)
    {
        Getter = getter;
        Setter = setter;
    }

    public object GetValue(object target)
    {
        return Getter((TTarget)target);
    }

    public void SetValue(object target, object value)
    {
        Setter((TTarget)target, (TProperty)value);
    }
}

/// <summary>
/// ISetterAccessorの実装
/// </summary>
/// <typeparam name="TTarget"></typeparam>
/// <typeparam name="TProperty"></typeparam>
internal sealed class SetterAccessor<TTarget, TProperty> : ISetterAccessor
{
    private readonly Action<TTarget, TProperty> Setter;

    public SetterAccessor(Action<TTarget, TProperty> setter)
    {
        Setter = setter;
    }

    public void SetValue(object target, object value)
    {
        Setter((TTarget)target, (TProperty)value);
    }
}

/// <summary>
/// IGetterAccessorの実装
/// </summary>
/// <typeparam name="TTarget"></typeparam>
/// <typeparam name="TProperty"></typeparam>
internal sealed class GetterAccessor<TTarget, TProperty> : IGetterAccessor
{
    private readonly Func<TTarget, TProperty> Getter;

    public GetterAccessor(Func<TTarget, TProperty> getter)
    {
        Getter = getter;
    }

    public object GetValue(object target)
    {
        return Getter((TTarget)target);
    }
}

/// <summary>
/// PropertyInfoからIAccessorへの変換
/// </summary>
public static class PropertyExtension
{
    /// <summary>
    /// リフレクションアクセッサー<br/>
    /// 読書の権限がないとき、nullを返す
    /// </summary>
    /// <param name="pi"></param>
    /// <returns></returns>
    public static IAccessor ToAccessor(this PropertyInfo pi)
    {
        if (!pi.CanWrite || !pi.CanWrite)
        {
            return null;
        }
        Type getterDelegateType = typeof(Func<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
        Delegate getter = Delegate.CreateDelegate(getterDelegateType, pi.GetGetMethod());

        Type setterDelegateType = typeof(Action<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
        Delegate setter = Delegate.CreateDelegate(setterDelegateType, pi.GetSetMethod());

        Type accessorType = typeof(Accessor<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
        IAccessor accessor = (IAccessor)Activator.CreateInstance(accessorType, getter, setter);

        return accessor;
    }

    /// <summary>
    /// リフレクションアクセッサー<br/>
    /// 読込の権限がないとき、nullを返す
    /// </summary>
    /// <param name="pi"></param>
    /// <returns></returns>
    public static ISetterAccessor ToSetterAccessor(this PropertyInfo pi)
    {
        if (!pi.CanWrite)
        {
            return null;
        }
        Type setterDelegateType = typeof(Action<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
        Delegate setter = Delegate.CreateDelegate(setterDelegateType, pi.GetSetMethod());

        Type accessorType = typeof(SetterAccessor<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
        ISetterAccessor accessor = (ISetterAccessor)Activator.CreateInstance(accessorType, setter);

        return accessor;
    }

    /// <summary>
    /// リフレクションアクセッサー<br/>
    /// 書込の権限がないとき、nullを返す
    /// </summary>
    /// <param name="pi"></param>
    /// <returns></returns>
    public static IGetterAccessor ToGetterAccessor(this PropertyInfo pi)
    {
        if (!pi.CanWrite)
        {
            return null;
        }
        Type getterDelegateType = typeof(Func<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
        Delegate getter = Delegate.CreateDelegate(getterDelegateType, pi.GetGetMethod());

        Type accessorType = typeof(GetterAccessor<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
        IGetterAccessor accessor = (IGetterAccessor)Activator.CreateInstance(accessorType, getter);

        return accessor;
    }
}