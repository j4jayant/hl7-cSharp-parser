using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace j4jayant.HL7.Parser
{
    public class Message
    {
        private Char[] DefaultSegmentSeparatorString = new Char[2] { '\r', '\n' };

        private int msgSegmentSeparatorIndex = 0;
        private String _HL7Message = String.Empty;
        private String _Version = String.Empty;
        private String _MessageStructure = String.Empty;
        private String _MessageControlID = String.Empty;
        private String _ProcessingID = String.Empty;
        private short _SegmentCount = 0;
        private List<String> AllSegments = null;

        //11 - VT(\v), 28 - FS, 13 - CR(\r)
        private Char[] messageTrimChars = new Char[] { Convert.ToChar(11), Convert.ToChar(28), Convert.ToChar(13), ' ', '\n' };

        private Char[] fieldDelimiters = new Char[] { '|', '^', '~', '\\', '&' };

        internal Char[] FieldDelimiters
        {
            get { return fieldDelimiters; }
            set { fieldDelimiters = value; }
        }

        private Dictionary<string, Segment> segmentList;

        public String HL7Message { get { return _HL7Message; } set { _HL7Message = value; } }
        public String Version { get { return _Version; } set { _Version = value; } }
        public String MessageStructure { get { return _MessageStructure; } set { _MessageStructure = value; } }
        public String MessageControlID { get { return _MessageControlID; } set { _MessageControlID = value; } }
        public String ProcessingID { get { return _ProcessingID; } set { _ProcessingID = value; } }
        public short SegmentCount { get { return _SegmentCount; } set { _SegmentCount = value; } }

        internal Dictionary<string, Segment> SegmentList
        {
            get
            {
                return segmentList;
            }
            set
            {
                segmentList = value;
            }
        }

        public Message()
        {
            SegmentList = new Dictionary<String, Segment>();
        }

        public Message(String strMessage)
        {
            HL7Message = strMessage;
            SegmentList = new Dictionary<String, Segment>();
        }

        /// <summary>
        /// Parse the HL7 message in text format, throws HL7Exception if error occurs
        /// </summary>
        /// <returns>boolean</returns>
        public bool ParseMessage()
        {
            bool isValid = false;
            bool isParsed = false;
            try
            {
                isValid = ValidateMessage();
            }
            catch (HL7Exception ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unhandeled Exceiption in validation - " + ex.Message, HL7Exception.BAD_MESSAGE);
            }

            if (isValid)
            {
                try
                {
                    if (AllSegments == null || AllSegments.Count <= 0)
                    {
                        AllSegments = MessageHelper.SplitString(HL7Message, new Char[1] { DefaultSegmentSeparatorString[msgSegmentSeparatorIndex] });
                    }

                    short SegSeqNo = 0;
                    foreach (String strSegment in AllSegments)
                    {
                        Segment segNew = new Segment();
                        String segmentName = strSegment.Substring(0, 3);
                        segNew.FieldDelimiters = new Char[] { FieldDelimiters[0], FieldDelimiters[1], FieldDelimiters[2], FieldDelimiters[4] };
                        segNew.Name = segmentName;
                        segNew.Value = strSegment;
                        segNew.SequenceNo = SegSeqNo++;

                        if (SegmentList.ContainsKey(segmentName))
                        {
                            SegmentList[segmentName].List.Add(segNew);
                        }
                        else
                        {
                            SegmentList.Add(segmentName, segNew);
                            SegmentList[segmentName].List.Add(segNew);
                        }
                    }
                    _SegmentCount = SegSeqNo;

                    String strSerializedMessage = String.Empty;
                    try
                    {
                        strSerializedMessage = SerializeMessageWithoutValidate(); //validation not required here as we haven't changed anything in message
                    }
                    catch (HL7Exception ex)
                    {
                        throw new HL7Exception("Failed to serialize parsed message with error -" + ex.Message, HL7Exception.PARSING_ERROR);
                    }

                    if (!String.IsNullOrEmpty(strSerializedMessage))
                    {
                        if (HL7Message.Equals(strSerializedMessage))
                            isParsed = true;
                    }
                    else
                    {
                        throw new HL7Exception("Unable to serialize to origional message -", HL7Exception.PARSING_ERROR);
                    }
                }
                catch (Exception ex)
                {
                    throw new HL7Exception("Failed to parse the message with error -" + ex.Message, HL7Exception.PARSING_ERROR);
                }
            }
            return isParsed;
        }

        /// <summary>
        /// validates the HL7 message for basic syntax
        /// </summary>
        /// <returns>boolean</returns>
        private bool ValidateMessage()
        {
            try
            {
                if (!String.IsNullOrEmpty(HL7Message))
                {
                    HL7Message = HL7Message.Trim(messageTrimChars);

                    //check message length - MSH+Delemeters+12Fields in MSH
                    if (HL7Message.Length < 20)
                    {
                        throw new HL7Exception("Message Length too short" + HL7Message.Length + " fields.", HL7Exception.BAD_MESSAGE);
                    }

                    //check if message starts with header segment
                    if (!HL7Message.StartsWith("MSH"))
                    {
                        throw new HL7Exception("MSH Segment not found at the beggining of the message", HL7Exception.BAD_MESSAGE);
                    }

                    //Find segment separator
                    if (HL7Message.Contains(DefaultSegmentSeparatorString[0]))
                        msgSegmentSeparatorIndex = 0;
                    else if (HL7Message.Contains(DefaultSegmentSeparatorString[1]))
                        msgSegmentSeparatorIndex = 1;
                    else
                        msgSegmentSeparatorIndex = 0;

                    //check Segment Name & 4th character of each segment
                    char fourthCharMSH = HL7Message[3];
                    AllSegments = MessageHelper.SplitString(HL7Message, new Char[1] { DefaultSegmentSeparatorString[msgSegmentSeparatorIndex] });

                    foreach (String strSegment in AllSegments)
                    {
                        bool isValidSegName = false;
                        String segmentName = strSegment.Substring(0, 3);
                        String segNameRegEx = "[A-Z][A-Z][A-Z1-9]";
                        isValidSegName = System.Text.RegularExpressions.Regex.IsMatch(segmentName, segNameRegEx);

                        if (!isValidSegName)
                        {
                            throw new HL7Exception("Invalid Segment Name Found :" + strSegment, HL7Exception.BAD_MESSAGE);
                        }

                        char fourthCharSEG = strSegment[3];

                        if (fourthCharMSH != fourthCharSEG)
                        {
                            throw new HL7Exception("Invalid Segment Found :" + strSegment, HL7Exception.BAD_MESSAGE);
                        }
                    }

                    Char[] _fieldDelimiters_Message = AllSegments[0].Substring(3, 8 - 3).ToArray<Char>();
                    FieldDelimiters = _fieldDelimiters_Message;

                    //count field separators, MSH.12 is required so there should be at least 11 field separators in MSH
                    int countFieldSepInMSH = AllSegments[0].Count(f => f == FieldDelimiters[0]);

                    if (countFieldSepInMSH < 11)
                    {
                        throw new HL7Exception("MSH segment doesn't contain all required fields", HL7Exception.BAD_MESSAGE);
                    }

                    //Find Message Version
                    char[] fieldSeparator = new char[1] { FieldDelimiters[0] };
                    char[] componentSeparator = new char[1] { FieldDelimiters[1] };

                    List<String> MSHFields = AllSegments[0].Split(fieldSeparator, StringSplitOptions.None).ToList<String>();
                    if (MSHFields.Count >= 12)
                    {
                        this._Version = MSHFields[11].Split(componentSeparator, StringSplitOptions.None)[0];
                    }
                    else
                    {
                        throw new HL7Exception("HL7 version not found in MSH Segment", HL7Exception.REQUIRED_FIELD_MISSING);
                    }

                    //Find Message Type & Trigger Event

                    try
                    {
                        String MSH_9 = MSHFields[8];
                        if (!String.IsNullOrEmpty(MSH_9))
                        {
                            System.String[] MSH_9_comps = MSH_9.Split(componentSeparator, StringSplitOptions.None);
                            if (MSH_9_comps.Length >= 3)
                            {
                                this._MessageStructure = MSH_9_comps[2];
                            }
                            else if (MSH_9_comps.Length > 0 && MSH_9_comps[0] != null && MSH_9_comps[0].Equals("ACK"))
                            {
                                this._MessageStructure = "ACK";
                            }
                            else if (MSH_9_comps.Length == 2)
                            {
                                this._MessageStructure = MSH_9_comps[0] + "_" + MSH_9_comps[1];
                            }
                            else
                            {
                                throw new HL7Exception("Message Type & Trigger Event value not found in message", HL7Exception.UNSUPPORTED_MESSAGE_TYPE);
                            }
                        }
                        else
                            throw new HL7Exception("MSH.10 not available", HL7Exception.UNSUPPORTED_MESSAGE_TYPE);
                    }
                    catch (System.IndexOutOfRangeException e)
                    {
                        throw new HL7Exception("Can't find message structure (MSH.9.3) - " + e.Message, HL7Exception.UNSUPPORTED_MESSAGE_TYPE);
                    }

                    try
                    {
                        this._MessageControlID = MSHFields[9];

                        if (String.IsNullOrEmpty(this._MessageControlID))
                            throw new HL7Exception("MSH.10 - Message Control ID not found", HL7Exception.REQUIRED_FIELD_MISSING);
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Error occured while accessing MSH.10 - " + ex.Message, HL7Exception.REQUIRED_FIELD_MISSING);
                    }

                    try
                    {
                        this._ProcessingID = MSHFields[10];

                        if (String.IsNullOrEmpty(this._ProcessingID))
                            throw new HL7Exception("MSH.11 - Processing ID not found", HL7Exception.REQUIRED_FIELD_MISSING);
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Error occured while accessing MSH.11 - " + ex.Message, HL7Exception.REQUIRED_FIELD_MISSING);
                    }
                }
                else
                    throw new HL7Exception("No Message Found", HL7Exception.BAD_MESSAGE);
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Failed to validate the message with error - " + ex.Message, HL7Exception.BAD_MESSAGE);
            }

            return true;
        }

        //validation not required here as we haven't changed anything in message
        private String SerializeMessageWithoutValidate()
        {
            String strMessage = String.Empty;

            String mshString = string.Empty;
            List<Segment> _segListOrdered = getAllSegmentsInOrder();
            _segListOrdered.RemoveAll(o => o.Name.Equals("MSH"));

            try
            {
                Segment mshSegment = this.segmentList["MSH"];
                mshString = mshSegment.Name + FieldDelimiters[0] + mshSegment.FieldList[1].Value + FieldDelimiters[0];

                int indexField = 0;
                try
                {
                    foreach (Field field in mshSegment.FieldList)
                    {
                        indexField++;
                        if (indexField <= 2)
                            continue;

                        if (field.ComponentList.Count > 0)
                        {
                            int indexCom = 0;
                            foreach (Component com in field.ComponentList)
                            {
                                indexCom++;
                                if (com.SubComponentList.Count > 0)
                                {
                                    int indexSubCom = 0;
                                    foreach (SubComponent subCom in com.SubComponentList)
                                    {
                                        indexSubCom++;
                                        mshString += subCom.Value;
                                        if (indexSubCom < com.SubComponentList.Count)
                                            mshString += FieldDelimiters[4];
                                    }
                                }
                                else
                                    mshString += com.Value;

                                if (indexCom < field.ComponentList.Count)
                                    mshString += FieldDelimiters[1];
                            }
                        }
                        else
                            mshString += field.Value;

                        if (indexField < mshSegment.FieldList.Count)
                            mshString += FieldDelimiters[0];
                    }
                }
                catch (Exception ex)
                {
                    throw new HL7Exception("Failed to serialize MSH segment with error - " + ex.Message, HL7Exception.SERIALIZATION_ERROR);
                }
                //mshString += DefaultSegmentSeparatorString[msgSegmentSeparatorIndex];

                foreach (Segment seg in _segListOrdered)
                {
                    strMessage += seg.Name + FieldDelimiters[0];
                    indexField = 0;
                    foreach (Field field in seg.FieldList)
                    {
                        indexField++;
                        if (field.ComponentList.Count > 0)
                        {
                            int indexCom = 0;
                            foreach (Component com in field.ComponentList)
                            {
                                indexCom++;
                                if (com.SubComponentList.Count > 0)
                                {
                                    int indexSubCom = 0;
                                    foreach (SubComponent subCom in com.SubComponentList)
                                    {
                                        indexSubCom++;
                                        strMessage += subCom.Value;
                                        if (indexSubCom < com.SubComponentList.Count)
                                            strMessage += FieldDelimiters[4];
                                    }
                                }
                                else
                                    strMessage += com.Value;

                                if (indexCom < field.ComponentList.Count)
                                    strMessage += FieldDelimiters[1];
                            }
                        }
                        else
                            strMessage += field.Value;

                        if (indexField < seg.FieldList.Count)
                            strMessage += FieldDelimiters[0];
                    }
                    strMessage += DefaultSegmentSeparatorString[msgSegmentSeparatorIndex];
                }

                strMessage = mshString + DefaultSegmentSeparatorString[msgSegmentSeparatorIndex] + strMessage;
                return strMessage.Trim(messageTrimChars);
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Failed to serialize the message with error - " + ex.Message, HL7Exception.SERIALIZATION_ERROR);
            }
        }

        //validation required here as user might have changed the message
        /// <summary>
        /// Serialize the message in text format
        /// </summary>
        /// <returns>String with HL7 message</returns>
        public String SerializeMessage()
        {
            String strMessage = String.Empty;

            if (ValidateMessage())
            {
                String mshString = string.Empty;
                List<Segment> _segListOrdered = getAllSegmentsInOrder();
                _segListOrdered.RemoveAll(o => o.Name.Equals("MSH"));

                try
                {
                    Segment mshSegment = this.segmentList["MSH"];
                    mshString = mshSegment.Name + FieldDelimiters[0] + mshSegment.FieldList[1].Value + FieldDelimiters[0];

                    int indexField = 0;
                    try
                    {
                        foreach (Field field in mshSegment.FieldList)
                        {
                            indexField++;
                            if (indexField <= 2)
                                continue;

                            if (field.ComponentList.Count > 0)
                            {
                                int indexCom = 0;
                                foreach (Component com in field.ComponentList)
                                {
                                    indexCom++;
                                    if (com.SubComponentList.Count > 0)
                                    {
                                        int indexSubCom = 0;
                                        foreach (SubComponent subCom in com.SubComponentList)
                                        {
                                            indexSubCom++;
                                            mshString += subCom.Value;
                                            if (indexSubCom < com.SubComponentList.Count)
                                                mshString += FieldDelimiters[4];
                                        }
                                    }
                                    else
                                        mshString += com.Value;

                                    if (indexCom < field.ComponentList.Count)
                                        mshString += FieldDelimiters[1];
                                }
                            }
                            else
                                mshString += field.Value;

                            if (indexField < mshSegment.FieldList.Count)
                                mshString += FieldDelimiters[0];
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Failed to serialize MSH segment with error - " + ex.Message, HL7Exception.SERIALIZATION_ERROR);
                    }
                    //mshString += DefaultSegmentSeparatorString[msgSegmentSeparatorIndex];

                    foreach (Segment seg in _segListOrdered)
                    {
                        strMessage += seg.Name + FieldDelimiters[0];
                        indexField = 0;
                        foreach (Field field in seg.FieldList)
                        {
                            indexField++;
                            if (field.ComponentList.Count > 0)
                            {
                                int indexCom = 0;
                                foreach (Component com in field.ComponentList)
                                {
                                    indexCom++;
                                    if (com.SubComponentList.Count > 0)
                                    {
                                        int indexSubCom = 0;
                                        foreach (SubComponent subCom in com.SubComponentList)
                                        {
                                            indexSubCom++;
                                            strMessage += subCom.Value;
                                            if (indexSubCom < com.SubComponentList.Count)
                                                strMessage += FieldDelimiters[4];
                                        }
                                    }
                                    else
                                        strMessage += com.Value;

                                    if (indexCom < field.ComponentList.Count)
                                        strMessage += FieldDelimiters[1];
                                }
                            }
                            else
                                strMessage += field.Value;

                            if (indexField < seg.FieldList.Count)
                                strMessage += FieldDelimiters[0];
                        }
                        strMessage += DefaultSegmentSeparatorString[msgSegmentSeparatorIndex];
                    }

                    strMessage = mshString + DefaultSegmentSeparatorString[msgSegmentSeparatorIndex] + strMessage;
                    return strMessage.Trim(messageTrimChars);
                }
                catch (Exception ex)
                {
                    throw new HL7Exception("Failed to serialize the message with error - " + ex.Message, HL7Exception.SERIALIZATION_ERROR);
                }
            }
            else
                throw new HL7Exception("Failed to validate updated message", HL7Exception.BAD_MESSAGE);
        }

        //get all segments in order as they appear in origional message
        //like this is the usual order IN1|1 IN2|1 IN1|2 IN2|2
        //segmentlist stroes segments based on key, so all the IN1s will be stored together without mainting the order
        private List<Segment> getAllSegmentsInOrder()
        {
            List<Segment> _list = new List<Segment>();

            foreach (String segName in SegmentList.Keys)
            {
                foreach (Segment seg in SegmentList[segName].List)
                {
                    _list.Add(seg);
                }
            }

            List<Segment> _listOrdered = _list.OrderBy(o => o.SequenceNo).ToList();

            return _listOrdered;
        }
        
        /// <summary>
        /// Get the Value of specific Field/Component/SubCpomponent, throws error if field/component index is not valid
        /// </summary>
        /// <param name="strValueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>Value of specified field/component/subcomponent</returns>
        public String getValue(String strValueFormat)
        {
            bool isValid = false;

            String segmentName = String.Empty;
            int fieldIndex = 0;
            int componentIndex = 0;
            int subComponentIndex = 0;
            int comCount = 0;
            String strValue = String.Empty;

            List<String> AllComponents = MessageHelper.SplitString(strValueFormat, new char[] { '.' });
            comCount = AllComponents.Count;

            isValid = validateValueFormat(AllComponents);

            if (isValid)
            {
                segmentName = AllComponents[0];
                if (SegmentList.ContainsKey(segmentName))
                {
                    if (comCount == 4)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        Int32.TryParse(AllComponents[2], out componentIndex);
                        Int32.TryParse(AllComponents[3], out subComponentIndex);

                        try
                        {
                            strValue = SegmentList[segmentName].FieldList[fieldIndex - 1].ComponentList[componentIndex - 1].SubComponentList[subComponentIndex - 1].Value;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("SubComponent not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else if (comCount == 3)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        Int32.TryParse(AllComponents[2], out componentIndex);

                        try
                        {
                            strValue = SegmentList[segmentName].FieldList[fieldIndex - 1].ComponentList[componentIndex - 1].Value;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("Component not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else if (comCount == 2)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        try
                        {
                            strValue = SegmentList[segmentName].FieldList[fieldIndex - 1].Value;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("Field not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            strValue = SegmentList[segmentName].Value;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("Segment Value not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                }
                else
                    throw new HL7Exception("Segment Name not available");
            }
            else
                throw new HL7Exception("Request Format is not valid");

            return strValue;
        }

        /// <summary>
        /// Sets the Value of specific Field/Component/SubCpomponent, throws error if field/component index is not valid
        /// </summary>
        /// <param name="strValueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <param name="strValue">Value for the specified field/component</param>
        /// <returns>boolean</returns>
        public bool setValue(String strValueFormat, String strValue)
        {
            bool isValid = false;
            bool isSet = false;

            String segmentName = String.Empty;
            int fieldIndex = 0;
            int componentIndex = 0;
            int subComponentIndex = 0;
            int comCount = 0;

            List<String> AllComponents = MessageHelper.SplitString(strValueFormat, new char[] { '.' });
            comCount = AllComponents.Count;

            isValid = validateValueFormat(AllComponents);

            if (isValid)
            {
                segmentName = AllComponents[0];
                if (SegmentList.ContainsKey(segmentName))
                {
                    if (comCount == 4)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        Int32.TryParse(AllComponents[2], out componentIndex);
                        Int32.TryParse(AllComponents[3], out subComponentIndex);

                        try
                        {
                            SegmentList[segmentName].FieldList[fieldIndex - 1].ComponentList[componentIndex - 1].SubComponentList[subComponentIndex - 1].Value = strValue;
                            isSet = true;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("SubComponent not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else if (comCount == 3)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        Int32.TryParse(AllComponents[2], out componentIndex);

                        try
                        {
                            SegmentList[segmentName].FieldList[fieldIndex - 1].ComponentList[componentIndex - 1].Value = strValue;
                            isSet = true;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("Component not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else if (comCount == 2)
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        try
                        {
                            SegmentList[segmentName].FieldList[fieldIndex - 1].Value = strValue;
                            isSet = true;
                        }
                        catch (Exception ex)
                        {
                            throw new HL7Exception("Field not available - " + strValueFormat + " Error: " + ex.Message);
                        }
                    }
                    else
                    {
                        throw new HL7Exception("Cannot overwrite a Segment Value");
                    }
                }
                else
                    throw new HL7Exception("Segment Name not available");
            }
            else
                throw new HL7Exception("Request Format is not valid");

            return isSet;
        }

        private bool validateValueFormat(List<String> AllComponents)
        {
            String segNameRegEx = "[A-Z][A-Z][A-Z1-9]";
            String otherRegEx = @"^[1-9]([0-9]{1,2})?$";
            bool isValid = false;

            if (AllComponents.Count > 0)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(AllComponents[0], segNameRegEx))
                {
                    for (int i = 1; i < AllComponents.Count; i++)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(AllComponents[i], otherRegEx))
                            isValid = true;
                        else
                            return false;

                    }
                }
                else
                    isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// check if specified field has components
        /// </summary>
        /// <param name="strValueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>boolean</returns>
        public bool IsComponentized(String strValueFormat)
        {
            bool isComponentized = false;
            bool isValid = false;

            String segmentName = String.Empty;
            int fieldIndex = 0;
            int comCount = 0;

            List<String> AllComponents = MessageHelper.SplitString(strValueFormat, new char[] { '.' });
            comCount = AllComponents.Count;

            isValid = validateValueFormat(AllComponents);

            if (isValid)
            {
                segmentName = AllComponents[0];
                if (comCount >= 2)
                {
                    try
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);

                        isComponentized = SegmentList[segmentName].FieldList[fieldIndex - 1].IsComponentized;
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Field not available - " + strValueFormat + " Error: " + ex.Message);
                    }
                }
                else
                    throw new HL7Exception("Field not identified in request");
            }
            else
                throw new HL7Exception("Request Format is not valid");

            return isComponentized;
        }

        /// <summary>
        /// check if specified fields has repeatitions
        /// </summary>
        /// <param name="strValueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>boolean</returns>
        public bool HasRepeatitions(String strValueFormat)
        {
            bool hasRepeatitions = false;
            bool isValid = false;

            String segmentName = String.Empty;
            int fieldIndex = 0;
            int comCount = 0;

            List<String> AllComponents = MessageHelper.SplitString(strValueFormat, new char[] { '.' });
            comCount = AllComponents.Count;

            isValid = validateValueFormat(AllComponents);

            if (isValid)
            {
                segmentName = AllComponents[0];
                if (comCount >= 2)
                {
                    try
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);

                        hasRepeatitions = SegmentList[segmentName].FieldList[fieldIndex - 1].HasRepetitions;
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Field not available - " + strValueFormat + " Error: " + ex.Message);
                    }
                }
                else
                    throw new HL7Exception("Field not identified in request");
            }
            else
                throw new HL7Exception("Request Format is not valid");

            return hasRepeatitions;
        }

        /// <summary>
        /// check if specified component has sub components
        /// </summary>
        /// <param name="strValueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>boolean</returns>
        public bool IsSubComponentized(String strValueFormat)
        {
            bool isSubComponentized = false;
            bool isValid = false;

            String segmentName = String.Empty;
            int fieldIndex = 0;
            int componentIndex = 0;
            int comCount = 0;

            List<String> AllComponents = MessageHelper.SplitString(strValueFormat, new char[] { '.' });
            comCount = AllComponents.Count;

            isValid = validateValueFormat(AllComponents);

            if (isValid)
            {
                segmentName = AllComponents[0];
                if (comCount >= 3)
                {
                    try
                    {
                        Int32.TryParse(AllComponents[1], out fieldIndex);
                        Int32.TryParse(AllComponents[2], out componentIndex);

                        isSubComponentized = SegmentList[segmentName].FieldList[fieldIndex - 1].ComponentList[componentIndex - 1].IsSubComponentized;
                    }
                    catch (Exception ex)
                    {
                        throw new HL7Exception("Component not available - " + strValueFormat + " Error: " + ex.Message);
                    }
                }
                else
                    throw new HL7Exception("Component not identified in request");
            }
            else
                throw new HL7Exception("Request Format is not valid");

            return isSubComponentized;
        }

        /// <summary>
        /// get acknowledgement message for this message
        /// </summary>
        /// <returns>String with ack message</returns>
        public String getACK()
        {
            String ackMsg = String.Empty;
            if (this.MessageStructure != "ACK")
            {
                ackMsg += "MSH" + new String(fieldDelimiters) + FieldDelimiters[0] + this.SegmentList["MSH"].FieldList[4].Value + FieldDelimiters[0] + this.SegmentList["MSH"].FieldList[5].Value + FieldDelimiters[0] + this.SegmentList["MSH"].FieldList[2].Value + FieldDelimiters[0] + this.SegmentList["MSH"].FieldList[3].Value + FieldDelimiters[0] + "ddmmyyyy" + FieldDelimiters[0] + this.SegmentList["MSH"].FieldList[7].Value + FieldDelimiters[0] + "ACK" + FieldDelimiters[0] + this.MessageControlID + FieldDelimiters[0] + this.ProcessingID + FieldDelimiters[0] + this.Version + this.DefaultSegmentSeparatorString[msgSegmentSeparatorIndex];
                ackMsg += "MSA" + FieldDelimiters[0] + "AA" + FieldDelimiters[0] + this.MessageControlID + this.DefaultSegmentSeparatorString[msgSegmentSeparatorIndex];
            }
            return ackMsg;
        }

        /// <summary>
        /// get negative ack for this message
        /// </summary>
        /// <param name="code">ack code like AR, AE</param>
        /// <param name="errMsg">error message to be sent with ACK</param>
        /// <returns>String with ack message</returns>
        public String getNACK(String code, String errMsg)
        {
            String ackMsg = String.Empty;
            if (this.MessageStructure != "ACK")
            {
                ackMsg = String.Empty;
                ackMsg += "MSH" + new String(fieldDelimiters) + FieldDelimiters[0] + this.SegmentList["MSH"].FieldList[4].Value + FieldDelimiters[0] + this.SegmentList["MSH"].FieldList[5].Value + FieldDelimiters[0] + this.SegmentList["MSH"].FieldList[2].Value + FieldDelimiters[0] + this.SegmentList["MSH"].FieldList[3].Value + FieldDelimiters[0] + "ddmmyyyy" + FieldDelimiters[0] + this.SegmentList["MSH"].FieldList[7].Value + FieldDelimiters[0] + "ACK" + FieldDelimiters[0] + this.MessageControlID + FieldDelimiters[0] + this.ProcessingID + FieldDelimiters[0] + this.Version + this.DefaultSegmentSeparatorString[msgSegmentSeparatorIndex];
                ackMsg += "MSA" + FieldDelimiters[0] + code + FieldDelimiters[0] + this.MessageControlID + FieldDelimiters[0] + errMsg + this.DefaultSegmentSeparatorString[msgSegmentSeparatorIndex];
            }
            return ackMsg;
        }

        public bool AddNewSegment(Segment newSeg)
        {
            try
            {
                newSeg.SequenceNo = SegmentCount++;
                if (SegmentList.ContainsKey(newSeg.Name))
                {
                    SegmentList[newSeg.Name].List.Add(newSeg);
                }
                else
                {
                    SegmentList.Add(newSeg.Name, newSeg);
                    SegmentList[newSeg.Name].List.Add(newSeg);
                }
                return true;
            }
            catch (Exception ex)
            {
                SegmentCount--;
                throw new HL7Exception("Unable to add new segment Error - " + ex.Message);
            }
        }

        public List<Segment> Segments()
        {
            return getAllSegmentsInOrder();
        }

        public List<Segment> Segments(String segmentName)
        {
            return getAllSegmentsInOrder().FindAll(o=> o.Name.Equals(segmentName));
        }

        public Segment DefaultSegment(String segmentName)
        {
            return getAllSegmentsInOrder().First(o => o.Name.Equals(segmentName));
        }
    }

}
