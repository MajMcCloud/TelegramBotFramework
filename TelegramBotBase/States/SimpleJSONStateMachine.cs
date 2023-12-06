using System;
using System.IO;
using Newtonsoft.Json;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.States;

/// <summary>
///     Is used for simple object structures like classes, lists or basic datatypes without generics and other compiler
///     based data types.
/// </summary>
public class SimpleJsonStateMachine : IStateMachine
{
    /// <summary>
    ///     Will initialize the state machine.
    /// </summary>
    /// <param name="file">Path of the file and name where to save the session details.</param>
    /// <param name="fallbackStateForm">
    ///     Type of Form which will be saved instead of Form which has
    ///     <seealso cref="Attributes.IgnoreState" /> attribute declared. Needs to be subclass of
    ///     <seealso cref="Form.FormBase" />.
    /// </param>
    /// <param name="overwrite">Declares of the file could be overwritten.</param>
    public SimpleJsonStateMachine(string file, Type fallbackStateForm = null, bool overwrite = true)
    {
        FallbackStateForm = fallbackStateForm;

        if (FallbackStateForm != null && !FallbackStateForm.IsSubclassOf(typeof(FormBase)))
        {
            throw new ArgumentException($"{nameof(FallbackStateForm)} is not a subclass of {nameof(FormBase)}");
        }

        FilePath = file ?? throw new ArgumentNullException(nameof(file));
        Overwrite = overwrite;
    }

    public string FilePath { get; set; }

    public bool Overwrite { get; set; }

    public Type FallbackStateForm { get; }

    public StateContainer LoadFormStates()
    {
        try
        {
            var content = File.ReadAllText(FilePath);

            var sc = JsonConvert.DeserializeObject<StateContainer>(content);

            return sc;
        }
        catch
        {
        }

        return new StateContainer();
    }

    public void SaveFormStates(SaveStatesEventArgs e)
    {
        if (File.Exists(FilePath))
        {
            if (!Overwrite)
            {
                throw new Exception("File exists already.");
            }

            File.Delete(FilePath);
        }

        try
        {
            var content = JsonConvert.SerializeObject(e.States, Formatting.Indented);

            File.WriteAllText(FilePath, content);
        }
        catch
        {
        }
    }
}