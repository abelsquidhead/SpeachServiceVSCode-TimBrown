using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace company.name
{

    class ProcessAudio
    {

   static void Main(string[] args)
        {
            Console.WriteLine("Processing audio files....");

            try 
            {
                // Only get files that begin with the letter "c".
                string[] dirs = Directory.GetFiles(@"./audiosrc", "*.wav");
                Console.WriteLine("The number of wav files is {0}.", dirs.Length);

                foreach (string dir in dirs) 
                {
                    //Console.WriteLine(dir);
                    Transcribe(dir).Wait();
                   
                }
            } 
            catch (Exception e) 
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }            
       

        }


        static async Task Transcribe(string sfileName)
        {
            Console.WriteLine("Processing file: " + sfileName);

            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            var config = SpeechConfig.FromSubscription("61bad01768bd4d708ab116f9766976e8", "eastus2");

            var stopRecognition = new TaskCompletionSource<int>();
            //String path = @"./transcribed/AHA_0278008BAF2019021712345501.txt";
            string path = @sfileName+".txt";

        // Creates a speech recognizer using file as audio input.
        // Replace with your own audio file name.
        using (var audioInput = AudioConfig.FromWavFileInput(@sfileName))
        {

            using (StreamWriter sr = File.AppendText(path))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    // Subscribes to events.
                    // microsoft.cognitiveservices.speech.SpeechRecognizer attributes

                    // Recognizing
                    recognizer.Recognizing += (s, e) =>
                    {
                        Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    };

                    // Recognized
                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");

                            // append to file
                            sr.WriteLine(e.Result.Text);

                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    // Canceled
                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\n    Session started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\n    Session stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                   Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                   await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                   
                }
            sr.Close();
            
            // tjb Console.ReadLine();
            }

        }

        }

    }

}