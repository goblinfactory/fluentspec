using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.DynamicProxy;

namespace FluentSpec.Classes {

    public class TestObjectFactory {

        public Type Type;
        public object Object;
        public TestProcessor Processor;
        private object[] Args;

        private readonly Type[] Types = new[] { typeof(TestProcessor) };

        public SubjectClass TestObjectFor<SubjectClass>(params object[] Args) {
            this.Args = Args;
             
            Type = typeof(SubjectClass);
            Processor = TestProcessor;
            Object = TestObject;

            CreateDependencies();

            return (SubjectClass) Object;
        }

        public virtual object TestObject { get { return 
            Type.IsClass ? CreateClassProxy : CreateInterfaceProxy
        ;}}

        public virtual object CreateClassProxy { get { return new 
            ProxyGenerator().CreateClassProxy(Type, Types, 
                ProxyGenerationOptions.Default, Args, ClassInterceptor)
        ;}}

        public virtual object CreateInterfaceProxy { get { return new 
            ProxyGenerator().CreateInterfaceProxyWithoutTarget(
                Type, Types, InterfaceInterceptor)
        ;}}

        public virtual TestProcessor TestProcessor { get { return 
            ObjectFactory.TestProcessor
        ;}}

        private TestObjectInterceptor ClassInterceptor { get { return 
            ObjectFactory.ClassInterceptor(Processor)
        ;}}

        private TestObjectInterceptor InterfaceInterceptor { get { return
            ObjectFactory.InterfaceInterceptor(Processor)
        ;}}

        private List<PropertyInfo> Properties { get {
            return new List<PropertyInfo>(Type.GetProperties())
        ;}}

        public virtual void CreateDependencies() {
            Properties.ForEach(CreateProperty)
        ;}

        public PropertyInfo Property;

        public void CreateProperty(PropertyInfo Property) { 
            this.Property = Property;
            try { if (ShouldTestProperty) SetupDependencyProperty(); }
            catch { /* no biggie really */ }
        }

        public virtual void SetupDependencyProperty() {
            Type = Property.PropertyType;
            Property.SetValue(Object, CreateInterfaceProxy, null);
        }

        public virtual bool ShouldTestProperty { get { return
            IsAbstractProperty
            && Property.CanWrite
            && Property.CanRead
            && HasNotSetProperty
        ;}}

        bool IsAbstractProperty { get { return
            Property.PropertyType.IsAbstract
            || Property.PropertyType.IsInterface
        ;}}

        public virtual bool HasNotSetProperty { get { return
            Property.GetValue(Object, null) == null
        ;}}
    }
}