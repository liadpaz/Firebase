This is a client library for Firebase in C# 8.0 in .NET Core 3.1.

# Usage

## General Firebase
Initialize firebase with the `Firebase.InitializeFirebase(string projectId, string apiKey)` method with your project id and WebApi as parameters.

After this you can get the firebase instance with the `Firebase.GetInstance()` method.

Any usage of firebase from this library should be from the `Firebase` instance you get from `InitializeFirebase` or `GetInstance` methods. 

## Authentication
If you need to register a user you can use the `FirebaseAuth.SignUpAsync(string email, string password)` method with the desired users email and password.

If you already registered and want to authenticate a user, you can use the `FirebaseAuth.SignInWithPasswordEmailAsync(string email, string password)` method with the users email and password as parameters.
Or if you have already authenticated a user and you want to reauthenticate with a refresh token you can use the `FirebaseAuth.SignInWithTokenAsync(string token)` method with the refresh token you got from signing in.

You can get the current signed user information with the `FirebaseAuth.GetCurrentUser()` method.

You can sign out with the `FirebaseAuth.Signout()` method.

## Real-Time Database
First you need to get a `FirebaseDatabase` instance, and then call the `GetReference(string child)` method to get a reference to a place in the database.

To read data use the `DatabaseReference.ReadAync()` method to get a json representation of the data in the database in the reference location, or you can use the `DatabaseReference.ReadAync<T>()` to cast the data to a desired type (may throw an exception if can't cast).

To write data use the `DatabaseReference.WriteAsync(object data)` method to write data in the database in the reference location or use the `DatabaseReference.WriteAsync(string data)` if you have the json representation of the data you need to write.

To delete data use the `DatabaseReference.DeleteAync()` method.

You can get to the root reference of your database with the `DatabaseReference.Root`, and get to a child reference with the `DatabaseReference.Child(string child)` method and to the parent with the `DatabaseReference.GetParent()` method.
