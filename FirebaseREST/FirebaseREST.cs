using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FirebaseREST
{
    public static class FirebaseREST
    {
        private static HttpClient signinClient = new HttpClient();
        private static HttpClient client = new HttpClient();

        private static string databaseURL; // = "https://greenhouse-test-258114.firebaseio.com/";
        private static string storageURL;

        private static string signinURL = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword";

        private static string idToken;

        static FirebaseREST()
        {
            signinClient.BaseAddress = new Uri(signinURL);
            signinClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// This property sets the database URL if not set yet, otherwise throws exception; cannot be set to null
        /// </summary>
        public static string DatabaseURL
        {
            set
            {
                if (value == null) throw new ArgumentNullException("Database URL cannot be null");
                if (databaseURL == null)
                {
                    databaseURL = value;
                    client.BaseAddress = new Uri(databaseURL);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                else
                {
                    throw new Exception("Database URL has already been set");
                }
            }
        }

        /// <summary>
        /// This property sets the storage URL if not set yet, otherwise throws exception; cannot be set to null
        /// </summary>
        public static string StorageURL
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Storage URL cannot be null");
                }
                if (storageURL == null)
                {
                    storageURL = value;
                }
                else
                {
                    throw new Exception("Storage URL has already been set");
                }
            }
        }

        public static async Task<bool> Login(string email, string password, string authCredential)
        {
            HttpResponseMessage response = await signinClient.PostAsync($"key={authCredential}", new StringContent(new UserAuth(email, password).ToString()));
            
            if (response.IsSuccessStatusCode)
            {
                idToken = JsonConvert.DeserializeObject<FirebaseToken>(await response.Content.ReadAsStringAsync()).idToken;
                return true;
            }
            return false;
        }

        /// <summary>
        /// This function puts data in the child in the database; return true if succeeded otherwise false
        /// </summary>
        /// <param name="child">the child of the database</param>
        /// <param name="data">the data to put</param>
        /// <returns>true if succeeded otherwise false</returns>
        public static async Task<bool> PutDatbaseData(string child, string data)
        {
            HttpResponseMessage response = await client.PutAsync($"{child}.json?auth={idToken}", new StringContent(data));

            return response.StatusCode switch {
                HttpStatusCode.OK => true,
                HttpStatusCode.Unauthorized => throw new DatabaseException("You are not authorized to the database"),
                _ => false
            };
        }

        /// <summary>
        /// This function returns the data from the child parameter in the database
        /// </summary>
        /// <param name="child">the child of the database to read from</param>
        /// <returns>the data in the child of the database</returns
        public static async Task<string> GetDatabaseData(string child)
        {
            HttpResponseMessage response = await client.GetAsync($"{child}.json?auth={idToken}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new DatabaseException("You are not authorized to the database");
            }
            else
            {
                throw new DatabaseException("Unable to read from database");
            }
        }

        /// <summary>
        /// This function deletes the data in the child in the database
        /// </summary>
        /// <param name="child">the child in the database to delete</param>
        /// <returns></returns>
        public static async Task DeleteDatabaseData(string child)
        {
            await client.DeleteAsync($"{child}.json?auth={idToken}");
        }
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
        public bool returnSecureToken = true;

        public UserAuth(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
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
