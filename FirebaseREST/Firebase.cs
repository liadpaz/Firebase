﻿#pragma warning disable 649

using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Firebase
{
    /// <summary>
    /// This class is for the general Firebase
    /// </summary>
    public sealed class Firebase
    {
        private static Firebase firebase = null;
        private static FirebaseAuth auth = null;
        private static FirebaseDatabase database = null;

        internal static string projectName;

        /// <summary>
        /// This function initializes the only Firebase instance on the app
        /// </summary>
        /// <param name="projectName">the Firebase project name</param>
        /// <param name="authCredential">the Web Api for the Firebase project</param>
        /// <returns>the only Firebase instance on the app</returns>
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

        /// <summary>
        /// Private constructor so you won't be able to create new instance from outside the class
        /// </summary>
        private Firebase() { }

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

    /// <summary>
    /// This class is for the Firebase Database
    /// </summary>
    public sealed class FirebaseDatabase
    {
        /// <summary>
        /// This function returns the reference to the Firebase Database at the <code>child</code> child
        /// </summary>
        /// <exception cref="ArgumentNullException">thrown if <code>child</code> is null or empty</exception>
        /// <param name="child">the child</param>
        /// <returns>the reference to the Firebase Database at the <code>child</code> child</returns>
        public DatabaseReference GetReference(string child)
        {
            if (string.IsNullOrEmpty(child)) throw new ArgumentNullException("Child cannot be null or empty");
            return new DatabaseReference($"{child}");
        }

        /// <summary>
        /// This function returns the reference to the root of the Firebase Database
        /// </summary>
        /// <returns>the reference to the root of the Firebase Database</returns>
        public DatabaseReference GetReference()
        {
            return new DatabaseReference();
        }
    }

    public sealed class DatabaseReference
    {
        private static readonly HttpClient client = new HttpClient();
        
        private readonly string child;

        /// <summary>
        /// Internal constructor that can only be called from within this library, sets the child of the database reference
        /// </summary>
        /// <param name="child">the databaseURL</param>
        internal DatabaseReference(string child)
        {
            this.child = child;
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri($"https://{Firebase.projectName}.firebaseio.com/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        /// <summary>
        /// Internal constructor that can only be called from within this library
        /// </summary>
        internal DatabaseReference()
        {
            child = string.Empty;
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri($"https://{Firebase.projectName}.firebaseio.com/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        /// <summary>
        /// This function returns the data from the database reference
        /// </summary>
        /// <returns>the data in the database</returns
        public async Task<string> Read()
        {
            HttpResponseMessage response = await client.GetAsync($"{child}.json?print=pretty&auth={FirebaseAuth.IdToken}");
            return response.StatusCode switch
            {
                HttpStatusCode.OK => await response.Content.ReadAsStringAsync(),
                HttpStatusCode.Unauthorized => throw new DatabaseException("You are not authorized to the database"),
                _ => null
            };
        }

        /// <summary>
        /// This function writes data in the database reference; return true if succeeded otherwise false
        /// </summary>
        /// <exception cref="DatabaseAuthException">thrown when the user is not permitted</exception>
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
        /// <exception cref="DatabaseAuthException">throw when the user is not permitted</exception>
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
        public DatabaseReference Child(string child)
        {
            return new DatabaseReference($"{this.child}/{child}");
        }

        /// <summary>
        /// This function returns the reference to the root of the database
        /// </summary>
        /// <returns>the reference to the root of the database</returns>
        public DatabaseReference Root()
        {
            return new DatabaseReference();
        }

        /// <summary>
        /// This function returns the reference to the parent of the current reference
        /// </summary>
        /// <returns>the reference to the parent of the current reference</returns>
        public DatabaseReference GetParent()
        {
            return new DatabaseReference(child.Substring(0, child.Length - child.LastIndexOf('/')));
        }
    }

    /// <summary>
    /// This class is for the Firebase authentication
    /// </summary>
    public sealed class FirebaseAuth
    {
        private static readonly string signInURL = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword";
        private static readonly string signUpURL = "https://identitytoolkit.googleapis.com/v1/accounts:signUp";
        private static readonly string passwordResetURL = "https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode";

        private static readonly HttpClient signInClient = new HttpClient
        {
            BaseAddress = new Uri(signInURL)
        };
        private static readonly HttpClient signUpClient = new HttpClient
        {
            BaseAddress = new Uri(signUpURL)
        };
        private static readonly HttpClient passwordResetClient = new HttpClient
        {
            BaseAddress = new Uri(passwordResetURL)
        };

        internal static string IdToken
        {
            private set;
            get;
        }
        private string apiKey;
        public static bool IsLoggedIn
        {
            private set;
            get;
        } = false;

        internal FirebaseAuth(string apiKey)
        {
            signInClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            signUpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            passwordResetClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.apiKey = apiKey;
        }

        public async Task<bool> SignIn(string email, string password)
        {
            HttpResponseMessage response = await signInClient.PostAsync($"?key={apiKey}", new StringContent(new UserAuth(email, password).ToString()));

            if (response.IsSuccessStatusCode)
            {    
                FirebaseUser user = JsonConvert.DeserializeObject<FirebaseUser>(await response.Content.ReadAsStringAsync());
                IdToken = user.idToken;
                IsLoggedIn = true;
                return true;
            }
            IsLoggedIn = false;
            throw new AuthenticationException("Couldn't sign in to firebase");
        }

        public async Task<bool> SignUp(string email, string password)
        {
            HttpResponseMessage response = await signUpClient.PostAsync($"?key={apiKey}", new StringContent(new UserAuth(email, password).ToString()));

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendPasswordResetEmail(string email)
        {
            HttpResponseMessage response = await passwordResetClient.PostAsync($"?key={apiKey}", new StringContent("{" + $"\"requestType\":\"PASSWORD_RESET\",\"email\":\"{email}\"" + "}"));

            return response.IsSuccessStatusCode;
        }
    }

    /// <summary>
    /// This class is the main Firebase exceptions class, which all the firebase exceptions derive from
    /// </summary>
    public class FirebaseException : Exception
    {
        internal FirebaseException() : base()
        {
        }

        internal FirebaseException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// This class is for any firebase authentication related exception 
    /// </summary>
    public class AuthenticationException : FirebaseException
    {
        public AuthenticationException() : base()
        {
        }

        public AuthenticationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// This class is for any firebase database related exception
    /// </summary>
    public class DatabaseException : FirebaseException
    {
        public DatabaseException() : base()
        {
        }

        public DatabaseException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// This class if for authenticating a firebase user (token)
    /// </summary>
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

    /// <summary>
    /// This class is for a firebase user details
    /// </summary>
    public class FirebaseUser
    {
        internal string kind;
        public string localId;
        public string email;
        public string displayName;
        public string idToken;
        internal string registered;
        internal string refreshToken;
        internal string expiresIn;
    }
}
