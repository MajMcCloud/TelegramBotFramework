using System.Linq.Expressions;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Navigation
{
    public static class GuidNavigation_Extensions
    {

        public static void AddGuidNavigation<TForm>(this IExternalActionManager manager, string method, Expression<Func<TForm, Guid>> propertySelector)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<TForm, Guid>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new GuidNavigation<TForm>(method, setter));
        }

        public static void AddGuidNavigation<TForm>(this IExternalActionManager manager, string method, Action<TForm, Guid> action)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new GuidNavigation<TForm>(method, action));
        }

        public static void AddGuidNavigation(this IExternalActionManager manager, Type formType, string value, Expression<Func<FormBase, Guid>> propertySelector)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<FormBase, Guid>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new GuidNavigation(formType, value, setter));
        }

        public static void AddGuidNavigation(this IExternalActionManager manager, Type formType, string method, Action<FormBase, Guid> action)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new GuidNavigation(formType, method, action));
        }
    }
}
