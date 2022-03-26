using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InTheHand.Net.Bluetooth;
using System.IO;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using InTheHand.Windows.Forms;
using System.Windows;
using System.Threading;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using System.Management;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;

//TODO : add MAC recogntion for new devices
//TODO : finish user interface
//TODO : fix case where user presses back instead of disconnecting (connectedDevices and bluetoothThreads don't get updated)
//TODO : make headers better and more easily modifiable
//TODO : fix stream write

namespace TestBluetoothRecieverMulti
{
    class Program
    {
        //CONSTANTS
        public const int UI_WIDTH = 60; //the maximum width in characters for the UI
        public const int UI_REFRESH = 1000; //the number of milliseconds between each refresh of the UI
        public const int UI_EVENT_LENGTH = 5; //the number of items to display on the eventList
        public const int MAX_DEVICES_SCAN = 20; //The max number of bluetooth devices that the code will scan for whe determining device identities
        public static readonly Guid BLUETOOTH_UID = new Guid("{39675b0d-6dd8-4622-847f-3e5acc607e27}"); //default uid
        
        /// <summary>
        /// Headers for csv
        /// </summary>
        public const string HEADERS = "Team_Number,Alliance_Colour,Match_Number,Scout_Name,Crossed_Auto_Line," +
            "Auto_Balls,Auto_Balls_Shot,Far_Balls,Far_Balls_Shot,Tarmac_Balls,Tarmac_Balls_Shot,Close_Balls," +
            "Close_Balls_Shot,Protected_Zone_Balls,Protected_Zone_Balls_Shot,Active_Defence_Time,Defence_Time," +
            "Defended_Teams,Climb_Time,Rung_Level,Foul_Count,\n";

        public static volatile bool bluetoothEnabled = false; //is bluetooth enabled, default value is no, this is here cause BluetoothInterface doesnt initialize without bluetooth (error prevention purposes essentially)
        public static volatile bool bluetoothPrevEnabled = false; //was bluetooth previously enabled, used to detect rising or falling edge of signal
        public static volatile bool csvFileAccessible = false; //is the scoutingInfo.csv file open, the default value is yes (false, as in not accessible)

        static void Main(string[] args)
        {
            Program.bluetoothTest(); //tests to see if bluetooth is enabled (or perhaps the computer doesnt have bluetoooth capabilities) this also starts the first server thread
            Thread UIThreadRef = new Thread(new ThreadStart(UIThread)); //starts UI thread, error detection is built into UI thread (should be ok right, better than starting another thread I hope)
            if (UIThreadRef.Name == null) //extremely unnecesscary but im fed up with bs problems so this check is staying in
            {
                UIThreadRef.Name = "UI Thread"; //names ui thread
            }
            UIThreadRef.Start(); //starts UI thread
        }

        /** @param s the base string to modify
         *  @param iteration the number of characters to skip when inserting characters (E.X. if iteration is 2 and char is 3, and base is 000000000, output 0003000300030003)
         *  @param insert the character to insert
         *  Inserts characters into a string at a fixed increment 
         */
        public static string stringInsert(string s, int iteration, char insert)
        {
            List<char> charList = new List<char>(s.ToCharArray());

            int counter = 0;
            int compensation = 0;

            while (counter+compensation < charList.Count)
            {
                if (counter > 0 && ((counter+compensation) % iteration) == (compensation % iteration))
                {
                    charList.Insert(counter+compensation, insert);
                    compensation += 1;
                }
                counter += 1;
            }

            return new string(charList.ToArray());

        }

