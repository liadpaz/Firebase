using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using static Firebase.Firestore.Document;

namespace Firebase {
#pragma warning disable IDE0044, IDE0051 , IDE0052, 169, 414, 649
    #region Firebase

    #region General Firebase

    /// <summary>
    /// This class is for the general Firebase
    /// </summary>
    public sealed class Firebase {
        private static Firebase firebase = null;
        private static FirebaseAuth auth = null;
        private static FirebaseDatabase database = null;
        private static Firestore firestore = null;

        internal static string ProjectId {
            private set;
            get;
        }
        internal static string ApiKey {
            private set;
            get;
        } = null;

        /// <summary>
        /// This function initializes the only Firebase instance on the app
        /// </summary>
        /// <param name="projectId">the Firebase project name</param>
        /// <param name="apiKey">the Web Api for the Firebase project</param>
        /// <returns>the only Firebase instance on the app</returns>
        public static Firebase InitializeFirebase(string projectId, string apiKey) {
            if (firebase == null) {
                ProjectId = projectId;
                ApiKey = apiKey;
                auth = new FirebaseAuth(apiKey);
                database = new FirebaseDatabase();
                firestore = new Firestore();
                firebase = new Firebase();
            }
            return firebase;
        }

        /// <summary>
        /// This function returns the only Firebase instance on the app
        /// </summary>
        /// <returns>the only Firebase instance on the app</returns>
        public static Firebase GetInstance() => firebase;

        /// <summary>
        /// Private constructor so you won't be able to create new instance from outside the class
        /// </summary>
        private Firebase() { }

        /// <summary>
        /// This function returns the only instance of the Firebase Real-Time Database
        /// </summary>
        public FirebaseDatabase FirebaseDatabase => database;

        /// <summary>
        /// This function returns the only instance of the Firebase Authentication
        /// </summary>
        public FirebaseAuth FirebaseAuth => auth;

        /// <summary>
        /// This function
        /// </summary>
        public Firestore Firestore => firestore;
    }

    #endregion General Firebase

    #region Real-Time Database

    /// <summary>
    /// This class is for the Firebase Real-Time Database
    /// </summary>
    public sealed class FirebaseDatabase {
        /// <summary>
        /// This function returns the reference to the Firebase Database at the <code>child</code> child
        /// </summary>
        /// <exception cref="ArgumentNullException">thrown if <code>child</code> is null or empty</exception>
        /// <param name="child">the child</param>
        /// <returns>the reference to the Firebase Database at the <code>child</code> child</returns>
        public DatabaseReference GetReference(string child) {
            if (string.IsNullOrEmpty(child)) {
                throw new ArgumentNullException("Child cannot be null or empty", nameof(child));
            }

            return new DatabaseReference($"{child}");
        }

        /// <summary>
        /// This function returns the reference to the root of the Firebase Database
        /// </summary>
        /// <returns>the reference to the root of the Firebase Database</returns>
        public DatabaseReference GetReference() => new DatabaseReference();
    }

    /// <summary>
    /// This class is for the Firebase Real-Time Database reference
    /// </summary>
    public sealed class DatabaseReference {
        private static readonly HttpClient client = new HttpClient();

        private readonly string child;

