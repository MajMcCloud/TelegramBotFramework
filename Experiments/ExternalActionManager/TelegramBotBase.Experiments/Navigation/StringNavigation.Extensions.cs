using System.Linq.Expressions;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Navigation
{
    public static class StringNavigation_Extensions
    {

        public static void AddStringNavigation<TForm>(this IExternalActionManager manager, string method, Expression<Func<TForm, String>> propertySelector)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<TForm, String>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new StringNavigation<TForm>(method, setter));
        }

        public static void AddStringNavigation<TForm>(this IExternalActionManager manager, string method, Action<TForm, String> action)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new StringNavigation<TForm>(method, action));
        }

        public static void AddStringNavigation(this IExternalActionManager manager, Type formType, string value, Expression<Func<FormBase, String>> propertySelector)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<FormBase, String>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new StringNavigation(formType, value, setter));
        }

        public static void AddStringNavigation(this IExternalActionManager manager, Type formType, string method, Action<FormBase, String> action)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new StringNavigation(formType, method, action));
        }
    }
}
