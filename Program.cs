using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace read_wd_dump
{
    class Program
    {

        public class subclassclass
        {
            public string label = "";
            public List<int> up = new List<int>();
            public List<int> down = new List<int>();
            public int instances = 0;
        }
        public static Dictionary<int, subclassclass> subclassdict = new Dictionary<int, subclassclass>();
        public static Dictionary<string, int> propdict = new Dictionary<string, int>(); //from wikidata property name to property id
        public static Dictionary<string, int> classdict = new Dictionary<string, int>(); //from wikidata class name to item id
        public static Dictionary<string, int> claimstatdict = new Dictionary<string, int>(); //statistics on how many claims for each property
        public static Dictionary<string, int> occupationstatdict = new Dictionary<string, int>(); //statistics on how many claims for each property
        public static Dictionary<string, int> countrystatdict = new Dictionary<string, int>(); //statistics on how many claims for each property
        public static Dictionary<string, int> positionstatdict = new Dictionary<string, int>(); //statistics on how many claims for each property
        public static Dictionary<string, string> propnamedict = new Dictionary<string, string>(); //from prop ID to prop label
        public static Dictionary<string, string> labeldict = new Dictionary<string, string>(); //from item ID to label

        public static List<string> wanted_labels = new List<string>();

        public class wdtopclass
        {
            public string id = "";
            public string type = "";
            public Dictionary<string, labelclass> labels = new Dictionary<string, labelclass>();
            public Dictionary<string, labelclass> descriptions = new Dictionary<string, labelclass>();
            public Dictionary<string, labelclass> aliases = new Dictionary<string, labelclass>();
            public string claims = "";
            public Dictionary<string, sitelinkclass> sitelinks = new Dictionary<string, sitelinkclass>();
            public int lastrevid = 0;
            public string modified = "";
        }

        public class wdtopclass2
        {
            public string id { get; set; }
            public string type { get; set; }
            public string labels { get; set; }
            public string descriptions { get; set; }
            public string aliases { get; set; }
            public string claims { get; set; }
            public string sitelinks { get; set; }
            public int lastrevid { get; set; }
            public string modified { get; set; }
        }

        public class sitelinkclass
        {
            public string site = "";
            public string title = "";
            public List<string> badges = new List<string>();
        }

        public class labelclass
        {
            public string lang = "";
            public string value = "";
        }

        public static string[] preferred_languages = { "en", "sv", "es", "de", "fr", "ceb", "da", "no","it","pt","en-gb","en-ca","tl","war","nl" };
        public static int BCoffset = 3000;

        public static int iqentity = 35120;
        public static int[] humanprops = { 21, 27, 569, 106, 39, 28640 };

        public static void fill_propdict()
        {
            propdict.Add("country", 17);
            propdict.Add("capital", 36);
            propdict.Add("commonscat", 373);
            propdict.Add("coat of arms", 94);
            propdict.Add("locatormap", 242);
            propdict.Add("flag", 41);
            propdict.Add("timezone", 421);
            propdict.Add("kids", 150);
            propdict.Add("parent", 131);
            propdict.Add("iso", 300);
            propdict.Add("borders", 47);
            propdict.Add("coordinates", 625);
            propdict.Add("inception", 571);
            propdict.Add("head of government", 6);
            propdict.Add("gnid", 1566);
            propdict.Add("follows", 155);
            propdict.Add("category dead", 1465);
            propdict.Add("category born", 1464);
            propdict.Add("category from", 1792);
            propdict.Add("image", 18);
            propdict.Add("banner", 948);
            //propdict.Add("sister city",190);
            propdict.Add("postal code", 281);
            propdict.Add("position", 625);
            propdict.Add("population", 1082);
            propdict.Add("instance", 31);
            propdict.Add("subclass", 279);
            propdict.Add("nexttowater", 206);

            propdict.Add("instance_of", 31);
            propdict.Add("subclass_of", 279);
            propdict.Add("sex", 21);
            propdict.Add("citizenship", 27);
            propdict.Add("birthdate", 569);
            propdict.Add("occupation", 106);
            propdict.Add("position_held", 39);
            propdict.Add("profession", 28640);
            //propdict.Add("",);

            //propdict.Add("",);

            classdict.Add("human", 5);
            //classdict.Add("",);
            //classdict.Add("",);
            //classdict.Add("",);

            using (StreamReader sr = new StreamReader(@"D:\Downloads\wikidata-properties.txt"))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    if (String.IsNullOrEmpty(s))
                        continue;
                    string[] words = s.Split('\t');
                    propnamedict.Add(words[0], words[1]);
                }

            }
        }

        public static bool human_properties(JObject wd)
        {
            int nprop = 0;

            Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

            for (int i=0; i<humanprops.Length;i++)
            {
                if (get_claims(wd, i, dictObj).Count > 0)
                    nprop++;
            }

            Console.WriteLine(nprop + " human properties");
            return (nprop >= 3);
        }


        public static int tryconvert(string word)
        {
            int i = -1;

            if (word.Length == 0)
                return i;

            try
            {
                i = Convert.ToInt32(word);
            }
            catch (OverflowException)
            {
                Console.WriteLine("i Outside the range of the Int32 type: " + word);
            }
            catch (FormatException)
            {
                //if ( !String.IsNullOrEmpty(word))
                //    Console.WriteLine("i Not in a recognizable format: " + word);
            }

            return i;

        }

        public static bool is_latin(string name)
        {
            return (get_alphabet(name) == "latin");
        }

        public static string get_alphabet(string name)
        {
            char[] letters = name.ToCharArray();
            int n = 0;
            int sum = 0;
            //int nlatin = 0;
            Dictionary<string, int> alphdir = new Dictionary<string, int>();
            foreach (char c in letters)
            {
                int uc = Convert.ToInt32(c);
                sum += uc;
                string alphabet = "none";
                if (uc <= 0x0040) alphabet = "none";
                //else if ((uc >= 0x0030) && (uc <= 0x0039)) alphabet = "number";
                //else if ((uc >= 0x0020) && (uc <= 0x0040)) alphabet = "punctuation";
                else if ((uc >= 0x0041) && (uc <= 0x007F)) alphabet = "latin";
                else if ((uc >= 0x00A0) && (uc <= 0x00FF)) alphabet = "latin";
                else if ((uc >= 0x0100) && (uc <= 0x017F)) alphabet = "latin";
                else if ((uc >= 0x0180) && (uc <= 0x024F)) alphabet = "latin";
                else if ((uc >= 0x0250) && (uc <= 0x02AF)) alphabet = "phonetic";
                else if ((uc >= 0x02B0) && (uc <= 0x02FF)) alphabet = "spacing modifier letters";
                else if ((uc >= 0x0300) && (uc <= 0x036F)) alphabet = "combining diacritical marks";
                else if ((uc >= 0x0370) && (uc <= 0x03FF)) alphabet = "greek and coptic";
                else if ((uc >= 0x0400) && (uc <= 0x04FF)) alphabet = "cyrillic";
                else if ((uc >= 0x0500) && (uc <= 0x052F)) alphabet = "cyrillic";
                else if ((uc >= 0x0530) && (uc <= 0x058F)) alphabet = "armenian";
                else if ((uc >= 0x0590) && (uc <= 0x05FF)) alphabet = "hebrew";
                else if ((uc >= 0x0600) && (uc <= 0x06FF)) alphabet = "arabic";
                else if ((uc >= 0x0700) && (uc <= 0x074F)) alphabet = "syriac";
                else if ((uc >= 0x0780) && (uc <= 0x07BF)) alphabet = "thaana";
                else if ((uc >= 0x0900) && (uc <= 0x097F)) alphabet = "devanagari";
                else if ((uc >= 0x0980) && (uc <= 0x09FF)) alphabet = "bengali";
                else if ((uc >= 0x0A00) && (uc <= 0x0A7F)) alphabet = "gurmukhi";
                else if ((uc >= 0x0A80) && (uc <= 0x0AFF)) alphabet = "gujarati";
                else if ((uc >= 0x0B00) && (uc <= 0x0B7F)) alphabet = "oriya";
                else if ((uc >= 0x0B80) && (uc <= 0x0BFF)) alphabet = "tamil";
                else if ((uc >= 0x0C00) && (uc <= 0x0C7F)) alphabet = "telugu";
                else if ((uc >= 0x0C80) && (uc <= 0x0CFF)) alphabet = "kannada";
                else if ((uc >= 0x0D00) && (uc <= 0x0D7F)) alphabet = "malayalam";
                else if ((uc >= 0x0D80) && (uc <= 0x0DFF)) alphabet = "sinhala";
                else if ((uc >= 0x0E00) && (uc <= 0x0E7F)) alphabet = "thai";
                else if ((uc >= 0x0E80) && (uc <= 0x0EFF)) alphabet = "lao";
                else if ((uc >= 0x0F00) && (uc <= 0x0FFF)) alphabet = "tibetan";
                else if ((uc >= 0x1000) && (uc <= 0x109F)) alphabet = "myanmar";
                else if ((uc >= 0x10A0) && (uc <= 0x10FF)) alphabet = "georgian";
                else if ((uc >= 0x1100) && (uc <= 0x11FF)) alphabet = "korean";
                else if ((uc >= 0x1200) && (uc <= 0x137F)) alphabet = "ethiopic";
                else if ((uc >= 0x13A0) && (uc <= 0x13FF)) alphabet = "cherokee";
                else if ((uc >= 0x1400) && (uc <= 0x167F)) alphabet = "unified canadian aboriginal syllabics";
                else if ((uc >= 0x1680) && (uc <= 0x169F)) alphabet = "ogham";
                else if ((uc >= 0x16A0) && (uc <= 0x16FF)) alphabet = "runic";
                else if ((uc >= 0x1700) && (uc <= 0x171F)) alphabet = "tagalog";
                else if ((uc >= 0x1720) && (uc <= 0x173F)) alphabet = "hanunoo";
                else if ((uc >= 0x1740) && (uc <= 0x175F)) alphabet = "buhid";
                else if ((uc >= 0x1760) && (uc <= 0x177F)) alphabet = "tagbanwa";
                else if ((uc >= 0x1780) && (uc <= 0x17FF)) alphabet = "khmer";
                else if ((uc >= 0x1800) && (uc <= 0x18AF)) alphabet = "mongolian";
                else if ((uc >= 0x1900) && (uc <= 0x194F)) alphabet = "limbu";
                else if ((uc >= 0x1950) && (uc <= 0x197F)) alphabet = "tai le";
                else if ((uc >= 0x19E0) && (uc <= 0x19FF)) alphabet = "khmer";
                else if ((uc >= 0x1D00) && (uc <= 0x1D7F)) alphabet = "phonetic";
                else if ((uc >= 0x1E00) && (uc <= 0x1EFF)) alphabet = "latin";
                else if ((uc >= 0x1F00) && (uc <= 0x1FFF)) alphabet = "greek and coptic";
                else if ((uc >= 0x2000) && (uc <= 0x206F)) alphabet = "none";
                else if ((uc >= 0x2070) && (uc <= 0x209F)) alphabet = "none";
                else if ((uc >= 0x20A0) && (uc <= 0x20CF)) alphabet = "none";
                else if ((uc >= 0x20D0) && (uc <= 0x20FF)) alphabet = "combining diacritical marks for symbols";
                else if ((uc >= 0x2100) && (uc <= 0x214F)) alphabet = "letterlike symbols";
                else if ((uc >= 0x2150) && (uc <= 0x218F)) alphabet = "none";
                else if ((uc >= 0x2190) && (uc <= 0x21FF)) alphabet = "none";
                else if ((uc >= 0x2200) && (uc <= 0x22FF)) alphabet = "none";
                else if ((uc >= 0x2300) && (uc <= 0x23FF)) alphabet = "none";
                else if ((uc >= 0x2400) && (uc <= 0x243F)) alphabet = "none";
                else if ((uc >= 0x2440) && (uc <= 0x245F)) alphabet = "optical character recognition";
                else if ((uc >= 0x2460) && (uc <= 0x24FF)) alphabet = "enclosed alphanumerics";
                else if ((uc >= 0x2500) && (uc <= 0x257F)) alphabet = "none";
                else if ((uc >= 0x2580) && (uc <= 0x259F)) alphabet = "none";
                else if ((uc >= 0x25A0) && (uc <= 0x25FF)) alphabet = "none";
                else if ((uc >= 0x2600) && (uc <= 0x26FF)) alphabet = "none";
                else if ((uc >= 0x2700) && (uc <= 0x27BF)) alphabet = "none";
                else if ((uc >= 0x27C0) && (uc <= 0x27EF)) alphabet = "none";
                else if ((uc >= 0x27F0) && (uc <= 0x27FF)) alphabet = "none";
                else if ((uc >= 0x2800) && (uc <= 0x28FF)) alphabet = "braille";
                else if ((uc >= 0x2900) && (uc <= 0x297F)) alphabet = "none";
                else if ((uc >= 0x2980) && (uc <= 0x29FF)) alphabet = "none";
                else if ((uc >= 0x2A00) && (uc <= 0x2AFF)) alphabet = "none";
                else if ((uc >= 0x2B00) && (uc <= 0x2BFF)) alphabet = "none";
                else if ((uc >= 0x2E80) && (uc <= 0x2EFF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x2F00) && (uc <= 0x2FDF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x2FF0) && (uc <= 0x2FFF)) alphabet = "none";
                else if ((uc >= 0x3000) && (uc <= 0x303F)) alphabet = "chinese/japanese";
                else if ((uc >= 0x3040) && (uc <= 0x309F)) alphabet = "chinese/japanese";
                else if ((uc >= 0x30A0) && (uc <= 0x30FF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x3100) && (uc <= 0x312F)) alphabet = "bopomofo";
                else if ((uc >= 0x3130) && (uc <= 0x318F)) alphabet = "korean";
                else if ((uc >= 0x3190) && (uc <= 0x319F)) alphabet = "chinese/japanese";
                else if ((uc >= 0x31A0) && (uc <= 0x31BF)) alphabet = "bopomofo";
                else if ((uc >= 0x31F0) && (uc <= 0x31FF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x3200) && (uc <= 0x32FF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x3300) && (uc <= 0x33FF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x3400) && (uc <= 0x4DBF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x4DC0) && (uc <= 0x4DFF)) alphabet = "none";
                else if ((uc >= 0x4E00) && (uc <= 0x9FFF)) alphabet = "chinese/japanese";
                else if ((uc >= 0xA000) && (uc <= 0xA48F)) alphabet = "chinese/japanese";
                else if ((uc >= 0xA490) && (uc <= 0xA4CF)) alphabet = "chinese/japanese";
                else if ((uc >= 0xAC00) && (uc <= 0xD7AF)) alphabet = "korean";
                else if ((uc >= 0xD800) && (uc <= 0xDB7F)) alphabet = "high surrogates";
                else if ((uc >= 0xDB80) && (uc <= 0xDBFF)) alphabet = "high private use surrogates";
                else if ((uc >= 0xDC00) && (uc <= 0xDFFF)) alphabet = "low surrogates";
                else if ((uc >= 0xE000) && (uc <= 0xF8FF)) alphabet = "private use area";
                else if ((uc >= 0xF900) && (uc <= 0xFAFF)) alphabet = "chinese/japanese";
                else if ((uc >= 0xFB00) && (uc <= 0xFB4F)) alphabet = "alphabetic presentation forms";
                else if ((uc >= 0xFB50) && (uc <= 0xFDFF)) alphabet = "arabic";
                else if ((uc >= 0xFE00) && (uc <= 0xFE0F)) alphabet = "variation selectors";
                else if ((uc >= 0xFE20) && (uc <= 0xFE2F)) alphabet = "combining half marks";
                else if ((uc >= 0xFE30) && (uc <= 0xFE4F)) alphabet = "chinese/japanese";
                else if ((uc >= 0xFE50) && (uc <= 0xFE6F)) alphabet = "small form variants";
                else if ((uc >= 0xFE70) && (uc <= 0xFEFF)) alphabet = "arabic";
                else if ((uc >= 0xFF00) && (uc <= 0xFFEF)) alphabet = "halfwidth and fullwidth forms";
                else if ((uc >= 0xFFF0) && (uc <= 0xFFFF)) alphabet = "specials";
                else if ((uc >= 0x10000) && (uc <= 0x1007F)) alphabet = "linear b";
                else if ((uc >= 0x10080) && (uc <= 0x100FF)) alphabet = "linear b";
                else if ((uc >= 0x10100) && (uc <= 0x1013F)) alphabet = "aegean numbers";
                else if ((uc >= 0x10300) && (uc <= 0x1032F)) alphabet = "old italic";
                else if ((uc >= 0x10330) && (uc <= 0x1034F)) alphabet = "gothic";
                else if ((uc >= 0x10380) && (uc <= 0x1039F)) alphabet = "ugaritic";
                else if ((uc >= 0x10400) && (uc <= 0x1044F)) alphabet = "deseret";
                else if ((uc >= 0x10450) && (uc <= 0x1047F)) alphabet = "shavian";
                else if ((uc >= 0x10480) && (uc <= 0x104AF)) alphabet = "osmanya";
                else if ((uc >= 0x10800) && (uc <= 0x1083F)) alphabet = "cypriot syllabary";
                else if ((uc >= 0x1D000) && (uc <= 0x1D0FF)) alphabet = "byzantine musical symbols";
                else if ((uc >= 0x1D100) && (uc <= 0x1D1FF)) alphabet = "musical symbols";
                else if ((uc >= 0x1D300) && (uc <= 0x1D35F)) alphabet = "tai xuan jing symbols";
                else if ((uc >= 0x1D400) && (uc <= 0x1D7FF)) alphabet = "none";
                else if ((uc >= 0x20000) && (uc <= 0x2A6DF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x2F800) && (uc <= 0x2FA1F)) alphabet = "chinese/japanese";
                else if ((uc >= 0xE0000) && (uc <= 0xE007F)) alphabet = "none";

                bool ucprint = false;
                if (alphabet != "none")
                {
                    n++;
                    if (!alphdir.ContainsKey(alphabet))
                        alphdir.Add(alphabet, 0);
                    alphdir[alphabet]++;
                }
                else if (uc != 0x0020)
                {
                    //Console.Write("c=" + c.ToString() + ", uc=0x" + uc.ToString("x5") + "|");
                    //ucprint = true;
                }
                if (ucprint)
                    Console.WriteLine();
            }

            int nmax = 0;
            string alphmax = "none";
            foreach (string alph in alphdir.Keys)
            {
                //Console.WriteLine("ga:" + alph + " " + alphdir[alph].ToString());
                if (alphdir[alph] > nmax)
                {
                    nmax = alphdir[alph];
                    alphmax = alph;
                }
            }

            if (letters.Length > 2 * n) //mostly non-alphabetic
                return "none";
            else if (nmax > n / 2) //mostly same alphabet
                return alphmax;
            else
                return "mixed"; //mixed alphabets
        }

        public static string get_best_label(JObject wd)
        {
            string label = "(no label)";
            if (wd["labels"].HasValues)
            {
                Dictionary<string, object> dictObj = wd["labels"].ToObject<Dictionary<string, object>>();
                for (int ilang=0; ilang<preferred_languages.Length; ilang++)
                    if ( dictObj.ContainsKey(preferred_languages[ilang]))
                    {
                        return wd["labels"][preferred_languages[ilang]]["value"].ToString();
                    }
                foreach (string lang in dictObj.Keys)
                {
                    label = wd["labels"][lang]["value"].ToString();
                    if ( is_latin(label))
                        return label;
                }
                foreach (string lang in dictObj.Keys)
                {
                    label = wd["labels"][lang]["value"].ToString();
                    return label;
                }
            }
            return label;
        }

        public static void separate_properties(string dumpfn, string propfn)
        {
            char[] trimchars = "[,]".ToCharArray();

            int nlines = 0;
            using (StreamWriter sw = new StreamWriter(propfn))
            using (StreamReader sr = new StreamReader(dumpfn))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    nlines++;
                    JObject wd = JObject.Parse(s);
                    if (nlines % 1000 == 0)
                    {
                        Console.WriteLine(nlines + " " + wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }
                    if (wd["type"].ToString() != "item")
                    {
                        Console.WriteLine(wd["id"]);// +" "+wd.labels["en"].value);
                        //Console.ReadLine();
                        sw.WriteLine(s);
                    }
                }
            }
        }

        public static List<DateTime> get_time_claims(JObject wd, string prop, Dictionary<string, object> dictObj)
        {
            List<DateTime> rl = new List<DateTime>();
            if (dictObj.ContainsKey(prop))
            {
                string bestrank = "";
                for (int iclaim = 0; iclaim < wd["claims"][prop].Count(); iclaim++)
                {
                    string rank = wd["claims"][prop][iclaim]["rank"].ToString();
                    if (rank == "preferred")
                    {
                        bestrank = "preferred";
                        break;
                    }
                    else if (rank == "normal")
                        bestrank = "normal";
                }
                for (int iclaim = 0; iclaim < wd["claims"][prop].Count(); iclaim++)
                {
                    string rank = wd["claims"][prop][iclaim]["rank"].ToString();
                    if (rank != bestrank)
                        continue;
                    string wdtype = wd["claims"][prop][iclaim]["mainsnak"]["datavalue"]["type"].ToString();
                    //Console.WriteLine(prop + ":" + wdtype);
                    if ( wdtype == "time")
                    {
                        string rs = wd["claims"][prop][iclaim]["mainsnak"]["datavalue"]["value"]["time"].ToString();
                        string precision = wd["claims"][prop][iclaim]["mainsnak"]["datavalue"]["value"]["precision"].ToString();
                        Console.WriteLine("get_wd_year:rs: " + rs);
                        if (String.IsNullOrEmpty(rs))
                            continue;

                        bool bc = (rs[0] == '-');
                        Console.WriteLine("bc = " + bc);

                        rs = rs.Remove(0, 1);
                        if (rs.Contains("-00-00"))
                            rs = rs.Replace("-00-00", "-01-01");

                        DateTime rt = new DateTime(9999, 1, 1);
                        try
                        {
                            rt = DateTime.Parse(rs);
                            if (bc)
                                rt = rt.AddYears(BCoffset);

                            double iprec = tryconvert(precision);
                            Console.WriteLine("iprec =" + iprec);
                            if (iprec < 0)
                                iprec = 0;
                            Console.WriteLine("precision =" + precision);
                            rt = rt.AddSeconds(iprec);
                            rl.Add(rt);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                    }
                }

            }

            return rl;
        }

        public static int qtoint (string q)
        {
            return tryconvert(q.Replace("Q", ""));
        }

        public static string get_datavalue(JToken jd)
        {
            string s = "";
            string wdtype = "";
            if (jd["datavalue"] != null)
                wdtype = jd["datavalue"]["type"].ToString();
            //Console.WriteLine(prop + ":" + wdtype);
            switch (wdtype)
            {
                case "string":
                    s = jd["datavalue"]["value"].ToString();
                    break;
                case "wikibase-entityid":
                    s = "Q" + jd["datavalue"]["value"]["numeric-id"].ToString();
                    break;
                case "quantity":
                    s = jd["datavalue"]["value"]["amount"].ToString();
                    break;
            }

            return s;
        }

        public static List<string> get_claims(JObject wd, int iprop, Dictionary<string, object> dictObj)
        {
            string prop = "P" + iprop.ToString();
            return get_claims(wd, prop, dictObj);
        }

        public static List<string> get_claims(JObject wd, string prop)
        {
            Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();
            return get_claims(wd, prop, dictObj);
        }

        public static List<string> get_claims(JObject wd, string prop, Dictionary<string, object> dictObj)
        {
            List<string> rl = new List<string>();

            if ( dictObj.ContainsKey(prop))
            {
                string bestrank = "";
                for (int iclaim = 0; iclaim < wd["claims"][prop].Count(); iclaim++)
                {
                    string rank = wd["claims"][prop][iclaim]["rank"].ToString();
                    if (rank == "preferred")
                    {
                        bestrank = "preferred";
                        break;
                    }
                    else if (rank == "normal")
                        bestrank = "normal";
                }
                for (int iclaim = 0; iclaim < wd["claims"][prop].Count(); iclaim++)
                {
                    string rank = wd["claims"][prop][iclaim]["rank"].ToString();
                    if (rank != bestrank)
                        continue;
                    string datavalue = get_datavalue(wd["claims"][prop][iclaim]["mainsnak"]);
                    if (!String.IsNullOrEmpty(datavalue))
                        rl.Add(datavalue);
                    //    Console.WriteLine(key + ":" + wd["claims"][key][iclaim]["mainsnak"]["datavalue"]["value"]);
                }

            }
            return rl;
        }

        public static void map_subclasses(JObject wd)
        {
            if (wd["type"].ToString() != "item")
                return;

            int iq = qtoint(wd["id"].ToString());
            if (iq < 0)
                return;

            Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

            List<string> instancelist = get_claims(wd, propdict["instance_of"], dictObj);
            List<string> subclasslist = get_claims(wd, propdict["subclass_of"], dictObj);

            foreach (string qi in instancelist)
            {
                int iqi = qtoint(qi);
                if ( !subclassdict.ContainsKey(iqi))
                {
                    subclassclass sc = new subclassclass();
                    subclassdict.Add(iqi, sc);
                }
                subclassdict[iqi].instances++;
            }

            if ( subclasslist.Count > 0)
            {
                if (!subclassdict.ContainsKey(iq))
                {
                    subclassclass sc = new subclassclass();
                    subclassdict.Add(iq, sc);
                }
            }

            foreach (string qi in subclasslist)
            {
                int iqi = qtoint(qi);
                if (!subclassdict.ContainsKey(iqi))
                {
                    subclassclass sc = new subclassclass();
                    subclassdict.Add(iqi, sc);
                }
                if (!subclassdict[iqi].down.Contains(iq))
                    subclassdict[iqi].down.Add(iq);
                if (!subclassdict[iq].up.Contains(iqi))
                    subclassdict[iq].up.Add(iqi);
            }

            if (subclassdict.ContainsKey(iq))
            {
                subclassdict[iq].label = get_best_label(wd);
                Console.WriteLine(subclassdict.Count + " subclasses. " + subclassdict[iq].label);
            }

            
        }

        public static void print_subclasses()
        {
            using (StreamWriter sw = new StreamWriter(@"D:\Downloads\wikidata-subclasses-new.txt"))
                foreach (int iq in subclassdict.Keys)
                {
                    Console.WriteLine(iq + " " + subclassdict[iq].label + " Up:" + subclassdict[iq].up.Count + " Down:" + subclassdict[iq].down.Count + " Instances:" + subclassdict[iq].instances);
                    sw.Write(iq + "\t" + subclassdict[iq].label + "\t" + subclassdict[iq].up.Count + "\t" + subclassdict[iq].down.Count + "\t" + subclassdict[iq].instances);
                    foreach (int us in subclassdict[iq].up)
                        sw.Write("\t" + us);
                    foreach (int us in subclassdict[iq].down)
                        sw.Write("\t" + us);
                    sw.WriteLine();
                }
        }

        public static List<int> donedown = new List<int>();
        public static List<int> doneup = new List<int>();
        public static int sum_down(int iq)
        {
            int isum = 0;
            donedown.Add(iq);
            foreach (int iqd in subclassdict[iq].down)
                if ( !donedown.Contains(iqd))
                    isum += sum_down(iqd);
            //Console.WriteLine(iq+" "+isum);
            return isum;
        }

        public static void follow_up(int iq, string prefix)
        {
            if (doneup.Contains(iq))
                return;

            doneup.Add(iq);
            Console.WriteLine(prefix+subclassdict[iq].label);
            foreach(int iqu in subclassdict[iq].up)
            {
                follow_up(iqu, prefix + "--");
            }
        }

        public static void list_up(JObject wd)
        {
            Console.WriteLine(wd["id"].ToString()+": "+get_best_label(wd));

            Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

            List<string> instancelist = get_claims(wd, propdict["instance_of"], dictObj);

            foreach(string sqi in instancelist)
            {
                int iqi = qtoint(sqi);
                follow_up(iqi, "--");
            }
        }

        public static bool search_up(int iq, int target)
        {
            bool found = false;

            if (iq == target)
                return true;

            if (doneup.Contains(iq))
                return false;

            doneup.Add(iq);

            if (!subclassdict.ContainsKey(iq))
                return false;

            foreach (int iqi in subclassdict[iq].up)
            {
                found = search_up(iqi, target);
                if (found)
                    break;
            }

            return found;
        }

        public static bool search_up(JObject wd,int target)
        {
            bool found = false;

            doneup.Clear();

            int iq = qtoint(wd["id"].ToString());
            if (iq == target)
                return true;

            doneup.Add(iq);

            if (subclassdict.ContainsKey(iq))
                return search_up(iq, target);
            
            Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

            List<string> instancelist = get_claims(wd, propdict["instance_of"], dictObj);

            foreach (string sqi in instancelist)
            {
                int iqi = qtoint(sqi);
                found = search_up(iqi, target);
                if (found)
                    break;
            }

            return found;
        }

        public static void read_subclasses()
        {
            Console.WriteLine("Reading subclass file");
            int nlines = 0;
            using (StreamReader sr = new StreamReader(@"D:\Downloads\wikidata-subclasses.txt"))
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    if (String.IsNullOrEmpty(s))
                        continue;
                    string[] words = s.Split('\t');
                    if (words.Length < 5)
                        continue;
                    int iq = tryconvert(words[0]);
                    if (iq < 0)
                        continue;
                    if (subclassdict.ContainsKey(iq))
                        continue;

                    subclassclass sc = new subclassclass();
                    sc.label = words[1];
                    sc.instances = tryconvert(words[4]);
                    int nup = tryconvert(words[2]);
                    int ndown = tryconvert(words[3]);
                    if (nup > 0)
                        for (int i = 0; i < nup; i++)
                        {
                            int iup = tryconvert(words[5 + i]);
                            if (iup != iqentity)
                                sc.up.Add(iup);
                        }
                    if (ndown > 0)
                        for (int i = 0; i < ndown; i++)
                        {
                            sc.down.Add(tryconvert(words[5 + nup + i]));
                        }

                    subclassdict.Add(iq, sc);

                    nlines++;

                    if (nlines < 10)
                        Console.WriteLine(iq + " " + subclassdict[iq].label + " Up:" + subclassdict[iq].up.Count + " Down:" + subclassdict[iq].down.Count + " Instances:" + subclassdict[iq].instances);
                }

            Console.WriteLine("Done reading " + nlines);
            Console.WriteLine("subclassdict.Count = " + subclassdict.Count);
        }

        public static void analyze_subclasses()
        {

            foreach (int iq in subclassdict.Keys)
            {
                donedown.Clear();
                if ((subclassdict[iq].up.Count == 0) && (subclassdict[iq].down.Count > 0))
                {
                    Console.WriteLine(subclassdict[iq].label);
                    int idown = sum_down(iq);
                    if ((idown > 10) && !String.IsNullOrEmpty(subclassdict[iq].label))
                    {
                        Console.WriteLine(idown + ": " + subclassdict[iq].label);
                    }
                }
            }
        }

        public static void make_small_dump(int n)
        {
            char[] trimchars = "[,]".ToCharArray();
            int nlines = 0;
            using (StreamWriter sw = new StreamWriter(@"D:\Downloads\wikidata-" + n.ToString() + "dump.json"))
            using (StreamReader sr = new StreamReader(@"D:\Downloads\wikidata-dump.json"))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    sw.WriteLine(s);
                    nlines++;
                    if (nlines > n)
                        break;

                }
            }
        }

        public static void get_claim_statistics(JObject wd, Dictionary<string,object> dictObj)
        {
            foreach (string prop in dictObj.Keys)
            {
                if (!claimstatdict.ContainsKey(prop))
                    claimstatdict.Add(prop, 0);
                claimstatdict[prop]++;
            }
            //propdict.Add("citizenship", 27);
            //propdict.Add("birthdate", 569);
            //propdict.Add("occupation", 106);
            //propdict.Add("position_held", 39);

            foreach (string ccc in get_claims(wd,propdict["citizenship"],dictObj))
            {
                if (!countrystatdict.ContainsKey(ccc))
                    countrystatdict.Add(ccc, 0);
                countrystatdict[ccc]++;
            }
            foreach (string ccc in get_claims(wd, propdict["occupation"], dictObj))
            {
                if (!occupationstatdict.ContainsKey(ccc))
                    occupationstatdict.Add(ccc, 0);
                occupationstatdict[ccc]++;
            }
            foreach (string ccc in get_claims(wd, propdict["position_held"], dictObj))
            {
                if (!positionstatdict.ContainsKey(ccc))
                    positionstatdict.Add(ccc, 0);
                positionstatdict[ccc]++;
            }

        }

        public static void save_statdict(string filename,Dictionary<string,int> dict)
        {
            using (StreamWriter sw = new StreamWriter(filename))
                foreach (string cc in dict.Keys)
                {
                    string ccc = "";
                    if (labeldict.ContainsKey(cc))
                        ccc = labeldict[cc];
                    else if (propnamedict.ContainsKey(cc))
                        ccc = propnamedict[cc];
                    sw.WriteLine(cc + "\t" + ccc + "\t"+dict[cc]);
                }

        }

        public static void read_labels()
        {
            //Console.WriteLine("Reading wanted labels");
            //using (StreamReader sr = new StreamReader(@"D:\Downloads\wikidata-wanted-labels.txt"))
            //{

            //    while (!sr.EndOfStream)
            //    {
            //        string s = sr.ReadLine();
            //        if (String.IsNullOrEmpty(s))
            //            continue;
            //        if ( !wanted_labels.Contains(s))
            //            wanted_labels.Add(s);
            //    }
            //}

            //Console.WriteLine("wanted labels " + wanted_labels.Count);
            Console.WriteLine("Getting labels");
            int n = 0;
            using (StreamReader sr = new StreamReader(@"D:\Downloads\wikidata-labels-humans.txt"))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    if (String.IsNullOrEmpty(s))
                        continue;
                    string[] words = s.Split('\t');
                    //if (( words[0].Length < 8) || (wanted_labels.Contains(words[0])))
                        labeldict.Add(words[0], words[1]);
                    n++;
                    if (n % 100000 == 0)
                        Console.WriteLine("n=" + n);
                }

            }

            //using (StreamWriter sw = new StreamWriter(@"D:\Downloads\wikidata-labels-humans")) 
            //{
            //    foreach (string s in labeldict.Keys)
            //    {
            //        sw.WriteLine(s + "\t" + labeldict[s]);
            //      }
            //}      

            Console.WriteLine("Done labels");
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            char[] trimchars = "[,]".ToCharArray();
            fill_propdict();
            read_labels();

            //read_subclasses();

            int nlines = 0;

            //make_small_dump(100000);
            //return;

            int nhumans = 0;
            int nhumanproperties = 0;

            //using (StreamWriter sw = new StreamWriter(@"D:\Downloads\wikidata-labels.txt"))
            using (StreamReader sr = new StreamReader(@"D:\Downloads\wikidata-dump-humans.json"))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    nlines++;
                    //Console.WriteLine(s);
                    //sw.WriteLine(s);
                    //var wd = serializer.Deserialize<wdtopclass2>(s);
                    //Dictionary<string, string> wddict = JsonConvert.DeserializeObject<Dictionary<string, string>>(s);
                    //wdtopclass wddict = JsonConvert.DeserializeObject<wdtopclass>(s);
                    JObject wd = JObject.Parse(s);

                    //sw.WriteLine(wd["id"].ToString() + "\t" + get_best_label(wd));

                    //doneup.Clear();
                    //list_up(wd);

                    //map_subclasses(wd);

                    //if (search_up(wd, classdict["human"]))
                    //{
                    //    Console.WriteLine(get_best_label(wd) + " is human.");
                    //    sw.WriteLine(s);
                    //    nhumans++;
                    //}
                    //else if (human_properties(wd))
                    //{
                    //    Console.WriteLine(get_best_label(wd) + " has human properties.");
                    //    nhumanproperties++;
                    //}

                    //Console.WriteLine(wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    //sw.WriteLine(wd["id"] + "\t" + get_best_label(wd));
                    if (nlines % 1000 == 0)
                    {
                        Console.WriteLine(nlines + " ============================= ");// + wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }

                    //continue;
                    //Console.WriteLine("<cr>");
                    //Console.ReadLine();

                    //continue;

                    //if (wd["sitelinks"].Contains("frwiki"))
                    //    Console.WriteLine(wd["sitelinks"]["frwiki"]["title"]);
                    //else
                    //    Console.WriteLine("no french");

                    //try
                    //{
                    //    Console.WriteLine("Count = "+wd["sitelinks"].Count());

                    Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

                    get_claim_statistics(wd, dictObj);

                    //    foreach (string key in dictObj.Keys)
                    //    {
                    //        Console.WriteLine(key + ":" + wd["claims"][key].Count());
                    //        //Console.WriteLine(key + ":" + wd["claims"][key].First()["mainsnak"]["datavalue"]["value"]);
                    //        for (int iclaim = 0; iclaim < wd["claims"][key].Count(); iclaim++)
                    //        {
                    //            string wdtype = wd["claims"][key][iclaim]["mainsnak"]["datavalue"]["type"].ToString();
                    //            Console.WriteLine(key + ":" + wdtype);
                    //            switch (wdtype)
                    //            {
                    //                case "string":
                    //                    Console.WriteLine(wd["claims"][key][iclaim]["mainsnak"]["datavalue"]["value"]);
                    //                    break;
                    //                case "wikibase-entityid":
                    //                    Console.WriteLine("Q"+wd["claims"][key][iclaim]["mainsnak"]["datavalue"]["value"]["numeric-id"]);
                    //                    break;

                    //            }
                    //            //    Console.WriteLine(key + ":" + wd["claims"][key][iclaim]["mainsnak"]["datavalue"]["value"]);
                    //        }
                    //    }
                    //}
                    //catch (NullReferenceException e)
                    //{
                    //    Console.WriteLine(e);
                    //    //Console.ReadLine();
                    //}
                    

                    //foreach (var vo in wd["sitelinks"])
                    //    Console.WriteLine(vo.ToString());
                    //foreach (JToken jt in wd["sitelinks"].Children())
                        //Console.WriteLine(jt.First["site"]);
                    //foreach (string sl in wd["sitelinks"].)
                        //Console.WriteLine(sl);
                }
            }

            //print_subclasses();

            //Console.WriteLine(nhumans + " humans.");
            //Console.WriteLine(nhumanproperties + " with human properties.");
            save_statdict(@"D:\Downloads\wikidata-countrystats.txt", countrystatdict);
            save_statdict(@"D:\Downloads\wikidata-occupationstats.txt", occupationstatdict);
            save_statdict(@"D:\Downloads\wikidata-positionstats.txt", positionstatdict);
            Console.WriteLine("Done");
            Console.ReadLine();

        }
    }
}
