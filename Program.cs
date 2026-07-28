using OpenCvSharp;

const string MODULE_NAME = "VideoStreamDisplay";
const string WINDOW_NAME = "Received video";
const string DEBUG_FRAME_FOLDER = "debug_frames";

// Set up the PipeClient and connect to the server
var client = new PipeClient();
client.MessageReceived += ReceivedMessageHandler;
client.Connect(MODULE_NAME);

Console.WriteLine($"{MODULE_NAME} is running...");
//Directory.CreateDirectory(DEBUG_FRAME_FOLDER);
Cv2.NamedWindow(WINDOW_NAME, WindowFlags.AutoSize);

// Press enter to exit
Console.ReadLine();
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

        Cv2.ImShow(WINDOW_NAME, frame);
        Cv2.WaitKey(1);
    }
}
