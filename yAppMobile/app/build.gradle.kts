plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

android {
    namespace = "com.example.yappmobile"
    compileSdk = 34

    defaultConfig {
        applicationId = "com.example.yappmobile"
        minSdk = 24
        targetSdk = 34
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        isCoreLibraryDesugaringEnabled = true
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }
    kotlinOptions {
        jvmTarget = "1.8"
    }
    buildFeatures {
        viewBinding = false
    }
}

dependencies {

    implementation("androidx.core:core-ktx:1.13.1")
    implementation("androidx.appcompat:appcompat:1.6.1")
    implementation("com.google.android.material:material:1.11.0")
    implementation("androidx.constraintlayout:constraintlayout:2.1.4")
    implementation("androidx.navigation:navigation-fragment-ktx:2.6.0")
    implementation("androidx.navigation:navigation-ui-ktx:2.6.0")

    // Amplify core dependency
    implementation("com.amplifyframework:aws-core:2.19.1")
    implementation("com.amplifyframework:aws-api:2.19.1")
    implementation("com.amplifyframework:aws-api-appsync:2.19.1")
    implementation("com.amplifyframework:aws-auth-cognito:2.19.1")
    implementation("com.amplifyframework.ui:authenticator:1.2.3")

    implementation ("androidx.browser:browser:1.5.0")

    coreLibraryDesugaring("com.android.tools:desugar_jdk_libs:1.1.5")
}