        /** Tests to see if the ScoutingInfo.csv is open
         *  If it is it isn't possible to write to
         *  Returns true if it is closed, and false if it is open
         */ 
        public static bool csvNotWritableTest()
        {
            bool open = false; // this method returns !open (variable stores whether or not the file is open)
            
            try
            {
                if (File.Exists("ScoutingInfo.csv")) { //only tries to read if the file exists, otherwise this test is pointless
                    File.ReadAllLines("ScoutingInfo.csv"); //tries to access and read the file
                }
            } catch (IOException e) //csv not writable
            {
                open = true; // if an ioException is thrown, this becomes true and the function returns false
            } catch (Exception e) //something else is going wrong
            {
                //log error maybe
            }

            //other housekeeping stuff
            if (!open) { //only run if it is possible to write to scoutingInfo.csv
                lock (BluetoothInterface.unwrittenMessageLock) //locks to avoid conflicts
                {
                    try
                    {
                        foreach (string message in BluetoothInterface.unwrittenMessages) //writes all unwritten messages to scoutinginfo.csv, if there are none this wont run
                        {
                            try //in case the scoutinginfo has been opened since the last check
                            {
                                File.AppendAllText("ScoutingInfo.csv", message + "\n"); //tries to write to ScoutingInfo.csv if it can't do it it qutis and breaks
                                BluetoothInterface.unwrittenMessages.Remove(message);
                            }
                            catch (IOException e)
                            {
                                open = true; //updates the value of open
                                break; //exits the loop
                            }
                            catch (Exception e) //something else goes wrong
                            {

                            }
                        }
                    } catch (Exception e) //I had a problem here once
                    {
                        //log error
                    }
                }
            }

            Program.csvFileAccessible = !open; //updates the public variable in Program

            return !open;
        }

        /** @return enabled true if bluetooth is enabled, false if bluetooth isn't enabled
        *  Tests to find if bluetooth is enabled, this is in program because bluetoothinterface doesnt initialize without bluetooth, all error stuf must be in main class
        *  Im kind of sorry about this method, it isn't too effiecient
        */
        public static bool bluetoothTest()
        {
            bluetoothPrevEnabled = bluetoothEnabled; //bluetooth prev enabled is set to the current value of bluetoothenabled
            bool enabled = true; //whether bluetooth is enabled will be stored in this variable
            BluetoothRadio myRadio = null; //creates myradio variable
            BluetoothClient client = null; //creates client variable

            try //if this throws no exeptions, bluetooth is enabled
            {
                myRadio = BluetoothRadio.PrimaryRadio; //tries to get local bluetooth radio
                client = new BluetoothClient(); //tries to create a new bluetooth client for the hardware
            }
            catch (PlatformNotSupportedException e) // if client failed to initialize the first time this method is run this throws an exception, oddly enough, the following times no exception is thrown so this may not be necesscary
            {
                enabled = false;
            }
            catch (Exception e) //Catches any exception not handled by the above case
            {
 
            }
            if (myRadio == null) //for some reason, if bluetooth isn't enabled: myRadio will be null, but client occasionally won't be, client seems to only be null the first time this method is run
            {
                enabled = false;
            }
            
            Program.bluetoothEnabled = enabled; //updates bluetoothEnabled

            //some other housekeeping stuff
            if (Program.bluetoothEnabled) //bluetooth enabled
            {
                if (!(bluetoothPrevEnabled)) //if a rising edge is detected (i know this isn't a signal but you get what I mean right)
                {
                    try //ensures that if client doesnt start the first time, MAC address is still known
                    {
                        BluetoothInterface.localAddress = myRadio.LocalAddress; //local bluetooth hardawre MAC address
                        BluetoothInterface.localAddressString = Program.stringInsert(BluetoothInterface.localAddress.ToString(), 2, ':'); //local bluetooth hardware MAC address stored as a string
                    } catch (Exception e) //handles any unexpected exceptions
                    {

                    } 
                    BluetoothInterface.connectAsServer();
                }
            } else //bluetooth is not enabled
            {
                lock (BluetoothInterface.connectedDevicesLock) //prevent interference
                {
                    BluetoothInterface.connectedDevices.Clear(); //clears connected devices
                }
            }

            return enabled;
        }

        /** Thread the UI runs on
         *  Updates the UI once every so often (defined in constants)
         */ 
        static void UIThread()
        {
            while (true)
            {
                updateUI();
                Thread.Sleep(UI_REFRESH);
            }
        }

