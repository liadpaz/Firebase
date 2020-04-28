using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Firebase {
	/// <summary>
	/// This class is for the Firebase Real-Time Database
	/// </summary>
	public sealed class FirebaseDatabase {
		/// <summary>
		/// This function returns the reference to the Firebase Database at the <code>child</code> child
		/// </summary>
		/// <param name="child">The child</param>
		/// <returns>The reference to the Firebase Database at the <code>child</code> child</returns>
		public DatabaseReference GetReference(string child = null) => new DatabaseReference($"{child}");
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
		/// <param name="child">The database URL</param>
		internal DatabaseReference(string child = null) {
			this.child = child;
			if (client.BaseAddress == null) {
				client.BaseAddress = new Uri($"https://{Firebase.ProjectId}.firebaseio.com/");
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}
		}

		/// <summary>
		/// Private constructor so the user won't be able to create a DatabaseReference
		/// </summary>
		private DatabaseReference() { }

		/// <summary>
		/// This function returns the data from the database reference
		/// </summary>
		/// <returns>The data in the database</returns
		public async Task<string> ReadAync() {
			string auth = (Firebase.GetInstance().FirebaseAuth.GetCurrentUser().idToken ?? null)?.Insert(0, "&auth=");
			HttpResponseMessage response = await client.GetAsync($"{child}.json?print=pretty{auth}");

			return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
		}

		/// <summary>
		/// This function returns the data from the database reference
		/// </summary>
		/// <returns>The data in the database</returns
		public async Task<T> ReadAync<T>() where T : class {
			string auth = (Firebase.GetInstance().FirebaseAuth.GetCurrentUser().idToken ?? null)?.Insert(0, "&auth=");
			HttpResponseMessage response = await client.GetAsync($"{child}.json?print=pretty{auth}");

			return response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync()) : null;
		}

		/// <summary>
		/// This function writes data in the database reference; return true if succeeded otherwise false
		/// </summary>
		/// <param name="data">The data to write</param>
		/// <returns><see langword="true"/> if succeeded otherwise <see langword="false"/></returns>
		public async Task<bool> WriteAsync(object data) {
			string auth = (Firebase.GetInstance().FirebaseAuth.GetCurrentUser().idToken ?? null)?.Insert(0, "?auth=");
			HttpResponseMessage response = await client.PutAsync($"{child}.json{auth}", new StringContent(JsonConvert.SerializeObject(data)));

			return response.IsSuccessStatusCode;
		}

		/// <summary>
		/// This function writes data in the database reference; return true if succeeded otherwise false
		/// </summary>
		/// <param name="data">The data to write in json format</param>
		/// <returns><see langword="true"/> if succeeded otherwise <see langword="false"/></returns>
		public async Task<bool> WriteAsync(string data) {
			string auth = (Firebase.GetInstance().FirebaseAuth.GetCurrentUser().idToken ?? null)?.Insert(0, "?auth=");
			HttpResponseMessage response = await client.PutAsync($"{child}.json{auth}", new StringContent(data));

			return response.IsSuccessStatusCode;
		}

		/// <summary>
		/// This function deletes the data in the child in the database
		/// </summary>
		/// <returns><see langword="true"/> if succeeded otherwise <see langword="false"/></returns>
		public async Task<bool> DeleteAsync() {
			string auth = (Firebase.GetInstance().FirebaseAuth.GetCurrentUser().idToken ?? null)?.Insert(0, "?auth=");
			HttpResponseMessage response = await client.DeleteAsync($"{child}.json{auth}");

			return response.IsSuccessStatusCode;
		}

		/// <summary>
		/// This function returns a database reference to the <c>child</c> of the current reference
		/// </summary>
		/// <param name="child">The child</param>
		/// <returns>Database reference to <paramref name="child"/> of the current reference</returns>
		public DatabaseReference Child(string child) => new DatabaseReference($"{this.child}/{child}");

		/// <summary>
		/// This function returns the reference to the root of the database
		/// </summary>
		/// <returns>The reference to the root of the database</returns>
		public DatabaseReference Root => new DatabaseReference();

		/// <summary>
		/// This function returns the reference to the parent of the current reference
		/// </summary>
		/// <returns>The reference to the parent of the current reference</returns>
		public DatabaseReference GetParent() => new DatabaseReference(child.Substring(0, child.LastIndexOf('/')));

		/// <summary>
		/// This fuction returns the database reference as string, it shows the path of the reference
		/// </summary>
		/// <returns>The database reference as string</returns>
		public override string ToString() => $"{client.BaseAddress}{child}";
	}
}
