# Messenger Cloud

This here is [Anonymous Messenger](https://github.com/Andrea-Bruno/AnonymousMessenger) helper software, it adopts trustless security. It offers an encrypted cloud storage where devices encrypt their contact list and other data and send it for safekeeping. This allows the recovery of the contacts, when the messenger account with the passprase is recovered. The device encrypts the data before sending it and therefore the cloud has no way of being able to see it unencrypted (trustless).
Both this project and the messaging software are open source and inspectable, no aspect of privacy and security has been overlooked, this work reaches military security levels.

You may also be interested in:

* [Anonymous messenger, messaging software with military-grade security](https://github.com/Andrea-Bruno/AnonymousMessenger)

This project has three open source dependencies for security and functionality. These dependencies are implemented here in the form of Nuget packages, and here are the sources on GitHub (you can replace the nuget packages with the source projects if you want):

* [Secure storage](https://github.com/Andrea-Bruno/SecureStorage) it is a powerful data safe, the cryptographic keys and data that would allow the software to be attacked are kept with this tool.

* [Encrypted messaging](https://github.com/Andrea-Bruno/EncryptedMessaging) it is a powerful low-level cryptographic protocol, of the Trustless type, which manages communication, groups and contacts (this software will never access your address book, this library is the heart of the application).

* [Communication channel](https://github.com/Andrea-Bruno/EncryptedMessaging/tree/master/CommunicationChannel) is the low-level socket communication protocol underlying encrypted communication.


The reasons that led to this project with dontnet is that it is an open source development environment, and effective security is achieved only by being able to inspect all parts of the code, including the development framework.
* [.NET is open source](https://dotnet.microsoft.com/en-us/platform/open-source)

Our target is very linux oriented, and the partnership between Microsoft and Canonical ensure the highest standard of security and reliability.

* [Microsoft and Canonical: partnering for security](https://ubuntu.com/blog/install-dotnet-on-ubuntu)

* [Red Hat works with Microsoft to ensure new major versions and service releases are available in tandem with Microsoft releases](https://developers.redhat.com/products/dotnet/overview)