        /** Updates the UI, which is all cli 
         * UI takes the following form (All lines clip after 60 characters):
         * 
         * Bluetooth: Enabled
         * ScoutingInfo.csv: Closed
         * __________________
         * 
         * Connected Devices: 
         *     Xperia XA1 - 00:CC:22:44:55
         *     Jacob's Fire - 00:CC:33:44:55
         *     Jacob's Fire - 00:CC:33:44:55 
         *     Jacob's Fire - 00:CC:33:44:55 
         *     Jacob's Fire - 00:CC:33:44:55 
         *     Jacob's Fire - 00:CC:33:44:55 
         *     Jacob's Fire - 00:CC:33:44:55 
         *     Jacob's Fire - 00:CC:33:44:55 
         *     ...
         * _________________
         * 
         * Log: (or something similar)
         *     Xperia XA1 - 00:CC:22:44:55 Has Connected
         *     Xperia XA1 Says: Alpha_Dogs,4946,,,,,,,,,,
         *     etc...
         * _________________
         * 
         * Console:
         * not sure how (or if) I'm going to do this yet but user input will go here
         * _________________
         * 
         */
        public static void updateUI()
        {
            Console.Clear();

            Program.bluetoothTest(); //tests to see if bluetooth is enabled
            Program.csvNotWritableTest(); //checks to see if the scoutingInfo file is writable to

            //Bluetooth section
            if (Program.bluetoothEnabled) //if bluetooth is enabled
            {
                UIWrite("Bluetooth: Enabled");
            } else //if bluetooth isn't enabled
            {
                UIWrite("Bluetooth: Disabled"); //UI update
            }
            //CSV section
            if (Program.csvFileAccessible) //if the file is accessible
            {
                UIWrite("ScoutingInfo.csv: Accessible");
            } else //if the file isn't accessible
            {
                UIWrite("ScoutingInfo.csv: Not Accessible"); //UI update
            }
            UIWrite("_______________________________________________________");
            Console.WriteLine("\n");//newline

            //server status
            UIWrite("Server Status:");

            UIWrite("     UID - " + BLUETOOTH_UID.ToString());
            UIWrite("     MAC - " + BluetoothInterface.localAddressString);

            UIWrite("_______________________________________________________");
            Console.WriteLine("\n");//newline

            //connected devices
            UIWrite("Connected Devices:");
            int a = BluetoothInterface.connectedDevices.Count; //a is the length of connected devices
            if (a > 8) //makes sure a doesn't get bigger than 8
            {
                a = 8;
            }
            try
            { //if bluetooth is enabled this should work
                for (int i = 0; i < a; i++) //increments at most 8 times
                {
                    if (i < BluetoothInterface.connectedDevices.Count) //write in the name of the device, otherwise put in a newline
                    {
                        UIWrite("     " + BluetoothInterface.connectedDevices[i] + "");
                    } else
                    {
                        Console.WriteLine("\n");//write in a newline
                    }
                }
                if (BluetoothInterface.connectedDevices.Count > 8)
                {
                    UIWrite("     ...");
                }
            } catch (Exception e) // if bluetooth isn't enabled (or something else goes wrong but its probably just that bluetooth isn't enabled)
            {

            }
            UIWrite("______________________________________________________");
            Console.WriteLine("\n");//newline

            //errors and problems
            UIWrite("Errors:");
            if (!(bluetoothEnabled)) //if bluetooth is not enabled
            {
                UIWrite("     Bluetooth Is Not Enabled, Please Enable Bluetooth");
                if (!(csvFileAccessible)) //only write a newline if there is going to be an entry after this
                {
                    Console.WriteLine("\n"); //newline
                }
            }
            if (!(csvFileAccessible)) //if the csv file is not writable to
            {
                UIWrite("     ScoutingInfo.csv Is Not Accessible To The Program");
                UIWrite("     It Is Read-Only, Or Currently Open");
                UIWrite("     Please Close The File Or Change Its Properties");
            }
            UIWrite("______________________________________________________");
            Console.WriteLine("\n");//newline

            //Events
            UIWrite("Events:");
            
            foreach (string s in BluetoothInterface.eventList)
            {
                if (s != null)
                {
                    UIWrite("     " + s);
                }
            }

            UIWrite("______________________________________________________");
            Console.WriteLine("\n");//newline
        }

