# ShareYourCAD - CAD model sharing web app using CAD Exchanger SDK

This is an example web application using the [CAD Exchanger SDK](https://cadexchanger.com/products/sdk/) and [CAD Exchanger Web Toolkit](https://cadexchanger.com/products/sdk/add-ons/web-toolkit/) which works similarly to other file sharing services, but for CAD and 3D files. The user is able to upload a model and then get a unique link to it. The recipient of the shared model can view it in the browser before downloading the original file.

This repository accompanies a [post in CAD Exchanger blog](https://cadexchanger.com/blog/how-to-build-3d-web-apps-part-6-building-a-simple-3d-web-app/).

The code in this repository is provided under a permissive BSD License. You may insert their source code into your application and modify as needed.

## Requirements

The dependencies of this project are:

* .NET 6 SDK
* MongoDB
* MongoDB C# Driver
* CAD Exchanger SDK
* FluentScheduler

## Running

To use this example, first obtain the CAD Exchanger SDK evaluation [here](https://cadexchanger.com/products/sdk/try/). Upon filling out the form you'll receive an email with an evaluation license key `cadex_license.cs`.

1. Unpack the SDK package to the `cadexsdk` folder of this project
2. Place your evaluation license key in the repository root.
2. Install [.NET SDK 6](https://dotnet.microsoft.com/en-us/download).
3. Install [MongoDB](https://www.mongodb.com/try/download/community) locally and start a server listening on standard port (27017) or, alternatively launch a Docker container from an [official image](https://hub.docker.com/_/mongo).
4. Run the following command to install development SSL certificate (not needed for Linux):
    ```
    > dotnet dev-certs https --trust
    ```
5. Run `dotnet run` to build and run the web server.
    ```
    > dotnet run
    Building...
    info: Microsoft.Hosting.Lifetime[14]
        Now listening on: https://localhost:7153
    info: Microsoft.Hosting.Lifetime[14]
        Now listening on: http://localhost:5177
    info: Microsoft.Hosting.Lifetime[0]
        Application started. Press Ctrl+C to shut down.
    info: Microsoft.Hosting.Lifetime[0]
        Hosting environment: Development
    info: Microsoft.Hosting.Lifetime[0]
        Content root path: C:\path\to\project\share-your-cad\
    ```
6. Click the URL from command output (in this case `https://localhost:7153`) to open the app pagein the browser.

# Learn more

If you'd like to learn more about CAD Exchanger, visit our [website](https://cadexchanger.com/). If you have any questions, please reach out to us [here](https://cadexchanger.com/contact-us/).
