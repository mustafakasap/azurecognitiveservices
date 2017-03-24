using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net.Http;

namespace TTSTest
{
	/*
	 * Class for retreiving an authentication code, Bearer that is required for CogS API calls.
	 * Retreived authentication code will expire after 10 minutes but this class will renew the
	 * expired tokens transperantly between each API call once it is expired.
	 * 
	 * This class is used as is (with very minor updates) from the Bing Speech API sample by Microsoft.
	 */
	public class Authentication
	{
		public static readonly string FetchTokenUri = "https://api.cognitive.microsoft.com/sts/v1.0";
		private string subscriptionKey;
		private string token;
		private Timer accessTokenRenewer;

		//Access token expires every 10 minutes. Renew it every 9 minutes only.
		private const int RefreshTokenDuration = 9;

		public Authentication(string subscriptionKey)
		{
			this.subscriptionKey = subscriptionKey;
			this.token = FetchToken(FetchTokenUri, subscriptionKey).Result;

			// renew the token every specfied minutes
			accessTokenRenewer = new Timer(new TimerCallback(OnTokenExpiredCallback),
										   this,
										   TimeSpan.FromMinutes(RefreshTokenDuration),
										   TimeSpan.FromMilliseconds(-1));
		}

		public string GetAccessToken()
		{
			return this.token;
		}

		private void RenewAccessToken()
		{
			this.token = FetchToken(FetchTokenUri, this.subscriptionKey).Result;
			Console.WriteLine("Renewed token.");
		}

		private void OnTokenExpiredCallback(object stateInfo)
		{
			try
			{
				RenewAccessToken();
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("Failed renewing access token. Details: {0}", ex.Message));
			}
			finally
			{
				try
				{
					accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
				}
				catch (Exception ex)
				{
					Console.WriteLine(string.Format("Failed to reschedule the timer to renew access token. Details: {0}", ex.Message));
				}
			}
		}

		private async Task<string> FetchToken(string fetchUri, string subscriptionKey)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
				UriBuilder uriBuilder = new UriBuilder(fetchUri);
				uriBuilder.Path += "/issueToken";

