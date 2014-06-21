using System;
using System.Linq.Expressions;

namespace SimpleInjector.Advanced.Core
{
    internal sealed class DependencyContextRewriter : ExpressionVisitor
    {
        internal Type ServiceType { get; set; }

        internal object ContextBasedFactory { get; set; }

        internal object RootFactory { get; set; }

        internal Expression Expression { get; set; }

        internal Type ImplementationType
        {
            get
            {
                var expression = this.Expression as NewExpression;

                if (expression != null)
                {
                    return expression.Constructor.DeclaringType;
                }

                return this.ServiceType;
            }
        }

        protected override Expression VisitInvocation(
            InvocationExpression node)
        {
            if (!this.IsRootedContextBasedFactory(node))
            {
                return base.VisitInvocation(node);
            }

            return Expression.Invoke(
                Expression.Constant(this.ContextBasedFactory),
                Expression.Constant(
                    new DependencyContext(
                        this.ServiceType,
                        this.ImplementationType)));
        }

        private bool IsRootedContextBasedFactory(
            InvocationExpression node)
        {
            var expression =
                node.Expression as ConstantExpression;

            if (expression == null)
            {
                return false;
            }

            return object.ReferenceEquals(expression.Value,
                this.RootFactory);
        }
    }
}