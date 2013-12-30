using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace j4jayant.HL7.Parser
{
    public class Segment
    {
        private String _Value;
        private String _Name;
        //internal Segment.SegmentList _list;
        private List<Segment> _List;
        private Char[] fieldDelimiters = new Char[] { '|', '^', '~', '&' };
        private short seqNo = 0;

        internal Char[] FieldDelimiters
        {
            get { return fieldDelimiters; }
            set { fieldDelimiters = value; }
        }

        //public List<Field> FieldList { get; set; }
        internal FieldCollection FieldList { get; set; }
        
        public Segment()
        {
            //FieldList = new List<Field>();
            FieldList = new FieldCollection();
        }

        public Segment(String pName)
        {
            //FieldList = new List<Field>();
            FieldList = new FieldCollection();
            _Name = pName;
        }

        public String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        internal short SequenceNo
        {
            get
            {
                return seqNo;
            }
            set
            {
                seqNo = value;
            }
        }

        internal String Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                if (_Value.Length > 0)
                {
                    //FieldList = new List<Field>();

                    char[] fieldSeparatorString = new char[1] { FieldDelimiters[0] };
                    List<String> AllFields = MessageHelper.SplitString(_Value, fieldSeparatorString);

                    if (AllFields.Count > 1)
                    {
                        if (Name == "MSH")
                        {
                            AllFields[0] = new String(fieldSeparatorString);
                        }
                        else
                            AllFields.RemoveAt(0);

                        foreach (String strField in AllFields)
                        {
                            Field field = new Field();
                            field.FieldDelimiters = new Char[3] { FieldDelimiters[1], FieldDelimiters[2], FieldDelimiters[3] };
                            field.Value = strField;
                            FieldList.Add(field);
                        }
                    }
                    else
                    {
                        Field field = new Field();
                        field.FieldDelimiters = new Char[3] { FieldDelimiters[1], FieldDelimiters[2], FieldDelimiters[3] };
                        field.Value = _Value;
                        FieldList.Add(field);
                    }
                }
            }

        }

        internal List<Segment> List
        {
            get
            {
                if (_List == null)
                    _List = new List<Segment>();
                return _List;
            }
            set
            {
                _List = value;
            }
        }

        public bool AddNewField(Field field)
        {
            try
            {
                this.FieldList.Add(field);
                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unable to add new field in segment " + this.Name + " Error - " + ex.Message);
            }
        }

        public bool AddNewField(Field field, int position)
        {
            position = position - 1;
            try
            {
                this.FieldList.Add(field, position);
                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unable to add new field in segment " + this.Name + " Error - " + ex.Message);
            }
        }

        public Field Fields(int position)
        {
            position = position - 1;
            Field field = null;

            try
            {
                field = FieldList[position];
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Field not availalbe Error-" + ex.Message);
            }

            return field;
        }

        public List<Field> GetAllFields()
        {
            return FieldList;
        }
    }

}
