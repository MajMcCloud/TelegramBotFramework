using System.Linq.Expressions;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Navigation
{
    public static class Int32Navigation_Extensions
    {

        public static void AddInt32Navigation<TForm>(this ExternalActionManager manager, string method, Expression<Func<TForm, int>> propertySelector)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<TForm, int>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new Int32Navigation<TForm>(method, setter));
        }

        public static void AddInt32Navigation<TForm>(this ExternalActionManager manager, string method, Action<TForm, int> action)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new Int32Navigation<TForm>(method, action));
        }

        public static void AddInt32Navigation(this ExternalActionManager manager, Type formType, string value, Expression<Func<FormBase, int>> propertySelector)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<FormBase, int>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new Int32Navigation(formType, value, setter));
        }

        public static void AddInt32Navigation(this ExternalActionManager manager, Type formType, string method, Action<FormBase, int> action)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new Int32Navigation(formType, method, action));
        }
    }
}
