# InlineAndReplyCombination Example


Here you got a basic example on how to use Inline buttons and a ReplyMarkup at the same time.

I used 2 ButtonGrid controls for this purpose. 

One for the ReplyKeyboard and one for the Inline buttons.

The ReplyKeyboard is within the base [*MultipleChoiceForm*](Baseclasses/MultipleChoiceForm) class just for reusing a bit of code.


You can put it into all single forms as well, if you wish. Works the same.


Session serialization is enabled by default via the JSON serializer.