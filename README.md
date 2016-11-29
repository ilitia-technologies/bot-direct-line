# Direct Line Connector Channel Demo for Microsoft Bot Framework

This repository was uploaded for demo purposes using the Direct Line Connector Channel to connect to Microsoft Bot Framework.
We use the ``Connector`` project in other solution as Portable Library for a Xamarin App but to simplify
this solution, we made a Console project to check it.

#Bot backend
We didn’t upload the "bot backend" yet, if you want to know more about it or want use it, 
please contact us at <appSupport@ilitia.com>.

Anyway you can use your SecretKey in the ``Connector`` project, change ``botConnectorSecretKey``
```c#
private const string botConnectorSecretKey = <botConnectorSecretKey>;
```
and change in the ``Console`` project, the deserialization part.
```c#
var text = JsonConvert.DeserializeObject<BotResponseModel>(mssg.text);
```
You can create your own bot here:

https://dev.botframework.com/

#Notes
We use a serializable object ``BotResponseModel`` in the bot response to include more fields
needed in our app but it is not necessary.


