using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Firebase {
#pragma warning disable IDE0044, IDE0051, IDE0052, IDE0060, 169, 414, 649
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

        private readonly string apiKey;
        private static FirebaseUser firebaseUser;
        public static bool IsLoggedIn => firebaseUser != null;

        internal FirebaseAuth(string apiKey) {
            if (string.IsNullOrEmpty(apiKey)) {
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
            }
            identityClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.apiKey = apiKey;
        }

        /// <summary>
        /// This function is used to sign in user to Firebase
        /// </summary>
        /// <param name="email">The user's email</param>
        /// <param name="password">The user's password</param>
        /// <returns><see langword="true"/> if successfully signed in, otherwise <see langword="false"/></returns>
        public async Task<FirebaseUser> SignInWithPasswordEmailAsync(string email, string password) {
            HttpResponseMessage response = await identityClient.PostAsync($"v1/accounts:signInWithPassword?key={apiKey}", new StringContent(UserAuth(email, password)));
            if (response.IsSuccessStatusCode) {
                firebaseUser = JsonConvert.DeserializeObject<FirebaseUser>(await response.Content.ReadAsStringAsync());
                Firestore.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firebaseUser.idToken);
                return firebaseUser;
            }
            firebaseUser = null;
            return null;
        }

        /// <summary>
        /// This function is used to sign up user to Firebase
        /// </summary>
        /// <param name="email">The user's email</param>
        /// <param name="password">The user's password</param>
        /// <returns><see langword="true"/> if successfully signed up the user, otherwise <see langword="false"/></returns>
        public async Task<FirebaseUser> SignUpAsync(string email, string password) {
            HttpResponseMessage response = await identityClient.PostAsync($"v1/accounts:signUp?key={apiKey}", new StringContent(UserAuth(email, password)));

            return response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<FirebaseUser>(await response.Content.ReadAsStringAsync()) : null;
        }

        /// <summary>
        /// This function send password reset email to a Firebase user
        /// </summary>
        /// <param name="email">The user's email</param>
        /// <returns><see langword="true"/> if the email was sent, otherwise <see langword="false"/></returns>
        public async Task<bool> SendPasswordResetEmailAsync(string email) {
            HttpResponseMessage response = await identityClient.PostAsync($"v1/accounts:sendOobCode?key={apiKey}", new StringContent(JsonConvert.SerializeObject(new { requestType = "PASSWORD_RESET", email })));

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// This function is used to sign in user with a refresh token granted from signin up a user or signed in user
        /// </summary>
        /// <param name="token">The refresh token</param>
        /// <returns><see langword="true"/> if succeded, otherwise <see langword="false"/></returns>
        public async Task<FirebaseUser> SignInWithTokenAsync(string token) {
            HttpResponseMessage response = await tokenClient.PostAsync($"v1/token?key={apiKey}", new StringContent(JsonConvert.SerializeObject(new { grant_type = "refresh_token", refreshToken = token })));
            if (response.IsSuccessStatusCode) {
                firebaseUser = JsonConvert.DeserializeObject<FirebaseUser>(await response.Content.ReadAsStringAsync());
                Firestore.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firebaseUser.idToken);
                return firebaseUser;
            }
            firebaseUser = null;
            return null;
        }

        /// <summary>
        /// This function returns the current user
        /// </summary>
        /// <returns>The current user</returns>
        public FirebaseUser GetCurrentUser() => firebaseUser;

        /// <summary>
        /// This function logs the user out
        /// </summary>
        public void SignOut() => firebaseUser = null;

        /// <summary>
        /// This function returns a new user for a user request content
        /// </summary>
        /// <param name="email">The user's email</param>
        /// <param name="password">The user's password</param>
        /// <returns>The user in a JSON format</returns>
        private static string UserAuth(string email, string password) => JsonConvert.SerializeObject(new { email, password, returnSecureToken = true });

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
    }
#pragma warning restore IDE0044, IDE0051 , IDE0052, IDE0060, 169, 414, 649
}