        /// <summary>
        /// Internal constructor that can only be called from within this library, sets the child of the database reference
        /// </summary>
        /// <param name="child">the databaseURL</param>
        internal DatabaseReference(string child) {
            this.child = child;
            if (client.BaseAddress == null) {
                client.BaseAddress = new Uri($"https://{Firebase.ProjectId}.firebaseio.com/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        /// <summary>
        /// Internal constructor that can only be called from within this library
        /// </summary>
        internal DatabaseReference() {
            child = string.Empty;
            if (client.BaseAddress == null) {
                client.BaseAddress = new Uri($"https://{Firebase.ProjectId}.firebaseio.com/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        /// <summary>
        /// This function returns the data from the database reference
        /// </summary>
        /// <returns>the data in the database</returns
        public async Task<string> Read() {
            HttpResponseMessage response = await client.GetAsync($"{child}.json?print=pretty&auth={FirebaseAuth.IdToken}");

            return response.StatusCode switch
            {
                HttpStatusCode.OK => await response.Content.ReadAsStringAsync(),
                _ => null
            };
        }

        /// <summary>
        /// This function writes data in the database reference; return true if succeeded otherwise false
        /// </summary>
        /// <exception cref="DatabaseAuthException">thrown when the user is not permitted</exception>
        /// <param name="data">the data to write</param>
        /// <returns>true if succeeded otherwise false</returns>
        public async Task<bool> Write(string data) {
            HttpResponseMessage response = await client.PutAsync($"{child}.json?auth={FirebaseAuth.IdToken}", new StringContent(data));

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                _ => false
            };
        }

        /// <summary>
        /// This function deletes the data in the child in the database
        /// </summary>
        /// <exception cref="DatabaseAuthException">throw when the user is not permitted</exception>
        /// <returns>true if succeeded otherwise false</returns>
        public async Task<bool> Delete() {
            HttpResponseMessage response = await client.DeleteAsync($"{child}.json?auth={FirebaseAuth.IdToken}");

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                _ => false
            };
        }

        /// <summary>
        /// This function returns a database reference to the <c>child</c> of the current reference
        /// </summary>
        /// <param name="child">the child</param>
        /// <returns>database reference to <c>child</c> of the current reference</returns>
        public DatabaseReference Child(string child) => new DatabaseReference($"{this.child}/{child}");

        /// <summary>
        /// This function returns the reference to the root of the database
        /// </summary>
        /// <returns>the reference to the root of the database</returns>
        public DatabaseReference Root => new DatabaseReference();

        /// <summary>
        /// This function returns the reference to the parent of the current reference
        /// </summary>
        /// <returns>the reference to the parent of the current reference</returns>
        public DatabaseReference GetParent() => new DatabaseReference(child.Substring(0, child.Length - child.LastIndexOf('/')));

        /// <summary>
        /// This fuction returns the database reference as string, it shows the path of the reference
        /// </summary>
        /// <returns>the database reference as string</returns>
        public override string ToString() => $"{client.BaseAddress}{child}";
    }

    #endregion Real-Time Database

    #region Firestore

    public sealed class Firestore {

        internal readonly static Regex documentRegex = new Regex("^[^/]*((([/][^/]*){1})(([/][^/]*){2})*){1}[^/]$");
        internal readonly static Regex collectionRegex = new Regex("^[^/]+(([/][^/]+){2})*$");

        internal static readonly HttpClient client = new HttpClient();

        internal Firestore() {
            if (client.BaseAddress == null) {
                client.BaseAddress = new Uri($"https://firestore.googleapis.com/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        public CollectionReference GetCollectionReference(string path = null) {
            if (string.IsNullOrEmpty(path)) {
                return new CollectionReference();
            }
            if (collectionRegex.IsMatch(path)) {
                return new CollectionReference(path);
            }
            throw new ArgumentException("Invalid path to collection", nameof(path));
        }

        public DocumentReference GetDocumentReference(string path = null) {
            if (documentRegex.IsMatch(path)) {
                return new DocumentReference(path);
            }
            throw new ArgumentException("Invalid path to document", nameof(path));
        }

        /// <summary>
        /// This function creates a query string from the queries entries
        /// </summary>
        /// <param name="queries">the queries to add to the string</param>
        /// <returns>the query string</returns>
        private static string CreateQueryString(params (string key, object value)[] queries) {
            IEnumerable<string> query = from value in queries
                                        where value.value != null
                                        select $"{value.key}={value.value}";
            return query.ToArray().Length != 0 ? $"?{string.Join("&", query)}" : null;
        }

        /// <summary>
        /// This function trims the path
        /// </summary>
        /// <param name="path">the path</param>
        /// <returns>the trimmed path</returns>
        private static string TrimPath(string path) => string.IsNullOrEmpty(path) ? null : $"/{path}";

        /// <summary>
        /// This function trims the last path piece
        /// </summary>
        /// <param name="path">the path to trim</param>
        /// <returns>the path w/o it's last piece</returns>
        private static string TrimLastPathPiece(string path) => path.Substring(0, path.LastIndexOf("/"));

        /// <summary>
        /// This class is for Firestore collection reference
        /// </summary>
        public sealed class CollectionReference {

            public string Path { get; }

            internal CollectionReference(string path = null) => Path = path;

            private CollectionReference() { }

            /// <summary>
            /// This function returns the Id of the current collection
            /// </summary>
            /// <returns>the Id of the current collection</returns>
            public string GetId() => Path.Substring(Path.LastIndexOf("/") + 1);

            /// <summary>
            /// This function creates a document in the Firestore database
            /// </summary>
            /// <param name="documentName">name of the document, can be null</param>
            /// <param name="document">the document to create</param>
            /// <returns>the document created</returns>
            public async Task<Document> CreateDocument(string documentName, Document document) {
                HttpResponseMessage response = await client.PostAsync($"v1/projects/{Firebase.ProjectId}/databases/(default)/documents{TrimPath(Path)}/{(string.IsNullOrWhiteSpace(documentName) ? null : $"?documentId={documentName}")}", new StringContent(document.ToString()));

                return JsonConvert.DeserializeObject<Document>(await response.Content.ReadAsStringAsync());
            }

            /// <summary>
            /// This function returns the reference to the document in the specified path
            /// </summary>
            /// <param name="path">the path to the document</param>
            /// <exception cref="ArgumentException">if the path doesn't match a path to a document</exception>
            /// <returns>the reference to the document in the specified path</returns>
            public DocumentReference GetDocumentReference(string path) {
                if (documentRegex.IsMatch($"{Path}/{path}")) {
                    return new DocumentReference($"{Path}/{path}");
                }
                throw new ArgumentException("Invalid path to document", nameof(path));
            }

            /// <summary>
            /// This function returns the <code>DocumentReference</code> parent of the current document reference
            /// </summary>
            /// <returns>the <code>DocumentReference</code> parent of the current document reference</returns>
            public DocumentReference GetParent() => new DocumentReference(TrimLastPathPiece(Path));

            /// <summary>
            /// This function gets the list of the documents in the current path
            /// </summary>
            /// <param name="collectionId">the collection, can be null</param>
            /// <param name="pageSize">max pages to get, can be null</param>
            /// <param name="pageToken">the last page token retrieved from the last fetch, can be null</param>
            /// <param name="orderBy"></param>
            /// <returns>list of the documents in the path</returns>
            public async Task<DocumentList> ListDocuments(string collectionId = null, int? pageSize = null, string pageToken = null, string orderBy = null) {
                string query = CreateQueryString(("pageSize", pageSize), ("pageToken", pageToken), ("orderBy", orderBy));
                HttpResponseMessage response = await client.GetAsync($"v1/projects/{Firebase.ProjectId}/databases/(default)/documents{TrimPath(Path)}/{collectionId}{query}");

                return JsonConvert.DeserializeObject<DocumentList>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// This class is for Firestore document reference
        /// </summary>
        public sealed class DocumentReference {

            public string Path { get; }

            internal DocumentReference(string path = null) => Path = path;

            private DocumentReference() { }

            /// <summary>
            /// This function returns the Id of the current document
            /// </summary>
            /// <returns>the Id of the current document</returns>
            public string GetId() => Path.Substring(Path.LastIndexOf("/") + 1);

            /// <summary>
            /// This function returns the reference to the sub collection in the path specified
            /// </summary>
            /// <param name="path">the path of the collection</param>
            /// <exception cref="ArgumentException">if the path doesn't match a path to a collection</exception>
            /// <returns>the reference to the collection in the path</returns>
            public CollectionReference Collection(string path) {
                if (collectionRegex.IsMatch($"{Path}/{path}")) {
                    return new CollectionReference($"{Path}/{path}");
                }
                throw new ArgumentException("Invalid path to collection", nameof(path));
            }

            /// <summary>
            /// This function deletes the current document from Firestore database
            /// </summary>
            /// <returns><see langword="true"/> if the document deleted, otherwise <see langword="false"/></returns>
            public async Task<bool> DeleteDocument() {
                HttpResponseMessage response = await client.DeleteAsync($"v1/projects/{Firebase.ProjectId}/databases/(default)/documents{TrimPath(Path)}");

                return response.IsSuccessStatusCode;
            }

            /// <summary>
            /// This function updates the current document from the database
            /// </summary>
            /// <returns>true if succeeded, otherwise false</returns>
            public async Task<Document> Get() {
                HttpResponseMessage response = await client.GetAsync($"v1/projects/{Firebase.ProjectId}/databases/(default)/documents{TrimPath(Path)}");

                return JsonConvert.DeserializeObject<Document>(await response.Content.ReadAsStringAsync());
            }

            /// <summary>
            /// This function returns the <code>CollectionReference</code> parent of the current document reference
            /// </summary>
            /// <returns>the <code>CollectionReference</code> parent of the current document reference</returns>
            public CollectionReference GetParent() => new CollectionReference(TrimLastPathPiece(Path));

            /// <summary>
            /// This function sets the content of the current document to the specified document, it overrides any content that pre-existed
            /// </summary>
            /// <param name="document">the document to set to</param>
            /// <returns>the document</returns>
            public async Task<Document> Set(Document document) => (await DeleteDocument()) ? await Update(document) : null;

            /// <summary>
            /// This function update the content of the current document to the specified document
            /// </summary>
            /// <param name="document">the document to update to</param>
            /// <param name="currentDocument">condition of the current state of the document</param>
            /// <returns>the updated document</returns>
            public async Task<Document> Update(Document document, Precondition currentDocument = null) {
                string query = CreateQueryString(currentDocument != null ? currentDocument.GetQuery() : (null, null));
                HttpResponseMessage response = await client.PatchAsync($"v1/projects/{Firebase.ProjectId}/databases/(default)/documents{TrimPath(Path)}{query}", new StringContent(document.ToString()));

                return JsonConvert.DeserializeObject<Document>(await response.Content.ReadAsStringAsync());
            }

            /// <summary>
            /// This function gets the list of collections in the current document
            /// </summary>
            /// <param name="pageSize">the max number of collections to show</param>
            /// <param name="pageToken">the page token aqquired from this function response</param>
            /// <returns>the list of collections in the path</returns>
            public async Task<CollectionIds> ListCollections(int? pageSize = null, string pageToken = null) {
                HttpResponseMessage response = await client.PostAsync($"v1/projects/{Firebase.ProjectId}/databases/(default)/documents{TrimPath(Path)}:listCollectionIds", new StringContent(new CollectionListIdContent(pageSize, pageToken).ToString()));

                return JsonConvert.DeserializeObject<CollectionIds>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// This class is for Firestore document
        /// </summary>
        public sealed class Document {
            [JsonProperty]
            private string name;
            [JsonProperty]
            private Dictionary<string, Value> fields;
            [JsonProperty]
            private string createTime;
            [JsonProperty]
            private string updateTime;

            [JsonIgnore]
            public Dictionary<string, Value> Fields => fields;

            private Document() { }

            internal Document(Dictionary<string, Value> fields = null) => this.fields = fields ?? new Dictionary<string, Value>();

            [JsonIgnore]
            public string Name => name.Substring(name.LastIndexOf("/") + 1);

            public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

            public sealed class Value {
                public enum Type {
                    None,
                    Boolean,
                    Integer,
                    Double,
                    Timestamp,
                    String,
                    Bytes,
                    Reference,
                    GeoPoint,
                    Array,
                    Map
                }

                [JsonIgnore]
                private Type type;
                [JsonProperty("booleanValue")]
                private bool? BooleanValue;
                [JsonProperty("integerValue")]
                private string IntegerValue;
                [JsonProperty("doubleValue")]
                private double? DoubleValue;
                [JsonProperty("timestampValue")]
                private string TimestampValue;
                [JsonProperty("stringValue")]
                private string StringValue;
                [JsonProperty("bytesValue")]
                private string BytesValue;
                [JsonProperty("referenceValue")]
                private string ReferenceValue;
                [JsonProperty("geoPointValue")]
                private LatLon GeoPointValue;
                [JsonProperty("arrayValue")]
                private ArrayValueType ArrayValue;
                [JsonProperty("mapValue")]
                private MapValueType MapValue;

                private Value() { }

                public Value(Type type, object value) {
                    this.type = type;
                    switch (type) {
                        case Type.Boolean: {
                            if (value is bool b) {
                                BooleanValue = b;
                            } else {
                                throw new ArgumentException("Entered non-boolean value for a boolean type value", nameof(value));
                            }
                            break;
                        }

                        case Type.Integer: {
                            if (int.TryParse(value.ToString(), out int ignoredInt)) {
                                IntegerValue = value.ToString();
                            } else {
                                throw new ArgumentException("Entered non-integer value for a integer type value", nameof(value));
                            }
                            break;
                        }

                        case Type.Double: {
                            if (double.TryParse(value.ToString(), out double d)) {
                                DoubleValue = d;
                            } else {
                                throw new ArgumentException("Entered non-double value for a double type value", nameof(value));
                            }
                            break;
                        }

                        case Type.Timestamp: {
                            if (value is DateTime date) {
                                TimestampValue = TimeZoneInfo.ConvertTimeToUtc(date).ToString();
                            } else {
                                throw new ArgumentException("Entered non-timestamp value for a timestamp type value", nameof(value));
                            }
                            break;
                        }

                        case Type.String: {
                            StringValue = value.ToString();
                            break;
                        }

                        case Type.Bytes: {
                            try {
                                BytesValue = Convert.ToBase64String(Encoding.Unicode.GetBytes(value.ToString()));
                            } catch {
                                throw new ArgumentException("Entered non-bytes value for a bytes type value", nameof(value));
                            }
                            break;
                        }

                        case Type.Reference: {
                            ReferenceValue = value.ToString();
                            break;
                        }

                        case Type.GeoPoint: {
                            if (value is LatLon latlon) {
                                GeoPointValue = latlon;
                            } else {
                                throw new ArgumentException("Entered non-geo-point value for a geo-point type value", nameof(value));
                            }
                            break;
                        }

                        case Type.Array: {
                            if (value is List<Value> array) {
                                ArrayValue = new ArrayValueType(array);
                            } else {
                                throw new ArgumentException("Entered non-array value for an array type value", nameof(value));
                            }
                            break;
                        }

                        case Type.Map: {
                            if (value is Dictionary<string, Value> map) {
                                MapValue = new MapValueType(map);
                            } else {
                                throw new ArgumentException("Entered non-map value for a map type value", nameof(value));
                            }
                            break;
                        }
                    }
                }

                /// <summary>
                /// This function adds a value to the current value whether it is an array or a map
                /// </summary>
                /// <exception cref="NullReferenceException">if the current value is nor array niether map</exception>
                /// <param name="value">the value to add</param>
                /// <param name="name">the name of the value, only if the current value is a map</param>
                public void AddValue(Value value, string name = null) {
                    if (ArrayValue != null) {
                        ArrayValue.AddValue(value);
                    } else if (MapValue != null) {
                        MapValue.AddValue(name, value);
                    } else {
                        throw new NullReferenceException("Type of this value is nor array niether map");
                    }
                }

                public bool ShouldSerializeBooleanValue() => type == Type.Boolean || BooleanValue != null;
                public bool ShouldSerializeIntegerValue() => type == Type.Integer || IntegerValue != null;
                public bool ShouldSerializeDoubleValue() => type == Type.Double || DoubleValue != null;
                public bool ShouldSerializeTimestampValue() => type == Type.Timestamp || TimestampValue != null;
                public bool ShouldSerializeStringValue() => type == Type.String || StringValue != null;
                public bool ShouldSerializeBytesValue() => type == Type.Bytes || BytesValue != null;
                public bool ShouldSerializeReferenceValue() => type == Type.Reference || ReferenceValue != null;
                public bool ShouldSerializeGeoPointValue() => type == Type.GeoPoint || GeoPointValue != null;
                public bool ShouldSerializeArrayValue() => type == Type.Array || ArrayValue != null;
                public bool ShouldSerializeMapValue() => type == Type.Map || MapValue != null;

                public class LatLon {
                    [JsonProperty]
                    private double latitude;
                    [JsonProperty]
                    private double longitude;

                    public LatLon(double lat, double lon) => (latitude, longitude) = (lat, lon);
                }

                public class MapValueType {
                    [JsonProperty]
                    private Dictionary<string, Value> fields;

                    public MapValueType(Dictionary<string, Value> fields = null) => this.fields = fields ?? new Dictionary<string, Value>();

                    /// <summary>
                    /// This function adds an entry to the dictionary
                    /// </summary>
                    /// <param name="key">the key of the entry</param>
                    /// <param name="value">the value of the entry</param>
                    public void AddValue(string key, Value value) => fields.Add(key, value);

                    public override string ToString() => JsonConvert.SerializeObject(this);
                }

                public class ArrayValueType {
                    [JsonProperty]
                    private List<Value> values;

                    public ArrayValueType(List<Value> values = null) => this.values = values ?? new List<Value>();

                    /// <summary>
                    /// This function adds an item to the array
                    /// </summary>
                    /// <param name="item">the item to add</param>
                    public void AddValue(Value item) => values.Add(item);

                    public override string ToString() => JsonConvert.SerializeObject(this);
                }
            }
        }

        /// <summary>
        /// This class is to build a document to upload to Firestore
        /// </summary>
        public class DocumentBuilder {
            private Document document;

            public DocumentBuilder(Dictionary<string, Value> fields) => document = new Document(fields);
            public DocumentBuilder() => document = new Document();

            /// <summary>
            /// This function adds a value to the current document builder
            /// </summary>
            /// <param name="key">the name of the value</param>
            /// <param name="value">the value to add</param>
            /// <returns>the current instance of document builder with the field added</returns>
            public DocumentBuilder AddField(string key, Value value) {
                document.Fields.Add(key, value);
                return this;
            }

            /// <summary>
            /// This function adds values to the current document builder
            /// </summary>
            /// <param name="fields">array of values to add</param>
            /// <returns>the current instance of document builder with the fields added</returns>
            public DocumentBuilder AddFields(params (string key, Value value)[] fields) {
                foreach ((string key, Value value) in fields) {
                    document.Fields.Add(key, value);
                }
                return this;
            }

            public Dictionary<string, Value> Fields => document.Fields;

            public void Clear() => document.Fields.Clear();

            /// <summary>
            /// This function builds the document
            /// </summary>
            /// <returns>the built document</returns>
            public Document Build() => document;
        }

        public sealed class DocumentMask {
            [JsonProperty]
            private readonly List<string> fieldPaths;

            public DocumentMask(List<string> fieldPaths = null) => this.fieldPaths = fieldPaths;

            public override string ToString() => JsonConvert.SerializeObject(this);
        }

        public sealed class DocumentList {
            [JsonProperty]
            private List<Document> documents;
            [JsonProperty]
            private string nextPageToken;

            private DocumentList() { }

            [JsonIgnore]
            public List<Document> Documents => documents;
            [JsonIgnore]
            public string NextPageToken => nextPageToken;
        }

        public sealed class CollectionListIdContent {
            [JsonProperty("pageSize")]
            private int? PageSize;
            [JsonProperty("pageToken")]
            private string PageToken;

            public CollectionListIdContent(int? pageSize = null, string pageToken = null) {
                PageSize = pageSize;
                PageToken = pageToken;
            }

            public bool ShouldSerializePageSize() => PageSize != null;
            public bool ShouldSerializePageToken() => PageToken != null;

            public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public sealed class CollectionIds {
            [JsonProperty]
            private List<string> collectionIds;
            [JsonProperty]
            private string nextPageInfo;

            [JsonIgnore]
            public List<string> GetCollectionIds => collectionIds;
            [JsonIgnore]
            public string NextPageInfo => nextPageInfo;

            public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public sealed class Precondition {
            private bool? exists = null;
            private string updateTime = null;

            public Precondition(bool exists) => this.exists = exists;
            public Precondition(string updateTime) => this.updateTime = updateTime;

            public (string, object) GetQuery() => exists != null ? ((string, object))("exists", exists) : ("updateTime", updateTime);
        }
    }

    #endregion Firestore

    #region Authentication

    /// <summary>
    /// This class is for the Firebase authentication
    /// </summary>
    public sealed class FirebaseAuth {

        private static readonly HttpClient identityClient = new HttpClient {
            BaseAddress = new Uri("https://identitytoolkit.googleapis.com")
        };
        private static readonly HttpClient tokenClient = new HttpClient {
            BaseAddress = new Uri("https://securetoken.googleapis.com")
        };

        internal static string IdToken {
            private set;
            get;
        }
        private readonly string apiKey;
        private FirebaseUser firebaseUser;
        public static bool IsLoggedIn {
            private set;
            get;
        } = false;

        internal FirebaseAuth(string apiKey) {
            identityClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.apiKey = apiKey;
        }

        /// <summary>
        /// This function is used to sign in user to Firebase
        /// </summary>
        /// <param name="email">the user's email</param>
        /// <param name="password">the user's password</param>
        /// <returns>true if successfully signed in, otherwise false</returns>
        public async Task<bool> SignInWithPasswordEmail(string email, string password) {
            HttpResponseMessage response = await identityClient.PostAsync($"v1/accounts:signInWithPassword?key={apiKey}", new StringContent(new UserAuth(email, password).ToString()));
            if (response.IsSuccessStatusCode) {
                firebaseUser = JsonConvert.DeserializeObject<FirebaseUser>(await response.Content.ReadAsStringAsync());
                IdToken = firebaseUser.idToken;
                IsLoggedIn = true;
                Firestore.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IdToken);
                return true;
            }
            IsLoggedIn = false;
            return false;
        }

        /// <summary>
        /// This function is used to sign up user to Firebase
        /// </summary>
        /// <param name="email">the user's email</param>
        /// <param name="password">the user's password</param>
        /// <returns>true if successfully signed up the user, otherwise false</returns>
        public async Task<FirebaseUser> SignUp(string email, string password) {
            HttpResponseMessage response = await identityClient.PostAsync($"v1/accounts:signUp?key={apiKey}", new StringContent(new UserAuth(email, password).ToString()));
            Console.WriteLine(JsonConvert.DeserializeObject<FirebaseUser>(await response.Content.ReadAsStringAsync()));
            return response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<FirebaseUser>(await response.Content.ReadAsStringAsync()) : null;
        }

        /// <summary>
        /// This function send password reset email to a Firebase user
        /// </summary>
        /// <param name="email">the user's email</param>
        /// <returns></returns>
        public async Task<bool> SendPasswordResetEmail(string email) {
            HttpResponseMessage response = await identityClient.PostAsync($"v1/accounts:sendOobCode?key={apiKey}", new StringContent("{" + $"\"requestType\":\"PASSWORD_RESET\",\"email\":\"{email}\"" + "}"));

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// This function is used to sign in user with a refresh token granted from signin up a user or signed in user
        /// </summary>
        /// <param name="token">the refresh token</param>
        /// <returns>true if succeded, otherwise false</returns>
        public async Task<bool> SignInWithToken(string token) {
            HttpResponseMessage response = await tokenClient.PostAsync($"v1/token?key={apiKey}", new StringContent(JsonConvert.SerializeObject(new { grant_type = "refresh_token", refreshToken = token })));
            if (response.IsSuccessStatusCode) {
                firebaseUser = JsonConvert.DeserializeObject<FirebaseUser>(await response.Content.ReadAsStringAsync());
                IdToken = firebaseUser.idToken;
                IsLoggedIn = true;
                Firestore.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IdToken);
                return true;
            }
            IsLoggedIn = false;
            return false;
        }

        /// <summary>
        /// This function returns the current user
        /// </summary>
        /// <returns>the current user</returns>
        public FirebaseUser GetCurrentUser() => firebaseUser;

        /// <summary>
        /// This class if for authenticating a firebase user (token)
        /// </summary>
        internal class UserAuth {
            [JsonProperty]
            private string email;
            [JsonProperty]
            private string password;
            [JsonProperty]
            private bool returnSecureToken = true;

            public UserAuth(string email, string password) {
                this.email = email;
                this.password = password;
            }

            public override string ToString() => JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// This class is for a firebase user details
        /// </summary>
        public class FirebaseUser {
            [JsonProperty]
            private string kind;
            public string localId;
            public string email;
            public string displayName;
            [JsonProperty]
            internal string idToken;
            [JsonProperty]
            private bool registered;
            public string refreshToken;
            [JsonProperty]
            private string expiresIn;

            public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public class AuthException : Exception {
            public AuthException(string message) : base(message) { }
        }
    }

    #endregion Authentication

    #endregion Firebase
#pragma warning restore IDE0044, IDE0051 , IDE0052, 169, 414, 649
}
