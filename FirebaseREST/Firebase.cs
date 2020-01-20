using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Firebase
{
    public sealed class Firebase
    {
        private static Firebase firebase = null;
        private static FirebaseAuth auth = null;
        private static FirebaseDatabase database = null;

        internal static string projectName;

        public static Firebase InitializeFirebase(string projectName, string authCredential)
        {
            if (firebase == null)
            {
                Firebase.projectName = projectName;
                auth = new FirebaseAuth(authCredential);
                database = new FirebaseDatabase();
                return firebase = new Firebase();
            }
            return firebase;
        }

        private Firebase() {}

        /// <summary>
        /// This function returns the only instance of the FirebaseDatabase
        /// </summary>
        /// <returns>the only instance of the FirebaseDatabase</returns>
        public FirebaseDatabase GetFirebaseDatabase()
        {
            return database;
        }

        /// <summary>
        /// This function returns the only instance of the FirebaseAuth
        /// </summary>
        /// <returns>the only instance of the FirebaseAuth</returns>
        public FirebaseAuth GetFirebaseAuth()
        {
            return auth;
        }
    }

    public sealed class FirebaseDatabase
    {
        public DatabaseReference GetReference(string child)
        {
            if (child == null) throw new ArgumentNullException("Child cannot be null");
            return new DatabaseReference($"{child}");
        }

        public DatabaseReference GetReference()
        {
            return new DatabaseReference();
        }
    }

    public sealed class DatabaseReference
    {
        private static HttpClient client = new HttpClient();

        string child;

        /// <summary>
        /// Internal constructor that can only be called from within this library, sets the databaseURL
        /// </summary>
        /// <param name="child">the databaseURL</param>
        internal DatabaseReference(string child)
        {
            this.child = child;
            if (Read().GetAwaiter().GetResult() is bool)
            {
                throw new DatabaseException("Child does not exist");
            }
            client.BaseAddress = new Uri($"https://{Firebase.projectName}.firebaseio.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        internal DatabaseReference()
        {
            child = string.Empty;
            client.BaseAddress = new Uri($"https://{Firebase.projectName}.firebaseio.com/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// This function returns the data from the database reference
        /// </summary>
        /// <returns>the data in the database</returns
        public async Task<dynamic> Read()
        {
            HttpResponseMessage response = await client.GetAsync($"{child}.json?auth={FirebaseAuth.IdToken}");
            Console.WriteLine(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            return response.StatusCode switch
            {
                HttpStatusCode.OK => await response.Content.ReadAsStringAsync(),
                HttpStatusCode.Unauthorized => throw new DatabaseException("You are not authorized to the database"),
                _ => false
            };
        }

        /// <summary>
        /// This function writes data in the database reference; return true if succeeded otherwise false
        /// </summary>
        /// <exception cref="DatabaseException">thrown when the user is not permitted</exception>
        /// <param name="data">the data to write</param>
        /// <returns>true if succeeded otherwise false</returns>
        public async Task<bool> Write(string data)
        {
            HttpResponseMessage response = await client.PutAsync($"{child}.json?auth={FirebaseAuth.IdToken}", new StringContent(data));

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.Unauthorized => throw new DatabaseException("You are not authorized to the database"),
                _ => false
            };
        }

        /// <summary>
        /// This function deletes the data in the child in the database
        /// </summary>
        /// <exception cref="DatabaseException">throw when the user is not permitted</exception>
        /// <returns>true if succeeded otherwise false</returns>
        public async Task<bool> Delete()
        {
            HttpResponseMessage response = await client.DeleteAsync($"{child}.json?auth={FirebaseAuth.IdToken}");

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.Unauthorized => throw new DatabaseException("You are not authorized to the database"),
                _ => false
            };
        }

        /// <summary>
        /// This function returns a database reference to the <c>child</c> of the current reference
        /// </summary>
        /// <param name="child">the child</param>
        /// <returns>database reference to <c>child</c> of the current reference</returns>
        public DatabaseReference GetChild(string child)
        {
            return new DatabaseReference($"{this.child}/{child}");
        }
    }

    /// <summary>
    /// Class for the Firebase authentication
    /// </summary>
    public sealed class FirebaseAuth
    {
        private static readonly string authURL = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword/";

        private static HttpClient authClient = new HttpClient()
        {
            BaseAddress = new Uri(authURL)
        };

        internal static string IdToken
        {
            private set;
            get;
        }
        private string authCredential;
        public static bool IsLoggedIn
        {
            private set;
            get;
        } = false;

        internal FirebaseAuth(string authCredential)
        {
            authClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.authCredential = authCredential;
        }

        public async Task<bool> SignIn(string email, string password)
        {
            HttpResponseMessage response = await authClient.PostAsync($"?key={authCredential}", new StringContent(new UserAuth(email, password).ToString()));

            if (response.IsSuccessStatusCode)
            {
                IdToken = JsonConvert.DeserializeObject<FirebaseToken>(await response.Content.ReadAsStringAsync()).idToken;
                Console.WriteLine(IdToken);
                IsLoggedIn = true;
                return true;
            }
            IsLoggedIn = false;
            return false;
        }

        //public async Task<bool> SignUp(string email, string password)
        //{

        //}
    }

    internal class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message)
        {
        }
    }

    internal class UserAuth
    {
        public string email;
        public string password;

        public UserAuth(string email, string password)
        {
            this.email = email;
            this.password = password;
        }

        public override string ToString()
        {
            return "{" + $"\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":\"true\"" + "}";
        }
    }

    internal class FirebaseToken
    {
        public string kind;
        public string localId;
        public string email;
        public string displayName;
        public string idToken;
        public string registered;
        public string refreshToken;
        public string expiresIn;
    }
}
