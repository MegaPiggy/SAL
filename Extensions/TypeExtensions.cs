using SALT.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class TypeExtensions
{
    public static object GetDefault(this Type t)
    {
        return typeof(TypeExtensions).GetStaticMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(null, null);
    }

    public static T GetDefaultGeneric<T>()
    {
        return default(T);
    }

    //public static Type ToType(this string name)
    //{
    //    return Type.GetType(name) ?? Base.assembly.GetType(name) ?? typeof(GameObject).Assembly.GetType(name) ?? typeof(SlimeDefinition).Assembly.GetType(name) ?? null;
    //}

    [Obsolete]
    public static Type ToType(this string TypeName)
    {

        // Try Type.GetType() first. This will work with types defined
        // by the Mono runtime, etc.
        var type = Type.GetType(TypeName, false, true);

        // If it worked, then we're done here
        if (type != null)
            return type;

        var saltType = SALT.Main.execAssembly.SearchForType(TypeName);
        if (saltType != null)
            return saltType;

        var saType = typeof(MainScript).Assembly.SearchForType(TypeName);
        if (saType != null)
            return saType;

        var unityType = typeof(UnityEngine.GameObject).Assembly.SearchForType(TypeName);
        if (unityType != null)
            return unityType;

        var physicsType = typeof(UnityEngine.Collider).Assembly.SearchForType(TypeName);
        if (physicsType != null)
            return physicsType;

        var audioType = typeof(UnityEngine.AudioSource).Assembly.SearchForType(TypeName);
        if (audioType != null)
            return audioType;

        var animationType = typeof(UnityEngine.Animation).Assembly.SearchForType(TypeName);
        if (animationType != null)
            return animationType;

        if (!TypeName.Contains(".")) return null;

        // Get the name of the assembly (Assumption is that we are using
        // fully-qualified type names)
        var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

        // Attempt to load the indicated Assembly
        var assembly = Assembly.LoadWithPartialName(assemblyName);
        if (assembly == null)
            return null;

        // Ask that assembly to return the proper Type
        return assembly.GetType(TypeName);

    }

    public static Type SearchForType(this Assembly assembly, string name)
    {
        foreach (Module module in assembly.GetModules())
        {
            foreach (Type mtype in module.GetTypes())
            {
                if (mtype.Name.ToLower() == name.ToLower() || (mtype.Namespace + "." + mtype.Name).ToLower() == name.ToLower())
                    return mtype;
            }
        }
        return null;
    }

    private const string MIDDLE = " cannot be converted to type ";

    private static void LogConvert(System.ArgumentException e, string what = "field")
    {
        if (e.Message.Contains(MIDDLE))
        {
            string j = e.Message.Substring(15).Replace("'", "");
            string p = j.Substring(0, j.Length - 1);
            int i = p.IndexOf(MIDDLE);
            UnityEngine.Debug.LogWarning($"Cannot set {what} because the value ({p.Substring(0, i)}) cannot be converted to the {what} type ({p.Substring(i + MIDDLE.Length)}).");
        }
    }

    /// <summary>
    /// Invokes a static method
    /// </summary>
    /// <param name="type">Type to get method from</param>
    /// <param name="name">The name of the method</param>
    /// <param name="list">parameters</param>
    public static object InvokeMethod(this Type type, string name, params object[] list)
    {
        try
        {
            MethodInfo method = type.GetStaticMethod(name, list.Select(o => o.GetType()).ToArray());

            if (method == null) return null;

            return method?.Invoke(null, list);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    /// <summary>
    /// Invokes a static method
    /// </summary>
    /// <typeparam name="T">Type of return</typeparam>
    /// <param name="type">Type to get method from</param>
    /// <param name="name">The name of the method</param>
    /// <param name="list">parameters</param>
    public static T InvokeMethod<T>(this Type type, string name, params object[] list)
    {
        try
        {
            MethodInfo method = type.GetStaticMethod(name, list.Select(o => o.GetType()).ToArray());

            if (method == null) return default;

            return (T)method?.Invoke(null, list);
        }
        catch
        {
            // ignored
        }

        return default;
    }

    /// <summary>
    /// Sets the value of a static field
    /// </summary>
    /// <param name="type">Type to set field value of</param>
    /// <param name="name">The name of the field</param>
    /// <param name="value">The value to set</param>
    public static void SetField(this Type type, string name, object value)
    {
        try
        {
            FieldInfo field = type.GetStaticField(name);

            if (field == null) return;

            field?.SetValue(null, value);
        }
        catch (System.ArgumentException e)
        {
            LogConvert(e);
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Sets the value of a static field
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="type">Type to set field value of</param>
    /// <param name="name">The name of the field</param>
    /// <param name="value">The value to set</param>
    public static void SetField<T>(this Type type, string name, T value)
    {
        try
        {
            FieldInfo field = type.GetStaticField(name);

            if (field == null) return;

            field?.SetValue(null, value);
        }
        catch (System.ArgumentException e)
        {
            LogConvert(e);
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Gets the value of a static field
    /// </summary>
    /// <param name="type">Type to get field value from</param>
    /// <param name="name">The name of the field</param>
    public static object GetField(this Type type, string name)
    {
        try
        {
            FieldInfo field = type.GetStaticField(name);

            if (field == null) return default;

            return field?.GetValue(null);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    /// <summary>
    /// Gets the value of a static field
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="type">Type to get field value from</param>
    /// <param name="name">The name of the field</param>
    public static T GetField<T>(this Type type, string name)
    {
        try
        {
            FieldInfo field = type.GetStaticField(name);

            if (field == null) return default;

            return (T)field?.GetValue(null);
        }
        catch
        {
            // ignored
        }

        return default;
    }

    /// <summary>
    /// Sets the value of a static property
    /// </summary>
    /// <param name="type">Type to set property value of</param>
    /// <param name="name">The name of the property</param>
    /// <param name="value">The value to set</param>
    public static void SetProperty(this Type type, string name, object value)
    {
        try
        {
            PropertyInfo field = type.GetStaticProperty(name);

            if (field == null) return;

            if (field.CanWrite)
                field.SetValue(null, value, null);
            else
                type.SetField($"<{name}>k__BackingField", value);
        }
        catch (System.ArgumentException e)
        {
            LogConvert(e, "property");
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Sets the value of a static property
    /// </summary>
    /// <param name="type">Type to set property value of</param>
    /// <param name="name">The name of the property</param>
    /// <param name="value">The value to set</param>
    /// <typeparam name="T">Type of value</typeparam>
    public static void SetProperty<T>(this Type type, string name, T value)
    {
        try
        {
            PropertyInfo field = type.GetStaticProperty(name);

            if (field == null) return;

            if (field.CanWrite)
                field.SetValue(null, value, null);
            else
                type.SetField<T>($"<{name}>k__BackingField", value);
        }
        catch (System.ArgumentException e)
        {
            LogConvert(e, "property");
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Gets the value of a static property
    /// </summary>
    /// <param name="type">Type to get property value from</param>
    /// <param name="name">The name of the property</param>
    public static object GetProperty(this Type type, string name)
    {
        try
        {
            PropertyInfo field = type.GetStaticProperty(name);

            if (field == null) return default;

            return field.CanRead ? field.GetValue(null, null) : type.GetField($"<{name}>k__BackingField");
        }
        catch
        {
            // ignored
        }

        return null;
    }

    /// <summary>
    /// Gets the value of a static property
    /// </summary>
    /// <param name="type">Type to get property value from</param>
    /// <param name="name">The name of the property</param>
    /// <typeparam name="T">Type of value</typeparam>
    public static T GetProperty<T>(this Type type, string name)
    {
        try
        {
            PropertyInfo field = type.GetStaticProperty(name);

            if (field == null) return default;

            return field.CanRead ? (T)field.GetValue(null, null) : type.GetField<T>($"<{name}>k__BackingField");
        }
        catch
        {
            // ignored
        }

        return default;
    }

    public static bool HasTypes(this ParameterInfo[] parameters, Type[] types)
    {
        if (types == null)
            throw new ArgumentNullException(nameof(types));
        if (parameters.Length != types.Length)
            return false;
        for (int index = 0; index < types.Length; ++index)
        {
            if (types[index] == (Type)null)
                throw new ArgumentNullException(nameof(types));
            if (parameters[index].ParameterType != types[index])
                return false;
        }
        return true;
    }

    public static Type GetNestedType(this Type type, string name, Accessibility accessibility = Accessibility.All) => type.GetNestedTypes(accessibility).FirstOrDefault(nt => nt.Name.Equals(name));
    public static Type[] GetNestedTypes(this Type type, Accessibility accessibility = Accessibility.All)
    {
        if (type == null)
            return Array.Empty<Type>();

        if (accessibility == Accessibility.All)
            return type.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        else if (accessibility == Accessibility.Public)
            return type.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
        else if (accessibility == Accessibility.NonPublic)
            return type.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        return type.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
    }

    public const string instanceConstructor = ".ctor";
    public const string staticConstructor = ".cctor";

    public static MethodInfo GetMethod(this Type type, string name) => type.GetInstanceMethod(name) ?? type.GetStaticMethod(name);
    public static MethodInfo GetMethod(this Type type, string name, Accessibility accessibility) => type.GetInstanceMethod(name, accessibility) ?? type.GetStaticMethod(name, accessibility);
    public static MethodInfo GetMethod(this Type type, string name, Type[] parameters) => type.GetInstanceMethod(name, parameters) ?? type.GetStaticMethod(name, parameters);

    public static MethodInfo[] GetMethods(this Type type) => type.GetInstanceMethods().Join(type.GetStaticMethods()).ToArray();
    public static MethodInfo[] GetMethods(this Type type, Accessibility accessibility) => type.GetInstanceMethods(accessibility).Join(type.GetStaticMethods(accessibility)).ToArray();
    public static ConstructorMethodList GetConstructors(this Type type)
    {
        if (type == null)
            return new ConstructorMethodList();

        ConstructorInfo[] instanceConstructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        ConstructorInfo[] staticConstructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        List<ConstructorInfo> constructors = instanceConstructors.Join(staticConstructors).ToList();
        List<MethodInfo> methods = type.GetInstanceMethods().Where(mi => mi.Name == instanceConstructor).Join(type.GetStaticMethods().Where(mi => mi.Name == staticConstructor)).ToList();
        return new ConstructorMethodList(constructors, methods);
    }
    public static ConstructorMethodList GetConstructors(this Type type, Accessibility accessibility)
    {
        if (type == null)
            return new ConstructorMethodList();

        ConstructorInfo[] instanceConstructors;
        ConstructorInfo[] staticConstructors;
        
        if (accessibility == Accessibility.All)
        {
            instanceConstructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            staticConstructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
        else if (accessibility == Accessibility.Public)
        {
            instanceConstructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            staticConstructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
        else if (accessibility == Accessibility.NonPublic)
        {
            instanceConstructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            staticConstructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }
        else
        {
            instanceConstructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            staticConstructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }

        List<ConstructorInfo> constructors = instanceConstructors.Join(staticConstructors).ToList();
        List<MethodInfo> methods = type.GetInstanceMethods(accessibility).Where(mi => mi.Name == instanceConstructor).Join(type.GetStaticMethods(accessibility).Where(mi => mi.Name == staticConstructor)).ToList();
        return new ConstructorMethodList(constructors, methods);
    }

    public static ConstructorMethodInfo GetInstanceConstructor(this Type type) => type.GetInstanceConstructors().FirstOrDefault();
    public static MethodInfo GetInstanceMethod(this Type type, string name) => type.GetInstanceMethods().FirstOrDefault(mi => mi.Name == name);
    public static ConstructorMethodInfo GetInstanceConstructor(this Type type, Accessibility accessibility) => type.GetInstanceConstructors(accessibility).FirstOrDefault();
    public static MethodInfo GetInstanceMethod(this Type type, string name, Accessibility accessibility) => type.GetInstanceMethods(accessibility).FirstOrDefault(mi => mi.Name == name);
    public static MethodInfo GetInstanceMethod(this Type type, string name, Type[] parameters) => type.GetInstanceMethods().FirstOrDefault(mi => mi.Name == name && mi.GetParameters().HasTypes(parameters));
    public static ConstructorMethodInfo GetInstanceConstructor(this Type type, Type[] parameters) => type.GetInstanceConstructors().FirstOrDefault(mi => mi.GetParameters().HasTypes(parameters), ci => ci.GetParameters().HasTypes(parameters));
    public static MethodInfo GetInstanceMethod(this Type type, string name, Accessibility accessibility, Type[] parameters) => type.GetInstanceMethods(accessibility).FirstOrDefault(mi => mi.Name == name && mi.GetParameters().HasTypes(parameters));
    public static ConstructorMethodInfo GetInstanceConstructor(this Type type, Accessibility accessibility, Type[] parameters) => type.GetInstanceConstructors(accessibility).FirstOrDefault(mi => mi.GetParameters().HasTypes(parameters), ci => ci.GetParameters().HasTypes(parameters));
    public static ConstructorMethodList GetInstanceConstructors(this Type type)
    {
        if (type == null)
            return new ConstructorMethodList();

        List<ConstructorInfo> constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList();
        List<MethodInfo> methods = type.GetInstanceMethods().Where(mi => mi.Name == instanceConstructor).ToList();
        return new ConstructorMethodList(constructors, methods);
    }
    public static ConstructorMethodList GetInstanceConstructors(this Type type, Accessibility accessibility)
    {
        if (type == null)
            return new ConstructorMethodList();

        List<MethodInfo> methods = type.GetInstanceMethods(accessibility).Where(mi => mi.Name == instanceConstructor).ToList();

        if (accessibility == Accessibility.All)
            return new ConstructorMethodList(type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList(), methods);
        else if (accessibility == Accessibility.Public)
            return new ConstructorMethodList(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList(), methods);
        else if (accessibility == Accessibility.NonPublic)
            return new ConstructorMethodList(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList(), methods);

        return new ConstructorMethodList(type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList(), methods);
    }
    public static MethodInfo[] GetInstanceMethods(this Type type) => type != null ? type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy) : Array.Empty<MethodInfo>();
    public static MethodInfo[] GetInstanceMethods(this Type type, Accessibility accessibility)
    {
        if (type == null)
            return Array.Empty<MethodInfo>();

        if (accessibility == Accessibility.All)
            return type.GetInstanceMethods();
        else if (accessibility == Accessibility.Public)
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        else if (accessibility == Accessibility.NonPublic)
            return type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        return type.GetInstanceMethods();
    }
    public static ConstructorMethodInfo GetStaticConstructor(this Type type) => type.GetStaticConstructors().FirstOrDefault();
    public static MethodInfo GetStaticMethod(this Type type, string name) => type.GetStaticMethods().FirstOrDefault(mi => mi.Name == name);
    public static ConstructorMethodInfo GetStaticConstructor(this Type type, Accessibility accessibility) => type.GetStaticConstructors(accessibility).FirstOrDefault();
    public static MethodInfo GetStaticMethod(this Type type, string name, Accessibility accessibility) => type.GetStaticMethods(accessibility).FirstOrDefault(mi => mi.Name == name);
    public static ConstructorMethodInfo GetStaticConstructor(this Type type, Type[] parameters) => type.GetStaticConstructors().FirstOrDefault(mi => mi.GetParameters().HasTypes(parameters), ci => ci.GetParameters().HasTypes(parameters));
    public static MethodInfo GetStaticMethod(this Type type, string name, Type[] parameters) => type.GetStaticMethods().FirstOrDefault(mi => mi.Name == name && mi.GetParameters().HasTypes(parameters));
    public static ConstructorMethodInfo GetStaticConstructor(this Type type, Accessibility accessibility, Type[] parameters) => type.GetStaticConstructors(accessibility).FirstOrDefault(mi => mi.GetParameters().HasTypes(parameters), ci => ci.GetParameters().HasTypes(parameters));
    public static MethodInfo GetStaticMethod(this Type type, string name, Accessibility accessibility, Type[] parameters) => type.GetStaticMethods(accessibility).FirstOrDefault(mi => mi.Name == name && mi.GetParameters().HasTypes(parameters));
    public static ConstructorMethodList GetStaticConstructors(this Type type)
    {
        if (type == null)
            return new ConstructorMethodList();

        List<ConstructorInfo> constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList();
        List<MethodInfo> methods = type.GetStaticMethods().Where(mi => mi.Name == instanceConstructor).ToList();
        return new ConstructorMethodList(constructors, methods);
    }
    public static ConstructorMethodList GetStaticConstructors(this Type type, Accessibility accessibility)
    {
        if (type == null)
            return new ConstructorMethodList();

        List<MethodInfo> methods = type.GetStaticMethods(accessibility).Where(mi => mi.Name == staticConstructor).ToList();

        if (accessibility == Accessibility.All)
            return new ConstructorMethodList(type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList(), methods);
        else if (accessibility == Accessibility.Public)
            return new ConstructorMethodList(type.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList(), methods);
        else if (accessibility == Accessibility.NonPublic)
            return new ConstructorMethodList(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList(), methods);

        return new ConstructorMethodList(type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList(), methods);
    }
    public static MethodInfo[] GetStaticMethods(this Type type) => type != null ? type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy) : Array.Empty<MethodInfo>();
    public static MethodInfo[] GetStaticMethods(this Type type, Accessibility accessibility)
    {
        if (type == null)
            return Array.Empty<MethodInfo>();

        if (accessibility == Accessibility.All)
            return type.GetStaticMethods();
        else if (accessibility == Accessibility.Public)
            return type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        else if (accessibility == Accessibility.NonPublic)
            return type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        return type.GetStaticMethods();
    }

    public static FieldInfo GetInstanceField(this Type type, string name) => type.GetInstanceFields().FirstOrDefault(fi => fi.Name == name);
    public static FieldInfo GetInstanceField<T>(this Type type, string name) => type.GetInstanceFields().FirstOrDefault(fi => fi.Name == name && fi.FieldType == typeof(T));
    public static FieldInfo[] GetInstanceFields(this Type type) => type != null ? type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy) : Array.Empty<FieldInfo>();
    public static FieldInfo[] GetInstanceFields(this Type type, Accessibility accessibility)
    {
        if (type == null)
            return Array.Empty<FieldInfo>();

        if (accessibility == Accessibility.All)
            return type.GetInstanceFields();
        else if (accessibility == Accessibility.Public)
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        else if (accessibility == Accessibility.NonPublic)
            return type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        return type.GetInstanceFields();
    }
    public static FieldInfo GetStaticField(this Type type, string name) => type.GetStaticFields().FirstOrDefault(fi => fi.Name == name);
    public static FieldInfo GetStaticField<T>(this Type type, string name) => type.GetStaticFields().FirstOrDefault(fi => fi.Name == name && fi.FieldType == typeof(T));
    public static FieldInfo[] GetStaticFields(this Type type) => type != null ? type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy) : Array.Empty<FieldInfo>();
    public static FieldInfo[] GetStaticFields(this Type type, Accessibility accessibility)
    {
        if (type == null)
            return Array.Empty<FieldInfo>();

        if (accessibility == Accessibility.All)
            return type.GetStaticFields();
        else if (accessibility == Accessibility.Public)
            return type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        else if (accessibility == Accessibility.NonPublic)
            return type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        return type.GetStaticFields();
    }

    public static PropertyInfo GetInstanceProperty(this Type type, string name) => type.GetInstanceProperties().FirstOrDefault(pi => pi.Name == name);
    public static PropertyInfo GetInstanceProperty<T>(this Type type, string name) => type.GetInstanceProperties().FirstOrDefault(pi => pi.Name == name && pi.PropertyType == typeof(T));
    public static PropertyInfo[] GetInstanceProperties(this Type type) => type != null ? type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy) : Array.Empty<PropertyInfo>();
    public static PropertyInfo[] GetInstanceProperties(this Type type, Accessibility accessibility)
    {
        if (type == null)
            return Array.Empty<PropertyInfo>();

        if (accessibility == Accessibility.All)
            return type.GetInstanceProperties();
        else if (accessibility == Accessibility.Public)
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        else if (accessibility == Accessibility.NonPublic)
            return type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        return type.GetInstanceProperties();
    }
    public static PropertyInfo GetStaticProperty(this Type type, string name) => type.GetStaticProperties().FirstOrDefault(pi => pi.Name == name);
    public static PropertyInfo GetStaticProperty<T>(this Type type, string name) => type.GetStaticProperties().FirstOrDefault(pi => pi.Name == name && pi.PropertyType == typeof(T));
    public static PropertyInfo[] GetStaticProperties(this Type type) => type != null ? type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy) : Array.Empty<PropertyInfo>();
    public static PropertyInfo[] GetStaticProperties(this Type type, Accessibility accessibility)
    {
        if (type == null)
            return Array.Empty<PropertyInfo>();

        if (accessibility == Accessibility.All)
            return type.GetStaticProperties();
        else if (accessibility == Accessibility.Public)
            return type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        else if (accessibility == Accessibility.NonPublic)
            return type.GetProperties(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        return type.GetStaticProperties();
    }

    public static bool HasInstanceMethod<T>(this Type target, string methodName)
    {
        return target.GetInstanceMethod(methodName) != null;
    }

    public static bool HasStaticMethod<T>(this Type target, string methodName)
    {
        return target.GetStaticMethod(methodName) != null;
    }

    public static bool HasMethod<T>(this Type target, string methodName)
    {
        return target.GetInstanceMethod(methodName) != null || target.GetStaticMethod(methodName) != null;
    }

    public static bool HasInstanceField<T>(this Type target, string fieldName)
    {
        return target.GetInstanceField(fieldName) != null;
    }

    public static bool HasStaticField<T>(this Type target, string fieldName)
    {
        return target.GetStaticField(fieldName) != null;
    }

    public static bool HasField<T>(this Type target, string fieldName)
    {
        return target.GetInstanceField(fieldName) != null || target.GetStaticField(fieldName) != null;
    }

    public static bool HasInstanceProperty<T>(this Type target, string propertyName)
    {
        return target.GetInstanceProperty(propertyName) != null;
    }

    public static bool HasStaticProperty<T>(this Type target, string propertyName)
    {
        return target.GetStaticProperty(propertyName) != null;
    }

    public static bool HasProperty<T>(this Type target, string propertyName)
    {
        return target.GetInstanceProperty(propertyName) != null || target.GetStaticProperty(propertyName) != null;
    }
}

public enum Accessibility
{
    Public,
    NonPublic,
    All = Public | NonPublic,
}

public class ConstructorMethodInfo
{
    private MethodInfo method = null;
    private ConstructorInfo constructor = null;

    public bool IsConstructor => constructor != null;
    public bool IsMethod => method != null;

    public MethodInfo Method => method;
    public ConstructorInfo Constructor => constructor;
    public MethodBase Base
    {
        get
        {
            if (IsConstructor)
                return constructor;
            if (IsMethod)
                return method;
            return null;
        }
    }

    public ConstructorMethodInfo()
    {
    }

    public ConstructorMethodInfo(ConstructorInfo constructor)
    {
        this.constructor = constructor;
    }

    public ConstructorMethodInfo(MethodInfo method)
    {
        this.method = method;
    }
}

public class ConstructorMethodList : IList<ConstructorMethodInfo>, IList<ConstructorInfo>, IList<MethodInfo>
{
    private List<ConstructorInfo> constructors;
    public List<ConstructorInfo> Constructors => constructors;
    private List<MethodInfo> methods;
    public List<MethodInfo> Methods => methods;
    public List<MethodBase> Bases
    {
        get
        {
            List<MethodBase> list = new List<MethodBase>();
            foreach (ConstructorInfo constructor in constructors)
                list.Add(constructor);
            foreach (MethodInfo method in methods)
                list.Add(method);
            return list;
        }
    }
    public List<ConstructorMethodInfo> ConstructorMethods
    {
        get
        {
            List<ConstructorMethodInfo> list = new List<ConstructorMethodInfo>();
            foreach (ConstructorInfo constructor in constructors)
                list.Add(new ConstructorMethodInfo(constructor));
            foreach (MethodInfo method in methods)
                list.Add(new ConstructorMethodInfo(method));
            return list;
        }
    }

    public int Count => ConstructorMethods.Count;

    public bool IsReadOnly => true;

    ConstructorInfo IList<ConstructorInfo>.this[int index] { get => constructors[index]; set => constructors[index] = value; }
    MethodInfo IList<MethodInfo>.this[int index] { get => methods[index]; set => methods[index] = value; }

    /// <exception cref="NotImplementedException"></exception>
    public ConstructorMethodInfo this[int index] { get => ConstructorMethods[index]; set => throw new NotImplementedException(); }

    public ConstructorMethodList()
    {
        this.methods = new List<MethodInfo>();
        this.constructors = new List<ConstructorInfo>();
    }

    public ConstructorMethodList(List<MethodInfo> methods, List<ConstructorInfo> constructors)
    {
        if (methods == null)
            this.methods = new List<MethodInfo>();
        else
            this.methods = methods;

        if (constructors == null)
            this.constructors = new List<ConstructorInfo>();
        else
            this.constructors = constructors;
    }

    public static explicit operator List<ConstructorInfo>(ConstructorMethodList cml) => cml.constructors;
    public static explicit operator List<MethodInfo>(ConstructorMethodList cml) => cml.methods;
    public static implicit operator List<MethodBase>(ConstructorMethodList cml) => cml.Bases;
    public static implicit operator List<ConstructorMethodInfo>(ConstructorMethodList cml) => cml.ConstructorMethods;

    public ConstructorMethodList(List<ConstructorInfo> constructors, List<MethodInfo> methods) : this(methods, constructors) {}

    public ConstructorMethodList(List<ConstructorInfo> constructors) : this(null, constructors) {}

    public ConstructorMethodList(List<MethodInfo> methods) : this(methods, null) { }

    public ConstructorMethodInfo First()
    {
        return ConstructorMethods.First();
    }

    public ConstructorMethodInfo FirstOrDefault()
    {
        return ConstructorMethods.FirstOrDefault(customDefault: null);
    }

    public ConstructorMethodInfo First(Func<MethodInfo, bool> mcheck, Func<ConstructorInfo, bool> ccheck)
    {
        MethodInfo method = methods.FirstOrDefault(mcheck);
        ConstructorInfo constructor = constructors.FirstOrDefault(ccheck);
        if (constructor != null)
            return new ConstructorMethodInfo(constructor);
        else if (method != null)
            return new ConstructorMethodInfo(method);
        else
            throw new InvalidOperationException("Sequence contains no matching element");
    }

    public ConstructorMethodInfo FirstOrDefault(Func<MethodInfo, bool> mcheck, Func<ConstructorInfo, bool> ccheck)
    {
        MethodInfo method = methods.FirstOrDefault(mcheck);
        ConstructorInfo constructor = constructors.FirstOrDefault(ccheck);
        if (constructor != null)
            return new ConstructorMethodInfo(constructor);
        else if (method != null)
            return new ConstructorMethodInfo(method);
        else
            return null;
    }

    public ConstructorMethodList Where(Func<MethodInfo, bool> mcheck, Func<ConstructorInfo, bool> ccheck)
    {
        return new ConstructorMethodList(methods.Where(mcheck).ToList(), constructors.Where(ccheck).ToList());
    }

    public ConstructorMethodList Join(ConstructorMethodList other)
    {
        return new ConstructorMethodList(methods.Join(other.methods).ToList(), constructors.Join(other.constructors).ToList());
    }

    /// <exception cref="NotImplementedException"></exception>
    public int IndexOf(ConstructorMethodInfo item)
    {
        throw new NotImplementedException();
    }

    /// <exception cref="NotImplementedException"></exception>
    public void Insert(int index, ConstructorMethodInfo item)
    {
        throw new NotImplementedException();
    }

    /// <exception cref="NotImplementedException"></exception>
    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public void Add(ConstructorMethodInfo item)
    {
        if (item.IsMethod)
            methods.Add(item.Method);
        else if (item.IsConstructor)
            constructors.Add(item.Constructor);
    }

    public void Clear()
    {
        methods.Clear();
        constructors.Clear();
    }

    public bool Contains(ConstructorMethodInfo item)
    {
        if (item.IsMethod)
            return methods.Contains(item.Method);
        else if (item.IsConstructor)
            return constructors.Contains(item.Constructor);
        return false;
    }

    /// <exception cref="NotImplementedException"></exception>
    public void CopyTo(ConstructorMethodInfo[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(ConstructorMethodInfo item)
    {
        if (item.IsMethod)
            return methods.Remove(item.Method);
        else if (item.IsConstructor)
            return constructors.Remove(item.Constructor);
        return false;
    }

    public IEnumerator<ConstructorMethodInfo> GetEnumerator()
    {
        return ConstructorMethods.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ConstructorMethods.GetEnumerator();
    }

    public int IndexOf(MethodInfo item)
    {
        return methods.IndexOf(item);
    }

    public void Insert(int index, MethodInfo item)
    {
        methods.Insert(index, item);
    }

    public void Add(MethodInfo item)
    {
        methods.Add(item);
    }

    public bool Contains(MethodInfo item)
    {
        return methods.Contains(item);
    }

    public void CopyTo(MethodInfo[] array, int arrayIndex)
    {
        methods.CopyTo(array, arrayIndex);
    }

    public bool Remove(MethodInfo item)
    {
        return methods.Remove(item);
    }

    IEnumerator<MethodInfo> IEnumerable<MethodInfo>.GetEnumerator()
    {
        return methods.GetEnumerator();
    }

    public int IndexOf(ConstructorInfo item)
    {
        return constructors.IndexOf(item);
    }

    public void Insert(int index, ConstructorInfo item)
    {
        constructors.Insert(index, item);
    }

    public void Add(ConstructorInfo item)
    {
        constructors.Add(item);
    }

    public bool Contains(ConstructorInfo item)
    {
        return constructors.Contains(item);
    }

    public void CopyTo(ConstructorInfo[] array, int arrayIndex)
    {
        constructors.CopyTo(array, arrayIndex);
    }

    public bool Remove(ConstructorInfo item)
    {
        return constructors.Remove(item);
    }

    IEnumerator<ConstructorInfo> IEnumerable<ConstructorInfo>.GetEnumerator()
    {
        return constructors.GetEnumerator();
    }
}