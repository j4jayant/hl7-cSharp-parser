using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace j4jayant.HL7.Parser
{
    public class Component
    {
        private String _Value;
        internal List<SubComponent> SubComponentList { get; set; }
        private Char[] subComponentSeparator = new Char[1] { '&' };
        private bool isSubComponentized = false;

        internal Char[] SubComponentSeparator
        {
            get { return subComponentSeparator; }
            set { subComponentSeparator = value; }
        }

        public bool IsSubComponentized
        {
            get { return isSubComponentized; }
            set { isSubComponentized = value; }
        }

        public Component()
        {
            SubComponentList = new List<SubComponent>();
        }
        public Component(String pValue)
        {
            SubComponentList = new List<SubComponent>();
            _Value = pValue;
        }

        public String Value
        {
            get
            {
                if (_Value == null)
                    return String.Empty;
                else
                    return _Value;
            }
            set
            {
                _Value = value;
                if (_Value.Length > 0)
                {
                    SubComponentList = new List<SubComponent>();
                    List<String> AllSubComponents = MessageHelper.SplitString(_Value, SubComponentSeparator);

                    if (AllSubComponents.Count > 1)
                    {
                        isSubComponentized = true;

                        foreach (String strSubComponent in AllSubComponents)
                        {
                            SubComponent subComponent = new SubComponent();
                            subComponent.Value = strSubComponent;
                            SubComponentList.Add(subComponent);
                        }
                    }
                    else
                    {
                        SubComponentList = new List<SubComponent>();
                        SubComponent subComponent = new SubComponent();
                        subComponent.Value = _Value;
                        SubComponentList.Add(subComponent);
                    }
                }
            }
        }

        public SubComponent SubComponents(int position)
        {
            position = position - 1;
            SubComponent sub = null;

            try
            {
                sub = SubComponentList[position];
            }
            catch (Exception ex)
            {
                throw new HL7Exception("SubComponent not availalbe Error-" + ex.Message);
            }

            return sub;
        }

        public List<SubComponent> SubComponents()
        {
            return SubComponentList;
        }
    }

    internal class ComponentCollection : List<Component>
    {
        internal ComponentCollection()
            : base()
        {

        }

        internal new Component this[int index]
        {
            get
            {
                Component com = null;
                if (index < base.Count)
                    com = base[index];
                return com;
            }
            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Add Component at next position
        /// </summary>
        /// <param name="com">Component</param>
        internal new void Add(Component com)
        {
            base.Add(com);
        }

        /// <summary>
        /// Add component at specific position
        /// </summary>
        /// <param name="com">Component</param>
        /// <param name="position">Position</param>
        internal void Add(Component com, int position)
        {
            position = position - 1;
            int listCount = base.Count;

            if (position <= listCount)
                base[position] = com;
            else
            {
                for (int comIndex = listCount + 1; comIndex <= position; comIndex++)
                {
                    Component blankCom = new Component();
                    blankCom.Value = String.Empty;
                    base.Add(blankCom);
                }
                base.Add(com);
            }
        }
    }
}
