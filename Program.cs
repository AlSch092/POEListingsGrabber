using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

//all in one file. next time I edit/add to this Ill make it a bit more readable.
namespace ListingGrabber
{
    class ListingResult
    {
        public string Buyout; //data-buyout
        public string CharacterName; //data-ign
        public string League; //data-league
        public string ItemName; //data-name
        public string StashTabName; //data-tab
        public int CellX; //data-x
        public int CellY; //data-y
        public int DesiredQuantity;
    } 

    class Program
    {
        public static int MAX_BYTES = 1000000;
        public static int MAX_LISTINGS = 1000;

        public static ListingResult[] ResultList = new ListingResult[MAX_LISTINGS];

        public static string GetSubstring(string l_Source, string l_FindCharacter)
        {
            if (!l_Source.Contains(l_FindCharacter)) return string.Empty;
            int End = l_Source.IndexOf(l_FindCharacter, 0) + l_FindCharacter.Length;
            Console.WriteLine(End);
            return l_Source.Substring(0, End - 1);
        }

        public static async Task WriteFile(string name, string text)
        {
            using (StreamWriter file = new StreamWriter(name, append: true))
                await file.WriteLineAsync(text);
        }

        public static async void GetItemListings(string server, string itemName, int itemQuantity, string parameters) //all done in 4hours
        {
            ServicePointManager.ServerCertificateValidationCallback = new
            RemoteCertificateValidationCallback //SSL hack
            (
                delegate { return true; }
            );

            Uri WebsiteURI = new Uri("https://poe.trade");

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(WebsiteURI);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.AllowAutoRedirect = true;

            byte[] ouat = new byte[MAX_BYTES];
            byte[] searchResult = new byte[MAX_BYTES];

            WebResponse r = request.GetResponse();
            int result1 = r.GetResponseStream().Read(ouat, 0, MAX_BYTES);

            // From byte array to string
            string s = System.Text.Encoding.UTF8.GetString(ouat, 0, ouat.Length);

            //REQUEST 2: SEARCH
            HttpWebRequest request2 = (HttpWebRequest)HttpWebRequest.Create(WebsiteURI + "search");
            request2.Method = "POST";
            request2.ContentType = "application/x-www-form-urlencoded";
            request2.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request2.AllowAutoRedirect = true;

            string postData = parameters;

            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byte1 = encoding.GetBytes(postData);

            Stream newStream = request2.GetRequestStream();
            newStream.Write (byte1, 0, byte1.Length);

            WebResponse response = (HttpWebResponse)request2.GetResponse();
            int result2 = response.GetResponseStream().Read(searchResult, 0, MAX_BYTES);

            s = System.Text.Encoding.UTF8.GetString(searchResult, 0, MAX_BYTES);

            int i = 0;

            int readIndex, endIndex;

            int index_buyout = s.IndexOf("data-buyout", 0);
            int index_ign = s.IndexOf("data-ign", 0);
            int index_league = s.IndexOf("data-league", 0);
            int index_ItemName = s.IndexOf("data-name", 0);
            int index_TabName = s.IndexOf("data-tab", 0);
            int index_X = s.IndexOf("data-x", 0);
            int index_Y = s.IndexOf("data-y", 0);

            await WriteFile("out.txt", "Results for " + itemName + " in " + server + ":");

            while (index_buyout > 0)
            {
                ListingResult result = new ListingResult();

                readIndex = index_buyout + "data-buyout".Length + 2; // = " for +2
                endIndex = s.IndexOf("\"", readIndex); //get closing quotation for end of line...
                string cost = s.Substring(readIndex, endIndex - readIndex); //tries to get string inbetween "..."
                result.Buyout = cost;

                readIndex = index_ign + "data-ign".Length + 2;
                endIndex = s.IndexOf("\"", readIndex); 
                string IGN = s.Substring(readIndex, endIndex - readIndex);
                result.CharacterName = IGN;

                readIndex = index_league + "data-league".Length + 2;
                endIndex = s.IndexOf("\"", readIndex);
                string league = s.Substring(readIndex, endIndex - readIndex);
                result.League = league;

                readIndex = index_ItemName + "data-name".Length + 2;
                endIndex = s.IndexOf("\"", readIndex); 
                string ItemName = s.Substring(readIndex, endIndex - readIndex);
                result.ItemName = ItemName;

                readIndex = index_TabName + "data-tab".Length + 2;
                endIndex = s.IndexOf("\"", readIndex); 
                string TabName = s.Substring(readIndex, endIndex - readIndex);
                result.StashTabName = TabName;

                readIndex = index_X + "data-x".Length + 2;
                endIndex = s.IndexOf("\"", readIndex); 
                string X = s.Substring(readIndex, endIndex - readIndex);
                result.CellX = Convert.ToInt32(X);

                readIndex = index_Y + "data-y".Length + 2;
                endIndex = s.IndexOf("\"", readIndex); 
                string Y = s.Substring(readIndex, endIndex - readIndex);
                result.CellY = Convert.ToInt32(Y);

                string Whisper = CreateWhisper(cost, IGN, league, ItemName, TabName, result.CellX, result.CellY);
                Console.WriteLine("Listing {0}: Price {1}, IGN: {2} for {3}, in Stash Tab {4} slots X: {5}, Y: {6}", i, cost, IGN, ItemName, TabName, X, Y);
                Console.WriteLine(Whisper);
                string WriteFileStr = "[" + league + "] " + cost + ", from IGN " + IGN + " at Tab " + TabName + " slots " + X + "," + Y;
           
                await WriteFile("out.txt", WriteFileStr);
                await WriteFile("whispers.txt", Whisper);

                index_buyout = s.IndexOf("data-buyout", index_Y);

                if(index_buyout > 0)
                    index_ign = s.IndexOf("data-ign", index_buyout);

                if (index_ign > 0)
                    index_league = s.IndexOf("data-league", index_ign);

                index_ItemName = s.IndexOf("data-name", index_league);
                index_TabName = s.IndexOf("data-tab", index_ItemName);
                index_X = s.IndexOf("data-x", index_TabName);
                index_Y = s.IndexOf("data-y", index_X);

                ResultList[i] = result;

                i += 1;                
            }

        }