        /** @param line the line to write to the ui
         *  Writes a line to the UI (cli) it just clips anything longer than max ui width characters then prints it
         */ 
        static void UIWrite(string line)
        {
            if (line.Length > Program.UI_WIDTH) // if line is longer than ui max width, write the first 60 characters
            {
                Console.WriteLine(line.Substring(0, Program.UI_WIDTH)); // writes the clipped line to the console
            } else
            {
                Console.WriteLine(line); //writes the full line to the console
            }
        }

    }

    /** Log class, represents a log file, can be written to
     * 
     */ 
    class Log
    {

        public String absolutePath; //the absolute path of the log file
        public String path; //the path of the file (not absolute)

        /** @param fileName the name of the log File for the log object to interact with
         *  Constructor for Log class
         */ 
        public Log(String fileName)
        {
            path = fileName; // updates path property

            if (!File.Exists(fileName)) //if file doesnt exist yet
            {
                File.WriteAllText(path, "NEW LOG\n"); //creates a new log and adds a header
            }
            writeLine("NEW RUN"); //adds new run header
        }

        /** @param line the line to write to the log file
         *  Writes a line to the log file, adds a newline character at the end, and a timestamp at the beginning
         */ 
        void writeLine(String line)
        {
            File.AppendAllText(path, "Uptime: " + getProgramUptime() + " - " + line + "\n"); //writes the line to the log file
        }

        /** @param lines an array of lines to write to the log
         *  Writes multiple lines to the log, a timestamp is put on the first line only
         *  All following lines have an indent (5 spaces)
         *  Used for writing sets of data (Line one might be header)
         */
        void writeLines(String[] lines)
        {
            bool timeStamp = true; //set to false once a timestamp if written
            foreach (String s in lines) //iterates through all provided strings
            {
                if (timeStamp) //timestamps the first line
                {
                    writeLine(s); //writes the first line
                    timeStamp = false; //never timestamp another line
                } else
                {
                    File.AppendAllText(path, "     " + s + "\n"); //writes line with no timestamp and an indent (5 spaces)
                }
            }

        }

        /** returns program uptime as a string
         * 
         */ 
        public string getProgramUptime()
        {
            return (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString("c");
        }

        /** Logs the bluetoothEnabled variable
         * 
         */ 
        public void recordBluetoothEnabled()
        {
            writeLine("Program Thinks Bluetooth Enabled: " + Program.bluetoothEnabled.ToString()); //prints the value of bluetoothEnabled
        }

        /** Logs the number of bluetooth threads running, and names of the bluetooth threads running
         * 
         */ 
        public void recordActiveThreads()
        {
            Thread[] threads = BluetoothInterface.bluetoothThreads.ToArray(); //array containing all threads
            string[] lines = new string[threads.Length+1]; //array where lines will be stored
            lines[0] = "Number Of Bluetooth Threads: " + threads.Length; //first line to write
            for (int i = 0; i < threads.Length; i++)
            {
                try
                { //this is just paranoia
                    lines[i + 1] = threads[i].Name; //adds name of thread to write list
                } catch
                {

                }
            }
            writeLines(lines); //records all bluetooth threads
        }

        /** Records that a device has connected
         *  @param deviceName the name of the device in question
         */ 
        public void recordDeviceConnection(string deviceName)
        {
            writeLine(deviceName + " Has Connected To The Server, A New Thread Has Been Opened To Handle New Connections");
        }

        /** Records that a device has disconnected
         *  @param deviceName the name of the device in question
         */
        public void recordDeviceDisconection(string deviceName)
        {
            writeLine(deviceName + " Has Disconnected From The Server");
        }

        /** @param e the exception to log
         *  Logs the stacktrace from an exception
         */ 
        public void recordException(Exception e)
        {
            writeLine(e.StackTrace);
        }
    }

