# How to host WCF-service in Revit

This code shows, how to deploy WCF-service in Autodesk Revit and call it from external application.

This code initially was written by Victor Checkalin for [Revit 2013](https://github.com/chekalin-v/RevitExternalAccessDemo)

However it did not work in Revit 2019. There is a discusstion in [adn-cis](http://adn-cis.org/forum/index.php?topic=745.new;topicseen#new) forum. 

The latest changes make it to work again.

# Content

This solution contains 2 project:

* RevitExternalAccessDemo - an external application for Autodesk Revit 2019

* RevitExternalAccessClient -  client application (console app, that call WCF-service methods)

# Start WCF-service in Revit

You should start Revit as administrator or use the [ServiceModel Registration Tool](https://docs.microsoft.com/en-us/dotnet/framework/wcf/servicemodelreg-exe) to register WCF 