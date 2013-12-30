using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace j4jayant.HL7.Parser
{
    public static class MessageHelper
    {

        static MessageHelper()
        {
        }

        public static List<String> SplitString(String strStringToSplit, String[] strSplitBy, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            return strStringToSplit.Split(strSplitBy, splitOptions).ToList<String>();
        }

        public static List<String> SplitString(String strStringToSplit, char[] chSplitBy, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            return strStringToSplit.Split(chSplitBy, splitOptions).ToList<String>();
        }
    }
}
