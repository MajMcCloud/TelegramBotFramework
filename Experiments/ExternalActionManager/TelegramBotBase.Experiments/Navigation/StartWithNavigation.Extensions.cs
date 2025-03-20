using System.Linq.Expressions;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Navigation
{
    public static class StartWithNavigation_Extensions
    {
        public static void AddStartsWithNavigation<TForm>(this IExternalActionManager manager, string method, Expression<Func<TForm, string>> propertySelector)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<TForm, string>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new StartWithNavigation<TForm>(method, setter));
        }
        public static void AddStartsWithNavigation<TForm>(this IExternalActionManager manager, string value, Action<TForm, string> setProperty)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new StartWithNavigation<TForm>(value, setProperty));
        }

        public static void AddStartsWithNavigation(this IExternalActionManager manager, Type formType, string value, Expression<Func<FormBase, string>> propertySelector)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<FormBase, string>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new StartWithNavigation(formType, value, setter));
        }

        public static void AddStartsWithNavigation(this IExternalActionManager manager, Type formType, string value, Action<FormBase, string> setProperty)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new StartWithNavigation(formType, value, setProperty));
        }

    }

}
