// C#

using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components.Forms;

namespace Web.Shared.Utilities
{
    public sealed class ModelValidator<TModel> where TModel : class
    {
        private readonly EditContext _editContext;
        
        public ModelValidator(IServiceProvider provider, TModel model)
        {
            _editContext = new EditContext(model ?? throw new ArgumentNullException(nameof(model)));
            _editContext.EnableDataAnnotationsValidation(provider);
        }

        public TModel Model => (TModel)_editContext.Model;
        public EditContext EditContext => _editContext;

        public bool AreValid(params Expression<Func<TModel, object?>>[] fields)
        {
            return ValidateFields(fields).Count == 0;
        }
        
        public Dictionary<string, string[]> ValidateFields(params Expression<Func<TModel, object?>>[] fields)
        {
            var messageStore = new ValidationMessageStore(_editContext);
            var dict = new Dictionary<string, string[]>(StringComparer.Ordinal);
    
            foreach (var accessor in fields)
            {
                var pathPackage = GetPath(accessor);
                var fieldId = new FieldIdentifier(_editContext.Model, pathPackage.FullPath);
        
                // Clear existing messages for this field
                messageStore.Clear(fieldId);
        
                // Get the property value
                var root = _editContext.Model.GetType();
                var value = _editContext.Model;
                object? parent = null;
                foreach (var propertyName in pathPackage.Parts)
                {
                    var property = root.GetProperty(propertyName);
                    root = property?.PropertyType ?? root;
                    parent = value;
                    value = property?.GetValue(value);
                }
        
                if (parent is null) throw new InvalidOperationException("Unable to find parent object.");
                
                // Validate just this property
                var validationContext = new ValidationContext(parent) 
                { 
                    MemberName = pathPackage.Parts.Last(), 
                };
                var results = new List<ValidationResult>();
        
                Validator.TryValidateProperty(value, validationContext, results);
        
                // Add any validation errors to the message store
                var messages = new List<string>();
                foreach (var result in results)
                {
                    var message = result.ErrorMessage;
                    if (message is null) continue;
                    
                    messageStore.Add(fieldId, message);
                    messages.Add(message);
                }
        
                if (messages.Count > 0)
                    dict[pathPackage.FullPath] = messages.ToArray();
            }
    
            // Notify EditContext that validation state changed
            _editContext.NotifyValidationStateChanged();
    
            return dict;
        }

        private class PathPackage
        {
            public required string FullPath { get; set; }
            public required IEnumerable<string> Parts { get; set; }
        }
        
        private static PathPackage GetPath(Expression<Func<TModel, object?>> expression)
        {
            // Build a "dotted" path from the expression, e.g., x => x.Contact.Name => "Contact.Name"
            Expression body = expression.Body;
            if (body is UnaryExpression u && u.NodeType == ExpressionType.Convert)
                body = u.Operand;

            var parts = new Stack<string>();
            var current = body as MemberExpression;
            while (current != null)
            {
                parts.Push(current.Member.Name);
                current = current.Expression as MemberExpression;
            }

            return new PathPackage
            {
                FullPath = string.Join(".", parts),
                Parts = parts
            };
        }
    }
}