				var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null);
				return await result.Content.ReadAsStringAsync();
			}
		}
	}


	/* Class for accessing Cognitive Services - Bing Text to Speech (TTS) API
	 * It enumerates available speaker types to be used as parameter while calling TTS API
	 * For authentication, API call needs Bearer key retreived by above Authentication class.
	 */
	public class BingTTS
	{
		// Only available English speakers enumerated. There are more other speakers 
		// available in API documentation for other languages.
		public enum LocaleVoice
		{
			en_AU_Catherine,
			en_AU_HayleyRUS,
			en_CA_Linda,
			en_CA_HeatherRUS,
			en_GB_Susan_Apollo,
			en_GB_HazelRUS,
			en_GB_George_Apollo,
			en_IN_Ravi_Apollo,
			en_IN_Heera_Apollo,
			en_US_ZiraRUS,
			en_US_BenjaminRUS
		};
		
		private Authentication auth;
		private string subscriptionKey;
		private LocaleVoice ttsSpeaker;

		public static readonly string ttsURI = "https://speech.platform.bing.com/synthesize";

		public BingTTS(string subscriptionKey)
		{
			this.subscriptionKey = subscriptionKey;
			this.ttsSpeaker = LocaleVoice.en_US_ZiraRUS;
			this.auth = new Authentication(this.subscriptionKey);
		}

		public void setSpeaker(LocaleVoice pLocaleVoice)
		{
			this.ttsSpeaker = pLocaleVoice;
		}

		public async Task<Stream> getSpeechFromText(string pText)
		{
			try
			{
				string sLocale, sVoice, sGender;

				switch (this.ttsSpeaker)
				{
					case LocaleVoice.en_AU_Catherine:
						sLocale = "en-AU"; sGender = "Female";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-AU, Catherine)";
						break;
					case LocaleVoice.en_AU_HayleyRUS:
						sLocale = "en-AU"; sGender = "Female";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-AU, HayleyRUS)";
						break;
					case LocaleVoice.en_CA_Linda:
						sLocale = "en-CA"; sGender = "Female";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-CA, Linda)";
						break;
					case LocaleVoice.en_CA_HeatherRUS:
						sLocale = "en-CA"; sGender = "Female";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-CA, HeatherRUS)";
						break;
					case LocaleVoice.en_GB_Susan_Apollo:
						sLocale = "en-GB"; sGender = "Female";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-GB, Susan, Apollo)";
						break;
					case LocaleVoice.en_GB_HazelRUS:
						sLocale = "en-GB"; sGender = "Female";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-GB, HazelRUS)";
						break;
					case LocaleVoice.en_GB_George_Apollo: 
						sLocale = "en-GB"; sGender = "Male";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-GB, George, Apollo)";
						break;
					case LocaleVoice.en_IN_Ravi_Apollo:
						sLocale = "en-IN"; sGender = "Male";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-IN, Ravi, Apollo)";
						break;
					case LocaleVoice.en_IN_Heera_Apollo:
						sLocale = "en-IN"; sGender = "Female";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-IN, Heera, Apollo)";
						break;
					case LocaleVoice.en_US_ZiraRUS:
						sLocale = "en-US"; sGender = "Female";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)";
						break;
					case LocaleVoice.en_US_BenjaminRUS:
						sLocale = "en-US"; sGender = "Male";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-US, BenjaminRUS)";
						break;
					default:
						sLocale = "en-US"; sGender = "Female";
						sVoice = "Microsoft Server Speech Text to Speech Voice (en-US, BenjaminRUS)";
						break;
				}


				string textTemplate = @"<speak version='1.0' xml:lang='en-US'> <voice xml:lang='{0}' xml:gender='{1}' name='{2}'> {3} </voice> </speak>";

				string textToSpeech = String.Format(textTemplate, sLocale, sGender, sVoice, pText);

				using (var client = new HttpClient())
				{
					string accessToken = auth.GetAccessToken();
					client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);
					client.DefaultRequestHeaders.TryAddWithoutValidation("X-Microsoft-OutputFormat", "riff-16khz-16bit-mono-pcm");
					client.DefaultRequestHeaders.UserAgent.ParseAdd("MyAgent/1.0");

					UriBuilder uriBuilder = new UriBuilder(ttsURI);
					HttpResponseMessage response = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, new StringContent(textToSpeech));
					response.EnsureSuccessStatusCode();

					Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
					return streamToReadFrom;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.WriteLine(ex.Message);
			}

			return null;
		}
	}

	/*
	 * Sample console application that show the usege of BingTTS class
	 */
	class Program
	{
		/*
		 * Convert Stream to a binary array (which is actual Wav audio file returned from BingTTS API
		 */
 		public static byte[] StreamToByteArray(Stream stream)
		{
			stream.Position = 0;
			byte[] buffer = new byte[stream.Length];
			for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
				totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
			return buffer;
		}

		static void Main(string[] args)
		{
			// Update below API key with your own one. To get a valid key, you need to create 
			// "Bing Speech API" cogtinive service under http://portal.azure.com
			// Please dont send emails, messages that "hey you forget your key public in your code"... 
			// Below key is not valid anymore.
			BingTTS tts = new BingTTS("e790ad4032ab4ce4aa6c34a5b9c2758e");
			tts.setSpeaker(BingTTS.LocaleVoice.en_CA_Linda); 

			// Type any text that you want to get its audio representation.
			string textToSpeech = "Now we have to worr but waiting the next break. by the way Air New Zeland Rocks!";

			try
			{
				using (Stream wavStream = tts.getSpeechFromText(textToSpeech).Result)
				{
					// Binary array representation of the audio file
					byte[] result = StreamToByteArray(wavStream);

					// Update below path with a valid one on your local PC
					File.WriteAllBytes("/Users/mkasap/desktop/test.wav", result);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.WriteLine(ex.Message);
			}

			Console.WriteLine("Finished.");
			Console.ReadLine();
		}
	}
}
