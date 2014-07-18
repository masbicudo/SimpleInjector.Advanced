using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleInjector.Advanced.Helpers
{
    // STRIPED DOWN copy of my TypeReplacementVisitor from Masb.Reflection
    internal class TypeReplacementVisitor : ExpressionVisitor
    {
        private readonly Type markerType;
        private readonly Type newType;

        public TypeReplacementVisitor(Type markerType, Type newType)
        {
            this.markerType = markerType;
            this.newType = newType;
        }

        private ConstructorInfo VisitConstructorInfo(ConstructorInfo ctor)
        {
            if (ctor == null)
                return null;

            var type2 = this.VisitType(ctor.ReflectedType);
            if (type2 != ctor.ReflectedType)
            {
                var result = type2.GetConstructors().Single(c => c.MetadataToken == ctor.MetadataToken);
                return result;
            }

            return ctor;
        }

        private MethodInfo VisitMethodInfo(MethodInfo mi)
        {
            if (mi == null)
                return null;

            Type[] typePrams = null;
            if (mi.IsGenericMethod)
                typePrams = mi.GetGenericArguments().Select(this.VisitType).ToArray();

            var type2 = this.VisitType(mi.ReflectedType);
            MethodInfo newMi = mi;
            if (type2 != mi.ReflectedType)
                newMi = type2.GetMethods().Single(c => c.MetadataToken == mi.MetadataToken);

            if (typePrams != null)
                newMi = newMi.GetGenericMethodDefinition().MakeGenericMethod(typePrams);

            return newMi;
        }

        private Type VisitType(Type type)
        {
            if (type == null)
                return null;

            if (type == this.markerType)
                return this.newType;

            if (type.IsGenericType)
            {
                var typePrams = type.GetGenericArguments().Select(this.VisitType).ToArray();
                var result = type.GetGenericTypeDefinition().MakeGenericType(typePrams);
                return result;
            }

            return type;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var newConstructor = this.VisitConstructorInfo(node.Constructor);
            var args = this.Visit(node.Arguments);
            return Expression.New(newConstructor, args);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.ConvertChecked)
                return Expression.ConvertChecked(this.Visit(node.Operand), this.VisitType(node.Type));

            if (node.NodeType == ExpressionType.Convert)
                return Expression.Convert(this.Visit(node.Operand), this.VisitType(node.Type));

            return base.VisitUnary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object != null)
            {
                var result = Expression.Call(
                    this.Visit(node.Object),
                    this.VisitMethodInfo(node.Method),
                    this.Visit(node.Arguments));

                return result;
            }

            if (node.Object == null)
            {
                var result = Expression.Call(
                    this.VisitMethodInfo(node.Method),
                    this.Visit(node.Arguments));

                return result;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var newObj = this.VisitAny(node.Value);
            if (!object.ReferenceEquals(newObj, node.Value))
                return Expression.Constant(newObj);

            return base.VisitConstant(node);
        }

        private object VisitAny(object obj)
        {
            return
                this.VisitMethodInfo(obj as MethodInfo) ??
                this.VisitConstructorInfo(obj as ConstructorInfo) ??
                this.VisitType(obj as Type) ??
                this.Visit(obj as Expression) ??
                obj;
        }
    }
}