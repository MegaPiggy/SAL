﻿using HarmonyLib;
using SALT.Config.Attributes;
using SALT.Config.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SALT.Config
{
    public class FieldBackedConfigElement : ConfigElement
    {

        protected FieldInfo field;
        public override Type ElementType => field.FieldType;

        protected override object Value
        {
            get { return field.GetValue(null); }
            set { field.SetValue(null, value); }
        }
        static bool GetAttributeOfType<T>(FieldInfo field, out T attribute) where T : Attribute
        {
            attribute = field.GetCustomAttributes(false).FirstOrDefault(x => x is T) as T;
            return attribute != null;
        }

        public override void SetValue<T>(T value)
        {
            if (value is int) value = (T)Options.Range.GetValue(value);
            base.SetValue(value);
        }

        public static ConfigElementOptions GenerateAttributesOptions(FieldInfo field)
        {


            return new ConfigElementOptions()
            {
                Comment = GetAttributeOfType<ConfigCommentAttribute>(field, out var comment) ? comment.Comment : null,
                Name = GetAttributeOfType<ConfigNameAttribute>(field, out var name) ? name.Name : field.Name,
                Parser = GetAttributeOfType<ConfigParserAttribute>(field, out var parser) ? parser.Parser : ParserRegistry.TryGetParser(field.FieldType, out var backupParser) ? backupParser : null,
                ReloadMode = GetAttributeOfType<ConfigReloadAttribute>(field, out var reload) ? reload.Mode : ReloadMode.NORMAL,
                Range = GetAttributeOfType<ConfigRangeAttribute>(field, out var range) ? new DoubleConstrainedInt(0, range.min, range.max) : DoubleConstrainedInt.zero
            };
        }

        public FieldBackedConfigElement(FieldInfo field) : base(GenerateAttributesOptions(field))
        {
            this.field = field;
            if (Value is int) Value = Options.Range.GetValue((int)Value);
            Options.DefaultValue = Value;
            if (Value is IStringParserProvider val) Options.Parser = val.GetParser();
            if (Options.Parser == null) throw new Exception(field.FieldType.ToString());
            if (GetAttributeOfType<ConfigCallbackAttribute>(field, out var attribute))
                OnValueChanged += x => AccessTools.Method(field.DeclaringType, attribute.methodName).Invoke(null, new[]{ Value, x });
        }
    }
    public class ConfigElement<T> : ConfigElement
    {
        public override Type ElementType => typeof(T);
        protected override object Value { get; set; }
        public ConfigElement(string name) : base(GenerateDefaultOptions(typeof(T), name))
        {
            if (Options.DefaultValue != null) Value = Options.DefaultValue;
        }
    }
    public abstract class ConfigElement
    {
        public abstract Type ElementType { get; }
        public ConfigElementOptions Options { get; protected set; }
        protected abstract object Value { get; set; }

        public delegate void OnValueChangedDelegate(object newVal);

        public event OnValueChangedDelegate OnValueChanged;

        public virtual T GetValue<T>()
        {
            return (T)Value;
        }

        protected ConfigElement(ConfigElementOptions options)
        {
            this.Options = options;

        }

        public virtual void SetValue<T>(T value)
        {
            OnValueChanged?.Invoke(value);
            Console.Console.Log("Set to " + value);
            Value = value;
        }

        public virtual void SetValue<T>(T value, bool log)
        {
            OnValueChanged?.Invoke(value);
            if (log)
                Console.Console.Log("Set to " + value);
            Value = value;
        }

        public static ConfigElementOptions GenerateDefaultOptions(Type type, string name)
        {
            return new ConfigElementOptions{
                Parser = ParserRegistry.TryGetParser(type, out var parser) ? parser : null,
                DefaultValue = type.IsValueType ? Activator.CreateInstance(type) : null,
                Name = name
            };
        }
    }

    public class ConfigElementComparer : IEqualityComparer<ConfigElement>
    {
        public bool Equals(ConfigElement x, ConfigElement y)
        {
            return x.Options.Name == y.Options.Name;
        }

        public int GetHashCode(ConfigElement obj)
        {
            throw new NotImplementedException();
        }
    }

    public class ConfigElementOptions
    {
        public IStringParser Parser { get; internal set; }
        public string Comment { get; internal set; }
        public object DefaultValue { get; internal set; }
        public string Name { get; internal set; }
        public DoubleConstrainedInt Range { get; internal set; }
        public ReloadMode ReloadMode { get; internal set; }
    }

    public enum ReloadMode
    {
        NORMAL,
        SAVE,
        RESTART
    }
}
