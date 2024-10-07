package com.example.yappmobile;

import android.app.Application;
import android.util.Log;
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
            Amplify.configure(getApplicationContext());
            Log.i("AmplifyInit", "Initialized Amplify");
        } catch (AmplifyException error) {
            System.out.println("Could not initialize Amplify." + error);
        }
    }
}
