using System;
using System.Threading.Tasks;

namespace TelegramBotBase.Form;

public class ModalDialog : FormBase
{
    /// <summary>
    ///     Contains the parent from where the modal dialog has been opened.
    /// </summary>
    public FormBase ParentForm { get; set; }


    /// <summary>
    /// Gets a value indicating whether the current form is displayed as a modal dialog.
    /// </summary>
    public bool IsDisplayedAsModal => ParentForm != null;

    /// <summary>
    ///     This is a modal only function and does everything to close this form.
    /// </summary>
    public async Task CloseForm()
    {
        await CloseControls();

        await OnClosed(EventArgs.Empty);


        if (IsDisplayedAsModal)
            await ParentForm.ReturnFromModal(this);
    }

    public override Task NavigateTo(FormBase newForm, params object[] args)
    {
        if(IsDisplayedAsModal)
            throw new InvalidOperationException("Cannot navigate to another form from a modal dialog. Please close the modal dialog first.");

        return base.NavigateTo(newForm, args);
    }
}