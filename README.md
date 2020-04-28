This is a client library for Firebase in C# 8.0 in .NET Core 3.1.

# Usage

## General Firebase
Initialize firebase with the `Firebase.InitializeFirebase(string projectId, string apiKey)` method with your project id and WebApi key as parameters.

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

## Firestore
Get a reference to a collection with the `GetCollectionReference(string path)` method and a reference to a document with the `GetDocumentReference(string path)` method of a firestore instance you have retrieved from a `Firebase` instance.

### CollectionReference
You can create a document in the collection with the `CollectionReference.CreateDocumentAsync(string documentName, Document document)` method to create a document with the document name and document as parameters. If you don't want to name the document you can use the `CollectionReference.CreateDocumentAsync(Document document)` method and Firestore will assign a random name to the document.

You can list the documents in the collection with the `CollectionReference.ListDocumentsAsync(string collectionId = null, int? pageSize = null, string pageToken = null, string orderBy = null)` method.

You can get the parent document of the collection with the `CollectionReference.GetParent()` method and get a document reference with the `CollectionReference.GetDocumentReference(string path)` method.

You can get the id of the collection with the `CollectionReference.GetId()` method.

### DocumentReference
You can get the document with the `DocumentReference.GetAsync()` method.

You can set (override) the document with the `DocumentReference.SetAsync(Document document)` method. Or you can update the document with the `DocumentReference.UpdateAsync(Document document, Precondition currentDocument = null)` method.

You can list the documents in the collection with the `DocumentReference.ListCollectionsAsync(int? pageSize = null, string pageToken = null)` method.

You can get a reference to a sub-collection with the `DocumentReference.GetAsync()` method, you can also get the name of the document with the `DocumentReference.GetName()`method. And you can get the parent collection with the `DocumentReference.GetParent()` method.
