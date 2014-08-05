using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using BMGR.Globals;
using System.Globalization;

namespace BMGR.Daemon
{
    public class PagerMessage:ReceivedMessage 
    {
        private struct Word
        {
            private bool _hasHebrewChars;
            private bool _hasEnglishChars;
            private bool _hasNumbers;

            private string _original;
            private string _fixed;
            public bool HasHebrewChars
            {
                get { return _hasHebrewChars; }
                set { _hasHebrewChars = value; }
            }
            public bool HasEnglishChars
            {
                get { return _hasEnglishChars; }
                set { _hasEnglishChars = value; }
            }
            public bool HasNumbers
            {
                get { return _hasNumbers; }
                set { _hasNumbers = value; }
            }
            public bool IsNumber
            {
                get { return _hasNumbers && !_hasEnglishChars && !_hasHebrewChars; }
            }
            public string Translated { get { return _fixed; } }

            public Word(string word)
                :this()
            {
                _original = word;

                _fixed = ConvertCharSet(_original);
                char[] ca = _fixed.ToCharArray();
                for (int i = 0; i < ca.Length; i++)
                {
                    if (!_hasNumbers && Char.IsDigit(ca[i]))
                        _hasNumbers = true;
                    if (!_hasEnglishChars && IsEnglishChar(ca[i]))
                        _hasEnglishChars = true;
                    if (!_hasHebrewChars && IsHebrewChar(ca[i]))
                        _hasHebrewChars = true;
                }
            }

            public void TurnCharsLeftRight()
            {
                char[] ca = _fixed.ToCharArray();
                for (int i = 0; i < ca.Length; i++)
                    ca[i] = TurnCharLeftRight(ca[i]);

                _fixed = new string(ca);
            }

            public static bool IsEnglishChar(char c)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                    return true;
                return false;
            }
            public static bool IsHebrewChar(char c)
            {
                if (c >= 'à' && c <= 'ú')
                    return true;
                return false;
            }
            private static string ConvertCharSet(string s)
            {
                char[] ca = s.ToCharArray();
                for (int i = 0; i < ca.Length; i++)
                {
                    char c;
                    if (ca[i] > 95 && ca[i] <= 122)
                    {
                        //1392 = Unicode values('à' - '`')
                        c = (char)(ca[i] + 1392); //Unicode values
                    }
                    else if (ca[i] < 32) //IGNORE non displayed characters
                        c = ' ';
                    else
                        c = ca[i];

                    ca[i] = c;
                    //ca[i] = TurnCharLeftRight(c);
                }
                return new string(ca);
            }
            private static char TurnCharLeftRight(char c)
            {
                switch (c)
                {
                    case '>': return '<';
                    case '<': return '>';
                    case '(': return ')';
                    case ')': return '(';
                    case '{': return '}';
                    case '}': return '{';
                    case '[': return ']';
                    case ']': return '[';
                    default: return c;
                }
            }


        }
        private static Dictionary<int, string> _capCodes;

        public PagerMessage(byte[] btArray)
        {
            InitCapCodeDictionary();
            //Extract the message from the byte array
            ParseMessage(btArray);

            if (!String.IsNullOrEmpty(Text))
            {
                Text = TranslateToHebrew(Text);
                if (Number<0)  
                    Number = 0;
            }
            else
                Text = String.Empty;
        
        }

        public PagerMessage(string serialString)
        {
            InitCapCodeDictionary();
            //Extract message from string
            ParseMessage(serialString);

            Text = !String.IsNullOrEmpty(Text) ? TranslateToHebrew(Text) : String.Empty;
        }

        
        /// <summary>
        /// Parses teh serial byte stream into the proper parts
        /// </summary>
        /// <param name="btArray">The Beeper serial string to be parsed</param>
        private void ParseMessage(byte[] btArray)
        {
            //[] = optional
            //<STX><CAP-CODE><[*]><MSG-NUMBER><TEXT><ETX><LF><CR><TIME><LF><CR><CHECKSUM><EOT>

            try
            {
                ExtractCapCode(btArray);
                int startPos = ExtractHeader(btArray);
                if (startPos > -1)
                {
                    startPos = ExtractText(btArray, startPos + 1);
                    if (startPos > -1)
                        ExtractTime(btArray, startPos + 1);
                    else
                        Received = DateTime.Now;
                }
            }
            catch
            {
                Trace.TraceError("Unable to parse serial stream! Serial string for debug purposes:\n" +
                    System.Text.Encoding.GetEncoding(862).GetString(btArray));
            }
        }