    static class BluetoothInterface //every part of this class needs to be able to work if bluetooth is disabled
    {
        private static readonly Object threadListLock = new Object(); //lock on bluetoothThreads, critical
        public static readonly Object connectedDevicesLock = new Object(); //lock on connected devices, critical-ish
        private static readonly Object messageWriteLock = new Object(); //lock on appending messages to the ScoutingInfo.csv file, critical
        public static readonly Object unwrittenMessageLock = new Object(); //lock on the unwritten messages list, not too crititcal
        private static readonly Object eventListLock = new object(); //lock on event list, not critical

        public static BluetoothRadio myRadio; //bluetooth radio
        public static BluetoothClient client; //bluetooth client interface with computer hardware

        public static BluetoothAddress localAddress; //local bluetooth hardware mac address
        public static string localAddressString; //local MAC stored as a string

        public static List<string> connectedDevices; //list of connected devices
        public static List<Thread> bluetoothThreads; //list of bluetooth threads
        public static List<string> unwrittenMessages; //list representing the unwritten messages
        public static string[] eventList; //Array of events to be written to the UI

        /** Constructor for Bluetoothinterface, necesscary so I can include try catchs to avoid initialization mistakes
         * 
         */ 
        static BluetoothInterface()
        {
            try { //To avoid initialization exceptions, all this stuff might not initialize correctly
                myRadio = BluetoothRadio.PrimaryRadio; //bluetooth radio
                client = new BluetoothClient(); //bluetooth client interface with computer hrdware (maybe actually IDK)

                localAddress = myRadio.LocalAddress; //local bluetooth hardawre MAC address
                localAddressString = Program.stringInsert(localAddress.ToString(), 2, ':'); //local bluetooth hardware MAC address stored as a string
            } catch (TypeInitializationException e)
            {
                
            } catch (Exception e)
            {

            }
            connectedDevices = new List<string>(); //list of all the device names of currently connected devices, devices are added when they connect, and removed when they disconnect
            bluetoothThreads = new List<Thread>(); //list of all currently running bluetooth threads, is locked every time a new one is added or removed
            unwrittenMessages = new List<string>(); //list of all the messages that couldn't be written for some reason. Messages get removed once they are written
            eventList = new string[Program.UI_EVENT_LENGTH]; //list of events to be displayed by the UI in the eventList
        }

        /** gets a list of paired bluetooth devices, takes a while though (like 10-20 seconds) IDK why its just retrieveing a few tables from the OS
         *  @return a list of devices, may be null if bluetooth is disabled
         */
        static BluetoothDeviceInfo[] getDevices()
        {
            BluetoothDeviceInfo[] devices = null;
            try {
                devices = client.DiscoverDevices(Program.MAX_DEVICES_SCAN);
            } catch (Exception e)
            {

            }
            return devices;
        }

