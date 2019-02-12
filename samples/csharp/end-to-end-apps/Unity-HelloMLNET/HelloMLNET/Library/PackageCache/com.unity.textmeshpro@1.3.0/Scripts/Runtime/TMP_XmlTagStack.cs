namespace TMPro
{
    /// <summary>
    /// Structure used to track basic XML tags which are binary (on / off)
    /// </summary>
    public struct TMP_BasicXmlTagStack
    {
        public byte bold;
        public byte italic;
        public byte underline;
        public byte strikethrough;
        public byte highlight;
        public byte superscript;
        public byte subscript;
        public byte uppercase;
        public byte lowercase;
        public byte smallcaps;

        /// <summary>
        /// Clear the basic XML tag stack.
        /// </summary>
        public void Clear()
        {
            bold = 0;
            italic = 0;
            underline = 0;
            strikethrough = 0;
            highlight = 0;
            superscript = 0;
            subscript = 0;
            uppercase = 0;
            lowercase = 0;
            smallcaps = 0;
        }

        public byte Add(FontStyles style)
        {
            switch (style)
            {
                case FontStyles.Bold:
                    bold += 1;
                    return bold;
                case FontStyles.Italic:
                    italic += 1;
                    return italic;
                case FontStyles.Underline:
                    underline += 1;
                    return underline;
                case FontStyles.Strikethrough:
                    strikethrough += 1;
                    return strikethrough;
                case FontStyles.Superscript:
                    superscript += 1;
                    return superscript;
                case FontStyles.Subscript:
                    subscript += 1;
                    return subscript;
                case FontStyles.Highlight:
                    highlight += 1;
                    return highlight;
            }

            return 0;
        }

        public byte Remove(FontStyles style)
        {
            switch (style)
            {
                case FontStyles.Bold:
                    if (bold > 1)
                        bold -= 1;
                    else
                        bold = 0;
                    return bold;
                case FontStyles.Italic:
                    if (italic > 1)
                        italic -= 1;
                    else
                        italic = 0;
                    return italic;
                case FontStyles.Underline:
                    if (underline > 1)
                        underline -= 1;
                    else
                        underline = 0;
                    return underline;
                case FontStyles.Strikethrough:
                    if (strikethrough > 1)
                        strikethrough -= 1;
                    else
                        strikethrough = 0;
                    return strikethrough;
                case FontStyles.Highlight:
                    if (highlight > 1)
                        highlight -= 1;
                    else
                        highlight = 0;
                    return highlight;
                case FontStyles.Superscript:
                    if (superscript > 1)
                        superscript -= 1;
                    else
                        superscript = 0;
                    return superscript;
                case FontStyles.Subscript:
                    if (subscript > 1)
                        subscript -= 1;
                    else
                        subscript = 0;
                    return subscript;
            }

            return 0;
        }
    }


    /// <summary>
    /// Structure used to track XML tags of various types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct TMP_XmlTagStack<T>
    {
        public T[] itemStack;
        public int index;

        private int m_capacity;
        private T m_defaultItem;

        private const int k_defaultCapacity = 4;
        //static readonly T[] m_emptyStack = new T[0];

        /// <summary>
        /// Constructor to create a new item stack.
        /// </summary>
        /// <param name="tagStack"></param>
        public TMP_XmlTagStack(T[] tagStack)
        {
            itemStack = tagStack;
            m_capacity = tagStack.Length;
            index = 0;

            m_defaultItem = default(T);
        }


        /// <summary>
        /// Function to clear and reset stack to first item.
        /// </summary>
        public void Clear()
        {
            index = 0;
        }


        /// <summary>
        /// Function to set the first item on the stack and reset index.
        /// </summary>
        /// <param name="item"></param>
        public void SetDefault(T item)
        {
            itemStack[0] = item;
            index = 1;
        }


        /// <summary>
        /// Function to add a new item to the stack.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (index < itemStack.Length)
            {
                itemStack[index] = item;
                index += 1;
            }
        }


        /// <summary>
        /// Function to retrieve an item from the stack.
        /// </summary>
        /// <returns></returns>
        public T Remove()
        {
            index -= 1;

            if (index <= 0)
            {
                index = 1;
                return itemStack[0];

            }

            return itemStack[index - 1];
        }

        public void Push(T item)
        {
            if (index == m_capacity)
            {
                m_capacity *= 2;
                if (m_capacity == 0)
                    m_capacity = k_defaultCapacity;

                System.Array.Resize(ref itemStack, m_capacity);
            }

            itemStack[index] = item;
            index += 1;
        }

        public T Pop()
        {
            if (index == 0)
                return default(T);

            index -= 1;
            T item = itemStack[index];
            itemStack[index] = m_defaultItem;

            return item;
        }


        /// <summary>
        /// Function to retrieve the current item from the stack.
        /// </summary>
        /// <returns>itemStack <T></returns>
        public T CurrentItem()
        {
            if (index > 0)
                return itemStack[index - 1];

            return itemStack[0];
        }


        /// <summary>
        /// Function to retrieve the previous item without affecting the stack.
        /// </summary>
        /// <returns></returns>
        public T PreviousItem()
        {
            if (index > 1)
                return itemStack[index - 2];

            return itemStack[0];
        }
    }
}