        /// <summary>
        /// Extract the message parts from the serial "string"
        /// This "String" version is no longer used, as there is a new ByteArray version which allows 
        /// the manipulation of the array before parsing.
        /// </summary>
        /// <param name="serialString">raw serial string received from the pager</param>
        private void ParseMessage(string serialString)
        {
            try
            {
                //[] = optional
                //<STX><CAP-CODE><[*]><MSG-NUMBER><TEXT><ETX><LF><CR><TIME><LF><CR><CHECKSUM><EOT>
                //string[] sp = serialString.Split(new char[] { (char)10, (char)13 });
                //textPart = sp[0];
                //timePart = sp[2];
                int startPos = serialString.IndexOf(SerialBeeper.STX);
                int endPos = serialString.IndexOf(SerialBeeper.ETX)+1;
                string textPart = serialString.Substring(startPos, endPos);
                startPos = endPos;
                endPos = serialString.IndexOf(SerialBeeper.EOT);
                string timePart = serialString.Substring(startPos, endPos-startPos-1 );
                const string expression = @"\x02(?<capcode>[^\*]?)(?<encrypted>\*?)(?<number>[0-9]*[\)\(])(?<text>.*)\x03";
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(expression);
                System.Text.RegularExpressions.Match match = regex.Match(textPart);
                if (match.Success)
                {
                    if (match.Groups["capcode"].Value != "")
                    {
                        int capcode = match.Groups["capcode"].Value.ToCharArray()[0];
                        if (_capCodes.ContainsKey(capcode))
                        {
                            AddressSlot = _capCodes[capcode];
                        }
                        else
                        {
                            //capCode not found in cap index. can be caused if the Pager SW
                            //Version is different, or if the "Send to Serial" event was raised
                            //manually by pressing the Pager buttons. (not automatic when the message
                            //arrived). in this case, we'll treat the capcode if exist as part of 
                            //the normal text.
                            AddressSlot = null;// "N/A";
                        }
                    }
                    //encrypted
                    if (match.Groups["encrypted"].Value == "*")
                        Encrypted = true;

                    //number
                    string sNum = match.Groups["number"].Value;
                    //the message number is received reversed. 
                    //we'll have to reverse it to get the correct number. 
                    sNum = Reverse(sNum.Remove(sNum.Length - 1, 1));
                    int i3;
                    if (int.TryParse(sNum, out i3))
                    {
                        Number = i3;
                    }
                    //text
                    string sText = match.Groups["text"].Value.TrimEnd(')');
                    Text = sText;
                }
                else
                {
                    #region no RegEx parsing - invalid serial string

                    //startPos = 0;
                    //endPos = 0;

                    //            string[] sp = SerialString.Split(new char[] { (char)10, (char)13 });
                    //int capcode = (int)sp[0].ToCharArray()[1];
                    int capcode = textPart.ToCharArray()[1];
                    if (_capCodes.ContainsKey(capcode))
                    {
                        AddressSlot = _capCodes[capcode];
                        //startPos = sp[0].IndexOf((char)capcode) + 1;
                        startPos = textPart.IndexOf((char)capcode) + 1;
                    }
                    else
                    {
                        //capCode not found in cap index. can be caused if the Pager SW
                        //Version is different, or if the "Send to Serial" event was raised
                        //manually by pressing the Pager buttons. (not automatic when the message
                        //arrived). in this case, we'll treat the capcode if exist as part of 
                        //the normal text.
                        AddressSlot = null;// "N/A";
                        startPos = serialString.IndexOf(SerialBeeper.STX) + 1;
                    }
                    endPos = serialString.IndexOf(SerialBeeper.ETX, startPos);

                    if (endPos - startPos - startPos > /*sp[0]*/textPart.Length)
                        Text = textPart.Substring(startPos, textPart.Length - startPos);  
                        //this.Text = sp[0].Substring(startPos, sp[0].Length - startPos);  
                    else
                        //this.Text = sp[0].Substring(startPos, endPos - startPos);
                        Text = textPart.Substring(startPos, endPos - startPos);

                    //string stime = sp[2].Trim();
                    //this.Received = DateTime.Parse(stime);



                    //"*" is the sign telling us whether the transmission was encrypted 
                    if (Text.StartsWith("*"))
                    {
                        Encrypted = true;
                        Text = Text.Remove(0, 1);
                    }

                    //find the message number (i.e: XX) )

                    const int CHARS_TO_CHECK = 4;
                    int i1 = Text.IndexOf('(', 0, CHARS_TO_CHECK);
                    int i2 = Text.IndexOf(')', 0, CHARS_TO_CHECK);
                    if ((i1 >= 0 && i1 <= CHARS_TO_CHECK) || (i2 >= 0 && i2 <= CHARS_TO_CHECK))
                    {
                        int i3;
                        if (int.TryParse(Reverse(Text.Substring(0, i1 + i2 + 1)), out i3))
                        {
                            Number = i3;
                            Text = Text.Remove(0, i1 + i2 + 2);
                        }
                    }

                    #endregion

                }

                string stime = timePart.Trim(); //sp[2].Trim();
                DateTime dtTime;
                DateTimeFormatInfo fi = new CultureInfo("he-IL", false).DateTimeFormat;
                if (DateTime.TryParse(stime, fi, DateTimeStyles.None, out dtTime))
                {
                    Received = dtTime.AddSeconds(DateTime.Now.Second);
                    //sometimes, with misbehaving stream, we get a future date.
                    //in that case - use NOW timestamp...
                    if (Received > DateTime.Now)
                        Received = DateTime.Now;
                }
                else
                    Received = DateTime.Now;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in ParseMessage: " + ex + "\nSerialString:" + serialString); 
                Text = "";
                Received = DateTime.Now;
            }
        }

