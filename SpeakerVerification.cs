using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using System.Net.Http;

/**************************************
 *  C# sample using REST API
 *  Speaker Verification Sample
 * 
 * 	User guide:
 *  1) You should "Create Profile" which generates a GUID
 *  2) Record 3 audio file (Wav, 16Khz, Mono) each file must contain same phrase spoken by same speaker
 *     Spoken phrase must be the one from the list that can be retreived from API. see "List of supported phrases"
 *  3) You can delete any profile by "Delete Profile"
 *  4) Enroll the above 3 recordings to a profile. Minimum 3 recording required. Enroll with "Enroll to Profile" section
 *  5) If you want to delete enrolments to start over, use section "Reset all enrollments"
 *  6) You can list all available profiles by using section "List all profiles"
 *  7) Now you can verify if a speaker belongs to existing profile. Record new wav file with same phrase that you 
 *     used in enrollment. Use section "Verify speaker" to verify if it is authenticated or not.
 * 
 **************************************/



namespace TestSpeakerAPI
{
	public class HTTPClientHelper
	{
		private string subscriptionKey = "";

		public HTTPClientHelper(string subscriptionKey)
		{
			this.subscriptionKey = subscriptionKey;
		}

		public string sendRequest(string method, string request_url, string content_type, byte[] body)
		{
			string responseString = "";

			try
			{
				HttpWebRequest request = null;
				request = (HttpWebRequest)HttpWebRequest.Create(request_url);
				request.Method = method;
				if (content_type != null)
					request.ContentType = content_type;
				if (body != null)
					request.ContentLength = body.Length;
				request.Headers["Ocp-Apim-Subscription-Key"] = subscriptionKey;

				if (body != null)
				{
					Stream requestStream = request.GetRequestStream();
					requestStream.Write(body, 0, body.Length);
					requestStream.Flush();
					requestStream.Close();
				}

				WebResponse response = request.GetResponse();
				Console.WriteLine(((HttpWebResponse)response).StatusCode);
				using (StreamReader sr = new StreamReader(response.GetResponseStream()))
				{
					responseString = sr.ReadToEnd();
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.WriteLine(ex.Message);

				return ex.Message;
			}

			return responseString;
		}
	}


	class Program
	{
		static void Main(string[] args)
		{
			string responseString = "";
			string subscriptionKey = "862add84d5c942228fb0eabb0e7efc3c";
			HTTPClientHelper hch = new HTTPClientHelper(subscriptionKey);

#if false
			// ************************************
			// List of supported phrases
			// ************************************
			responseString = hch.sendRequest(
				"GET",      // method
				"https://westus.api.cognitive.microsoft.com/spid/v1.0/verificationPhrases?locale=en-us", // request_url
				null,       // content_type
				null        // body
			);
			Console.WriteLine(responseString);
#endif

#if false
			// ************************************
			// Create Profile
			// ************************************
			byte[] body = Encoding.ASCII.GetBytes("{\"locale\":\"en-us\",}");
			responseString = hch.sendRequest(
				"POST", 				// method
				"https://westus.api.cognitive.microsoft.com/spid/v1.0/verificationProfiles", // request_url
				"application/json",		// content_type
				body					// body
			);
			Console.WriteLine(responseString);
#endif

#if false
			// ************************************
			// Delete Profile
			// ************************************
			responseString = hch.sendRequest(
				"DELETE",   // method
				"https://westus.api.cognitive.microsoft.com/spid/v1.0/verificationProfiles/9f14d26a-4cfc-4615-8be6-34bac7d56738", // request_url
				null,     	// content_type
				null        // body
			);
			Console.WriteLine(responseString);
#endif

#if false
			// ************************************
			// Reset all enrollments
			// ************************************
			byte[] empty = new byte[0];
			responseString = hch.sendRequest(
				"POST",     // method
				"https://westus.api.cognitive.microsoft.com/spid/v1.0/verificationProfiles/0c9ffad3-ee7a-47a4-8bf4-cb1575162179/reset", // request_url
				"",       // content_type
				empty     // body
			);
			Console.WriteLine(responseString);
#endif

#if false
			// ************************************
			// Enroll to Profile
			// ************************************
			byte[][] samples = new byte[3][];
			samples[0] = File.ReadAllBytes("/Users/mkasap/Desktop/sample1.wav");
			samples[1] = File.ReadAllBytes("/Users/mkasap/Desktop/sample2.wav");
			samples[2] = File.ReadAllBytes("/Users/mkasap/Desktop/sample3.wav");

			Console.WriteLine("Enrolling audio sample to a profile");
			for (int i = 0; i < 3; i++)
			{
				responseString = hch.sendRequest(
					"POST",     // method
					"https://westus.api.cognitive.microsoft.com/spid/v1.0/verificationProfiles/0c9ffad3-ee7a-47a4-8bf4-cb1575162179/enroll", // request_url
					"application/octet-stream",     // content_type
					samples[i]  // body
				);
				Console.WriteLine(responseString);
			}
#endif

#if false
			// ************************************
			// List all profiles
			// ************************************
			responseString = hch.sendRequest(
				"GET",    // method
				"https://westus.api.cognitive.microsoft.com/spid/v1.0/verificationProfiles", // request_url
				null,     // content_type
				null      // body
			);
			Console.WriteLine(responseString);
#endif

#if true
			// ************************************
			// Verify speaker
			// ************************************
			byte[][] samples = new byte[3][];
			samples[0] = File.ReadAllBytes("/Users/mkasap/Desktop/test1.wav");
			samples[1] = File.ReadAllBytes("/Users/mkasap/Desktop/test2.wav");
			samples[2] = File.ReadAllBytes("/Users/mkasap/Desktop/test3.wav");

			Console.WriteLine("Speaker verification test");
			for (int i = 0; i < 3; i++)
			{
				responseString = hch.sendRequest(
					"POST",     // method
					"https://westus.api.cognitive.microsoft.com/spid/v1.0/verify?verificationProfileId=0c9ffad3-ee7a-47a4-8bf4-cb1575162179", // request_url
					"application/octet-stream",     // content_type
					samples[i]  // body
				);
				Console.WriteLine(responseString);
			}
#endif


		}
	}
}
