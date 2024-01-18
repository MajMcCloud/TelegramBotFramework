# ExternalActionManager

Idea of this experiment is to find a good intuitive way for handling "Unhandled calls".
Right now they are "thrown" into an event handler and you have to take care over them on yourself.


Source of them would be in most cases Telegram Bot messages which has an Inlinekeyboard attached.

And the button press should navigate to a different form, or invoke a different action.


## How does it work ?


- Open the Bot
- Run the "/start" command
- Now you are on the only FormBase where you can get without interaction
- Use the "/test" command to invoke an "outside" message with some different buttons.
- Use them to navigate to different "hidden" forms


## Future ideas

- adding an Action for int and/or long datatypes



Begin: 18.01.2024