        /// <summary>
        /// Extract and translate the DateTime part of the beeper byte stream
        /// </summary>
        /// <param name="btArray"></param>
        /// <param name="startPos"></param>
        /// <returns>position of the EOT byte in the array</returns>
        private int ExtractTime(byte[] btArray, int startPos)
        {
            int endPos = Array.IndexOf(btArray, (byte)SerialBeeper.EOT, startPos);
            if (endPos > -1)
            {
                string stime = System.Text.Encoding.ASCII.GetString(btArray, startPos, endPos - startPos).Trim();
                DateTime dtTime;
                DateTimeFormatInfo fi = new CultureInfo("he-IL", false).DateTimeFormat;
                if (DateTime.TryParse(stime, fi, DateTimeStyles.None, out dtTime))
                {
                    Received = dtTime.AddSeconds(DateTime.Now.Second);
                    //sometimes, with misbehaving stream, we get a future date.
                    //in that case - use NOW timestamp...
                    if (Received>DateTime.Now)
                        Received = DateTime.Now;
                }
                else
                    Received = DateTime.Now;
            }
            return endPos;
        }

        private int ExtractText(byte[] btArray,int startPos)
        {
            try
            {
                int endPos = Array.IndexOf(btArray, (byte)SerialBeeper.ETX, startPos);
                if (endPos > -1)
                {
                    Text = System.Text.Encoding.ASCII.GetString(btArray, startPos, endPos - startPos);
                }
                return endPos;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to parse the message text part from the serial stream.");
                Trace.TraceInformation(ex.ToString());
                return -1;
            }
        }

        private int ExtractHeader(byte[] btArray)
        {
            //[] = optional
            //<STX><CAP-CODE><[*]><MSG-NUMBER><TEXT><ETX><LF><CR><TIME><LF><CR><CHECKSUM><EOT>
            const byte ENCRYPTION = 0x2a;
            const byte LEFT_PARENTHESIS = 0x28;
            const byte RIGHT_PARENTHESIS = 0x29;

            int startPos = Array.IndexOf(btArray, (byte)SerialBeeper.STX);
            int endPos=-1;

            Encrypted = false;
            if (startPos > -1)
            {
                //encryption
                startPos += 2;
                byte enc = btArray[startPos];
                if (enc == ENCRYPTION)
                {
                    Encrypted = true;
                    startPos++;
                }

                #region Message number
                //number
                Number = -1;
                int x = startPos;
                while (x < Array.IndexOf(btArray, (byte)SerialBeeper.ETX))
                {
                    if (btArray[x] == LEFT_PARENTHESIS || btArray[x] == RIGHT_PARENTHESIS)
                    {
                        endPos = x;
                        break;
                    }
                    x++;
                } 

                //endPos = Array.IndexOf(btArray, LEFT_PARENTHESIS,startPos,5);
                if (endPos == -1)
                {
                    endPos = Array.IndexOf(btArray, RIGHT_PARENTHESIS,startPos,5 );
                    if (endPos == -1)
                    {
                        return startPos - 1;
                    }
                }
                //since the number is trasmitted in reverse, we'll collect the digits from 
                //end to start
                int i=endPos-1;
                StringBuilder sb = new StringBuilder();
                while (i >= startPos)
                {
                    sb.Append((char)btArray[i]);
                    i--;
                }
                string s = sb.ToString();
                if (s != string.Empty)
                {
                    Number = int.Parse(s);
                }
                #endregion
            }
            return endPos;
        }

        private void ExtractCapCode(byte[] btArray)
        {
            AddressSlot = null;
            int startPos = Array.IndexOf(btArray, (byte)SerialBeeper.STX);

            if (startPos <= -1) return;

            //last pager batch received with undocumented SW change which fixed previous batches
            //invalid capcode signaling. the "new" capcode byte value added 128 (FF hex) in order
            //to bypass the EOX/EOT bugs in some addresses slots.
            //We'll need to extract the original value in order to maintain compatibility 
            //with previous slot address mapping.
            int capcode = btArray[startPos + 1] % 128;
            if (_capCodes.ContainsKey(capcode))
            {
                AddressSlot = _capCodes[capcode];
            }
        }

