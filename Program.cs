using OpenCvSharp;

const string MODULE_NAME = "VideoStreamDisplay";
const string WINDOW_NAME = "Received video";
const string DEBUG_FRAME_FOLDER = "debug_frames";

// Created a frame object outside the thread of the evenhandler
var frameLock = new object();
Mat? latestFrame = null;

// Set up the PipeClient and connect to the server
var client = new PipeClient();
client.MessageReceived += ReceivedMessageHandler;
client.Connect(MODULE_NAME);

Console.WriteLine($"{MODULE_NAME} is running...");
Console.WriteLine("Press enter or escape to exit.");
Directory.CreateDirectory(DEBUG_FRAME_FOLDER);
Cv2.NamedWindow(WINDOW_NAME, WindowFlags.AutoSize);

// Create a task that listens for console into for closing the window.
// Its not pretty, but it works
var exitTask = Task.Run(Console.ReadLine);

while (!exitTask.IsCompleted)
{
    Mat? frameToShow = null;

    lock (frameLock)
    {
        if (latestFrame is not null)
        {
            frameToShow = latestFrame;
            latestFrame = null;
        }
    }

    if (frameToShow is not null)
    {
        Cv2.ImShow(WINDOW_NAME, frameToShow);
        frameToShow.Dispose();
    }

    if (Cv2.WaitKey(1) == 27)
        break;

    Thread.Sleep(1);
}

// Creating a lock to handle issues relating to multithreading and access to the window and frame
lock (frameLock)
{
    latestFrame?.Dispose();
}

Cv2.DestroyAllWindows();


void ReceivedMessageHandler(object? eventSender, TartaMessage tartaMessage)
{
    if (tartaMessage.Type == "message" && tartaMessage.SubCategory == "videoframe")
    {
        var encodedFrame = Convert.FromBase64String(tartaMessage.Payload);
        using var frame = Cv2.ImDecode(encodedFrame, ImreadModes.Color);

        if (frame.Empty())
        {
            Console.WriteLine("Received an empty or invalid JPEG frame.");
            return;
        }

        //var debugFramePath = Path.Combine(DEBUG_FRAME_FOLDER, $"frame_{DateTime.Now:yyyyMMdd_HHmmss_fff}.jpg");
        //Cv2.ImWrite(debugFramePath, frame);

        lock (frameLock)
        {
            latestFrame?.Dispose();
            latestFrame = frame.Clone();
        }
    }
}