        /** Kills all bluetooth threads, then clears thread list
         * 
         */ 
        public static void killAllThreads() {
            try
            {
                lock (threadListLock)
                {
                    foreach (Thread thread in bluetoothThreads) //stops all threads
                    {
                        try
                        {
                            thread.Interrupt(); //interrupts thread
                            if (!(thread.Join(1000))) //if thread hasn't ended after 1 second
                            {
                                continue;
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    bluetoothThreads.Clear(); //clears the list
                }
            } catch (Exception e) //catches case where bluetoothThreads has 0 elements, not necesscary to do anything about it, 
            {

            }
        }

        /** Adds an event to the eventlist
         * 
         */ 
        public static void addEvent(string s)
        {
            lock (eventListLock) //avoid conflicts 
            {
                if (!(eventList[eventList.Length - 1] == null)) //if the last element in the eventlist isn't null
                {
                    Array.Copy(eventList, 1, eventList, 0, eventList.Length - 1);
                    eventList[eventList.Length - 1] = s;
                } else
                {
                    for (int i = 0; i < eventList.Length; i++)
                    {
                        if (eventList[i] == null)
                        {
                            eventList[i] = s;
                            break;
                        }
                    }
                }
            }
        }

        /** @param messages an array containing the messages to write
         *  Writes recieved messages to scoutinginfo.csv
         */ 
        public static void writeMessages(string[] messages)
        {
            if (!File.Exists("ScoutingInfo.csv")) //checks to make sure the scoutinginfo.cs exists
            {
                lock (messageWriteLock) //locks to avoid confliscts
                {
                    File.WriteAllText("ScoutingInfo.csv", Program.HEADERS); //writes headers
                }
            }
            foreach (string s in messages) //iterates through messages in messages
            {
                foreach (string line in s.Split('\n')) //iterates through each line in each message
                {
                    if (!(line == "" || line == ",,,,,,,,,,,,,,,,," || line == "end")) //filters out blank lines or lines filled with just commas or end
                    {
                        try //to avoid errors writing
                        {
                            lock (messageWriteLock) //locks to avoid conflicts
                            {
                                File.AppendAllText("ScoutingInfo.csv", line + "\n"); //writes line to scoutinginfo.csv (adds newline at end)
                            }
                        } catch (IOException e) //if couldn't write, probably the file is open in another program
                        {
                            lock (unwrittenMessageLock) //locks to avoid errors
                            {
                                unwrittenMessages.Add(line); //adds the unwritten message to the list

                                bool end = false; //exit for loop
                                int fileNum = 0; //fileNum to try
                                while (!end) //loops until end is true
                                {
                                    try
                                    {
                                        if (!(File.Exists("AdditionalScoutingInfo_" + fileNum + ".csv"))) //if file doesnt yet exist. add headers
                                        {
                                            File.WriteAllText("AdditionalScoutingInfo_" + fileNum + ".csv", Program.HEADERS); //writes headers
                                        }
                                        File.AppendAllText("AdditionalScoutingInfo_" + fileNum + ".csv", line + "\n"); //appends the unwritten message to the backup file
                                        end = true; //it is now ok for the program to end
                                    } catch (IOException q) //if the backup file is open too (seriously connor if the program ever gets here im gonna kill you)
                                    {
                                        fileNum += 1; //increment the fileNum by one and create a new backup file, in essence a backup-backup file (can happen as many times as necesscary, I may add a timeout if i need to)
                                    } catch (Exception q) //This shouldn't happen but it doesn't hurt to have it
                                    {
                                        //maybe break here, I need to think about it
                                    }
                                }
                            }
                        } catch (Exception e) //god knows what went wrong
                        {

                        }
                    }
                }
            }
        }

        /** Starts a new thread that waits for and handles connections
         *  Should be called after a new connection is established
         */
        public static void connectAsServer()
        {
            if (Program.bluetoothTest()) //only run if bluetooth is enabled
            {
                try
                {
                    new Thread(new ThreadStart(BluetoothInterface.waitConnectThread)).Start(); //Starts a new thread waiting for server connection to the list
                } catch (Exception e) //cant start the thread for some reason
                {
                    
                }
            }
        }

        /** waits for a device to connect, once a device connects, a new instance of this thread should be started
         *  
         */ 
         static void waitConnectThread()
        {
            lock (threadListLock) //locks to avoid errors
            {
                BluetoothInterface.bluetoothThreads.Add(Thread.CurrentThread); //updates bluetoothThreads
            }
            if (Thread.CurrentThread.Name == null) //not necesscary but I've had enough random errors so its going in
            {
                Thread.CurrentThread.Name = "Bluetooth Thread " + bluetoothThreads.Count; //names the thread if it hasn't already been named (Name is a write once property)
            }
            if (Program.bluetoothEnabled) {
                string deviceName = null; //the name of the device connected to this thread, starts null
                List<string> messages = new List<string>(); //creates a new string list to store any recieved messages that aren't blank or "end"
                try
                {
                    BluetoothListener serverListener = new BluetoothListener(Program.BLUETOOTH_UID); //creates a listener bluetooth connections on mac address
                    serverListener.Start(); //listens for a connection

                    BluetoothClient conn = null; //Prepares to accept and establish a client connection

                    var connection = Task.Run(() => serverListener.AcceptBluetoothClient()); //Accepts client, times out after 30 seconds
                    if (connection.Wait(TimeSpan.FromMinutes(1)))
                    {
                        conn = connection.Result; //establishes client connection if it didn't timeout
                    } else
                    {
                        
                    }

                    serverListener.Stop(); //stops listening for client connections
                    connectAsServer(); //opens another thread to handle a new client connection to a new server with the same uid

                    var stream = conn.GetStream(); // byte stream between server and client

                    byte[] deviceNameStream = new byte[1024]; //bytes will be written to this array
                    int bytesRead = stream.Read(deviceNameStream, 0, deviceNameStream.Length); //reads bytes from stream, variable bytesread stores the numkber of bytes read, not the bytes themselves, thos go to deviceNameStream

                    StringBuilder deviceNameBuilder = new StringBuilder();
                    deviceNameBuilder.AppendFormat("{0}", Encoding.ASCII.GetString(deviceNameStream, 0, bytesRead)); //builds string, name of device that just connected
                    deviceName = deviceNameBuilder.ToString(); //stores name of connected device
                    lock (connectedDevicesLock) //prevent interference
                    {
                        connectedDevices.Add(deviceName); //upodates connected devices
                    }

                    addEvent(deviceName + " Has Connected");

                    bool lastBlank = false; //if the last message recieved over the stream had no content, this will be true
                    bool end = false; //terminates connection when this is true

                    while (!(end))
                    {

                        byte[] streamData = new byte[8192]; //messages from the stream are written here
                        bytesRead = stream.Read(streamData, 0, streamData.Length); //reads the stream, storing any messages in streamData

                        StringBuilder message = new StringBuilder();
                        message.AppendFormat("{0}", Encoding.ASCII.GetString(streamData, 0, bytesRead)); //formats the message recieved from the connected device

                        addEvent("" + deviceName + " Says: " + Regex.Replace(message.ToString(), @"\t|\n|\r", "") + "");

                        if (message.ToString() == "end" || Array.IndexOf(message.ToString().Split('\n'), "end") != -1) //if the message says "end", or contains end preceded and followed by newlines, terminate the loop, and end the connection
                        {
                            messages.Add(message.ToString()); //necesscary in case end is at the end of a csv to write, but not properly seperated (writemessages will filter out the "end" anyways)
                            end = true;
                        }
                        else if (message.ToString() == "" && !(lastBlank)) //if the message is blank, update the value of lastblank
                        {
                            lastBlank = true;
                        }
                        else if (message.ToString() == "" && lastBlank) //if this message is blank, and the previous one was as well, end the loop and connection
                        {
                            end = true;
                        }
                        else //an actual message was recieved
                        {
                            lastBlank = false; //update the value of lastBlank
                            messages.Add(message.ToString()); //adds the message to the messages list
                        }
                        if (!Program.bluetoothEnabled) //ends if bluetooth isn't enabled
                        {
                            end = true;
                        }
                    }
                    stream.Write(Encoding.ASCII.GetBytes("end"), 0, Encoding.ASCII.GetByteCount("end")); //lets the client know the message was recieved
                    conn.Close(); //end the connection once the device disconnects
                    writeMessages(messages.ToArray<string>()); //writes messages
                    messages.Clear(); //clears messages array
                    lock (connectedDevicesLock)
                    {
                        connectedDevices.Remove(deviceName); //updates connected devices
                    }
                } catch (Exception e)                {
                    if (!(deviceName == null)) //only run if deviceName has been updated (i.e. a device has connected)
                    {
                        lock (connectedDevicesLock) //removes references to currently connected device from connected devices
                        {
                            connectedDevices.Remove(deviceName); //updates connected devices
                        }
                    }
                    if (messages.Count > 0) //if there are messages that haven't yet been written to the server
                    {
                        writeMessages(messages.ToArray<string>()); //writes messages
                        messages.Clear(); //clears messages list
                    }
                }
            }
            //thread ends in the event of an error, or after it has run 
            lock (threadListLock) //locks to avoid conflicts
            {
                BluetoothInterface.bluetoothThreads.Remove(Thread.CurrentThread); //updates bluetooththreads
            }
        }
    }
}