        private static void InitCapCodeDictionary()
        {
            if (_capCodes != null) return;

            _capCodes = new Dictionary<int, string>();
            _capCodes.Add(0x01, "1A");
            _capCodes.Add(0x11, "1B");
            _capCodes.Add(0x21, "1C");
            _capCodes.Add(0x31, "1D");
            _capCodes.Add(0x02, "2A");
            _capCodes.Add(0x12, "2B");
            _capCodes.Add(0x22, "2C");
            _capCodes.Add(0x32, "2D");
            _capCodes.Add(0x03, "3A");
            _capCodes.Add(0x13, "3B");
            _capCodes.Add(0x23, "3C");
            _capCodes.Add(0x33, "3D");
            _capCodes.Add(0x04, "4A");
            _capCodes.Add(0x14, "4B");
            _capCodes.Add(0x24, "4C");
            _capCodes.Add(0x34, "4D");
            _capCodes.Add(0x05, "5A");
            _capCodes.Add(0x15, "5B");
            _capCodes.Add(0x25, "5C");
            _capCodes.Add(0x35, "5D");
            _capCodes.Add(0x06, "6A");
            _capCodes.Add(0x16, "6B");
            _capCodes.Add(0x26, "6C");
            _capCodes.Add(0x36, "6D");
            _capCodes.Add(0x07, "7A");
            _capCodes.Add(0x17, "7B");
            _capCodes.Add(0x27, "7C");
            _capCodes.Add(0x37, "7D");
            _capCodes.Add(0x08, "8A");
            _capCodes.Add(0x18, "8B");
            _capCodes.Add(0x28, "8C");
            _capCodes.Add(0x38, "8D");
        }

        private static string TranslateToHebrew(string s)
        {
            string[] words = s.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] outWords = new string[words.Length];

            //if all the words are english, we assume this came from an ENGLISH cell, 
            //in that case we don't need to reverse any of the words.
            bool shouldReverse = !IsEnglishSentence(words);
            for (int iWord = 0; iWord < words.Length; iWord++)
            {
                Word word = new Word(words[iWord]);

                if (shouldReverse)
                {
                    //word.TurnCharsLeftRight();
                    if (IsEnglishWord(word.Translated))
                        outWords[iWord] = Reverse(word.Translated);
                    else if (word.HasNumbers)
                    {
                        word.TurnCharsLeftRight();
                        outWords[iWord] = ReverseNumbersInWord(word.Translated);
                    }
                    else if (!word.IsNumber)
                    {
                        word.TurnCharsLeftRight();
                        outWords[iWord] = word.Translated;
                    }
                    else
                    {    
                        word.TurnCharsLeftRight();
                        outWords[iWord] = word.Translated;
                    }
                }
                else
                {
                    outWords[iWord] = word.Translated;
                }
            }
            string output = String.Join(" ", outWords);

            return output;
        }

        private static string ReverseNumbersInWord(string word)
        {
            char[] input = word.ToCharArray();

            int start = -1;
            int end = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsDigit(input[i]))
                {
                    if (start == -1)
                        if (i > 0 && !Word.IsHebrewChar(input[i - 1]) && !Word.IsEnglishChar(input[i-1]) && input[i-1]!= '(' && input[i-1] != ')')
                            start = i - 1;
                        else
                            start = i;
                    end = i;
                }
                else
                    if (start > -1 && (i - end > 1))
                    {
                        Array.Reverse(input, start, end - start + 1);
                        start = -1;
                    }
            }

            if (start > -1 && start != end )
                Array.Reverse(input, start, end - start + 1);
            return new string(input);
        }

        /// <summary>
        /// Reverse a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string Reverse(string input)
        {
            char[] ca = input.ToCharArray();
            Array.Reverse(ca);
            return new String(ca);
        }

        private static bool IsEnglishSentence(IList<string> words)
        {
            for (int i = 0; i < words.Count; i++)
            {
                if (IsHebrewWord(words[i]))
                return false;
            }
            return true;
        }

        private static bool IsEnglishWord(string input)
        {
            Word word = new Word(input);
            return !word.HasHebrewChars && word.HasEnglishChars;
        }

        private static bool IsHebrewWord(string input)
        {
            Word word = new Word(input);
            return word.HasHebrewChars;
        }
        
        public override string ToString()
        {
            return String.Concat (Received.ToString("MM/dd/yyyy HH:mm "), Text);
        }
    }
}
