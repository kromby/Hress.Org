[![DeepSource](https://app.deepsource.com/gh/kromby/Hress.Org.svg/?label=active+issues&show_trend=true&token=VdBUWHkYb47h94MHDxuGLAuZ)](https://app.deepsource.com/gh/kromby/Hress.Org/)

This project was bootstrapped with [Create React App](https://github.com/facebook/create-react-app).

## Prerequisites

### .env file
In the root of the project a file with the name '.env' must be created with the following configuration values.

```
    REACT_APP_API_PATH=http://localhost:7072

    REACT_APP_LEGACY_API_PATH=https://ezhressapi.azurewebsites.net
    REACT_APP_LEGACY_API_CODE=ADD CODE HERE

    REACT_APP_IMAGE_PATH=https://ezcontentapi.azurewebsites.net

    REACT_APP_OMDB=ADD KEY HERE
```

### _http Folder
Files in _http folder are for the [REST client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).


In settings.json in VS Code add the following to setup environments.

```
    "rest-client.environmentVariables": {
        "$shared": {
        },
        "local": {
            "host": "http://localhost:7072",
            "apiCode": "",
        },
        "production": {
            "host": "Add the correct prod URL",
            "apiCode": "Add the correct prodCode",
        }
    }
```

## Available Scripts

In the project directory, you can run:

### `npm start`

Runs the app in the development mode.<br />
Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

The page will reload if you make edits.<br />
You will also see any lint errors in the console.

### `npm test`

Launches the test runner in the interactive watch mode.<br />
See the section about [running tests](https://facebook.github.io/create-react-app/docs/running-tests) for more information.

### `npm run build`

Builds the app for production to the `build` folder.<br />
It correctly bundles React in production mode and optimizes the build for the best performance.

The build is minified and the filenames include the hashes.<br />
Your app is ready to be deployed!

See the section about [deployment](https://facebook.github.io/create-react-app/docs/deployment) for more information.

### `npm run eject`

**Note: this is a one-way operation. Once you `eject`, you can’t go back!**

If you aren’t satisfied with the build tool and configuration choices, you can `eject` at any time. This command will remove the single build dependency from your project.

Instead, it will copy all the configuration files and the transitive dependencies (Webpack, Babel, ESLint, etc) right into your project so you have full control over them. All of the commands except `eject` will still work, but they will point to the copied scripts so you can tweak them. At this point you’re on your own.

You don’t have to ever use `eject`. The curated feature set is suitable for small and middle deployments, and you shouldn’t feel obligated to use this feature. However we understand that this tool wouldn’t be useful if you couldn’t customize it when you are ready for it.

## Learn More

You can learn more in the [Create React App documentation](https://facebook.github.io/create-react-app/docs/getting-started).

To learn React, check out the [React documentation](https://reactjs.org/).

### Code Splitting

This section has moved here: https://facebook.github.io/create-react-app/docs/code-splitting

### Analyzing the Bundle Size

This section has moved here: https://facebook.github.io/create-react-app/docs/analyzing-the-bundle-size

### Making a Progressive Web App

This section has moved here: https://facebook.github.io/create-react-app/docs/making-a-progressive-web-app

### Advanced Configuration

This section has moved here: https://facebook.github.io/create-react-app/docs/advanced-configuration

### Deployment

This section has moved here: https://facebook.github.io/create-react-app/docs/deployment

### `npm run build` fails to minify

This section has moved here: https://facebook.github.io/create-react-app/docs/troubleshooting#npm-run-build-fails-to-minify

## Knowledge base

### Error: error:0308010C:digital envelope routines::unsupported

You can try one of these:

1. Downgrade to Node.js v16.
   1. You can reinstall the current LTS version from Node.js’ website.
   2. You can also use nvm. For Windows, use nvm-windows.

2. Enable legacy OpenSSL provider.
   1. On Unix-like (Linux, macOS, Git bash, etc.):
        ```
        export NODE_OPTIONS=--openssl-legacy-provider
        ```
   2. On Windows command prompt:
        ```
        set NODE_OPTIONS=--openssl-legacy-provider
        ```
   3. On PowerShell:
        ``` 
        $env:NODE_OPTIONS = "--openssl-legacy-provider"
        ```
        
https://stackoverflow.com/questions/69692842/error-message-error0308010cdigital-envelope-routinesunsupported
