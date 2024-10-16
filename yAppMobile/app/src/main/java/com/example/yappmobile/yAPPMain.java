package com.example.yappmobile;

import android.app.Application;
import android.util.Log;

import com.amplifyframework.auth.cognito.AWSCognitoAuthPlugin;
import com.amplifyframework.core.Amplify;
import com.amplifyframework.api.aws.AWSApiPlugin;
import com.amplifyframework.AmplifyException;

public class yAPPMain extends Application {
    @Override
    public void onCreate() {
        super.onCreate();

        try {
            // Initialize plugins
            Amplify.addPlugin(new AWSApiPlugin());
            Amplify.addPlugin(new AWSCognitoAuthPlugin());
            Amplify.configure(getApplicationContext());
            Log.i("AmplifyConfig", Amplify.API.getPlugin("awsAPIPlugin").toString());
            Log.i("AmplifyConfig",Amplify.Auth.getPlugin("awsCognitoAuthPlugin").toString());
            Log.i("AmplifyInit", "Initialized Amplify");
        } catch (AmplifyException error) {
            System.out.println("Could not initialize Amplify." + error);
        }
    }
}