        static void ReadFileAndGetListings(string fileName)
        {
            string itemName = string.Empty;
            string server = string.Empty;
            string parameters = string.Empty;

            string[] lines = System.IO.File.ReadAllLines(@fileName);
            int counter = 0;

            // Display the file contents by using a foreach loop.
            foreach (string line in lines)
            {
                if(counter == 0)
                {
                    server = line;
                    parameters = "league=" + server + "&";
                }
                else if(counter == 1)
                {
                    itemName = line;
                    parameters += "name=" + itemName + "&";
                }
                else
                {
                    parameters += line + "&";
                }

                counter += 1;
            }


            GetItemListings(server, itemName, 1, parameters);
        }

        static string CreateWhisper(string Buyout, string CharacterName, string League, string ItemName, string StashTabName, int CellX, int CellY)
        {
            string Whisper = "@" + CharacterName + " ";
            Whisper += "Hi, I would like to buy your " + ItemName + " ";
            Whisper += "listed for " + Buyout + " ";
            Whisper += "in " + League + " ";
            Whisper += "(stash tab \"" + StashTabName + "\"; ";
            Whisper += "position: left " + CellX + ", top " + CellY + ")";

            return Whisper;
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if(args.Length == 2)
            {
                GetItemListings(args[0], args[1], 1, "league=" + args[0] + "&type=&base=&name=" + args[1] + "&dmg_min=&dmg_max=&aps_min=&aps_max=&crit_min=&crit_max=&dps_min=&dps_max=&edps_min=&edps_max=&pdps_min=&pdps_max=&armour_min=&armour_max=&evasion_min=&evasion_max=&shield_min=&shield_max=&block_min=&block_max=&sockets_min=&sockets_max=&link_min=&link_max=&sockets_r=&sockets_g=&sockets_b=&sockets_w=&linked_r=&linked_g=&linked_b=&linked_w=&rlevel_min=&rlevel_max=&rstr_min=&rstr_max=&rdex_min=&rdex_max=&rint_min=&rint_max=&mod_name=&mod_min=&mod_max=&mod_weight=&group_type=And&group_min=&group_max=&group_count=1&q_min=&q_max=&level_min=&level_max=&ilvl_min=&ilvl_max=&rarity=&progress_min=&progress_max=&sockets_a_min=&sockets_a_max=&map_series=&altart=&identified=&corrupted=&crafted=&enchanted=&fractured=&synthesised=&mirrored=&veiled=&shaper=&elder=&crusader=&redeemer=&hunter=&warlord=&replica=&seller=&thread=&online=x&capquality=x&buyout_min=&buyout_max=&buyout_currency=&has_buyout=1&exact_currency=x");
            }
            else if(args.Length == 1)
            {
                ReadFileAndGetListings(args[0]);
            }
            else
            {
                Console.WriteLine("Usage (command line): ./ListingGrabber <server> <itemName>");
                Console.WriteLine("Usage (input file): ./ListingGrabber fileName");
                Console.ReadLine();
            }
        }
    }
}
