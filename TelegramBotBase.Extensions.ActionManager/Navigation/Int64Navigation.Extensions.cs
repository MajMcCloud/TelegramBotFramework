using System.Linq.Expressions;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Navigation
{
    public static class Int64Navigation_Extensions
    {

        public static void AddInt64Navigation<TForm>(this IExternalActionManager manager, string method, Expression<Func<TForm, long>> propertySelector)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<TForm, long>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new Int64Navigation<TForm>(method, setter));
        }

        public static void AddInt64Navigation<TForm>(this IExternalActionManager manager, string method, Action<TForm, long> action)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new Int64Navigation<TForm>(method, action));
        }

        public static void AddInt64Navigation(this IExternalActionManager manager, Type formType, string value, Expression<Func<FormBase, long>> propertySelector)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<FormBase, long>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new Int64Navigation(formType, value, setter));
        }

        public static void AddInt64Navigation(this IExternalActionManager manager, Type formType, string method, Action<FormBase, long> action)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new Int64Navigation(formType, method, action));
        }
    }
}
