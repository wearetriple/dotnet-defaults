# Buckaroo kickstart

[Buckaroo](https://www.buckaroo.nl/) is a Payment Service Provider (PSP) from Dutch origin which is on the market for quite some time already. In the &C project (june 2022), we've used this provider for the registration of customers and their subscriptions, and the automatic billing - using the '[Subscription](https://support.buckaroo.nl/categorieen/subscriptions?mark=Subscriptions)' and '[Credit Management](https://support.buckaroo.nl/categorieen/credit-management?mark=credit%20management)' parts of Buckaroo respectively. The .NET SDK has not been used.

The code in this project functions as a kickstart to get you up and running faster. It contains the HTTP Client to be injected which will be sending requests using [HMAC](https://support.buckaroo.nl/categorieen/integratie/hmac). Also included is an example request to [GET a debtor](https://dev.buckaroo.nl/AdditionalServices/Description/creditmanagement#debtorinfo) (a.k.a. customer).

During development, it was noticed that many requirements and features were not or incompletely documented. It is therefore adviced to seek (technical) support at Buckaroo when a feature or implementation you are looking for is missing from the documentation.

Documentation can be found here:
- [Plaza](https://plaza.buckaroo.nl/)
   _Portal from which to access your created data by GUI_
- [Dev Documentation](https://dev.buckaroo.nl/)
   _Code examples, sandbox_
- [Support](https://support.buckaroo.nl/)
   _Functional support pages, FAQ_
