using System;
using Esprima.Ast;

namespace Esprima.Utils
{
    public partial class AstVisitorEventSource
    {
        public event EventHandler<Node>? VisitingJSXSpreadAttribute;
        public event EventHandler<Node>? VisitedJSXSpreadAttribute;
        public event EventHandler<Node>? VisitingJSXElement;
        public event EventHandler<Node>? VisitedJSXElement;
        public event EventHandler<Node>? VisitingJSXAttribute;
        public event EventHandler<Node>? VisitedJSXAttribute;
        public event EventHandler<Node>? VisitingJSXIdentifier;
        public event EventHandler<Node>? VisitedJSXIdentifier;
        public event EventHandler<Node>? VisitingJSXClosingElement;
        public event EventHandler<Node>? VisitedJSXClosingElement;
        public event EventHandler<Node>? VisitingJSXText;
        public event EventHandler<Node>? VisitedJSXText;
        public event EventHandler<Node>? VisitingJSXClosingFragment;
        public event EventHandler<Node>? VisitedJSXClosingFragment;
        public event EventHandler<Node>? VisitingJSXOpeningFragment;
        public event EventHandler<Node>? VisitedJSXOpeningFragment;
        public event EventHandler<Node>? VisitingJSXOpeningElement;
        public event EventHandler<Node>? VisitedJSXOpeningElement;
        public event EventHandler<Node>? VisitingJSXNamespacedName;
        public event EventHandler<Node>? VisitedJSXNamespacedName;
        public event EventHandler<Node>? VisitingJSXMemberExpression;
        public event EventHandler<Node>? VisitedJSXMemberExpression;
        public event EventHandler<Node>? VisitingJSXEmptyExpression;
        public event EventHandler<Node>? VisitedJSXEmptyExpression;
        public event EventHandler<Node>? VisitingJSXExpressionContainer;
        public event EventHandler<Node>? VisitedJSXExpressionContainer;
        
        protected internal override void VisitJSXSpreadAttribute(JSXSpreadAttribute jsxSpreadAttribute)
        {
            VisitingJSXSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
            base.VisitJSXSpreadAttribute(jsxSpreadAttribute);
            VisitedJSXSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
        }

        protected internal override void VisitJSXElement(JSXElement jsxElement)
        {
            VisitingJSXElement?.Invoke(this, jsxElement);
            base.VisitJSXElement(jsxElement);
            VisitedJSXElement?.Invoke(this, jsxElement);
        }

        protected internal override void VisitJSXAttribute(JSXAttribute jsxAttribute)
        {
            VisitingJSXAttribute?.Invoke(this, jsxAttribute);
            base.VisitJSXAttribute(jsxAttribute);
            VisitedJSXAttribute?.Invoke(this, jsxAttribute);
        }

        protected internal override void VisitJSXIdentifier(JSXIdentifier jsxIdentifier)
        {
            VisitingJSXIdentifier?.Invoke(this, jsxIdentifier);
            base.VisitJSXIdentifier(jsxIdentifier);
            VisitedJSXIdentifier?.Invoke(this, jsxIdentifier);
        }

        protected internal override void VisitJSXClosingElement(JSXClosingElement jsxClosingElement)
        {
            VisitingJSXClosingElement?.Invoke(this, jsxClosingElement);
            base.VisitJSXClosingElement(jsxClosingElement);
            VisitedJSXClosingElement?.Invoke(this, jsxClosingElement);
        }

        protected internal override void VisitJSXText(JSXText jsxText)
        {
            VisitingJSXText?.Invoke(this, jsxText);
            base.VisitJSXText(jsxText);
            VisitedJSXText?.Invoke(this, jsxText);
        }

        protected internal override void VisitJSXClosingFragment(JSXClosingFragment jsxClosingFragment)
        {
            VisitingJSXClosingFragment?.Invoke(this, jsxClosingFragment);
            base.VisitJSXClosingFragment(jsxClosingFragment);
            VisitedJSXClosingFragment?.Invoke(this, jsxClosingFragment);
        }

        protected internal override void VisitJSXOpeningFragment(JSXOpeningFragment jsxOpeningFragment)
        {
            VisitingJSXOpeningFragment?.Invoke(this, jsxOpeningFragment);
            base.VisitJSXOpeningFragment(jsxOpeningFragment);
            VisitedJSXOpeningFragment?.Invoke(this, jsxOpeningFragment);
        }

        protected internal override void VisitJSXOpeningElement(JSXOpeningElement jsxOpeningElement)
        {
            VisitingJSXOpeningElement?.Invoke(this, jsxOpeningElement);
            base.VisitJSXOpeningElement(jsxOpeningElement);
            VisitedJSXOpeningElement?.Invoke(this, jsxOpeningElement);
        }

        protected internal override void VisitJSXNamespacedName(JSXNamespacedName jsxNamespacedName)
        {
            VisitingJSXNamespacedName?.Invoke(this, jsxNamespacedName);
            base.VisitJSXNamespacedName(jsxNamespacedName);
            VisitedJSXNamespacedName?.Invoke(this, jsxNamespacedName);
        }

        protected internal override void VisitJSXMemberExpression(JSXMemberExpression jsxMemberExpression)
        {
            VisitingJSXMemberExpression?.Invoke(this, jsxMemberExpression);
            base.VisitJSXMemberExpression(jsxMemberExpression);
            VisitedJSXMemberExpression?.Invoke(this, jsxMemberExpression);
        }

        protected internal override void VisitJSXEmptyExpression(JSXEmptyExpression jsxEmptyExpression)
        {
            VisitingJSXEmptyExpression?.Invoke(this, jsxEmptyExpression);
            base.VisitJSXEmptyExpression(jsxEmptyExpression);
            VisitedJSXEmptyExpression?.Invoke(this, jsxEmptyExpression);
        }

        protected internal override void VisitJSXExpressionContainer(JSXExpressionContainer jsxExpressionContainer)
        {
            VisitingJSXExpressionContainer?.Invoke(this, jsxExpressionContainer);
            base.VisitJSXExpressionContainer(jsxExpressionContainer);
            VisitedJSXExpressionContainer?.Invoke(this, jsxExpressionContainer);
        }
    }
}
