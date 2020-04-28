namespace Firebase {
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
        }

        /// <summary>
        /// This function initializes the only Firebase instance on the app
        /// </summary>
        /// <param name="projectId">The Firebase project name</param>
        /// <param name="apiKey">The Web Api for the Firebase project</param>
        /// <returns>The only Firebase instance on the app</returns>
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
        /// This getter returns the only instance of the Firebase Real-Time Database
        /// </summary>
        public FirebaseDatabase FirebaseDatabase => database;

        /// <summary>
        /// This getter returns the only instance of the Firebase Authentication
        /// </summary>
        public FirebaseAuth FirebaseAuth => auth;

        /// <summary>
        /// This getter returns the only instance of the Firebase Firestore
        /// </summary>
        public Firestore FirebaseFirestore => firestore;
    }
}
